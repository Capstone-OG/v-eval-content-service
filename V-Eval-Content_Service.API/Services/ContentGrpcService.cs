using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VEval.Shared.Grpc.Content;
using V_Eval_Content_Service.API.Caching;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Helpers;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.API.Services;

public class ContentGrpcService : ContentService.ContentServiceBase
{
    private static readonly SemaphoreSlim SkillsTreeLock = new(1, 1);
    private static readonly SemaphoreSlim SkillsLookupLock = new(1, 1);

    private readonly IContentDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ContentGrpcService> _logger;

    public ContentGrpcService(
        IContentDbContext context,
        IMemoryCache cache,
        ILogger<ContentGrpcService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    private sealed record SkillLookupItem(Guid SkillId, string Name, Guid? ParentId);

    private async Task<(Dictionary<Guid, Skill> SkillMap, HashSet<Guid> ParentIdsWithChildren)> GetSkillsLookupAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ContentCacheKeys.SkillsLookup, out (Dictionary<Guid, Skill>, HashSet<Guid>) cached))
        {
            return cached;
        }

        await SkillsLookupLock.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(ContentCacheKeys.SkillsLookup, out cached))
            {
                return cached;
            }

            var lightweightSkills = await _context.Skills
                .AsNoTracking()
                .Select(s => new SkillLookupItem(s.SkillId, s.Name, s.ParentId))
                .ToListAsync(ct);

            var skillMap = lightweightSkills.ToDictionary(
                s => s.SkillId,
                s => new Skill { SkillId = s.SkillId, Name = s.Name, ParentId = s.ParentId });

            var parentIdsWithChildren = lightweightSkills
                .Where(s => s.ParentId.HasValue)
                .Select(s => s.ParentId!.Value)
                .ToHashSet();

            var value = (skillMap, parentIdsWithChildren);
            _cache.Set(ContentCacheKeys.SkillsLookup, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            return value;
        }
        finally
        {
            SkillsLookupLock.Release();
        }
    }

    internal static string ResolveDomainCode(Guid domainId, string? domainName)
        => SubjectTaxonomyHelper.ResolveDomainCode(domainId, domainName);

    internal static string InferSubjectName(string? chapterName, string? skillName, string domainCode)
        => SubjectTaxonomyHelper.InferSubjectName(chapterName, skillName, domainCode);

    internal static (string Level, string ChapterName, string LessonName) ResolveSkillLineage(
        Skill skill,
        Dictionary<Guid, Skill> skillMap,
        HashSet<Guid> parentIdsWithChildren)
    {
        if (!skill.ParentId.HasValue)
        {
            bool isChapter = skill.Name.StartsWith("Chương", StringComparison.OrdinalIgnoreCase)
                || parentIdsWithChildren.Contains(skill.SkillId);
            return isChapter
                ? ("CHAPTER", skill.Name, string.Empty)
                : ("LEGACY", string.Empty, string.Empty);
        }

        if (skillMap.TryGetValue(skill.ParentId.Value, out var parent))
        {
            if (!parent.ParentId.HasValue)
            {
                // Direct child of a Chapter -> LESSON (or leaf PATTERN if it has no children)
                bool hasChildren = parentIdsWithChildren.Contains(skill.SkillId);
                return hasChildren
                    ? ("LESSON", parent.Name, skill.Name)
                    : ("PATTERN", parent.Name, skill.Name);
            }

            if (skillMap.TryGetValue(parent.ParentId.Value, out var grandParent))
            {
                return ("PATTERN", grandParent.Name, parent.Name);
            }

            return ("PATTERN", parent.Name, skill.Name);
        }

        return ("PATTERN", string.Empty, string.Empty);
    }

    public override async Task<GetSkillsTreeResponse> GetSkillsTree(
        GetSkillsTreeRequest request,
        ServerCallContext context)
    {
        bool learningNodesOnly = string.Equals(request.SubjectId, "LEARNING_NODES", StringComparison.OrdinalIgnoreCase);
        var cacheKey = learningNodesOnly ? ContentCacheKeys.SkillsTreeLearningNodes : ContentCacheKeys.SkillsTreeAll;

        if (_cache.TryGetValue(cacheKey, out GetSkillsTreeResponse? cached) && cached != null)
        {
            return cached;
        }

        await SkillsTreeLock.WaitAsync(context.CancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out cached) && cached != null)
            {
                return cached;
            }

            var skills = await _context.Skills
                .AsNoTracking()
                .AsSplitQuery()
                .Include(s => s.Prerequisites)
                .Include(s => s.Domain)
                .OrderBy(s => s.SkillId)
                .ToListAsync(context.CancellationToken);

            var skillMap = skills.ToDictionary(s => s.SkillId);
            var parentIdsWithChildren = skills
                .Where(s => s.ParentId.HasValue)
                .Select(s => s.ParentId!.Value)
                .ToHashSet();

            var materialsBySkill = await _context.Materials
                .AsNoTracking()
                .GroupBy(m => m.SkillId)
                .Select(g => new
                {
                    SkillId = g.Key,
                    MaterialId = g.OrderBy(m => m.CreatedAt).Select(m => m.MaterialId).FirstOrDefault(),
                    VideoUrl = g.OrderBy(m => m.CreatedAt).Select(m => m.VideoUrl).FirstOrDefault(v => !string.IsNullOrEmpty(v))
                })
                .ToDictionaryAsync(x => x.SkillId, context.CancellationToken);

            var questionCountsBySkill = await _context.Questions
                .AsNoTracking()
                .GroupBy(q => q.SkillId)
                .Select(g => new { SkillId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SkillId, x => x.Count, context.CancellationToken);

            var response = new GetSkillsTreeResponse();

            foreach (var s in skills)
            {
                var domainCode = ResolveDomainCode(s.DomainId, s.Domain?.Name);
                var (level, chapterName, lessonName) = ResolveSkillLineage(s, skillMap, parentIdsWithChildren);

                if (learningNodesOnly && level != "PATTERN")
                {
                    continue;
                }

                var subjectName = InferSubjectName(chapterName, s.Name, domainCode);

                materialsBySkill.TryGetValue(s.SkillId, out var mat);
                // If pattern has no direct material, inherit parent lesson's material or a sibling pattern's material in the same lesson
                if (mat == null && s.ParentId.HasValue)
                {
                    materialsBySkill.TryGetValue(s.ParentId.Value, out mat);
                    if (mat == null)
                    {
                        var siblingId = skills
                            .Where(sib => sib.ParentId == s.ParentId && sib.SkillId != s.SkillId && materialsBySkill.ContainsKey(sib.SkillId))
                            .Select(sib => sib.SkillId)
                            .FirstOrDefault();
                        if (siblingId != Guid.Empty)
                        {
                            materialsBySkill.TryGetValue(siblingId, out mat);
                        }
                    }
                }

                questionCountsBySkill.TryGetValue(s.SkillId, out var qCount);

                var node = new SkillNode
                {
                    SkillId = s.SkillId.ToString(),
                    Name = s.Name,
                    Description = !string.IsNullOrEmpty(chapterName) ? $"{subjectName} • {chapterName}" : (s.Domain?.Name ?? string.Empty),
                    Weight = s.Weight ?? 0.05,
                    DomainId = s.DomainId.ToString(),
                    DomainName = s.Domain?.Name ?? string.Empty,
                    DomainCode = domainCode,
                    ParentId = s.ParentId?.ToString() ?? string.Empty,
                    Level = level,
                    ChapterName = chapterName,
                    LessonName = lessonName,
                    SubjectName = subjectName,
                    MaterialId = mat != null && mat.MaterialId != Guid.Empty ? mat.MaterialId.ToString() : string.Empty,
                    VideoUrl = mat?.VideoUrl ?? string.Empty,
                    QuestionCount = qCount
                };

                foreach (var prereq in s.Prerequisites)
                {
                    node.PrerequisiteIds.Add(prereq.PrerequisiteId.ToString());
                }

                response.Skills.Add(node);
            }

            _cache.Set(cacheKey, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });

            _logger.LogInformation("Delivered & cached skills tree with {Count} skills (filter={Filter}).", response.Skills.Count, request.SubjectId);
            return response;
        }
        finally
        {
            SkillsTreeLock.Release();
        }
    }

    public override async Task<GetQuestionsByDifficultyResponse> GetQuestionsByDifficulty(
        GetQuestionsByDifficultyRequest request,
        ServerCallContext context)
    {
        var skillIds = request.SkillIds
            .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        var query = _context.Questions
            .AsNoTracking()
            .AsQueryable();

        if (skillIds.Count > 0)
        {
            query = query.Where(q => skillIds.Contains(q.SkillId));
        }

        var diff = (request.Difficulty ?? string.Empty).Trim().ToUpperInvariant();
        query = diff switch
        {
            "EASY" => query.Where(q => q.DifficultyLevel <= 2),
            "MEDIUM" => query.Where(q => q.DifficultyLevel == 3),
            "HARD" => query.Where(q => q.DifficultyLevel >= 4),
            _ => query
        };

        int limit = request.Limit > 0 ? Math.Min(request.Limit, 100) : 20;
        var questions = await query
            .OrderBy(q => q.DifficultyLevel)
            .ThenBy(q => q.CreatedAt)
            .Take(limit)
            .Select(q => new
            {
                q.QuestionId,
                q.SkillId,
                q.DifficultyLevel,
                SkillName = q.Skill != null ? q.Skill.Name : null
            })
            .ToListAsync(context.CancellationToken);

        var response = new GetQuestionsByDifficultyResponse();
        foreach (var q in questions)
        {
            response.Questions.Add(new QuestionSummary
            {
                QuestionId = q.QuestionId.ToString(),
                SkillId = q.SkillId.ToString(),
                Difficulty = q.DifficultyLevel.ToString(),
                Title = q.SkillName ?? "Câu hỏi khảo thí"
            });
        }

        return response;
    }

    public override async Task<GetExamAnswerKeyResponse> GetExamAnswerKey(
        GetExamAnswerKeyRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ExamId, out var examId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Exam ID không hợp lệ."));
        }

        var cacheKey = ContentCacheKeys.ExamAnswerKey(examId);
        if (_cache.TryGetValue(cacheKey, out GetExamAnswerKeyResponse? cached) && cached != null)
        {
            return cached;
        }

        var examQuestions = await _context.ExamQuestions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(eq => eq.Question)
                .ThenInclude(q => q.Passage)
            .Include(eq => eq.Question)
                .ThenInclude(q => q.Skill)
                    .ThenInclude(s => s.Domain)
            .Where(eq => eq.ExamId == examId)
            .OrderBy(eq => eq.QuestionOrder)
            .ToListAsync(context.CancellationToken);

        if (examQuestions.Count == 0)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy câu hỏi nào cho đề thi {examId}."));
        }

        var (skillMap, parentIdsWithChildren) = await GetSkillsLookupAsync(context.CancellationToken);

        var response = new GetExamAnswerKeyResponse
        {
            ExamId = examId.ToString()
        };

        foreach (var eq in examQuestions)
        {
            var q = eq.Question;
            var domainId = q.Skill?.DomainId ?? Guid.Empty;
            var domainName = q.Skill?.Domain?.Name ?? string.Empty;
            var domainCode = ResolveDomainCode(domainId, domainName);

            string chapterName = string.Empty;
            string lessonName = string.Empty;
            if (q.Skill != null)
            {
                (_, chapterName, lessonName) = ResolveSkillLineage(q.Skill, skillMap, parentIdsWithChildren);
            }

            var subjectName = InferSubjectName(chapterName, q.Skill?.Name, domainCode);

            var key = new QuestionAnswerKey
            {
                QuestionId = eq.QuestionId.ToString(),
                QuestionOrder = eq.QuestionOrder,
                CorrectOption = q.CorrectOption.ToString(),
                SkillId = q.SkillId.ToString(),
                DifficultyLevel = q.DifficultyLevel,
                SkillName = q.Skill?.Name ?? string.Empty,
                DomainId = domainId != Guid.Empty ? domainId.ToString() : string.Empty,
                DomainName = domainName,
                Content = q.ContentLatex ?? string.Empty,
                Explanation = q.Explanation ?? string.Empty,
                PassageContent = q.Passage?.Content ?? string.Empty,
                PassageTitle = q.Passage?.Title ?? string.Empty,
                DomainCode = domainCode,
                SubjectName = subjectName,
                ChapterName = chapterName,
                LessonName = lessonName
            };

            key.Options.Add(q.OptionA ?? string.Empty);
            key.Options.Add(q.OptionB ?? string.Empty);
            key.Options.Add(q.OptionC ?? string.Empty);
            key.Options.Add(q.OptionD ?? string.Empty);

            response.AnswerKeys.Add(key);
        }

        _cache.Set(cacheKey, response, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        });

        _logger.LogInformation("Delivered & cached answer keys for exam {ExamId} ({Count} questions)", examId, examQuestions.Count);
        return response;
    }

    public override async Task<GetQuestionDetailResponse> GetQuestionDetail(
        GetQuestionDetailRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.QuestionId, out var questionId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Question ID không hợp lệ."));
        }

        var question = await _context.Questions
            .AsNoTracking()
            .Include(q => q.Skill)
                .ThenInclude(s => s.Domain)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId, context.CancellationToken);

        if (question == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy câu hỏi {questionId}."));
        }

        var domainId = question.Skill?.DomainId ?? Guid.Empty;
        var domainName = question.Skill?.Domain?.Name ?? string.Empty;
        var domainCode = ResolveDomainCode(domainId, domainName);

        string chapterName = string.Empty;
        if (question.Skill?.ParentId.HasValue == true)
        {
            var parent = await _context.Skills.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SkillId == question.Skill.ParentId.Value, context.CancellationToken);
            if (parent?.ParentId.HasValue == true)
            {
                var grandParent = await _context.Skills.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SkillId == parent.ParentId.Value, context.CancellationToken);
                chapterName = grandParent?.Name ?? parent.Name;
            }
            else if (parent != null)
            {
                chapterName = parent.Name;
            }
        }

        var subjectName = InferSubjectName(chapterName, question.Skill?.Name, domainCode);

        var response = new GetQuestionDetailResponse
        {
            QuestionId = question.QuestionId.ToString(),
            SkillId = question.SkillId.ToString(),
            Difficulty = question.DifficultyLevel.ToString(),
            Title = question.Skill?.Name ?? "Câu hỏi khảo thí",
            Content = question.ContentLatex,
            Explanation = question.Explanation ?? string.Empty,
            DomainId = domainId != Guid.Empty ? domainId.ToString() : string.Empty,
            DomainName = domainName,
            DomainCode = domainCode,
            SubjectName = subjectName
        };

        response.Options.Add(new AnswerOption { OptionId = "A", Content = question.OptionA ?? string.Empty, IsCorrect = question.CorrectOption == 'A' });
        response.Options.Add(new AnswerOption { OptionId = "B", Content = question.OptionB ?? string.Empty, IsCorrect = question.CorrectOption == 'B' });
        response.Options.Add(new AnswerOption { OptionId = "C", Content = question.OptionC ?? string.Empty, IsCorrect = question.CorrectOption == 'C' });
        response.Options.Add(new AnswerOption { OptionId = "D", Content = question.OptionD ?? string.Empty, IsCorrect = question.CorrectOption == 'D' });

        return response;
    }

    public override async Task<GetMilestoneQuizResponse> GetMilestoneQuiz(
        GetMilestoneQuizRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Nhận yêu cầu GetMilestoneQuiz: SkillId={SkillId}, ExamId={ExamId}, Count={Count}, ReadOnly={ReadOnly}",
            request.SkillId, request.ExamId, request.QuestionCount, request.ReadOnly);

        int count = request.QuestionCount > 0 ? request.QuestionCount : 5;
        Guid? parsedExamId = Guid.TryParse(request.ExamId, out var eid) ? eid : null;
        Guid? parsedSkillId = Guid.TryParse(request.SkillId, out var sid) ? sid : null;

        var questionsList = new List<Question>();
        var seenIds = new HashSet<Guid>();
        var seenContents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string title = "Bài Quiz củng cố chặng học";
        Guid finalExamId = parsedExamId ?? Guid.NewGuid();

        static bool IsBrokenOptionText(string? opt)
        {
            var trimmed = (opt ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed)) return true;
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[\.\u2026\-_\s\?]+$")) return true;
            var lower = trimmed.ToLowerInvariant();
            return lower == "n/a" || lower == "tbd" || lower == "null" || lower == "undefined" || lower == "chưa có";
        }

        static bool HasBrokenOrMissingOptions(Question q)
        {
            var opts = new[] { q.OptionA, q.OptionB, q.OptionC, q.OptionD };
            if (opts.Any(IsBrokenOptionText)) return true;

            var distinctValid = opts
                .Select(o => System.Text.RegularExpressions.Regex.Replace((o ?? string.Empty).Trim(), @"\s+", " "))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (distinctValid < 2) return true;

            var corr = char.ToUpperInvariant(q.CorrectOption);
            if (corr != 'A' && corr != 'B' && corr != 'C' && corr != 'D') return true;

            return false;
        }

        static bool IsBrokenOrphanPassageQuestion(Question q)
        {
            if (q.Passage != null && !string.IsNullOrWhiteSpace(q.Passage.Content)) return false;
            var text = (q.ContentLatex ?? string.Empty).ToLowerInvariant();
            return text.Contains("of the passage") || text.Contains("the passage") || text.Contains("in paragraph")
                || text.Contains("đoạn văn trên") || text.Contains("văn bản trên") || text.Contains("bài đọc trên")
                || text.Contains("ngữ liệu trên") || text.Contains("thông tin trên") || text.Contains("đoạn trích trên");
        }

        void AppendUnique(IEnumerable<Question> items, string? requiredSubject = null)
        {
            foreach (var q in items.OrderBy(_ => Random.Shared.Next()))
            {
                if (questionsList.Count >= count) break;
                if (q.IsFlagged) continue;
                if (HasBrokenOrMissingOptions(q)) continue;
                if (IsBrokenOrphanPassageQuestion(q)) continue;
                if (!string.IsNullOrEmpty(requiredSubject)
                    && SubjectTaxonomyHelper.IsContentLanguageMismatch(q.ContentLatex, requiredSubject))
                {
                    continue;
                }
                string normalizedContent = System.Text.RegularExpressions.Regex.Replace(
                    (q.ContentLatex ?? string.Empty).Trim(),
                    @"\s+",
                    " ");
                if (string.IsNullOrEmpty(normalizedContent)) continue;
                if (seenIds.Add(q.QuestionId) && seenContents.Add(normalizedContent))
                {
                    questionsList.Add(q);
                }
            }
        }

        // 1. Nếu có SkillId: Ưu tiên tối thượng là KHOANH VÙNG CHUẨN XÁC MÔN HỌC (Subject Isolation - Tuyệt đối không lẫn môn khác!)
        if (parsedSkillId.HasValue)
        {
            var allSkills = await _context.Skills.AsNoTracking().ToListAsync(context.CancellationToken);
            var skillMap = allSkills.ToDictionary(s => s.SkillId);

            if (skillMap.TryGetValue(parsedSkillId.Value, out var targetSkill))
            {
                title = $"Bài Quiz củng cố: {targetSkill.Name}";

                var parentIdsWithChildren = allSkills.Where(s => s.ParentId.HasValue).Select(s => s.ParentId!.Value).ToHashSet();
                var targetDomainCode = ResolveDomainCode(targetSkill.DomainId, null);
                var (_, targetCh, _) = ResolveSkillLineage(targetSkill, skillMap, parentIdsWithChildren);
                var targetSubject = InferSubjectName(targetCh, targetSkill.Name, targetDomainCode);

                // Xác định TẤT CẢ các Skill thuộc CÙNG MÔN HỌC này (Toán học, Ngữ văn, Tiếng Anh, v.v.)
                var sameSubjectSkillIds = allSkills
                    .Where(s =>
                    {
                        var sDomainCode = ResolveDomainCode(s.DomainId, null);
                        var (_, ch, _) = ResolveSkillLineage(s, skillMap, parentIdsWithChildren);
                        return InferSubjectName(ch, s.Name, sDomainCode) == targetSubject;
                    })
                    .Select(s => s.SkillId)
                    .ToHashSet();

                // 1a. Nếu có ExamId được truyền vào: CHỈ LẤY những câu hỏi trong đề thi đó mà THUỘC CÙNG MÔN HỌC!
                if (parsedExamId.HasValue)
                {
                    var examQuestions = await _context.ExamQuestions
                        .AsNoTracking()
                        .Include(eq => eq.Question)
                            .ThenInclude(q => q.Passage)
                        .Include(eq => eq.Question)
                            .ThenInclude(q => q.Skill)
                        .Where(eq => eq.ExamId == parsedExamId.Value)
                        .OrderBy(eq => eq.QuestionOrder)
                        .ToListAsync(context.CancellationToken);

                    if (examQuestions.Count > 0)
                    {
                        var sameSubjectExamQuestions = examQuestions
                            .Where(eq => sameSubjectSkillIds.Contains(eq.Question.SkillId))
                            .Select(eq => eq.Question);
                        AppendUnique(sameSubjectExamQuestions, targetSubject);

                        var exam = await _context.MockExams
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.ExamId == parsedExamId.Value, context.CancellationToken);
                        if (exam != null) title = exam.Title;
                    }
                }

                // 1b. Ưu tiên cao nhất: Câu hỏi đúng dạng bài (targetSkill) và các skill con/cháu
                if (questionsList.Count < count)
                {
                    var directAndChildIds = new HashSet<Guid> { targetSkill.SkillId };
                    foreach (var child in allSkills.Where(s => s.ParentId == targetSkill.SkillId))
                    {
                        directAndChildIds.Add(child.SkillId);
                        foreach (var grandChild in allSkills.Where(s => s.ParentId == child.SkillId))
                        {
                            directAndChildIds.Add(grandChild.SkillId);
                        }
                    }

                    var directQuestions = await _context.Questions
                        .AsNoTracking()
                        .Include(q => q.Passage)
                        .Include(q => q.Skill)
                        .Where(q => directAndChildIds.Contains(q.SkillId) && !seenIds.Contains(q.QuestionId))
                        .OrderBy(q => q.DifficultyLevel)
                        .ThenBy(q => q.CreatedAt)
                        .Take((count - questionsList.Count) * 2)
                        .ToListAsync(context.CancellationToken);
                    AppendUnique(directQuestions, targetSubject);
                }

                // 1c. Ưu tiên 2: Cùng Chương (sibling patterns/lessons trong cùng chapter của môn học)
                if (questionsList.Count < count && targetSkill.ParentId.HasValue)
                {
                    var chapterId = targetSkill.ParentId.Value;
                    if (skillMap.TryGetValue(chapterId, out var p) && p.ParentId.HasValue)
                    {
                        chapterId = p.ParentId.Value;
                    }

                    var chapterSkillIds = allSkills
                        .Where(s => s.SkillId == chapterId || s.ParentId == chapterId
                            || (s.ParentId.HasValue && skillMap.TryGetValue(s.ParentId.Value, out var sp) && sp.ParentId == chapterId))
                        .Where(s => sameSubjectSkillIds.Contains(s.SkillId))
                        .Select(s => s.SkillId)
                        .ToHashSet();

                    if (chapterSkillIds.Count > 0)
                    {
                        var chapterQuestions = await _context.Questions
                            .AsNoTracking()
                            .Include(q => q.Passage)
                            .Include(q => q.Skill)
                            .Where(q => chapterSkillIds.Contains(q.SkillId) && !seenIds.Contains(q.QuestionId))
                            .OrderBy(q => q.DifficultyLevel)
                            .Take((count - questionsList.Count) * 2)
                            .ToListAsync(context.CancellationToken);
                        AppendUnique(chapterQuestions, targetSubject);
                    }
                }

                // 1d. Ưu tiên 3 (Chỉ chạy khi cả Dạng bài lẫn Chương hiện tại đều có 0 câu hỏi):
                // Nếu đã có ít nhất 1 câu đúng dạng bài/cùng chương -> KHÔNG kéo câu hỏi từ chương khác (tránh Tổ hợp xác suất bị dính Độ dài đường thẳng!)
                if (questionsList.Count == 0)
                {
                    var sameSubjectQuestions = await _context.Questions
                        .AsNoTracking()
                        .Include(q => q.Passage)
                        .Include(q => q.Skill)
                        .Where(q => sameSubjectSkillIds.Contains(q.SkillId) && !seenIds.Contains(q.QuestionId))
                        .OrderBy(q => q.DifficultyLevel)
                        .Take(Math.Min(count, 5) * 3)
                        .ToListAsync(context.CancellationToken);
                    AppendUnique(sameSubjectQuestions, targetSubject);
                }

                // TUYỆT ĐỐI KHÔNG CÓ BƯỚC 2d (same domain) HAY BƯỚC 3 (fallback toàn bộ bảng questions)!
                // Khi học sinh học chặng Toán học, chỉ trả về câu hỏi Toán học. Nếu ngân hàng có bao nhiêu câu thì trả về bấy nhiêu câu.
            }
        }
        else
        {
            // Trường hợp KHÔNG có SkillId cụ thể (ví dụ yêu cầu lấy đề thi thuần túy theo ExamId):
            if (parsedExamId.HasValue)
            {
                var examQuestions = await _context.ExamQuestions
                    .AsNoTracking()
                    .Include(eq => eq.Question)
                        .ThenInclude(q => q.Passage)
                    .Include(eq => eq.Question)
                        .ThenInclude(q => q.Skill)
                    .Where(eq => eq.ExamId == parsedExamId.Value)
                    .OrderBy(eq => eq.QuestionOrder)
                    .ToListAsync(context.CancellationToken);

                if (examQuestions.Count > 0)
                {
                    AppendUnique(examQuestions.Select(eq => eq.Question));
                    var exam = await _context.MockExams
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.ExamId == parsedExamId.Value, context.CancellationToken);
                    if (exam != null) title = exam.Title;
                }
            }

            if (questionsList.Count < count)
            {
                var fallbackQuestions = await _context.Questions
                    .AsNoTracking()
                    .Include(q => q.Passage)
                    .Include(q => q.Skill)
                    .Where(q => !seenIds.Contains(q.QuestionId))
                    .OrderBy(q => q.DifficultyLevel)
                    .Take((count - questionsList.Count) * 2)
                    .ToListAsync(context.CancellationToken);
                AppendUnique(fallbackQuestions);
            }
        }

        // Chỉ lưu MockExam mới khi Caller truyền ExamId cụ thể VÀ không đặt ReadOnly=true
        // Tránh spam tạo hàng chục đề TOPICAL_PRACTICE rác vào bảng MockExams mỗi lần gọi PLAR!
        if (!request.ReadOnly && parsedExamId.HasValue && questionsList.Count > 0)
        {
            var existingExam = await _context.MockExams
                .FirstOrDefaultAsync(e => e.ExamId == finalExamId, context.CancellationToken);

            if (existingExam == null)
            {
                var newExam = new MockExam
                {
                    ExamId = finalExamId,
                    Title = title,
                    DurationMinutes = count * 2,
                    TotalQuestions = questionsList.Count,
                    IsPublished = true,
                    ExamCategory = "TOPICAL_PRACTICE",
                    CreatedAt = DateTime.UtcNow
                };

                _context.MockExams.Add(newExam);

                int order = 1;
                foreach (var q in questionsList)
                {
                    _context.ExamQuestions.Add(new ExamQuestion
                    {
                        ExamId = finalExamId,
                        QuestionId = q.QuestionId,
                        QuestionOrder = order++
                    });
                }

                await _context.SaveChangesAsync(context.CancellationToken);
            }
        }

        var response = new GetMilestoneQuizResponse
        {
            ExamId = finalExamId.ToString(),
            Title = title,
            DurationMinutes = count * 2,
            TotalQuestions = questionsList.Count
        };

        int qOrder = 1;
        foreach (var q in questionsList)
        {
            var item = new QuizQuestionItem
            {
                QuestionId = q.QuestionId.ToString(),
                QuestionOrder = qOrder++,
                Content = q.ContentLatex,
                DifficultyLevel = q.DifficultyLevel,
                SkillId = q.SkillId.ToString(),
                SkillName = q.Skill?.Name ?? "Kỹ năng chuyên đề",
                PassageTitle = q.Passage?.Title ?? string.Empty,
                PassageContent = q.Passage?.Content ?? string.Empty,
                PassageImageUrl = q.Passage?.ImageUrl ?? string.Empty
            };

            // Tuyệt đối bảo mật: KHÔNG trả về IsCorrect hay CorrectOption cho Client học sinh!
            item.Options.Add(new QuizOption { OptionId = "A", Content = q.OptionA ?? string.Empty });
            item.Options.Add(new QuizOption { OptionId = "B", Content = q.OptionB ?? string.Empty });
            item.Options.Add(new QuizOption { OptionId = "C", Content = q.OptionC ?? string.Empty });
            item.Options.Add(new QuizOption { OptionId = "D", Content = q.OptionD ?? string.Empty });

            response.Questions.Add(item);
        }

        return response;
    }
}
