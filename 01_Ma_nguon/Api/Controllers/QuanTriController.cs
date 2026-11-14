using CongMotCua.Api.Models.Requests;
using CongMotCua.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CongMotCua.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class QuanTriController(WorkflowService workflow, CatalogService catalog) : ControllerBase
{
    [HttpGet("quan-tri")]
    public async Task<object> GetDanhMuc()
    {
        return await catalog.GetCatalog(await workflow.Actor(User));
    }

    [HttpPost("quan-tri/bo-phan")]
    public async Task<object> LuuBoPhan(DepartmentInput input)
    {
        return await catalog.SaveDepartment(input, await workflow.Actor(User));
    }

    [HttpPost("quan-tri/thu-tuc")]
    public async Task<object> LuuThuTuc(ProcedureInput input)
    {
        return await catalog.SaveProcedure(input, await workflow.Actor(User));
    }

    [HttpPost("quan-tri/nguoi-dung")]
    public async Task<object> CapNhatNguoiDung(UserAdminInput input)
    {
        return await catalog.SetUserActive(input, await workflow.Actor(User));
    }

    [HttpGet("bao-cao")]
    public async Task<object> GetBaoCao()
    {
        return await catalog.Report(await workflow.Actor(User));
    }
}
