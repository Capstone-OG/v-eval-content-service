using MediatR;
using System;

namespace V_Eval_Content_Service.Application.MockExams.Commands.PublishMockExam
{
    public class PublishMockExamCommand : IRequest<bool>
    {
        public Guid ExamId { get; set; }

        public PublishMockExamCommand(Guid examId)
        {
            ExamId = examId;
        }
    }
}
