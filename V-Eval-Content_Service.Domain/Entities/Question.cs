using System;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Domain.Entities
{
    public class Question
    {
        public Guid QuestionId { get; set; }
        public Guid SkillId { get; set; }
        public Guid? PassageId { get; set; }
        public int DifficultyLevel { get; set; }
        public string ContentLatex { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public char CorrectOption { get; set; }
        public string? Explanation { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Skill Skill { get; set; } = null!;
        public virtual Passage? Passage { get; set; }
        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    }
}
