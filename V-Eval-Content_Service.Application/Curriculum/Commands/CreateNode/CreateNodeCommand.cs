using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.Curriculum.DTOs;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.Curriculum.Commands.CreateNode;

public record CreateNodeCommand(CreateCurriculumNodeDto Dto) : IRequest<Result<Guid>>;

public class CreateNodeCommandHandler : IRequestHandler<CreateNodeCommand, Result<Guid>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<CreateNodeCommandHandler> _logger;

    public CreateNodeCommandHandler(
        IContentDbContext context,
        ILogger<CreateNodeCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateNodeCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (dto.DomainId == Guid.Empty)
        {
            return Result<Guid>.Failure(
                Error.Validation("Curriculum.InvalidDomain", "DomainId không được để trống."));
        }
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<Guid>.Failure(
                Error.Validation("Curriculum.InvalidName", "Tên chuyên đề/dạng bài không được để trống."));
        }

        try
        {
            var newId = Guid.NewGuid();
            var skill = new Skill
            {
                SkillId = newId,
                DomainId = dto.DomainId,
                ParentId = dto.ParentId,
                Name = dto.Name.Trim(),
                Weight = dto.Weight ?? 0.05,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Skills.AddAsync(skill, cancellationToken);

            // Nếu tạo dạng bài (có nội dung lý thuyết hoặc video)
            if (!string.IsNullOrWhiteSpace(dto.TheoryRecap) || !string.IsNullOrWhiteSpace(dto.SolutionMethod) || !string.IsNullOrWhiteSpace(dto.VideoUrl))
            {
                string formattedContent = $"### 1. Kiến thức nền tảng SGK GDPT 2018\n{dto.TheoryRecap?.Trim()}\n\n### 2. Phương pháp giải chi tiết từng bước\n{dto.SolutionMethod?.Trim()}";
                var material = new Material
                {
                    MaterialId = Guid.NewGuid(),
                    SkillId = newId,
                    Title = skill.Name,
                    Content = formattedContent,
                    VideoUrl = dto.VideoUrl,
                    DurationSeconds = dto.DurationSeconds,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Materials.AddAsync(material, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Tạo thành công Node mới: {Name} ({SkillId})", skill.Name, skill.SkillId);
            return Result<Guid>.Success(newId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo mới node {Name}", dto.Name);
            return Result<Guid>.Failure(
                Error.Failure("Curriculum.CreateNodeError", $"Không thể tạo mới node: {ex.Message}"));
        }
    }
}
