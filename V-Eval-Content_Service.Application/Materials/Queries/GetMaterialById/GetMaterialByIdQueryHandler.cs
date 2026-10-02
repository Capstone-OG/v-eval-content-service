using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Materials.DTOs;

namespace V_Eval_Content_Service.Application.Materials.Queries.GetMaterialById;

public class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, Result<MaterialDetailDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<GetMaterialByIdQueryHandler> _logger;

    public GetMaterialByIdQueryHandler(
        IContentDbContext context,
        ILogger<GetMaterialByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<MaterialDetailDto>> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving material details for MaterialId: {MaterialId}", request.MaterialId);

        var material = await _context.Materials
            .Include(m => m.Skill)
            .ThenInclude(s => s.Domain)
            .FirstOrDefaultAsync(m => m.MaterialId == request.MaterialId, cancellationToken);

        if (material == null)
        {
            _logger.LogWarning("Material with ID {MaterialId} not found.", request.MaterialId);
            return Result<MaterialDetailDto>.Failure(
                Error.NotFound("MaterialNotFound", $"Không tìm thấy tài liệu bài giảng có ID {request.MaterialId}."));
        }

        var response = new MaterialDetailDto
        {
            MaterialId = material.MaterialId,
            SkillId = material.SkillId,
            SkillName = material.Skill.Name,
            DomainName = material.Skill.Domain?.Name ?? "Miền Năng Lực Chưa Phân Loại",
            Title = material.Title,
            Content = material.Content,
            VideoUrl = material.VideoUrl,
            DurationSeconds = material.DurationSeconds,
            FileUrl = material.FileUrl,
            CreatedAt = material.CreatedAt,
            Message = "Lấy thông tin chi tiết bài giảng thành công."
        };

        return Result<MaterialDetailDto>.Success(response);
    }
}
