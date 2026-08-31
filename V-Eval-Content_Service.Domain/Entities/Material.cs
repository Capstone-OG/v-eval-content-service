using System;

namespace V_Eval_Content_Service.Domain.Entities
{
    public class Material
    {
        public Guid MaterialId { get; set; }
        public Guid SkillId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Skill Skill { get; set; } = null!;
    }
}
