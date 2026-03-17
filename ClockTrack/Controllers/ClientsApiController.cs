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
public class ClientsApiController : ControllerBase
{
    private readonly ClockTrackContext _context;
    private readonly ClientService _clientsService;

    public ClientsApiController(ClockTrackContext context, ClientService clientService)
    {
        _context = context;
        _clientsService = clientService;
    }

   
    [HttpGet]    
    [Route("GetAllClients")]
    public async Task<ActionResult<List<ClientDTO>>> GetAllClients()
    {
        return await _clientsService.GetAllClientsAsync();
    }

    [HttpGet]
    [Route("GetClientImage/{clientId}")]
    public async Task<IActionResult> GetClientImage(int clientId)
    {
        var client = await _context.Client.FindAsync(clientId);

        if (client == null || client.ImageData == null)
        {
            return NotFound();
        }

        return File(client.ImageData, client.ImageMimeType);
    }

    [HttpPost]
    [Route("QuickCreate")]
    public async Task<IActionResult> QuickCreate([FromBody] QuickCreateNameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
            return BadRequest("Name is required.");

        var client = new Client { Name = request.Name.Trim() };
        _context.Client.Add(client);
        await _context.SaveChangesAsync();

        return Ok(new { id = client.Id, name = client.Name });
    }
}

public class QuickCreateNameRequest
{
    public string Name { get; set; }
}
