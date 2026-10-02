using System;
using MediatR;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Materials.DTOs;

namespace V_Eval_Content_Service.Application.Materials.Commands.CreateMaterial;

public record CreateMaterialCommand(
    Guid SkillId,
    string Title,
    string Content,
    string? VideoUrl = null,
    int DurationSeconds = 0,
    string? FileUrl = null
) : IRequest<Result<MaterialDetailDto>>;
