using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Common.Models;
using V_Eval_Content_Service.Application.MockExams.Commands.DeleteMockExam;
using V_Eval_Content_Service.Application.MockExams.Commands.ImportMockExam;
using V_Eval_Content_Service.Application.MockExams.Commands.PublishMockExam;
using V_Eval_Content_Service.Application.MockExams.Queries.GetMockExamById;
using V_Eval_Content_Service.Application.MockExams.Queries.GetMockExams;

namespace V_Eval_Content_Service.API.Controllers;

[Route("api/content/exams")]
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
    /// Phê duyệt và phát hành đề thi (Chuyển trạng thái từ Chờ duyệt sang Đã duyệt)
    /// </summary>
    [HttpPatch("{id:guid}/publish")]
    [HttpPut("{id:guid}/publish")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishExam([FromRoute] Guid id)
    {
        var success = await Mediator.Send(new PublishMockExamCommand(id));
        if (!success)
        {
            return HandleResult(Result.Failure(
                Error.NotFound("Exam.NotFound", $"Không tìm thấy đề thi với ID: {id}")));
        }
        return Ok(new { message = "Đã phê duyệt và xuất bản đề thi thành công!", exam_id = id, is_published = true });
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

    /// <summary>
    /// Core Flow 2 - API 19: Đóng gói bộ đề Quiz củng cố thích ứng (Adaptive Remedial Quiz / Micro Worksheet)
    /// </summary>
    /// <remarks>
    /// - Nhận danh sách kỹ năng mục tiêu `SkillIds` (hoặc `CommonWeakSkillIds` từ Practice Service micro-grouping).
    /// - Bốc 5-10 câu hỏi theo dải độ khó Bloom (mặc định Bloom 1-3).
    /// - Tự động tạo bản ghi `MockExam` loại `QUIZ` phục vụ gán bài tập cho học sinh hoặc bàn học.
    /// </remarks>
    [HttpPost("quiz")]
    [ProducesResponseType(typeof(V_Eval_Content_Service.Application.MockExams.DTOs.QuizExamResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateQuizExam([FromBody] V_Eval_Content_Service.Application.MockExams.DTOs.CreateQuizExamRequestDto request)
    {
        var command = new V_Eval_Content_Service.Application.MockExams.Commands.CreateQuizExam.CreateQuizExamCommand(
            Title: request.Title,
            SkillIds: request.SkillIds,
            QuestionCount: request.QuestionCount,
            MinBloomLevel: request.MinBloomLevel,
            MaxBloomLevel: request.MaxBloomLevel
        );

        var result = await Mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetExamById), new { id = result.Value.ExamId }, result.Value);
        }

        return HandleResult(result);
    }
}
