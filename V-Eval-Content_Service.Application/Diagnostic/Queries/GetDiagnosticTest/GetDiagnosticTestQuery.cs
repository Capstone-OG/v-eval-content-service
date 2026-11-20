using MediatR;
using Microsoft.EntityFrameworkCore;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Diagnostic.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Diagnostic.Queries.GetDiagnosticTest;

public record GetDiagnosticTestQuery(Guid? ExcludeExamId = null, List<Guid>? ExcludeExamIds = null)
    : IRequest<Result<DiagnosticExamDto>>;

public class GetDiagnosticTestQueryHandler : IRequestHandler<GetDiagnosticTestQuery, Result<DiagnosticExamDto>>
{
    private readonly IContentDbContext _context;

    public GetDiagnosticTestQueryHandler(IContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DiagnosticExamDto>> Handle(
        GetDiagnosticTestQuery request,
        CancellationToken cancellationToken)
    {
        var excludedIds = new HashSet<Guid>();
        if (request.ExcludeExamId.HasValue) excludedIds.Add(request.ExcludeExamId.Value);
        if (request.ExcludeExamIds != null)
        {
            foreach (var id in request.ExcludeExamIds) excludedIds.Add(id);
        }

        // 1. Tìm đề thi chẩn đoán (ExamCategory = "DIAGNOSTIC"), loại trừ toàn bộ đề đã làm
        var baseQuery = _context.MockExams
            .AsNoTracking()
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q.Passage)
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q.Skill)
                        .ThenInclude(s => s.Domain)
            .Where(e => e.ExamCategory == "DIAGNOSTIC");

        var filteredQuery = excludedIds.Count > 0
            ? baseQuery.Where(e => !excludedIds.Contains(e.ExamId))
            : baseQuery;

