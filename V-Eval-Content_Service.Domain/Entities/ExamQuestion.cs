using System;

namespace V_Eval_Content_Service.Domain.Entities
{
    public class ExamQuestion
    {
        public Guid ExamId { get; set; }
        public Guid QuestionId { get; set; }
        public int QuestionOrder { get; set; }

        // Navigation Properties
        public virtual MockExam Exam { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
    }
}
