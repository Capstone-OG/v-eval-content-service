using System;

namespace V_Eval_Content_Service.Application.MockExams.Queries.GetMockExams
{
    public class MockExamSummaryDto
    {
        public Guid ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsPublished { get; set; }
        public string ExamCategory { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public int? DifficultyLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
