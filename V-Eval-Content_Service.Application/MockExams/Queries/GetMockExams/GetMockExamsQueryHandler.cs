using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using V_Eval_Content_Service.Application.Common.Interfaces;

namespace V_Eval_Content_Service.Application.MockExams.Queries.GetMockExams
{
    public class GetMockExamsQueryHandler : IRequestHandler<GetMockExamsQuery, List<MockExamSummaryDto>>
    {
        private readonly IContentDbContext _context;

        public GetMockExamsQueryHandler(IContentDbContext context)
        {
            _context = context;
        }

        public async Task<List<MockExamSummaryDto>> Handle(GetMockExamsQuery request, CancellationToken cancellationToken)
        {
            return await _context.MockExams
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new MockExamSummaryDto
                {
                    ExamId = e.ExamId,
                    Title = e.Title,
                    DurationMinutes = e.DurationMinutes,
                    TotalQuestions = e.TotalQuestions,
                    IsPublished = e.IsPublished,
                    ExamCategory = e.ExamCategory,
                    SubjectCode = e.SubjectCode,
                    DifficultyLevel = e.DifficultyLevel,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
