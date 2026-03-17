using Microsoft.AspNetCore.Mvc;
using ClockTrack.Data;
using ClockTrack.Models;
using System.Threading.Tasks;
using ClockTrack.Models.ViewModels;
using ClockTrack.Services;
using ClockTrack.Models.Dto;

[Route("api/[controller]")]
[ApiController]
public class AttorneysApiController : ControllerBase
{
    private readonly ClockTrackContext _context;
    private readonly AttorneyService _attorneyService;

    public AttorneysApiController(ClockTrackContext
        context, AttorneyService attorneyService)
    {
        _context = context;
        _attorneyService = attorneyService;
    }

    [HttpGet]
    [Route("GetAllAttorneys")]
    public async Task<ActionResult<List<AttorneyDTO>>> GetAllAttorneys()
    {
        return await _attorneyService.GetAllAttorneysAsync();
    }


}
