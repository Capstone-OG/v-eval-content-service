using System;
using System.Collections.Generic;

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
    public List<CurriculumSkillDto> Skills { get; set; } = new();
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
