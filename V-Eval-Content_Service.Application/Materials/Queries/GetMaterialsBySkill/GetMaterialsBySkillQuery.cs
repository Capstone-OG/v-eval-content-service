using System;
using MediatR;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Materials.DTOs;

namespace V_Eval_Content_Service.Application.Materials.Queries.GetMaterialsBySkill;

public record GetMaterialsBySkillQuery(Guid SkillId) : IRequest<Result<GetMaterialsBySkillResponseDto>>;
