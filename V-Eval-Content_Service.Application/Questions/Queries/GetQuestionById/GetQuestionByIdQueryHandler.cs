using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Questions.DTOs;
using V_Eval_Content_Service.Domain.Constants;

namespace V_Eval_Content_Service.Application.Questions.Queries.GetQuestionById;

public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, Result<QuestionDetailDto>>
{
    private readonly IContentDbContext _context;

    public GetQuestionByIdQueryHandler(IContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<QuestionDetailDto>> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Include(q => q.Skill)
                .ThenInclude(s => s.Domain)
            .Include(q => q.Passage)
            .FirstOrDefaultAsync(q => q.QuestionId == request.QuestionId, cancellationToken);

        if (question == null)
        {
            return Result<QuestionDetailDto>.Failure(
                Error.NotFound("QuestionNotFound", $"Không tìm thấy câu hỏi có ID {request.QuestionId}."));
        }

        var response = new QuestionDetailDto
        {
            QuestionId = question.QuestionId,
            SkillId = question.SkillId,
            SkillName = question.Skill.Name,
            DomainName = question.Skill.Domain?.Name ?? "Miền Năng Lực Chưa Phân Loại",
            DomainCode = question.Skill.Domain?.DomainId.ToString() ?? "",
            PassageId = question.PassageId,
            PassageTitle = question.Passage?.Title,
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
            CreatedAt = question.CreatedAt
        };

        return Result<QuestionDetailDto>.Success(response);
    }
}
