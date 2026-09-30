using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Materials.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Materials.Commands.CreateMaterial;

public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, Result<MaterialDetailDto>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<CreateMaterialCommandHandler> _logger;

    public CreateMaterialCommandHandler(
        IContentDbContext context,
        ILogger<CreateMaterialCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<MaterialDetailDto>> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating material lecture for SkillId {SkillId} with Title '{Title}'", request.SkillId, request.Title);

        var skill = await _context.Skills
            .Include(s => s.Domain)
            .FirstOrDefaultAsync(s => s.SkillId == request.SkillId, cancellationToken);

        if (skill == null)
        {
            _logger.LogWarning("Skill with ID {SkillId} not found in knowledge graph.", request.SkillId);
            return Result<MaterialDetailDto>.Failure(
                Error.NotFound("SkillNotFound", $"Không tìm thấy kỹ năng có ID {request.SkillId} trong hệ thống tri thức."));
        }

        var material = new Material
        {
            MaterialId = Guid.NewGuid(),
            SkillId = request.SkillId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim(),
            DurationSeconds = request.DurationSeconds,
            FileUrl = string.IsNullOrWhiteSpace(request.FileUrl) ? null : request.FileUrl.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Materials.AddAsync(material, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Material lecture {MaterialId} created successfully for Skill {SkillName}", material.MaterialId, skill.Name);

        var response = new MaterialDetailDto
        {
            MaterialId = material.MaterialId,
            SkillId = skill.SkillId,
            SkillName = skill.Name,
            DomainName = skill.Domain?.Name ?? "Miền Năng Lực Chưa Phân Loại",
            Title = material.Title,
            Content = material.Content,
            VideoUrl = material.VideoUrl,
            DurationSeconds = material.DurationSeconds,
            FileUrl = material.FileUrl,
            CreatedAt = material.CreatedAt,
            Message = "Khởi tạo bài giảng video lý thuyết thành công."
        };

        return Result<MaterialDetailDto>.Success(response);
    }
}
