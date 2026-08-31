using System;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Domain.Entities
{
    public class Passage
    {
        public Guid PassageId { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // Navigation Properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
