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
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Curriculum.Commands.DeleteNode;

public record DeleteNodeCommand(string NodeIdStr) : IRequest<Result<bool>>
{
    public DeleteNodeCommand(Guid nodeId) : this(nodeId.ToString()) { }
}

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
        var rawId = request.NodeIdStr?.Trim();
        if (string.IsNullOrWhiteSpace(rawId))
        {
            return Result<bool>.Failure(
                Error.Validation("Curriculum.InvalidNodeId", "Mã Node không hợp lệ."));
        }

        try
        {
            var allSkills = await _context.Skills.ToListAsync(cancellationToken);
            var idsToDelete = new HashSet<Guid>();

            // 1. Hỗ trợ nhóm chuyên đề phẳng synthetic "chap-general-{domainId}"
            CompetencyDomain? domainToDelete = null;

            if (rawId.StartsWith("chap-general-", StringComparison.OrdinalIgnoreCase))
            {
                var domainStr = rawId.Substring("chap-general-".Length);
                if (!Guid.TryParse(domainStr, out var domainId))
                {
                    return Result<bool>.Failure(
                        Error.Validation("Curriculum.InvalidDomainId", $"Mã Miền không hợp lệ: {domainStr}"));
                }

                var flatSkills = allSkills.Where(s => s.DomainId == domainId && s.ParentId == null).ToList();
                if (flatSkills.Count == 0)
                {
                    return Result<bool>.Success(true);
                }

                foreach (var fs in flatSkills)
                {
                    idsToDelete.Add(fs.SkillId);
                    CollectDescendantIds(fs.SkillId, allSkills, idsToDelete);
                }
            }
            // 2. Hỗ trợ xóa toàn bộ Miền Năng Lực "domain-{domainId}"
            else if (rawId.StartsWith("domain-", StringComparison.OrdinalIgnoreCase))
            {
                var domainStr = rawId.Substring("domain-".Length);
                if (!Guid.TryParse(domainStr, out var domainId))
                {
                    return Result<bool>.Failure(
                        Error.Validation("Curriculum.InvalidDomainId", $"Mã Miền không hợp lệ: {domainStr}"));
                }

                domainToDelete = await _context.CompetencyDomains.FindAsync(new object[] { domainId }, cancellationToken);
                var domainSkills = allSkills.Where(s => s.DomainId == domainId).ToList();
                foreach (var ds in domainSkills)
                {
                    idsToDelete.Add(ds.SkillId);
                    CollectDescendantIds(ds.SkillId, allSkills, idsToDelete);
                }
            }
            else
            {
                // 3. Node chuẩn (Guid) - có thể là Skill hoặc Miền năng lực
                if (!Guid.TryParse(rawId, out var nodeId) || nodeId == Guid.Empty)
                {
                    return Result<bool>.Failure(
                        Error.Validation("Curriculum.InvalidNodeId", $"Mã Node không hợp lệ: {rawId}"));
                }

                var target = allSkills.FirstOrDefault(s => s.SkillId == nodeId);
                if (target != null)
                {
                    idsToDelete.Add(nodeId);
                    CollectDescendantIds(nodeId, allSkills, idsToDelete);
                }
                else
                {
                    domainToDelete = await _context.CompetencyDomains.FindAsync(new object[] { nodeId }, cancellationToken);
                    if (domainToDelete != null)
                    {
                        var domainSkills = allSkills.Where(s => s.DomainId == nodeId).ToList();
                        foreach (var ds in domainSkills)
                        {
                            idsToDelete.Add(ds.SkillId);
                            CollectDescendantIds(ds.SkillId, allSkills, idsToDelete);
                        }
                    }
                    else
                    {
                        return Result<bool>.Failure(
                            Error.NotFound("Curriculum.NodeNotFound", $"Không tìm thấy node hoặc miền có ID: {nodeId}"));
                    }
                }
            }

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

            // Xóa CompetencyDomain nếu đang thực hiện xóa Miền
            if (domainToDelete != null)
            {
                _context.CompetencyDomains.Remove(domainToDelete);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Đã xóa node/nhóm {RawId} chứa {Count} nodes liên quan", rawId, idsToDelete.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa node {RawId}", rawId);
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
