using MediatR;
using System;

namespace V_Eval_Content_Service.Application.MockExams.Queries.GetMockExamById
{
    public class GetMockExamByIdQuery : IRequest<MockExamDetailDto?>
    {
        public Guid ExamId { get; set; }

        public GetMockExamByIdQuery(Guid examId)
        {
            ExamId = examId;
        }
    }
}
