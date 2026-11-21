using Microsoft.AspNetCore.Mvc;
using V_Eval_Content_Service.API.Controllers.Base;
using V_Eval_Content_Service.Application.Diagnostic.DTOs;
using V_Eval_Content_Service.Application.Diagnostic.Queries.GetDiagnosticTest;

namespace V_Eval_Content_Service.API.Controllers;

[Route("api/content")]
public class DiagnosticController : ApiControllerBase
{
    /// <summary>
    /// Core Flow 1 - Bước 2: Lấy bộ đề thi khảo sát chẩn đoán năng lực ban đầu (30 câu hỏi cân bằng 4 Miền / 9 Môn GDPT 2018)
    /// </summary>
    [HttpGet("diagnostic-test")]
    [ProducesResponseType(typeof(DiagnosticExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiagnosticTest(
        [FromQuery] Guid? excludeExamId = null,
        [FromQuery] string? excludeExamIds = null)
    {
        var parsedExcludeIds = new List<Guid>();
        if (!string.IsNullOrWhiteSpace(excludeExamIds))
        {
            foreach (var raw in excludeExamIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(raw, out var parsed))
                {
                    parsedExcludeIds.Add(parsed);
                }
            }
        }

        var result = await Mediator.Send(new GetDiagnosticTestQuery(excludeExamId, parsedExcludeIds));
        return HandleResult(result);
    }
}
