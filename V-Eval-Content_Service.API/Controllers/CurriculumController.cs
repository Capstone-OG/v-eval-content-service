using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
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
}
