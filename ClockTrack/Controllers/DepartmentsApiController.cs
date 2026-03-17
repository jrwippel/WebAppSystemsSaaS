using Microsoft.AspNetCore.Mvc;
using ClockTrack.Data;
using ClockTrack.Models;
using System.Threading.Tasks;
using ClockTrack.Models.ViewModels;
using ClockTrack.Services;
using ClockTrack.Models.Dto;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepartmentsApiController : ControllerBase
{
    private readonly ClockTrackContext _context;
    private readonly DepartmentService _departmentsService;

    public DepartmentsApiController(ClockTrackContext context, DepartmentService departmentService)
    {
        _context = context;
        _departmentsService = departmentService;
    }

    [HttpGet]
    [Route("GetAllDepartments")]
    public async Task<ActionResult<List<DepartmentDto>>> GetAllDepartments()
    {
        return await _departmentsService.GetAllDepartmentsAsync();
    }

    [HttpPost]
    [Route("QuickCreate")]
    public async Task<IActionResult> QuickCreate([FromBody] QuickCreateDeptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
            return BadRequest("Name is required.");

        var dept = new Department(request.Name.Trim());
        _context.Department.Add(dept);
        await _context.SaveChangesAsync();

        return Ok(new { id = dept.Id, name = dept.Name });
    }
}

public class QuickCreateDeptRequest
{
    public string Name { get; set; }
}
