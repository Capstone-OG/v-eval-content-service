using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using V_Eval_Content_Service.Application.Common.Converters;

namespace V_Eval_Content_Service.Application.Curriculum.DTOs;

public class CurriculumDomainDto
{
    public string Id { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;
    public string DomainCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Gradient { get; set; } = string.Empty;
    public string ActiveBg { get; set; } = string.Empty;
    
    // Cây phân cấp chuẩn sư phạm SGK: Chương -> Bài học -> Dạng bài
    public List<CurriculumChapterDto> Chapters { get; set; } = new();

    // Danh sách phẳng kỹ năng (phục vụ backward-compatibility)
    public List<CurriculumSkillDto> Skills { get; set; } = new();
}

public class CurriculumChapterDto
{
    public string ChapterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }
    public List<CurriculumLessonDto> Lessons { get; set; } = new();
}

public class CurriculumLessonDto
{
    public string LessonId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public double Weight { get; set; }
    public List<CurriculumPatternDto> Patterns { get; set; } = new();
}

public class CurriculumPatternDto
{
    public string PatternId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    
    // Khối 1: Nền tảng SGK & Khối 2: Phương pháp giải riêng chi tiết từng bước
    public string TheoryRecap { get; set; } = string.Empty;
    public string SolutionMethod { get; set; } = string.Empty;
    public List<CurriculumFormulaDto> KeyFormulas { get; set; } = new();
    public string CrossModel { get; set; } = string.Empty;
    public string CrossNote { get; set; } = string.Empty;

    // Quan hệ tiên quyết DAG (để bundle khi P(L) < 10%)
    public List<string> PrerequisiteIds { get; set; } = new();

    // Khối 3: Video bài giảng & ví dụ mẫu
    public CurriculumVideoLessonDto? VideoLesson { get; set; }
    public CurriculumSampleQuestionDto? SampleQuestion { get; set; }

    // Khối 4: Ngân hàng câu hỏi thích ứng
    public int QuestionBankCount { get; set; }
}

public class CurriculumSkillDto
{
    public string SkillId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string CrossModel { get; set; } = string.Empty;
    public string CrossNote { get; set; } = string.Empty;
    public string Theory { get; set; } = string.Empty;
    public List<CurriculumFormulaDto> KeyFormulas { get; set; } = new();
    public List<string> PrerequisiteIds { get; set; } = new();
    public CurriculumVideoLessonDto? VideoLesson { get; set; }
    public CurriculumSampleQuestionDto? SampleQuestion { get; set; }
}

public class CurriculumFormulaDto
{
    public string Label { get; set; } = string.Empty;
    public string Latex { get; set; } = string.Empty;
}

public class CurriculumVideoLessonDto
{
    public string Title { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Views { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
}

public class CurriculumSampleQuestionDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int Correct { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

// =========================================================================
// DTOs phục vụ Nhập liệu / Import Tri Thức Từ Giảng Viên & AI Engine (Dynamic)
// =========================================================================

public class ImportCurriculumRequestDto
{
    public Guid DomainId { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public string? DomainDescription { get; set; }
    public List<ImportChapterDto> Chapters { get; set; } = new();
}

public class ImportChapterDto
{
    public Guid? ChapterId { get; set; }
    public string ChapterName { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string? Description { get; set; }
    public List<ImportLessonDto> Lessons { get; set; } = new();
}

public class ImportLessonDto
{
    public Guid? LessonId { get; set; }
    public string LessonName { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string? Summary { get; set; }
    public List<ImportPatternDto> Patterns { get; set; } = new();
}

public class ImportPatternDto
{
    public Guid? PatternId { get; set; }
    public string PatternName { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string TheoryRecap { get; set; } = string.Empty;
    public string SolutionMethod { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DurationSeconds { get; set; } = 0;
    public List<Guid> PrerequisiteSkillIds { get; set; } = new();
    public List<ImportQuestionDto> Questions { get; set; } = new();
}

public class ImportQuestionDto
{
    public Guid? QuestionId { get; set; }
    public int DifficultyLevel { get; set; } = 2;
    public string ContentLatex { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    [JsonConverter(typeof(FlexibleCharJsonConverter))]
    public char CorrectOption { get; set; } = 'A';
    public string? Explanation { get; set; }
}

public class ImportCurriculumResponseDto
{
    public int ChaptersImported { get; set; }
    public int LessonsImported { get; set; }
    public int PatternsImported { get; set; }
    public int MaterialsImported { get; set; }
    public int QuestionsImported { get; set; }
    public string Message { get; set; } = string.Empty;
}

// =========================================================================
// DTOs phục vụ Hiệu Đính / Chỉnh Sửa Tri Thức Dành Riêng Cho Giảng Viên
// =========================================================================

public class UpdateCurriculumPatternDto
{
    public string Name { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string TheoryRecap { get; set; } = string.Empty;
    public string SolutionMethod { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int DurationSeconds { get; set; }
    public List<Guid>? PrerequisiteSkillIds { get; set; }
    public ImportQuestionDto? SampleQuestion { get; set; }
}

public class UpdateCurriculumNodeDto
{
    public string Name { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
}

public class CreateCurriculumNodeDto
{
    public Guid DomainId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string? TheoryRecap { get; set; }
    public string? SolutionMethod { get; set; }
    public string? VideoUrl { get; set; }
    public int DurationSeconds { get; set; } = 0;
}

public class CurriculumNodeDetailDto
{
    public Guid SkillId { get; set; }
    public Guid DomainId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public string Message { get; set; } = string.Empty;
}

