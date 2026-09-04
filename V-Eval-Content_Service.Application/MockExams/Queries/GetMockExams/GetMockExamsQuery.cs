using MediatR;
using System.Collections.Generic;

namespace V_Eval_Content_Service.Application.MockExams.Queries.GetMockExams
{
    public class GetMockExamsQuery : IRequest<List<MockExamSummaryDto>>
    {
    }
}
