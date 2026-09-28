using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using V_Eval_Content_Service.Application.Common.Interfaces;

namespace V_Eval_Content_Service.Application.MockExams.Commands.PublishMockExam
{
    public class PublishMockExamCommandHandler : IRequestHandler<PublishMockExamCommand, bool>
    {
        private readonly IContentDbContext _context;

        public PublishMockExamCommandHandler(IContentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(PublishMockExamCommand request, CancellationToken cancellationToken)
        {
            var exam = await _context.MockExams
                .FirstOrDefaultAsync(e => e.ExamId == request.ExamId, cancellationToken);

            if (exam == null)
            {
                return false;
            }

            exam.IsPublished = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
