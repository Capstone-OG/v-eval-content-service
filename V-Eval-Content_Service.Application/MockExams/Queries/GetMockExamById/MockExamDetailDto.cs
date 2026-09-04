using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace V_Eval_Content_Service.Application.MockExams.Queries.GetMockExamById
{
    public class MockExamDetailDto
    {
        [JsonPropertyName("exam_id")]
        public Guid ExamId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("file_name")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("total_questions")]
        public int TotalQuestions { get; set; }

        [JsonPropertyName("total_passages")]
        public int TotalPassages { get; set; }

        [JsonPropertyName("total_single_questions")]
        public int TotalSingleQuestions { get; set; }

        [JsonPropertyName("exam_category")]
        public string ExamCategory { get; set; } = string.Empty;

        [JsonPropertyName("subject_code")]
        public string? SubjectCode { get; set; }

        [JsonPropertyName("difficulty_level")]
        public int? DifficultyLevel { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("passages")]
        public List<ExamPassageDto> Passages { get; set; } = new();

        [JsonPropertyName("single_questions")]
        public List<ExamQuestionItemDto> SingleQuestions { get; set; } = new();
    }

    public class ExamPassageDto
    {
        [JsonPropertyName("passage_id")]
        public Guid PassageId { get; set; }

        [JsonPropertyName("start_question")]
        public int StartQuestion { get; set; }

        [JsonPropertyName("end_question")]
        public int EndQuestion { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();

        [JsonPropertyName("questions")]
        public List<ExamQuestionItemDto> Questions { get; set; } = new();
    }

    public class ExamQuestionItemDto
    {
        [JsonPropertyName("question_id")]
        public Guid QuestionId { get; set; }

        [JsonPropertyName("question_number")]
        public int QuestionNumber { get; set; }

        [JsonPropertyName("page_number")]
        public int PageNumber { get; set; } = 1;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public Dictionary<string, string> Options { get; set; } = new();

        [JsonPropertyName("correct_option")]
        public string CorrectOption { get; set; } = "A";

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        [JsonPropertyName("suggested_skill_name")]
        public string? SuggestedSkillName { get; set; }

        [JsonPropertyName("difficulty_level")]
        public int DifficultyLevel { get; set; } = 2;

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();
    }
}
