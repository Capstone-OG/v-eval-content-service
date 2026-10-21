using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using V_Eval_Content_Service.Application.Common.Converters;

namespace V_Eval_Content_Service.Application.Questions.DTOs;

/// <summary>
/// DTO tùy chọn đáp án của câu hỏi
/// </summary>
public class QuestionOptionItemDto
{
    public char Key { get; set; } // 'A', 'B', 'C', 'D'
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

/// <summary>
/// DTO yêu cầu tạo mới câu hỏi trắc nghiệm gốc (Core Flow 2 - API 16)
/// </summary>
public class CreateQuestionRequestDto
{
    public Guid SkillId { get; set; }
    public Guid? PassageId { get; set; }
    public int DifficultyLevel { get; set; } = 2; // Revised Bloom's Taxonomy 1-6 (1: Remember -> 6: Create)
    public string ContentLatex { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    [JsonConverter(typeof(FlexibleCharJsonConverter))]
    public char CorrectOption { get; set; } // 'A', 'B', 'C', 'D'
    public string? Explanation { get; set; }
}

/// <summary>
/// DTO yêu cầu hiệu đính câu hỏi (Core Flow 2 - API 17)
/// </summary>
public class UpdateQuestionRequestDto
{
    public Guid SkillId { get; set; }
    public Guid? PassageId { get; set; }
    public int DifficultyLevel { get; set; }
    public string ContentLatex { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    [JsonConverter(typeof(FlexibleCharJsonConverter))]
    public char CorrectOption { get; set; }
    public string? Explanation { get; set; }
}

/// <summary>
/// DTO chi tiết câu hỏi trả về
/// </summary>
public class QuestionDetailDto
{
    public Guid QuestionId { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string DomainCode { get; set; } = string.Empty;
    public Guid? PassageId { get; set; }
    public string? PassageTitle { get; set; }
    public int DifficultyLevel { get; set; }
    public string DifficultyName { get; set; } = string.Empty;
    public string ContentLatex { get; set; } = string.Empty;
    public List<QuestionOptionItemDto> Options { get; set; } = new();
    public char CorrectOption { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// DTO danh sách câu hỏi theo kỹ năng hoặc chuyên đề
/// </summary>
public class QuestionListResponseDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public List<QuestionDetailDto> Questions { get; set; } = new();
}
