using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Curriculum.Commands.ImportCurriculum;
using V_Eval_Content_Service.Application.Curriculum.DTOs;
using V_Eval_Content_Service.Application.Curriculum.Queries.GetCurriculumTree;

namespace V_Eval_Content_Service.API.Controllers;

/// <summary>
/// Quản trị dàn kiến thức, khung năng lực và sơ đồ tri thức liên môn (Curriculum & Knowledge Ontology)
/// </summary>
[Route("api/content/curriculum")]
public class CurriculumController : ApiControllerBase
{
    /// <summary>
    /// Lấy toàn bộ cây dàn kiến thức 4 miền năng lực, chuyên đề, công thức KaTeX và liên kết liên môn từ Database
    /// </summary>
    /// <remarks>
    /// - Cung cấp dữ liệu động 100% cho phân hệ Dàn kiến thức Web Client.
    /// - Tích hợp đầy đủ danh mục kỹ năng chuẩn, câu hỏi mẫu, video bài giảng và đồ thị tiên quyết.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<CurriculumDomainDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurriculumTree()
    {
        var result = await Mediator.Send(new GetCurriculumTreeQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Nạp dữ liệu cây tri thức phân cấp từ Giảng viên, Ban Chuyên Môn hoặc AI Engine (Dynamic Ingestion)
    /// </summary>
    /// <remarks>
    /// - Hỗ trợ cả 2 kênh: Giảng viên/Academic tải lên hoặc AI Engine tự động nạp từ SGK.
    /// - Tự động upsert phân cấp: Miền -> Chương -> Bài học -> Dạng bài -> Materials -> Questions -> DAG Tiên quyết.
    /// </remarks>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportCurriculumResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ImportCurriculum([FromBody] ImportCurriculumRequestDto dto)
    {
        var result = await Mediator.Send(new ImportCurriculumCommand(dto));
        return HandleResult(result);
    }

    /// <summary>
    /// Giảng viên hiệu đính toàn bộ 4 khối nội dung của một Dạng bài cụ thể (Pattern Curation)
    /// </summary>
    /// <remarks>
    /// - Cho phép cập nhật tên dạng, trọng số, Khối 1 (Nền tảng SGK), Khối 2 (Phương pháp giải KaTeX từng bước), Khối 3 (Video bài giảng), Khối 4 (Câu hỏi mẫu).
    /// </remarks>
    [HttpPut("patterns/{id:guid}")]
    [ProducesResponseType(typeof(CurriculumPatternDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePattern([FromRoute] System.Guid id, [FromBody] UpdateCurriculumPatternDto dto)
    {
        var result = await Mediator.Send(new V_Eval_Content_Service.Application.Curriculum.Commands.UpdatePattern.UpdatePatternCommand(id, dto));
        return HandleResult(result);
    }

    /// <summary>
    /// Giảng viên cập nhật tên, mô tả hoặc thứ bậc của Chương / Bài học (Node Update)
    /// </summary>
    [HttpPut("nodes/{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNode([FromRoute] System.Guid id, [FromBody] UpdateCurriculumNodeDto dto)
    {
        var result = await Mediator.Send(new V_Eval_Content_Service.Application.Curriculum.Commands.UpdateNode.UpdateNodeCommand(id, dto));
        return HandleResult(result);
    }

    /// <summary>
    /// Giảng viên thêm mới Chương, Bài học hoặc Dạng bài vào Cây tri thức
    /// </summary>
    [HttpPost("nodes")]
    [ProducesResponseType(typeof(System.Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNode([FromBody] CreateCurriculumNodeDto dto)
    {
        var result = await Mediator.Send(new V_Eval_Content_Service.Application.Curriculum.Commands.CreateNode.CreateNodeCommand(dto));
        return HandleResult(result);
    }

    /// <summary>
    /// Giảng viên xóa một Chương, Bài học hoặc Dạng bài khỏi Cây tri thức
    /// </summary>
    [HttpDelete("nodes/{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNode([FromRoute] System.Guid id)
    {
        var result = await Mediator.Send(new V_Eval_Content_Service.Application.Curriculum.Commands.DeleteNode.DeleteNodeCommand(id));
        return HandleResult(result);
    }
}
