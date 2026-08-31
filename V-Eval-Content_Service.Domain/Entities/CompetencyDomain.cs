using System;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Domain.Entities
{
    public class CompetencyDomain
    {
        public Guid DomainId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
