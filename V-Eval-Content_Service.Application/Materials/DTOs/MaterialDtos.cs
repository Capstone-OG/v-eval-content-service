using System;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Application.Materials.DTOs;

public class CreateMaterialRequestDto
{
    public Guid SkillId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DurationSeconds { get; set; } = 0;
    public string? FileUrl { get; set; }
}

public class MaterialDetailDto
{
    public Guid MaterialId { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DurationSeconds { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class MaterialItemDto
{
    public Guid MaterialId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DurationSeconds { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetMaterialsBySkillResponseDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public int TotalMaterials { get; set; }
    public IReadOnlyList<MaterialItemDto> Materials { get; set; } = new List<MaterialItemDto>();
}
