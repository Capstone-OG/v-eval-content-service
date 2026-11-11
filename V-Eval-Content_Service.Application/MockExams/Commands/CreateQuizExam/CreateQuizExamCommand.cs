using System;
using System.Collections.Generic;
using MediatR;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.MockExams.DTOs;

namespace V_Eval_Content_Service.Application.MockExams.Commands.CreateQuizExam;

public record CreateQuizExamCommand(
    string? Title,
    List<Guid> SkillIds,
    int QuestionCount = 5,
    int MinBloomLevel = 1,
    int MaxBloomLevel = 3
) : IRequest<Result<QuizExamResponseDto>>;
