using MediatR;
using System;

namespace V_Eval_Content_Service.Application.MockExams.Commands.DeleteMockExam
{
    public class DeleteMockExamCommand : IRequest<bool>
    {
        public Guid ExamId { get; set; }

        public DeleteMockExamCommand(Guid examId)
        {
            ExamId = examId;
        }
    }
}
