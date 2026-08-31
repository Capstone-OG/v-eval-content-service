using System;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Domain.Entities
{
    public class MockExam
    {
        public Guid ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsPublished { get; set; } = false;
        public string ExamCategory { get; set; } = "FULL_MOCK"; // FULL_MOCK, SUBJECT_PRACTICE, TOPICAL_PRACTICE
        public string? SubjectCode { get; set; } // MATH, PHYS, CHEM, ENGL, VIET, LOGIC
        public int? DifficultyLevel { get; set; } // 1: Easy, 2: Medium, 3: Hard
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    }
}
