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

namespace V_Eval_Content_Service.Application.Questions.Commands.UpdateQuestion;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, Result<QuestionDetailDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<UpdateQuestionCommandHandler> _logger;

    public UpdateQuestionCommandHandler(
        IContentDbContext context,
        ILogger<UpdateQuestionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<QuestionDetailDto>> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating question {QuestionId}", request.QuestionId);

        var question = await _context.Questions
            .FirstOrDefaultAsync(q => q.QuestionId == request.QuestionId, cancellationToken);

        if (question == null)
        {
            _logger.LogWarning("Question with ID {QuestionId} not found.", request.QuestionId);
            return Result<QuestionDetailDto>.Failure(
                Error.NotFound("QuestionNotFound", $"Không tìm thấy câu hỏi có ID {request.QuestionId}."));
        }

        var skill = await _context.Skills
            .Include(s => s.Domain)
            .FirstOrDefaultAsync(s => s.SkillId == request.SkillId, cancellationToken);

        if (skill == null)
        {
            _logger.LogWarning("Skill with ID {SkillId} not found.", request.SkillId);
            return Result<QuestionDetailDto>.Failure(
                Error.NotFound("SkillNotFound", $"Không tìm thấy kỹ năng có ID {request.SkillId} trong hệ thống tri thức."));
        }

        Passage? passage = null;
        if (request.PassageId.HasValue && request.PassageId.Value != Guid.Empty)
        {
            passage = await _context.Passages
                .FirstOrDefaultAsync(p => p.PassageId == request.PassageId.Value, cancellationToken);

            if (passage == null)
            {
                return Result<QuestionDetailDto>.Failure(
                    Error.NotFound("PassageNotFound", $"Không tìm thấy bài đọc hiểu có ID {request.PassageId.Value}."));
            }
        }

        question.SkillId = request.SkillId;
        question.PassageId = request.PassageId;
        question.DifficultyLevel = request.DifficultyLevel;
        question.ContentLatex = request.ContentLatex.Trim();
        question.OptionA = request.OptionA.Trim();
        question.OptionB = request.OptionB.Trim();
        question.OptionC = request.OptionC.Trim();
        question.OptionD = request.OptionD.Trim();
        question.CorrectOption = char.ToUpperInvariant(request.CorrectOption);
        question.Explanation = string.IsNullOrWhiteSpace(request.Explanation) ? null : request.Explanation.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Question {QuestionId} updated successfully.", question.QuestionId);

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
            Message = "Hiệu đính câu hỏi thành công."
        };

        return Result<QuestionDetailDto>.Success(response);
    }
}
