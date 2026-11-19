using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.MockExams.Commands.ImportMockExam
{
    public class ImportMockExamCommandHandler : IRequestHandler<ImportMockExamCommand, Guid>
    {
        private readonly IContentDbContext _context;

        public ImportMockExamCommandHandler(IContentDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(ImportMockExamCommand request, CancellationToken cancellationToken)
        {
            // Nạp sẵn danh sách Skills & Domains 1 lần để ánh xạ nhanh (tránh N+1 query cho đề 120 câu)
            var existingSkills = await _context.Skills
                .Include(s => s.Parent)
                .ToListAsync(cancellationToken);
            var existingDomains = await _context.CompetencyDomains
                .ToListAsync(cancellationToken);

            // 1. Khởi tạo thực thể đề thi
            var mockExam = new MockExam
            {
                ExamId = Guid.NewGuid(),
                Title = request.Title,
                DurationMinutes = request.DurationMinutes,
                TotalQuestions = request.Passages.Sum(p => p.Questions.Count) + request.SingleQuestions.Count,
                IsPublished = false,
                ExamCategory = request.ExamCategory,
                SubjectCode = request.SubjectCode,
                DifficultyLevel = request.DifficultyLevel,
                CreatedAt = DateTime.UtcNow
            };

            _context.MockExams.Add(mockExam);

            // 2. Xử lý các chùm câu hỏi đọc hiểu (Passages)
            foreach (var passageDto in request.Passages)
            {
                var passage = new Passage
                {
                    PassageId = Guid.NewGuid(),
                    Content = passageDto.Content,
                    ImageUrl = passageDto.ImageUrl
                };
                _context.Passages.Add(passage);

                foreach (var qDto in passageDto.Questions)
                {
                    var skillId = await ResolveOrCreateSkillIdAsync(
                        qDto,
                        request.SubjectCode,
                        existingSkills,
                        existingDomains,
                        cancellationToken);

                    var questionContent = !string.IsNullOrWhiteSpace(qDto.ImageUrl) && !qDto.Content.Contains(qDto.ImageUrl)
                        ? $"![Hình minh họa]({qDto.ImageUrl})\n\n{qDto.Content}"
                        : qDto.Content;

                    var question = new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        SkillId = skillId,
                        PassageId = passage.PassageId,
                        DifficultyLevel = qDto.DifficultyLevel,
                        ContentLatex = questionContent,
                        OptionA = qDto.Options.A,
                        OptionB = qDto.Options.B,
                        OptionC = qDto.Options.C,
                        OptionD = qDto.Options.D,
                        CorrectOption = qDto.CorrectOption,
                        Explanation = qDto.Explanation,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Questions.Add(question);

                    var examQuestion = new ExamQuestion
                    {
                        ExamId = mockExam.ExamId,
                        QuestionId = question.QuestionId,
                        QuestionOrder = qDto.QuestionNumber
                    };
                    _context.ExamQuestions.Add(examQuestion);
                }
            }

            // 3. Xử lý các câu hỏi đơn lẻ (SingleQuestions)
            foreach (var qDto in request.SingleQuestions)
            {
                var skillId = await ResolveOrCreateSkillIdAsync(
                    qDto,
                    request.SubjectCode,
                    existingSkills,
                    existingDomains,
                    cancellationToken);

                var questionContent = !string.IsNullOrWhiteSpace(qDto.ImageUrl) && !qDto.Content.Contains(qDto.ImageUrl)
                    ? $"![Hình minh họa]({qDto.ImageUrl})\n\n{qDto.Content}"
                    : qDto.Content;

                var question = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    SkillId = skillId,
                    PassageId = null,
                    DifficultyLevel = qDto.DifficultyLevel,
                    ContentLatex = questionContent,
                    OptionA = qDto.Options.A,
                    OptionB = qDto.Options.B,
                    OptionC = qDto.Options.C,
                    OptionD = qDto.Options.D,
                    CorrectOption = qDto.CorrectOption,
                    Explanation = qDto.Explanation,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Questions.Add(question);

                var examQuestion = new ExamQuestion
                {
                    ExamId = mockExam.ExamId,
                    QuestionId = question.QuestionId,
                    QuestionOrder = qDto.QuestionNumber
                };
                _context.ExamQuestions.Add(examQuestion);
            }

            // 4. Lưu lại toàn bộ các thay đổi vào PostgreSQL trong 1 Transaction duy nhất
            await _context.SaveChangesAsync(cancellationToken);

            return mockExam.ExamId;
        }

        private async Task<Guid> ResolveOrCreateSkillIdAsync(
            QuestionDto qDto,
            string? subjectCode,
            List<Skill> existingSkills,
            List<CompetencyDomain> existingDomains,
            CancellationToken cancellationToken)
        {
            // Tầng 1: Ưu tiên tuyệt đối SkillId chuẩn từ cây Khung chương trình nếu tồn tại trong DB
            if (qDto.SkillId.HasValue && qDto.SkillId.Value != Guid.Empty)
            {
                var exactNode = existingSkills.FirstOrDefault(s => s.SkillId == qDto.SkillId.Value);
                if (exactNode != null)
                {
                    return exactNode.SkillId;
                }
            }

            // Tầng 2: Khớp tên nút theo thứ tự ưu tiên nút nằm trong cây (ParentId != null) trước khi khớp phẳng
            var normalizedTarget = (qDto.SuggestedSkillName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedTarget))
            {
                var treeMatch = existingSkills
                    .Where(s => s.ParentId.HasValue)
                    .FirstOrDefault(s => string.Equals(s.Name.Trim(), normalizedTarget, StringComparison.OrdinalIgnoreCase));
                if (treeMatch != null) return treeMatch.SkillId;

                var anyExactMatch = existingSkills
                    .FirstOrDefault(s => string.Equals(s.Name.Trim(), normalizedTarget, StringComparison.OrdinalIgnoreCase));
                if (anyExactMatch != null) return anyExactMatch.SkillId;
            }

            // Tầng 2b: Khớp theo tên Bài học / Chương nếu có truyền kèm
            if (!string.IsNullOrWhiteSpace(qDto.LessonName))
            {
                var lessonTarget = qDto.LessonName.Trim();
                var lessonMatch = existingSkills
                    .Where(s => s.ParentId.HasValue)
                    .FirstOrDefault(s => string.Equals(s.Name.Trim(), lessonTarget, StringComparison.OrdinalIgnoreCase));
                if (lessonMatch != null) return lessonMatch.SkillId;
            }

            if (!string.IsNullOrWhiteSpace(qDto.ChapterName))
            {
                var chapterTarget = qDto.ChapterName.Trim();
                var chapterMatch = existingSkills
                    .FirstOrDefault(s => string.Equals(s.Name.Trim(), chapterTarget, StringComparison.OrdinalIgnoreCase));
                if (chapterMatch != null) return chapterMatch.SkillId;
            }

            // Tầng 3: Fallback an toàn (chỉ tạo Skill mới khi chưa có nút nào khớp trong cây)
            string fallbackSkillName = string.IsNullOrWhiteSpace(normalizedTarget) ? "Tổng hợp" : normalizedTarget;

            string domainName = !string.IsNullOrWhiteSpace(qDto.DomainName)
                ? qDto.DomainName.Trim()
                : ResolveDomainNameBySubject(subjectCode);

            var domain = existingDomains
                .FirstOrDefault(d => string.Equals(d.Name.Trim(), domainName, StringComparison.OrdinalIgnoreCase));
            if (domain == null)
            {
                domain = new CompetencyDomain
                {
                    DomainId = Guid.NewGuid(),
                    Name = domainName,
                    Description = $"Lĩnh vực năng lực {domainName}"
                };
                _context.CompetencyDomains.Add(domain);
                existingDomains.Add(domain);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var newSkill = new Skill
            {
                SkillId = Guid.NewGuid(),
                DomainId = domain.DomainId,
                Name = fallbackSkillName,
                CreatedAt = DateTime.UtcNow
            };
            _context.Skills.Add(newSkill);
            existingSkills.Add(newSkill);
            await _context.SaveChangesAsync(cancellationToken);

            return newSkill.SkillId;
        }

        private static string ResolveDomainNameBySubject(string? subjectCode)
        {
            if (string.IsNullOrWhiteSpace(subjectCode)) return "Tổng hợp";
            return subjectCode.Trim().ToUpperInvariant() switch
            {
                "MATH" => "Toán học & Tư duy định lượng",
                "VIET" => "Sử dụng Ngôn ngữ Tiếng Việt",
                "ENGL" => "Sử dụng Ngôn ngữ Tiếng Anh",
                "PHYS" or "CHEM" or "BIOL" => "Tư duy Khoa học / Khoa học tự nhiên",
                "LOGIC" => "Tư duy Logic & Giải quyết vấn đề",
                _ => "Tổng hợp"
            };
        }
    }
}
