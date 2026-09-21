using MediatR;
using Microsoft.EntityFrameworkCore;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Diagnostic.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Diagnostic.Queries.GetDiagnosticTest;

public record GetDiagnosticTestQuery : IRequest<Result<DiagnosticExamDto>>;

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
        // 1. Tìm đề thi chẩn đoán (ưu tiên ExamCategory = "DIAGNOSTIC")
        var exam = await _context.MockExams
            .AsNoTracking()
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q.Passage)
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q.Skill)
            .Where(e => e.ExamCategory == "DIAGNOSTIC")
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Nếu chưa có đề gắn cờ DIAGNOSTIC, lấy đề mẫu đầu tiên làm fallback
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
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (exam == null)
        {
            return Result<DiagnosticExamDto>.Failure(
                Error.NotFound("Diagnostic.NotFound", "Không tìm thấy bộ đề khảo sát chẩn đoán năng lực ban đầu trong hệ thống."));
        }

        // Lấy tối đa 30 câu hỏi đầu tiên theo thứ tự QuestionOrder (hoặc toàn bộ nếu đúng 30 câu)
        var orderedExamQuestions = exam.ExamQuestions
            .OrderBy(eq => eq.QuestionOrder)
            .Take(30)
            .ToList();

        if (orderedExamQuestions.Count == 0)
        {
            return Result<DiagnosticExamDto>.Failure(
                Error.NotFound("Diagnostic.EmptyQuestions", "Đề khảo sát chẩn đoán chưa có câu hỏi nào được gán."));
        }

        var passagesList = new List<DiagnosticPassageDto>();
        var singleQuestionsList = new List<DiagnosticQuestionItemDto>();

        // Phân nhóm câu hỏi đọc hiểu theo chùm Passage (nếu có)
        var passageGroups = orderedExamQuestions
            .Where(eq => eq.Question.PassageId.HasValue)
            .GroupBy(eq => eq.Question.PassageId!.Value)
            .ToList();

        foreach (var group in passageGroups)
        {
            var firstEq = group.First();
            var passage = firstEq.Question.Passage;

            var passageDto = new DiagnosticPassageDto
            {
                PassageId = group.Key,
                Content = passage?.Content ?? string.Empty,
                ImageUrl = passage?.ImageUrl,
                StartQuestion = group.Min(g => g.QuestionOrder),
                EndQuestion = group.Max(g => g.QuestionOrder),
                Questions = group.Select(MapDiagnosticQuestionItem).ToList()
            };

            passagesList.Add(passageDto);
        }

        passagesList = passagesList.OrderBy(p => p.StartQuestion).ToList();

        // Các câu hỏi đơn lẻ
        var singles = orderedExamQuestions
            .Where(eq => !eq.Question.PassageId.HasValue)
            .Select(MapDiagnosticQuestionItem)
            .ToList();

        singleQuestionsList.AddRange(singles);

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

    private static DiagnosticQuestionItemDto MapDiagnosticQuestionItem(ExamQuestion eq)
    {
        var q = eq.Question;
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
            DifficultyLevel = q.DifficultyLevel
        };
    }
}
