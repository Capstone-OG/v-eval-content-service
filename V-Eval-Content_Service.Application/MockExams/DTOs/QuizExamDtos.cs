using System;
using System.Collections.Generic;
using V_Eval_Content_Service.Application.Questions.DTOs;

namespace V_Eval_Content_Service.Application.MockExams.DTOs;

/// <summary>
/// Yêu cầu đóng gói bộ đề Quiz củng cố / Phiếu bài tập vi mô (Core Flow 2 - API 19)
/// </summary>
public class CreateQuizExamRequestDto
{
    public string? Title { get; set; }
    public List<Guid> SkillIds { get; set; } = new();
    public int QuestionCount { get; set; } = 5;
    public int MinBloomLevel { get; set; } = 1;
    public int MaxBloomLevel { get; set; } = 3;
}

/// <summary>
/// Kết quả đóng gói bộ đề Quiz củng cố trả về
/// </summary>
public class QuizExamResponseDto
{
    public Guid ExamId { get; set; } // Phục vụ gán làm WorksheetId cho Practice Service
    public string Title { get; set; } = string.Empty; // Phục vụ gán làm WorksheetTitle
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public string ExamCategory { get; set; } = "QUIZ";
    public List<Guid> TargetSkillIds { get; set; } = new();
    public List<QuestionDetailDto> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
