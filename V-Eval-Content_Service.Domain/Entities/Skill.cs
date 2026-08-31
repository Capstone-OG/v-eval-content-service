using System;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Domain.Entities
{
    public class Skill
    {
        public Guid SkillId { get; set; }
        public Guid DomainId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public double? Weight { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual CompetencyDomain Domain { get; set; } = null!;
        public virtual Skill? Parent { get; set; }
        public virtual ICollection<Skill> InverseParent { get; set; } = new List<Skill>();
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
