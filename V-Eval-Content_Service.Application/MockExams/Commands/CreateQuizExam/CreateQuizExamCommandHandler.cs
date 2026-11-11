using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.MockExams.DTOs;
using V_Eval_Content_Service.Application.Questions.DTOs;
using V_Eval_Content_Service.Domain.Constants;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.MockExams.Commands.CreateQuizExam;

public class CreateQuizExamCommandHandler : IRequestHandler<CreateQuizExamCommand, Result<QuizExamResponseDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<CreateQuizExamCommandHandler> _logger;

    public CreateQuizExamCommandHandler(
        IContentDbContext context,
        ILogger<CreateQuizExamCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<QuizExamResponseDto>> Handle(CreateQuizExamCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Packaging quiz exam for {SkillCount} skills, Target count: {Count}", 
            request.SkillIds.Count, request.QuestionCount);

        // 1. Kiểm tra danh sách Kỹ năng mục tiêu
        var validSkills = await _context.Skills
            .Include(s => s.Domain)
            .Where(s => request.SkillIds.Contains(s.SkillId))
            .ToListAsync(cancellationToken);

        if (validSkills.Count == 0)
        {
            return Result<QuizExamResponseDto>.Failure(
                Error.NotFound("SkillsNotFound", "Không tìm thấy kỹ năng hợp lệ nào trong danh mục yêu cầu."));
        }

        var skillIds = validSkills.Select(s => s.SkillId).ToList();
        var skillNames = string.Join(", ", validSkills.Select(s => s.Name).Take(2));

        // 2. Truy vấn ngân hàng câu hỏi theo Bloom bracket
        var candidateQuestions = await _context.Questions
            .Include(q => q.Skill)
                .ThenInclude(s => s.Domain)
            .Include(q => q.Passage)
            .Where(q => skillIds.Contains(q.SkillId))
            .ToListAsync(cancellationToken);

        if (candidateQuestions.Count == 0)
        {
            return Result<QuizExamResponseDto>.Failure(
                Error.NotFound("QuestionsNotFound", $"Chưa có câu hỏi nào trong ngân hàng đề thuộc các kỹ năng [{skillNames}]."));
        }

        // Ưu tiên câu hỏi trong dải Bloom yêu cầu (ví dụ Bloom 1-3 cho củng cố)
        var filtered = candidateQuestions
            .Where(q => q.DifficultyLevel >= request.MinBloomLevel && q.DifficultyLevel <= request.MaxBloomLevel)
            .ToList();

        // Fallback: Nếu không đủ thì dùng toàn bộ câu hỏi của kỹ năng đó
        var pool = filtered.Count >= request.QuestionCount ? filtered : candidateQuestions;

        // Trộn ngẫu nhiên (Shuffle) và bốc đủ số lượng QuestionCount
        var rng = new Random();
        var selectedQuestions = pool
            .OrderBy(_ => rng.Next())
            .Take(request.QuestionCount)
            .ToList();

        // 3. Khởi tạo thực thể MockExam dạng QUIZ / WORKSHEET
        var examTitle = !string.IsNullOrWhiteSpace(request.Title) 
            ? request.Title.Trim() 
            : $"Phiếu Luyện Tập Vi Mô - Củng Cố [{skillNames}]";

        var quizExam = new MockExam
        {
            ExamId = Guid.NewGuid(),
            Title = examTitle,
            DurationMinutes = Math.Max(10, selectedQuestions.Count * 2), // 2 phút/câu
            TotalQuestions = selectedQuestions.Count,
            IsPublished = true,
            ExamCategory = "QUIZ",
            DifficultyLevel = request.MaxBloomLevel,
            CreatedAt = DateTime.UtcNow
        };

        await _context.MockExams.AddAsync(quizExam, cancellationToken);

        // 4. Liên kết câu hỏi vào đề qua ExamQuestions
        for (int i = 0; i < selectedQuestions.Count; i++)
        {
            var examQuestion = new ExamQuestion
            {
                ExamId = quizExam.ExamId,
                QuestionId = selectedQuestions[i].QuestionId,
                QuestionOrder = i + 1
            };
            await _context.ExamQuestions.AddAsync(examQuestion, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quiz exam {ExamId} packaged successfully with {QuestionCount} questions.", 
            quizExam.ExamId, selectedQuestions.Count);

        var questionDtos = selectedQuestions.Select(q => new QuestionDetailDto
        {
            QuestionId = q.QuestionId,
            SkillId = q.SkillId,
            SkillName = q.Skill.Name,
            DomainName = q.Skill.Domain?.Name ?? "Miền Năng Lực Chưa Phân Loại",
            DomainCode = q.Skill.Domain?.DomainId.ToString() ?? "",
            PassageId = q.PassageId,
            PassageTitle = q.Passage?.Title,
            DifficultyLevel = q.DifficultyLevel,
            DifficultyName = BloomTaxonomy.GetName(q.DifficultyLevel),
            ContentLatex = q.ContentLatex,
            Options = new List<QuestionOptionItemDto>
            {
                new() { Key = 'A', Content = q.OptionA, IsCorrect = q.CorrectOption == 'A' },
                new() { Key = 'B', Content = q.OptionB, IsCorrect = q.CorrectOption == 'B' },
                new() { Key = 'C', Content = q.OptionC, IsCorrect = q.CorrectOption == 'C' },
                new() { Key = 'D', Content = q.OptionD, IsCorrect = q.CorrectOption == 'D' }
            },
            CorrectOption = q.CorrectOption,
            Explanation = q.Explanation,
            CreatedAt = q.CreatedAt
        }).ToList();

        var response = new QuizExamResponseDto
        {
            ExamId = quizExam.ExamId,
            Title = quizExam.Title,
            DurationMinutes = quizExam.DurationMinutes,
            TotalQuestions = quizExam.TotalQuestions,
            ExamCategory = quizExam.ExamCategory,
            TargetSkillIds = skillIds,
            Questions = questionDtos,
            CreatedAt = quizExam.CreatedAt,
            Message = "Đóng gói bộ đề Quiz củng cố thích ứng thành công."
        };

        return Result<QuizExamResponseDto>.Success(response);
    }
}
