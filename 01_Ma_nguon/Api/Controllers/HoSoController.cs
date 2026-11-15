using CongMotCua.Api.Models.Requests;
using CongMotCua.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CongMotCua.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class HoSoController(WorkflowService workflow, HoSoQueryService queries) : ControllerBase
{
    [HttpGet("ho-so")]
    public async Task<object> GetDanhSach(string? q, string? status, string? deadline, string? work,
        int page = 1, int pageSize = 10, string? thuTucId = null, string? sort = null, bool includeFilters = false)
    {
        return await queries.List(await workflow.Actor(User), q, status, deadline, work, page, pageSize,
            thuTucId, sort, includeFilters);
    }

    [HttpGet("thong-ke")]
    public async Task<object> GetThongKe()
    {
        return await queries.Statistics(await workflow.Actor(User));
    }

    [HttpGet("ho-so/{id}")]
    public async Task<object> GetHoSo(string id)
    {
        return await workflow.Detail(id, await workflow.Actor(User));
    }

    [HttpPost("ho-so")]
    public async Task<IResult> TaoHoSo(DraftInput input)
    {
        return Results.Ok(await workflow.SaveDraft(null, input, await workflow.Actor(User)));
    }

    [HttpPut("ho-so/{id}/nhap")]
    public async Task<IResult> SuaBanNhap(string id, [FromQuery, BindRequired] int version, DraftInput input)
    {
        return Results.Ok(await workflow.SaveDraft(id, input, await workflow.Actor(User), version));
    }

    [HttpPost("ho-so/{id}/{thaoTac}")]
    public async Task<IResult> XuLyHoSo(string id, string thaoTac, ActionInput input)
    {
        return Results.Ok(await workflow.Act(id, thaoTac, input, await workflow.Actor(User)));
    }
}
