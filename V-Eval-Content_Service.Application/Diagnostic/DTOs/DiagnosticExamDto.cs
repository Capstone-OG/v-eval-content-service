namespace V_Eval_Content_Service.Application.Diagnostic.DTOs;

public class DiagnosticExamDto
{
    public Guid ExamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalPassages { get; set; }
    public int TotalSingleQuestions { get; set; }
    public List<DiagnosticPassageDto> Passages { get; set; } = new();
    public List<DiagnosticQuestionItemDto> SingleQuestions { get; set; } = new();
}

public class DiagnosticPassageDto
{
    public Guid PassageId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int StartQuestion { get; set; }
    public int EndQuestion { get; set; }
    public List<DiagnosticQuestionItemDto> Questions { get; set; } = new();
}

public class DiagnosticQuestionItemDto
{
    public Guid QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string> Options { get; set; } = new();
    public string SuggestedSkillName { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
}
