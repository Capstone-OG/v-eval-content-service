using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using VEval.Shared.Grpc.Content;
using V_Eval_Content_Service.Application.Common.Interfaces;

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
}
