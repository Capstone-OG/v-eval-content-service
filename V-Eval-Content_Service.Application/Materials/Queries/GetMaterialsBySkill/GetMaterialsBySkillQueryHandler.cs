using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Materials.DTOs;

namespace V_Eval_Content_Service.Application.Materials.Queries.GetMaterialsBySkill;

public class GetMaterialsBySkillQueryHandler : IRequestHandler<GetMaterialsBySkillQuery, Result<GetMaterialsBySkillResponseDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<GetMaterialsBySkillQueryHandler> _logger;

    public GetMaterialsBySkillQueryHandler(
        IContentDbContext context,
        ILogger<GetMaterialsBySkillQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<GetMaterialsBySkillResponseDto>> Handle(GetMaterialsBySkillQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving materials for SkillId: {SkillId}", request.SkillId);

        var skill = await _context.Skills
            .Include(s => s.Domain)
            .Include(s => s.Materials)
            .FirstOrDefaultAsync(s => s.SkillId == request.SkillId, cancellationToken);

        if (skill == null)
        {
            _logger.LogWarning("Skill with ID {SkillId} not found.", request.SkillId);
            return Result<GetMaterialsBySkillResponseDto>.Failure(
                Error.NotFound("SkillNotFound", $"Không tìm thấy kỹ năng có ID {request.SkillId} trong hệ thống tri thức."));
        }

        var materialItems = skill.Materials
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MaterialItemDto
            {
                MaterialId = m.MaterialId,
                Title = m.Title,
                Content = m.Content,
                VideoUrl = m.VideoUrl,
                DurationSeconds = m.DurationSeconds,
                FileUrl = m.FileUrl,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        var response = new GetMaterialsBySkillResponseDto
        {
            SkillId = skill.SkillId,
            SkillName = skill.Name,
            DomainName = skill.Domain?.Name ?? "Miền Năng Lực Chưa Phân Loại",
            TotalMaterials = materialItems.Count,
            Materials = materialItems
        };

        return Result<GetMaterialsBySkillResponseDto>.Success(response);
    }
}
