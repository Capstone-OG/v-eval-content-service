using MediatR;
using System;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Application.MockExams.Commands.ImportMockExam
{
    public class ImportMockExamCommand : IRequest<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } = 150;
        public string ExamCategory { get; set; } = "FULL_MOCK"; // FULL_MOCK, SUBJECT_PRACTICE, TOPICAL_PRACTICE
        public string? SubjectCode { get; set; }
        public int? DifficultyLevel { get; set; }

        public List<PassageDto> Passages { get; set; } = new();
        public List<QuestionDto> SingleQuestions { get; set; } = new();
    }

    public class PassageDto
    {
        public int StartQuestion { get; set; }
        public int EndQuestion { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class QuestionDto
    {
        public int QuestionNumber { get; set; }
        public int PageNumber { get; set; }
        public string Content { get; set; } = string.Empty;
        public OptionsDto Options { get; set; } = new();
        public char CorrectOption { get; set; } = 'A';
        public string? Explanation { get; set; }
        public int DifficultyLevel { get; set; } = 2; // 1: Easy, 2: Medium, 3: Hard
        public string? SuggestedSkillName { get; set; } // Ví dụ: Cực trị hàm số
    }

    public class OptionsDto
    {
        public string A { get; set; } = string.Empty;
        public string B { get; set; } = string.Empty;
        public string C { get; set; } = string.Empty;
        public string D { get; set; } = string.Empty;
    }
}
