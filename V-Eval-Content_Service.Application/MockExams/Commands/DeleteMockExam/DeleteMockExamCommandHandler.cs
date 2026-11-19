using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using V_Eval_Content_Service.Application.Common.Interfaces;

namespace V_Eval_Content_Service.Application.MockExams.Commands.DeleteMockExam
{
    public class DeleteMockExamCommandHandler : IRequestHandler<DeleteMockExamCommand, bool>
    {
        private readonly IContentDbContext _context;

        public DeleteMockExamCommandHandler(IContentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteMockExamCommand request, CancellationToken cancellationToken)
        {
            var exam = await _context.MockExams
                .Include(e => e.ExamQuestions)
                .FirstOrDefaultAsync(e => e.ExamId == request.ExamId, cancellationToken);

            if (exam == null)
            {
                return false;
            }

            var questionIds = exam.ExamQuestions.Select(eq => eq.QuestionId).Distinct().ToList();

            // Chỉ xóa các câu hỏi không bị tham chiếu bởi đề thi nào khác (tránh lỗi khóa ngoại PostgreSQL 23503)
            var sharedQuestionIds = await _context.ExamQuestions
                .Where(eq => questionIds.Contains(eq.QuestionId) && eq.ExamId != request.ExamId)
                .Select(eq => eq.QuestionId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var exclusiveQuestionIds = questionIds.Except(sharedQuestionIds).ToList();

            var exclusiveQuestions = await _context.Questions
                .Where(q => exclusiveQuestionIds.Contains(q.QuestionId))
                .ToListAsync(cancellationToken);

            var passageIds = exclusiveQuestions
                .Where(q => q.PassageId.HasValue)
                .Select(q => q.PassageId!.Value)
                .Distinct()
                .ToList();

            // Xóa liên kết ExamQuestions của đề thi này
            _context.ExamQuestions.RemoveRange(exam.ExamQuestions);

            // Xóa các câu hỏi độc quyền thuộc đề thi này
            if (exclusiveQuestions.Count > 0)
            {
                _context.Questions.RemoveRange(exclusiveQuestions);
            }

            // Kiểm tra và dọn dẹp các chùm đọc hiểu mồ côi (không còn câu hỏi nào khác sử dụng)
            foreach (var pId in passageIds)
            {
                var hasOtherQuestions = await _context.Questions
                    .AnyAsync(q => q.PassageId == pId && !exclusiveQuestionIds.Contains(q.QuestionId), cancellationToken);

                if (!hasOtherQuestions)
                {
                    var passage = await _context.Passages.FirstOrDefaultAsync(p => p.PassageId == pId, cancellationToken);
                    if (passage != null)
                    {
                        _context.Passages.Remove(passage);
                    }
                }
            }

            // Xóa đề thi MockExam
            _context.MockExams.Remove(exam);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
