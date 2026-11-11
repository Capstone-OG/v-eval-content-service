using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Questions.Commands.CreateQuestion;
using V_Eval_Content_Service.Application.Questions.Commands.DeleteQuestion;
using V_Eval_Content_Service.Application.Questions.Commands.UpdateQuestion;
using V_Eval_Content_Service.Application.Questions.DTOs;
using V_Eval_Content_Service.Application.Questions.Queries.GetQuestionById;
using V_Eval_Content_Service.Application.Questions.Queries.GetQuestionsBySkill;

namespace V_Eval_Content_Service.API.Controllers;

/// <summary>
/// Quản trị ngân hàng câu hỏi gốc (Master Question Bank) - Core Flow 2 (APIs 16 - 18)
/// </summary>
[Route("api/content/questions")]
public class QuestionsController : ApiControllerBase
{
    /// <summary>
    /// Core Flow 2 - API 16: Thêm mới câu hỏi trắc nghiệm gốc vào ngân hàng đề
    /// </summary>
    /// <remarks>
    /// - Tiếp nhận công thức LaTeX stem (`ContentLatex`), 4 phương án trắc nghiệm A-D, phương án đúng (`CorrectOption`).
    /// - Định lượng độ khó theo Revised Bloom's Taxonomy (1: Nhớ, 2: Hiểu, 3: Vận dụng, 4: Phân tích, 5: Đánh giá, 6: Sáng tạo).
    /// - Gắn với mã kỹ năng `SkillId` chuẩn trong đồ thị DAG 12 kỹ năng.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequestDto request)
    {
        var command = new CreateQuestionCommand(
            SkillId: request.SkillId,
            PassageId: request.PassageId,
            DifficultyLevel: request.DifficultyLevel,
            ContentLatex: request.ContentLatex,
            OptionA: request.OptionA,
            OptionB: request.OptionB,
            OptionC: request.OptionC,
            OptionD: request.OptionD,
            CorrectOption: request.CorrectOption,
            Explanation: request.Explanation
        );

        var result = await Mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetQuestionById), new { id = result.Value.QuestionId }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Core Flow 2 - API 17: Hiệu đính nội dung câu hỏi trong ngân hàng đề
    /// </summary>
    /// <remarks>
    /// - Cập nhật nội dung câu hỏi LaTeX, các phương án đáp án, đáp án đúng, cấp độ Bloom và lời giải chi tiết.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(QuestionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequestDto request)
    {
        var command = new UpdateQuestionCommand(
            QuestionId: id,
            SkillId: request.SkillId,
            PassageId: request.PassageId,
            DifficultyLevel: request.DifficultyLevel,
            ContentLatex: request.ContentLatex,
            OptionA: request.OptionA,
            OptionB: request.OptionB,
            OptionC: request.OptionC,
            OptionD: request.OptionD,
            CorrectOption: request.CorrectOption,
            Explanation: request.Explanation
        );

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Core Flow 2 - API 18: Xóa câu hỏi khỏi ngân hàng đề gốc
    /// </summary>
    /// <remarks>
    /// - Gỡ bỏ câu hỏi sai sót và giải phóng các liên kết đề thi tương ứng.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var result = await Mediator.Send(new DeleteQuestionCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Tra cứu chi tiết một câu hỏi theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionById(Guid id)
    {
        var result = await Mediator.Send(new GetQuestionByIdQuery(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Tra cứu danh sách câu hỏi theo kỹ năng chuẩn và cấp độ Bloom
    /// </summary>
    [HttpGet("by-skill/{skillId:guid}")]
    [ProducesResponseType(typeof(QuestionListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionsBySkill(
        Guid skillId, 
        [FromQuery] int? bloomLevel = null,
        [FromQuery] int limit = 50)
    {
        var result = await Mediator.Send(new GetQuestionsBySkillQuery(skillId, bloomLevel, limit));
        return HandleResult(result);
    }
}
