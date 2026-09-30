using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Materials.Commands.CreateMaterial;
using V_Eval_Content_Service.Application.Materials.DTOs;
using V_Eval_Content_Service.Application.Materials.Queries.GetMaterialById;
using V_Eval_Content_Service.Application.Materials.Queries.GetMaterialsBySkill;

namespace V_Eval_Content_Service.API.Controllers;

/// <summary>
/// Quản trị bài giảng lý thuyết, video bài giảng và tài liệu học tập theo kỹ năng (Core Flow 2 - Giai Đoạn 4)
/// </summary>
[Route("api/content/materials")]
public class MaterialsController : ApiControllerBase
{
    /// <summary>
    /// Core Flow 2 - API 20: Tạo mới và liên kết bài giảng video lý thuyết chuẩn theo kỹ năng
    /// </summary>
    /// <remarks>
    /// - Dành cho Giám đốc chuyên môn (Academic Director).
    /// - Tiếp nhận thông tin bài giảng chuẩn (`Title`, `Content`, `VideoUrl`, `DurationSeconds`, `FileUrl`) gắn với mã kỹ năng `SkillId`.
    /// - Cung cấp `MaterialId` cho chặng học trong Lộ trình cá nhân hóa của học sinh.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(MaterialDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialRequestDto request)
    {
        var command = new CreateMaterialCommand(
            SkillId: request.SkillId,
            Title: request.Title,
            Content: request.Content,
            VideoUrl: request.VideoUrl,
            DurationSeconds: request.DurationSeconds,
            FileUrl: request.FileUrl
        );

        var result = await Mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMaterialById), new { id = result.Value.MaterialId }, result.Value);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Core Flow 2 - API 21: Tra cứu danh sách bài giảng lý thuyết / video theo mã kỹ năng
    /// </summary>
    /// <remarks>
    /// - Dành cho Học sinh (Student), Hệ thống liên dịch vụ (Practice Service), hoặc Public.
    /// - Truy vấn nội dung lý thuyết tóm tắt, đường dẫn video bài giảng chuẩn và thời lượng phục vụ việc xem bài giảng chặng học.
    /// </remarks>
    [HttpGet("by-skill/{skillId:guid}")]
    [ProducesResponseType(typeof(GetMaterialsBySkillResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMaterialsBySkill(Guid skillId)
    {
        var query = new GetMaterialsBySkillQuery(skillId);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Tra cứu chi tiết một bài giảng lý thuyết theo ID
    /// </summary>
    /// <remarks>
    /// - Lấy chi tiết thông tin bài giảng, đường dẫn video, tài liệu đính kèm và tên kỹ năng liên kết.
    /// </remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MaterialDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMaterialById(Guid id)
    {
        var query = new GetMaterialByIdQuery(id);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}
