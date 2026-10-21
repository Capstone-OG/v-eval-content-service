using System;
using MediatR;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Questions.DTOs;

namespace V_Eval_Content_Service.Application.Questions.Commands.UpdateQuestion;

public record UpdateQuestionCommand(
    Guid QuestionId,
    Guid SkillId,
    Guid? PassageId,
    int DifficultyLevel,
    string ContentLatex,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    char CorrectOption,
    string? Explanation = null
) : IRequest<Result<QuestionDetailDto>>;
