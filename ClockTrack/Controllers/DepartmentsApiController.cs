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


}
