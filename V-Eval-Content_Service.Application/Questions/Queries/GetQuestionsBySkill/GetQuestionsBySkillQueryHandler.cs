using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Questions.DTOs;
using V_Eval_Content_Service.Domain.Constants;

namespace V_Eval_Content_Service.Application.Questions.Queries.GetQuestionsBySkill;

public class GetQuestionsBySkillQueryHandler : IRequestHandler<GetQuestionsBySkillQuery, Result<QuestionListResponseDto>>
{
    private readonly IContentDbContext _context;

    public GetQuestionsBySkillQueryHandler(IContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<QuestionListResponseDto>> Handle(GetQuestionsBySkillQuery request, CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .Include(s => s.Domain)
            .FirstOrDefaultAsync(s => s.SkillId == request.SkillId, cancellationToken);

        if (skill == null)
        {
            return Result<QuestionListResponseDto>.Failure(
                Error.NotFound("SkillNotFound", $"Không tìm thấy kỹ năng có ID {request.SkillId}."));
        }

        var query = _context.Questions
            .Include(q => q.Passage)
            .Where(q => q.SkillId == request.SkillId);

        if (request.BloomLevel.HasValue && request.BloomLevel.Value > 0)
        {
            query = query.Where(q => q.DifficultyLevel == request.BloomLevel.Value);
        }

        var limit = request.Limit > 0 ? request.Limit : 50;
        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var items = questions.Select(q => new QuestionDetailDto
        {
            QuestionId = q.QuestionId,
            SkillId = skill.SkillId,
            SkillName = skill.Name,
            DomainName = skill.Domain?.Name ?? "Miền Năng Lực Chưa Phân Loại",
            DomainCode = skill.Domain?.DomainId.ToString() ?? "",
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

        var response = new QuestionListResponseDto
        {
            SkillId = skill.SkillId,
            SkillName = skill.Name,
            TotalQuestions = items.Count,
            Questions = items
        };

        return Result<QuestionListResponseDto>.Success(response);
    }
}
