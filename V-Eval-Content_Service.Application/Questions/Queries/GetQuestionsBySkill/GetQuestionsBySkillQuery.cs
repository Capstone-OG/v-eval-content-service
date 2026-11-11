using System;
using MediatR;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Questions.DTOs;

namespace V_Eval_Content_Service.Application.Questions.Queries.GetQuestionsBySkill;

public record GetQuestionsBySkillQuery(
    Guid SkillId, 
    int? BloomLevel = null,
    int Limit = 50
) : IRequest<Result<QuestionListResponseDto>>;