        var exam = await filteredQuery
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Nếu có loại trừ nhưng không còn đề mới, fallback trả về đề chẩn đoán mới nhất
        if (exam == null && excludedIds.Count > 0)
        {
            exam = await baseQuery
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // 3. Nếu chưa có đề gắn cờ DIAGNOSTIC, lấy đề mới nhất làm fallback
        if (exam == null)
        {
            exam = await _context.MockExams
                .AsNoTracking()
                .Include(e => e.ExamQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q.Passage)
                .Include(e => e.ExamQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q.Skill)
                            .ThenInclude(s => s.Domain)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (exam == null)
        {
            return Result<DiagnosticExamDto>.Failure(
                Error.NotFound("Diagnostic.NotFound", "Không tìm thấy bộ đề khảo sát chẩn đoán năng lực ban đầu trong hệ thống."));
        }

        var orderedExamQuestions = exam.ExamQuestions
            .OrderBy(eq => eq.QuestionOrder)
            .Take(30)
            .ToList();

        if (orderedExamQuestions.Count == 0)
        {
            return Result<DiagnosticExamDto>.Failure(
                Error.NotFound("Diagnostic.EmptyQuestions", "Đề khảo sát chẩn đoán chưa có câu hỏi nào được gán."));
        }

        // Resolve full lineage (Chapter -> Lesson -> Pattern & Subject) for every question
        var allSkills = await _context.Skills.AsNoTracking().ToListAsync(cancellationToken);
        var skillMap = allSkills.ToDictionary(s => s.SkillId);

        DiagnosticQuestionItemDto MapQuestion(ExamQuestion eq)
        {
            var q = eq.Question;
            var domainId = q.Skill?.DomainId ?? Guid.Empty;
            var domainName = q.Skill?.Domain?.Name ?? "Chưa xác định miền";
            var domainCode = ResolveDomainCode(domainId, domainName);

            string chapterName = string.Empty;
            string lessonName = string.Empty;
            if (q.Skill?.ParentId.HasValue == true && skillMap.TryGetValue(q.Skill.ParentId.Value, out var parent))
            {
                if (parent.ParentId.HasValue && skillMap.TryGetValue(parent.ParentId.Value, out var grandParent))
                {
                    chapterName = grandParent.Name;
                    lessonName = parent.Name;
                }
                else
                {
                    chapterName = parent.Name;
                    lessonName = q.Skill.Name;
                }
            }

            var subjectName = InferSubjectName(chapterName, q.Skill?.Name, domainCode);

            return new DiagnosticQuestionItemDto
            {
                QuestionId = q.QuestionId,
                QuestionNumber = eq.QuestionOrder,
                Content = q.ContentLatex,
                Options = new Dictionary<string, string>
                {
                    { "A", q.OptionA ?? string.Empty },
                    { "B", q.OptionB ?? string.Empty },
                    { "C", q.OptionC ?? string.Empty },
                    { "D", q.OptionD ?? string.Empty }
                },
                SuggestedSkillName = q.Skill?.Name ?? "Năng lực tổng hợp",
                DifficultyLevel = q.DifficultyLevel,
                SkillId = q.SkillId,
                DomainId = domainId,
                DomainName = domainName,
                DomainCode = domainCode,
                SubjectName = subjectName,
                ChapterName = chapterName,
                LessonName = lessonName
            };
        }

        var passagesList = new List<DiagnosticPassageDto>();
        var passageGroups = orderedExamQuestions
            .Where(eq => eq.Question.PassageId.HasValue)
            .GroupBy(eq => eq.Question.PassageId!.Value)
            .ToList();

        foreach (var group in passageGroups)
        {
            var firstEq = group.First();
            var passage = firstEq.Question.Passage;

            passagesList.Add(new DiagnosticPassageDto
            {
                PassageId = group.Key,
                Content = passage?.Content ?? string.Empty,
                ImageUrl = passage?.ImageUrl,
                StartQuestion = group.Min(g => g.QuestionOrder),
                EndQuestion = group.Max(g => g.QuestionOrder),
                Questions = group.Select(MapQuestion).ToList()
            });
        }

        passagesList = passagesList.OrderBy(p => p.StartQuestion).ToList();

        var singleQuestionsList = orderedExamQuestions
            .Where(eq => !eq.Question.PassageId.HasValue)
            .Select(MapQuestion)
            .ToList();

        var resultDto = new DiagnosticExamDto
        {
            ExamId = exam.ExamId,
            Title = exam.ExamCategory == "DIAGNOSTIC" ? exam.Title : "Bài Khảo Sát Đánh Giá Năng Lực Đầu Vào (30 Câu Chẩn Đoán)",
            DurationMinutes = exam.ExamCategory == "DIAGNOSTIC" ? exam.DurationMinutes : 45,
            TotalQuestions = orderedExamQuestions.Count,
            TotalPassages = passagesList.Count,
            TotalSingleQuestions = singleQuestionsList.Count,
            Passages = passagesList,
            SingleQuestions = singleQuestionsList
        };

        return Result<DiagnosticExamDto>.Success(resultDto);
    }

    private static string ResolveDomainCode(Guid domainId, string? domainName)
    {
        var id = domainId.ToString().ToLowerInvariant();
        if (id == "b581ee4c-7277-4be8-a156-c5b20a0a59f6") return "DOM_LANG";
        if (id == "6f3765db-943e-4810-bf74-d6a8bdc215da") return "DOM_MATH";
        if (id == "77777777-7777-7777-7777-000000000001") return "DOM_NAT_SCI";
        if (id == "77777777-7777-7777-7777-000000000002") return "DOM_SOC_SCI";

        var name = (domainName ?? string.Empty).ToLowerInvariant();
        if (name.Contains("ngôn ngữ")) return "DOM_LANG";
        if (name.Contains("toán")) return "DOM_MATH";
        if (name.Contains("tự nhiên")) return "DOM_NAT_SCI";
        if (name.Contains("xã hội")) return "DOM_SOC_SCI";
        return "DOM_MATH";
    }

    private static string InferSubjectName(string? chapterName, string? skillName, string domainCode)
    {
        var combined = $"{chapterName} {skillName}".ToLowerInvariant();
        if (combined.Contains("(toán") || combined.Contains("toán học")) return "Toán học";
        if (combined.Contains("tư duy logic") || combined.Contains("phân tích số liệu")) return "Tư duy Logic & Số liệu";
        if (combined.Contains("(tiếng anh") || combined.Contains("tiếng anh") || combined.Contains("english")) return "Tiếng Anh";
        if (combined.Contains("(ngữ văn") || combined.Contains("ngữ văn") || combined.Contains("tiếng việt")) return "Ngữ văn";
        if (combined.Contains("(vật lí") || combined.Contains("(vật lý") || combined.Contains("vật lí") || combined.Contains("vật lý")) return "Vật lí";
        if (combined.Contains("(hóa học") || combined.Contains("(hoá học") || combined.Contains("hóa học") || combined.Contains("hoá học")) return "Hóa học";
        if (combined.Contains("(sinh học") || combined.Contains("sinh học")) return "Sinh học";
        if (combined.Contains("(lịch sử") || combined.Contains("lịch sử")) return "Lịch sử";
        if (combined.Contains("(địa lí") || combined.Contains("(địa lý") || combined.Contains("địa lí") || combined.Contains("địa lý")) return "Địa lí";
        if (combined.Contains("gdkt") || combined.Contains("kinh tế và pháp luật") || combined.Contains("pháp luật")) return "GDKT & Pháp luật";

        return domainCode switch
        {
            "DOM_LANG" => "Ngữ văn",
            "DOM_MATH" => "Toán học",
            "DOM_NAT_SCI" => "Vật lí",
            "DOM_SOC_SCI" => "Lịch sử",
            _ => "Toán học"
        };
    }
}
