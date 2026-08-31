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
                    var skillId = await GetOrCreateSkillIdAsync(qDto.SuggestedSkillName, request.SubjectCode, cancellationToken);

                    var question = new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        SkillId = skillId,
                        PassageId = passage.PassageId,
                        DifficultyLevel = qDto.DifficultyLevel,
                        ContentLatex = qDto.Content,
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
                var skillId = await GetOrCreateSkillIdAsync(qDto.SuggestedSkillName, request.SubjectCode, cancellationToken);

                var question = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    SkillId = skillId,
                    PassageId = null,
                    DifficultyLevel = qDto.DifficultyLevel,
                    ContentLatex = qDto.Content,
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

        private async Task<Guid> GetOrCreateSkillIdAsync(string? suggestedSkillName, string? subjectCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(suggestedSkillName))
            {
                suggestedSkillName = "Tổng hợp";
            }

            // Tìm skill theo tên
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Name.ToLower() == suggestedSkillName.Trim().ToLower(), cancellationToken);
            if (skill != null)
            {
                return skill.SkillId;
            }

            // Xác định Lĩnh vực lớn dựa vào mã môn
            string domainName = "Tổng hợp";
            if (!string.IsNullOrWhiteSpace(subjectCode))
            {
                domainName = subjectCode.ToUpper() switch
                {
                    "MATH" => "Toán học & Tư duy định lượng",
                    "VIET" => "Sử dụng Ngôn ngữ Tiếng Việt",
                    "ENGL" => "Sử dụng Ngôn ngữ Tiếng Anh",
                    "PHYS" or "CHEM" or "BIOL" => "Tư duy Khoa học / Khoa học tự nhiên",
                    "LOGIC" => "Tư duy Logic & Giải quyết vấn đề",
                    _ => "Tổng hợp"
                };
            }

            // Tìm hoặc tạo mới CompetencyDomain
            var domain = await _context.CompetencyDomains
                .FirstOrDefaultAsync(d => d.Name.ToLower() == domainName.ToLower(), cancellationToken);
            if (domain == null)
            {
                domain = new CompetencyDomain
                {
                    DomainId = Guid.NewGuid(),
                    Name = domainName,
                    Description = $"Lĩnh vực năng lực {domainName}"
                };
                _context.CompetencyDomains.Add(domain);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Tạo mới Skill dưới Domain này
            var newSkill = new Skill
            {
                SkillId = Guid.NewGuid(),
                DomainId = domain.DomainId,
                Name = suggestedSkillName.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.Skills.Add(newSkill);
            await _context.SaveChangesAsync(cancellationToken);

            return newSkill.SkillId;
        }
    }
}
