using System;
using MediatR;
using V_Eval_Content_Service.Application.Common.Models;

namespace V_Eval_Content_Service.Application.Questions.Commands.DeleteQuestion;

public record DeleteQuestionCommand(Guid QuestionId) : IRequest<Result<string>>;
