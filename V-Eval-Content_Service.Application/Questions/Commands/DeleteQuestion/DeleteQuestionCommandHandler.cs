using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using V_Eval_Content_Service.Application.Common.Interfaces;
using V_Eval_Content_Service.Application.Common.Models;

namespace V_Eval_Content_Service.Application.Questions.Commands.DeleteQuestion;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, Result<string>>
{
    private readonly IContentDbContext _context;
    private readonly ILogger<DeleteQuestionCommandHandler> _logger;

    public DeleteQuestionCommandHandler(
        IContentDbContext context,
        ILogger<DeleteQuestionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting question {QuestionId}", request.QuestionId);

        var question = await _context.Questions
            .Include(q => q.ExamQuestions)
            .FirstOrDefaultAsync(q => q.QuestionId == request.QuestionId, cancellationToken);

        if (question == null)
        {
            _logger.LogWarning("Question {QuestionId} not found.", request.QuestionId);
            return Result<string>.Failure(
                Error.NotFound("QuestionNotFound", $"Không tìm thấy câu hỏi có ID {request.QuestionId}."));
        }

        // Xóa liên kết đề thi nếu có
        if (question.ExamQuestions.Count > 0)
        {
            _context.ExamQuestions.RemoveRange(question.ExamQuestions);
        }

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Question {QuestionId} deleted successfully.", request.QuestionId);
        return Result<string>.Success($"Đã xóa câu hỏi '{request.QuestionId}' khỏi ngân hàng câu hỏi.");
    }
}
