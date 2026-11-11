using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Curriculum.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Curriculum.Commands.UpdateNode;

public record UpdateNodeCommand(Guid NodeId, UpdateCurriculumNodeDto Dto) : IRequest<Result<bool>>;

public class UpdateNodeCommandHandler : IRequestHandler<UpdateNodeCommand, Result<bool>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<UpdateNodeCommandHandler> _logger;

    public UpdateNodeCommandHandler(
        IContentDbContext context,
        ILogger<UpdateNodeCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateNodeCommand request,
        CancellationToken cancellationToken)
    {
        var nodeId = request.NodeId;
        var dto = request.Dto;

        if (nodeId == Guid.Empty)
        {
            return Result<bool>.Failure(
                Error.Validation("Curriculum.InvalidNodeId", "Mã Node không hợp lệ."));
        }

        var skill = await _context.Skills
            .Include(s => s.Materials)
            .FirstOrDefaultAsync(s => s.SkillId == nodeId, cancellationToken);

        if (skill == null)
        {
            return Result<bool>.Failure(
                Error.NotFound("Curriculum.NodeNotFound", $"Không tìm thấy node có ID: {nodeId}"));
        }

        try
        {
            _logger.LogInformation("Giảng viên cập nhật Node (Chương/Bài học): {NodeName} ({NodeId})", skill.Name, skill.SkillId);

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                skill.Name = dto.Name.Trim();
            }
            if (dto.Weight.HasValue)
            {
                skill.Weight = dto.Weight.Value;
            }
            if (dto.ParentId.HasValue)
            {
                skill.ParentId = dto.ParentId.Value == Guid.Empty ? null : dto.ParentId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                var mat = skill.Materials.FirstOrDefault();
                if (mat == null)
                {
                    mat = new Material
                    {
                        MaterialId = Guid.NewGuid(),
                        SkillId = skill.SkillId,
                        Title = skill.Name,
                        Content = dto.Description.Trim(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Materials.AddAsync(mat, cancellationToken);
                }
                else
                {
                    mat.Title = skill.Name;
                    mat.Content = dto.Description.Trim();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật node {NodeId}", nodeId);
            return Result<bool>.Failure(
                Error.Failure("Curriculum.UpdateNodeError", $"Không thể lưu thay đổi: {ex.Message}"));
        }
    }
}
