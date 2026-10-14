using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;

namespace V_Eval_Content_Service.Application.Curriculum.Commands.DeleteNode;

public record DeleteNodeCommand(Guid NodeId) : IRequest<Result<bool>>;

public class DeleteNodeCommandHandler : IRequestHandler<DeleteNodeCommand, Result<bool>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<DeleteNodeCommandHandler> _logger;

    public DeleteNodeCommandHandler(
        IContentDbContext context,
        ILogger<DeleteNodeCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteNodeCommand request,
        CancellationToken cancellationToken)
    {
        var nodeId = request.NodeId;
        if (nodeId == Guid.Empty)
        {
            return Result<bool>.Failure(
                Error.Validation("Curriculum.InvalidNodeId", "Mã Node không hợp lệ."));
        }

        try
        {
            var allSkills = await _context.Skills.ToListAsync(cancellationToken);
            var target = allSkills.FirstOrDefault(s => s.SkillId == nodeId);
            if (target == null)
            {
                return Result<bool>.Failure(
                    Error.NotFound("Curriculum.NodeNotFound", $"Không tìm thấy node có ID: {nodeId}"));
            }

            // Thu thập toàn bộ ID của target và các con cháu (descendants)
            var idsToDelete = new HashSet<Guid> { nodeId };
            CollectDescendantIds(nodeId, allSkills, idsToDelete);

            // Xóa quan hệ DAG Prerequisites
            var prereqs = await _context.SkillPrerequisites
                .Where(p => idsToDelete.Contains(p.SkillId) || idsToDelete.Contains(p.PrerequisiteId))
                .ToListAsync(cancellationToken);
            _context.SkillPrerequisites.RemoveRange(prereqs);

            // Xóa Materials
            var materials = await _context.Materials
                .Where(m => idsToDelete.Contains(m.SkillId))
                .ToListAsync(cancellationToken);
            _context.Materials.RemoveRange(materials);

            // Xóa Questions
            var questions = await _context.Questions
                .Where(q => idsToDelete.Contains(q.SkillId))
                .ToListAsync(cancellationToken);
            _context.Questions.RemoveRange(questions);

            // Xóa Skills
            var skillsToDelete = allSkills.Where(s => idsToDelete.Contains(s.SkillId)).ToList();
            _context.Skills.RemoveRange(skillsToDelete);

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Đã xóa node {Name} ({SkillId}) và {Count} nodes liên quan", target.Name, target.SkillId, idsToDelete.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa node {NodeId}", nodeId);
            return Result<bool>.Failure(
                Error.Failure("Curriculum.DeleteError", $"Không thể xóa node: {ex.Message}"));
        }
    }

    private static void CollectDescendantIds(Guid parentId, List<V_Eval_Content_Service.Domain.Entities.Skill> allSkills, HashSet<Guid> collected)
    {
        var children = allSkills.Where(s => s.ParentId == parentId).ToList();
        foreach (var child in children)
        {
            if (collected.Add(child.SkillId))
            {
                CollectDescendantIds(child.SkillId, allSkills, collected);
            }
        }
    }
}
