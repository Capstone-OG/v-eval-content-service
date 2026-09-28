using System;

namespace V_Eval_Content_Service.Domain.Entities
{
    /// <summary>
    /// Bảng nối quan hệ tiên quyết giữa các kỹ năng trong Cây khung năng lực (Đồ thị có hướng DAG)
    /// </summary>
    public class SkillPrerequisite
    {
        /// <summary>
        /// Kỹ năng mục tiêu (cần kỹ năng tiên quyết để học)
        /// </summary>
        public Guid SkillId { get; set; }

        /// <summary>
        /// Kỹ năng tiên quyết (phải hoàn thành hoặc nắm vững trước)
        /// </summary>
        public Guid PrerequisiteId { get; set; }

        // Navigation Properties
        public virtual Skill Skill { get; set; } = null!;
        public virtual Skill PrerequisiteSkill { get; set; } = null!;
    }
}
