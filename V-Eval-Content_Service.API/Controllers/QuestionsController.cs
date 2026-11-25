using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using V_Eval_Content_Service.API.Caching;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Common.Interfaces;
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
    private readonly IContentDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public QuestionsController(IContentDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }
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
            ContentCacheKeys.InvalidateCatalog(_cache);
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
        if (result.IsSuccess) ContentCacheKeys.InvalidateCatalog(_cache);
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
        if (result.IsSuccess) ContentCacheKeys.InvalidateCatalog(_cache);
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

    public record FlagQuestionBody(string? ReasonCode = null, string? Note = null);

    /// <summary>
    /// Lấy danh sách tất cả câu hỏi đang bị khóa chờ kiểm duyệt (IsFlagged = true) từ DB
    /// </summary>
    [HttpGet("flagged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFlaggedQuestions(CancellationToken ct)
    {
        var flagged = await _dbContext.Questions
            .AsNoTracking()
            .Include(q => q.Skill)
                .ThenInclude(s => s.Domain)
            .Where(q => q.IsFlagged)
            .OrderByDescending(q => q.FlaggedAt ?? q.CreatedAt)
            .Take(200)
            .Select(q => new
            {
                flagId = "db_" + q.QuestionId.ToString(),
                questionId = q.QuestionId.ToString(),
                skillId = q.SkillId.ToString(),
                skillName = q.Skill != null ? q.Skill.Name : "Chưa rõ chuyên đề",
                subjectName = q.Skill != null && q.Skill.Domain != null ? q.Skill.Domain.Name : "Tổng hợp",
                questionContent = q.ContentLatex,
                options = new[] { q.OptionA, q.OptionB, q.OptionC, q.OptionD },
                correctOption = q.CorrectOption.ToString(),
                explanation = q.Explanation ?? string.Empty,
                difficultyLevel = q.DifficultyLevel,
                reasonCode = "FLAGGED_DB",
                reasonLabel = string.IsNullOrWhiteSpace(q.FlagReason) ? "Học sinh báo lỗi câu hỏi" : q.FlagReason,
                note = q.FlagReason ?? string.Empty,
                sourcePage = "Hệ thống (DB)",
                reportedBy = "Học sinh",
                reportedAt = q.FlaggedAt ?? q.CreatedAt,
                status = "PENDING",
                fromDatabase = true
            })
            .ToListAsync(ct);

        return Ok(flagged);
    }

    /// <summary>
    /// Khóa tạm thời câu hỏi bị học sinh báo cáo lỗi (chờ Giảng viên/Quản trị duyệt, không phát ở bất kỳ đâu)
    /// </summary>
    [HttpPost("{id:guid}/flag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagQuestion(Guid id, [FromBody] FlagQuestionBody? body, CancellationToken ct)
    {
        var question = await _dbContext.Questions.FirstOrDefaultAsync(q => q.QuestionId == id, ct);
        if (question == null)
        {
            return NotFound(new { message = $"Không tìm thấy câu hỏi {id}." });
        }

        question.IsFlagged = true;
        question.FlagReason = string.Join(" — ", new[] { body?.ReasonCode, body?.Note }.Where(s => !string.IsNullOrWhiteSpace(s)));
        question.FlaggedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        return Ok(new
        {
            questionId = question.QuestionId,
            isFlagged = question.IsFlagged,
            flagReason = question.FlagReason,
            flaggedAt = question.FlaggedAt,
            message = "Đã khóa câu hỏi khỏi hệ thống phát đề để chờ kiểm duyệt."
        });
    }

    /// <summary>
    /// Mở khóa lại câu hỏi sau khi Giảng viên/Quản trị đã kiểm duyệt &amp; sửa lỗi
    /// </summary>
    [HttpPost("{id:guid}/unflag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnflagQuestion(Guid id, CancellationToken ct)
    {
        var question = await _dbContext.Questions.FirstOrDefaultAsync(q => q.QuestionId == id, ct);
        if (question == null)
        {
            return NotFound(new { message = $"Không tìm thấy câu hỏi {id}." });
        }

        question.IsFlagged = false;
        question.FlagReason = null;
        question.FlaggedAt = null;
        await _dbContext.SaveChangesAsync(ct);

        return Ok(new
        {
            questionId = question.QuestionId,
            isFlagged = question.IsFlagged,
            message = "Đã mở khóa câu hỏi trở lại ngân hàng đề."
        });
    }
}
