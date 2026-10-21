using System;
using MediatR;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Questions.DTOs;

namespace V_Eval_Content_Service.Application.Questions.Queries.GetQuestionById;

public record GetQuestionByIdQuery(Guid QuestionId) : IRequest<Result<QuestionDetailDto>>;
