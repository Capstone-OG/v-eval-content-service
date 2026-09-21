using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.MockExams.Commands.DeleteMockExam;
using V_Eval_Content_Service.Application.MockExams.Commands.ImportMockExam;
using V_Eval_Content_Service.Application.MockExams.Queries.GetMockExamById;
using V_Eval_Content_Service.Application.MockExams.Queries.GetMockExams;

namespace V_Eval_Content_Service.API.Controllers;

[Route("api/v1/content/exams")]
public class MockExamsController : ApiControllerBase
{
    /// <summary>
    /// Lấy danh sách toàn bộ đề thi có trong ngân hàng đề
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MockExamSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExams()
    {
        var exams = await Mediator.Send(new GetMockExamsQuery());
        return Ok(exams);
    }

    /// <summary>
    /// Lấy chi tiết một đề thi theo ExamId
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MockExamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExamById([FromRoute] Guid id)
    {
        var exam = await Mediator.Send(new GetMockExamByIdQuery(id));
        if (exam == null)
        {
            return HandleResult(Result<MockExamDetailDto>.Failure(
                Error.NotFound("Exam.NotFound", $"Không tìm thấy đề thi với mã: {id}")));
        }
        return Ok(exam);
    }

    /// <summary>
    /// Import dữ liệu đề thi bóc tách từ AI Engine vào hệ thống
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportExam([FromBody] ImportMockExamCommand command)
    {
        var examId = await Mediator.Send(command);
        return Ok(new
        {
            exam_id = examId,
            message = "Import đề thi thành công vào Supabase PostgreSQL!"
        });
    }

    /// <summary>
    /// Xóa đề thi theo ExamId
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExam([FromRoute] Guid id)
    {
        var success = await Mediator.Send(new DeleteMockExamCommand(id));
        if (!success)
        {
            return HandleResult(Result.Failure(
                Error.NotFound("Exam.NotFound", $"Không tìm thấy đề thi với ID: {id} để xóa")));
        }
        return Ok(new { message = "Đã xóa đề thi thành công!", exam_id = id });
    }
}
