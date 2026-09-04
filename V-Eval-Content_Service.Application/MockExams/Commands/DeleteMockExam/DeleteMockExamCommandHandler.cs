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

            var questionIds = exam.ExamQuestions.Select(eq => eq.QuestionId).ToList();

            var questions = await _context.Questions
                .Where(q => questionIds.Contains(q.QuestionId))
                .ToListAsync(cancellationToken);

            var passageIds = questions
                .Where(q => q.PassageId.HasValue)
                .Select(q => q.PassageId!.Value)
                .Distinct()
                .ToList();

            // Xóa liên kết ExamQuestions
            _context.ExamQuestions.RemoveRange(exam.ExamQuestions);

            // Xóa các câu hỏi thuộc đề thi này
            _context.Questions.RemoveRange(questions);

            // Kiểm tra và dọn dẹp các chùm đọc hiểu mồ côi (không còn câu hỏi nào khác sử dụng)
            foreach (var pId in passageIds)
            {
                var hasOtherQuestions = await _context.Questions
                    .AnyAsync(q => q.PassageId == pId && !questionIds.Contains(q.QuestionId), cancellationToken);

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
