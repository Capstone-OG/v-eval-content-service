using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using VEval.Shared.Grpc.Content;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.API.Services;

public class ContentGrpcService : ContentService.ContentServiceBase
{
    private readonly IContentDbContext _context;
    private readonly ILogger<ContentGrpcService> _logger;

    public ContentGrpcService(IContentDbContext context, ILogger<ContentGrpcService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task<GetSkillsTreeResponse> GetSkillsTree(
        GetSkillsTreeRequest request,
        ServerCallContext context)
    {
        var skills = await _context.Skills
            .AsNoTracking()
            .Include(s => s.Prerequisites)
            .Include(s => s.Domain)
            .OrderBy(s => s.SkillId)
            .ToListAsync(context.CancellationToken);

        var response = new GetSkillsTreeResponse();

        foreach (var s in skills)
        {
            var node = new SkillNode
            {
                SkillId = s.SkillId.ToString(),
                Name = s.Name,
                Description = s.Domain?.Name ?? string.Empty,
                Weight = s.Weight ?? 0.05,
                DomainId = s.DomainId.ToString(),
                DomainName = s.Domain?.Name ?? string.Empty
            };

            foreach (var prereq in s.Prerequisites)
            {
                node.PrerequisiteIds.Add(prereq.PrerequisiteId.ToString());
            }

            response.Skills.Add(node);
        }

        _logger.LogInformation("Delivered skills tree with {Count} skills for path planning.", response.Skills.Count);
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

        var examQuestions = await _context.ExamQuestions
            .AsNoTracking()
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

        var response = new GetExamAnswerKeyResponse
        {
            ExamId = examId.ToString()
        };

        foreach (var eq in examQuestions)
        {
            response.AnswerKeys.Add(new QuestionAnswerKey
            {
                QuestionId = eq.QuestionId.ToString(),
                QuestionOrder = eq.QuestionOrder,
                CorrectOption = eq.Question.CorrectOption.ToString(),
                SkillId = eq.Question.SkillId.ToString(),
                DifficultyLevel = eq.Question.DifficultyLevel,
                SkillName = eq.Question.Skill?.Name ?? string.Empty,
                DomainId = eq.Question.Skill?.DomainId.ToString() ?? string.Empty,
                DomainName = eq.Question.Skill?.Domain?.Name ?? string.Empty
            });
        }

        _logger.LogInformation("Delivered answer keys for exam {ExamId} ({Count} questions)", examId, examQuestions.Count);
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
            .FirstOrDefaultAsync(q => q.QuestionId == questionId, context.CancellationToken);

        if (question == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy câu hỏi {questionId}."));
        }

        var response = new GetQuestionDetailResponse
        {
            QuestionId = question.QuestionId.ToString(),
            SkillId = question.SkillId.ToString(),
            Difficulty = question.DifficultyLevel.ToString(),
            Title = question.Skill?.Name ?? "Câu hỏi khảo thí",
            Content = question.ContentLatex,
            Explanation = question.Explanation ?? string.Empty
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
        _logger.LogInformation("Nhận yêu cầu GetMilestoneQuiz: SkillId={SkillId}, ExamId={ExamId}, Count={Count}",
            request.SkillId, request.ExamId, request.QuestionCount);

        int count = request.QuestionCount > 0 ? request.QuestionCount : 5;
        Guid? parsedExamId = Guid.TryParse(request.ExamId, out var eid) ? eid : null;
        Guid? parsedSkillId = Guid.TryParse(request.SkillId, out var sid) ? sid : null;

        var questionsList = new List<Question>();
        string title = "Bài Quiz củng cố chặng học";
        Guid finalExamId = parsedExamId ?? Guid.NewGuid();

        // 1. Nếu có ExamId, tìm câu hỏi gắn với đề thi đó
        if (parsedExamId.HasValue)
        {
            var examQuestions = await _context.ExamQuestions
                .AsNoTracking()
                .Include(eq => eq.Question)
                    .ThenInclude(q => q.Skill)
                .Where(eq => eq.ExamId == parsedExamId.Value)
                .OrderBy(eq => eq.QuestionOrder)
                .ToListAsync(context.CancellationToken);

            if (examQuestions.Count > 0)
            {
                questionsList = examQuestions.Select(eq => eq.Question).ToList();
                var exam = await _context.MockExams
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ExamId == parsedExamId.Value, context.CancellationToken);
                if (exam != null)
                {
                    title = exam.Title;
                }
            }
        }

        // 2. Nếu chưa có câu hỏi từ ExamId, tìm theo SkillId
        if (questionsList.Count == 0 && parsedSkillId.HasValue)
        {
            var skillQuestions = await _context.Questions
                .AsNoTracking()
                .Include(q => q.Skill)
                .Where(q => q.SkillId == parsedSkillId.Value)
                .Take(count)
                .ToListAsync(context.CancellationToken);

            questionsList.AddRange(skillQuestions);

            var skill = await _context.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SkillId == parsedSkillId.Value, context.CancellationToken);
            if (skill != null)
            {
                title = $"Bài Quiz củng cố: {skill.Name}";
            }
        }

        // 3. Fallback: Nếu ngân hàng chưa đủ câu hỏi cho skill cụ thể, lấy các câu hỏi mẫu để sinh Quiz
        if (questionsList.Count < count)
        {
            int remaining = count - questionsList.Count;
            var existingIds = questionsList.Select(q => q.QuestionId).ToHashSet();
            var fallbackQuestions = await _context.Questions
                .AsNoTracking()
                .Include(q => q.Skill)
                .Where(q => !existingIds.Contains(q.QuestionId))
                .Take(remaining)
                .ToListAsync(context.CancellationToken);

            questionsList.AddRange(fallbackQuestions);
        }

        // 4. Nếu database chưa có câu hỏi nào (edge case cực hiếm), tạo câu hỏi mẫu chuẩn
        if (questionsList.Count == 0)
        {
            var dummySkillId = parsedSkillId ?? Guid.NewGuid();
            for (int i = 1; i <= count; i++)
            {
                questionsList.Add(new Question
                {
                    QuestionId = Guid.NewGuid(),
                    SkillId = dummySkillId,
                    DifficultyLevel = 2,
                    ContentLatex = $"Câu hỏi củng cố số {i}: Chọn phương án đúng nhất.",
                    OptionA = "Phương án A",
                    OptionB = "Phương án B",
                    OptionC = "Phương án C",
                    OptionD = "Phương án D",
                    CorrectOption = 'A'
                });
            }
        }

        // Tạo hoặc liên kết MockExam và ExamQuestions để lưu bảng đáp án phục vụ API 6 chấm điểm bảo mật
        var existingExam = await _context.MockExams
            .FirstOrDefaultAsync(e => e.ExamId == finalExamId, context.CancellationToken);

        if (existingExam == null)
        {
            var newExam = new MockExam
            {
                ExamId = finalExamId,
                Title = title,
                DurationMinutes = count * 2, // 2 phút/câu
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
            _logger.LogInformation("Đã khởi tạo đề Quiz củng cố {ExamId} ({Count} câu) cho chặng học.", finalExamId, questionsList.Count);
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
                SkillName = q.Skill?.Name ?? "Kỹ năng chuyên đề"
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
