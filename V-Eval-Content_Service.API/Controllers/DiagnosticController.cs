using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Diagnostic.DTOs;
using V_Eval_Content_Service.Application.Diagnostic.Queries.GetDiagnosticTest;

namespace V_Eval_Content_Service.API.Controllers;

[Route("api/v1/content")]
public class DiagnosticController : ApiControllerBase
{
    /// <summary>
    /// Core Flow 1 - Bước 2: Lấy bộ đề thi khảo sát chẩn đoán năng lực ban đầu (30 câu hỏi)
    /// </summary>
    /// <remarks>
    /// API trả về cấu trúc đề thi gồm 30 câu hỏi theo định dạng chuẩn ĐGNL V-ACT.
    /// Toàn bộ đáp án đúng và lời giải chi tiết được ẩn hoàn toàn để chống gian lận.
    /// </remarks>
    [HttpGet("diagnostic-test")]
    [ProducesResponseType(typeof(DiagnosticExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiagnosticTest()
    {
        var result = await Mediator.Send(new GetDiagnosticTestQuery());
        return HandleResult(result);
    }
}
