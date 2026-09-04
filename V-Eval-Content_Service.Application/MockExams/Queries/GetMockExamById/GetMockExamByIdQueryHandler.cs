using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Domain.Entities;

namespace V_Eval_Content_Service.Application.MockExams.Queries.GetMockExamById
{
    public class GetMockExamByIdQueryHandler : IRequestHandler<GetMockExamByIdQuery, MockExamDetailDto?>
    {
        private readonly IContentDbContext _context;

        public GetMockExamByIdQueryHandler(IContentDbContext context)
        {
            _context = context;
        }

        public async Task<MockExamDetailDto?> Handle(GetMockExamByIdQuery request, CancellationToken cancellationToken)
        {
            var exam = await _context.MockExams
                .AsNoTracking()
                .Include(e => e.ExamQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q.Passage)
                .Include(e => e.ExamQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q.Skill)
                .FirstOrDefaultAsync(e => e.ExamId == request.ExamId, cancellationToken);

            if (exam == null)
            {
                return null;
            }

            var orderedExamQuestions = exam.ExamQuestions
                .OrderBy(eq => eq.QuestionOrder)
                .ToList();

            var passagesList = new List<ExamPassageDto>();
            var singleQuestionsList = new List<ExamQuestionItemDto>();

            // Phân nhóm câu hỏi theo Passage
            var passageGroups = orderedExamQuestions
                .Where(eq => eq.Question.PassageId.HasValue)
                .GroupBy(eq => eq.Question.PassageId!.Value)
                .ToList();

            foreach (var group in passageGroups)
            {
                var firstEq = group.First();
                var passage = firstEq.Question.Passage;

                var passageDto = new ExamPassageDto
                {
                    PassageId = group.Key,
                    Content = passage?.Content ?? string.Empty,
                    ImageUrl = passage?.ImageUrl,
                    StartQuestion = group.Min(g => g.QuestionOrder),
                    EndQuestion = group.Max(g => g.QuestionOrder),
                    Questions = group.Select(MapQuestionItem).ToList()
                };

                passagesList.Add(passageDto);
            }

            // Sắp xếp các chùm đọc hiểu theo thứ tự câu hỏi bắt đầu
            passagesList = passagesList.OrderBy(p => p.StartQuestion).ToList();

            // Các câu hỏi đơn lẻ không thuộc chùm
            var singles = orderedExamQuestions
                .Where(eq => !eq.Question.PassageId.HasValue)
                .Select(MapQuestionItem)
                .ToList();

            singleQuestionsList.AddRange(singles);

            return new MockExamDetailDto
            {
                ExamId = exam.ExamId,
                Title = exam.Title,
                FileName = exam.Title,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                TotalPassages = passagesList.Count,
                TotalSingleQuestions = singleQuestionsList.Count,
                ExamCategory = exam.ExamCategory,
                SubjectCode = exam.SubjectCode,
                DifficultyLevel = exam.DifficultyLevel,
                CreatedAt = exam.CreatedAt,
                Passages = passagesList,
                SingleQuestions = singleQuestionsList
            };
        }

        private static ExamQuestionItemDto MapQuestionItem(ExamQuestion eq)
        {
            var q = eq.Question;
            return new ExamQuestionItemDto
            {
                QuestionId = q.QuestionId,
                QuestionNumber = eq.QuestionOrder,
                PageNumber = 1,
                Content = q.ContentLatex,
                Options = new Dictionary<string, string>
                {
                    { "A", q.OptionA ?? string.Empty },
                    { "B", q.OptionB ?? string.Empty },
                    { "C", q.OptionC ?? string.Empty },
                    { "D", q.OptionD ?? string.Empty }
                },
                CorrectOption = q.CorrectOption.ToString(),
                Explanation = q.Explanation,
                SuggestedSkillName = q.Skill?.Name ?? "Tổng hợp",
                DifficultyLevel = q.DifficultyLevel
            };
        }
    }
}
