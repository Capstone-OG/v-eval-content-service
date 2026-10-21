using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Questions.DTOs;
using V_Eval_Content_Service.Domain.Constants;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Questions.Commands.CreateQuestion;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Result<QuestionDetailDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<CreateQuestionCommandHandler> _logger;

    public CreateQuestionCommandHandler(
        IContentDbContext context,
        ILogger<CreateQuestionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<QuestionDetailDto>> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating question item for SkillId {SkillId} at Bloom level {DifficultyLevel}", 
            request.SkillId, request.DifficultyLevel);

        // 1. Kiểm tra tồn tại Kỹ năng
        var skill = await _context.Skills
            .Include(s => s.Domain)
            .FirstOrDefaultAsync(s => s.SkillId == request.SkillId, cancellationToken);

        if (skill == null)
        {
            _logger.LogWarning("Skill with ID {SkillId} not found in database.", request.SkillId);
            return Result<QuestionDetailDto>.Failure(
                Error.NotFound("SkillNotFound", $"Không tìm thấy kỹ năng có ID {request.SkillId} trong hệ thống tri thức."));
        }

        // 2. Kiểm tra tồn tại Bài đọc hiểu (nếu có)
        Passage? passage = null;
        if (request.PassageId.HasValue && request.PassageId.Value != Guid.Empty)
        {
            passage = await _context.Passages
                .FirstOrDefaultAsync(p => p.PassageId == request.PassageId.Value, cancellationToken);

            if (passage == null)
            {
                _logger.LogWarning("Passage with ID {PassageId} not found in database.", request.PassageId.Value);
                return Result<QuestionDetailDto>.Failure(
                    Error.NotFound("PassageNotFound", $"Không tìm thấy bài đọc hiểu có ID {request.PassageId.Value}."));
            }
        }

        // 3. Khởi tạo thực thể Question
        var question = new Question
        {
            QuestionId = Guid.NewGuid(),
            SkillId = request.SkillId,
            PassageId = request.PassageId,
            DifficultyLevel = request.DifficultyLevel,
            ContentLatex = request.ContentLatex.Trim(),
            OptionA = request.OptionA.Trim(),
            OptionB = request.OptionB.Trim(),
            OptionC = request.OptionC.Trim(),
            OptionD = request.OptionD.Trim(),
            CorrectOption = char.ToUpperInvariant(request.CorrectOption),
            Explanation = string.IsNullOrWhiteSpace(request.Explanation) ? null : request.Explanation.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Questions.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Question {QuestionId} created successfully for Skill '{SkillName}'", question.QuestionId, skill.Name);

        var response = new QuestionDetailDto
        {
            QuestionId = question.QuestionId,
            SkillId = skill.SkillId,
            SkillName = skill.Name,
            DomainName = skill.Domain?.Name ?? "Miền Năng Lực Chưa Phân Loại",
            DomainCode = skill.Domain?.DomainId.ToString() ?? "",
            PassageId = question.PassageId,
            PassageTitle = passage?.Title,
            DifficultyLevel = question.DifficultyLevel,
            DifficultyName = BloomTaxonomy.GetName(question.DifficultyLevel),
            ContentLatex = question.ContentLatex,
            Options = new List<QuestionOptionItemDto>
            {
                new() { Key = 'A', Content = question.OptionA, IsCorrect = question.CorrectOption == 'A' },
                new() { Key = 'B', Content = question.OptionB, IsCorrect = question.CorrectOption == 'B' },
                new() { Key = 'C', Content = question.OptionC, IsCorrect = question.CorrectOption == 'C' },
                new() { Key = 'D', Content = question.OptionD, IsCorrect = question.CorrectOption == 'D' }
            },
            CorrectOption = question.CorrectOption,
            Explanation = question.Explanation,
            CreatedAt = question.CreatedAt,
            Message = "Thêm mới câu hỏi vào ngân hàng đề gốc thành công."
        };

        return Result<QuestionDetailDto>.Success(response);
    }
}
