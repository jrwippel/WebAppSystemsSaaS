using Microsoft.AspNetCore.Mvc;
using ClockTrack.Data;
using ClockTrack.Filters;
using ClockTrack.Helper;
using ClockTrack.Services;
using Microsoft.EntityFrameworkCore;


namespace ClockTrack.Controllers
{
    [PaginaParaUsuarioLogado]
    public class ActiviesController : Controller
    {

        private readonly ClockTrackContext _context;
        private readonly ClientService _clientService;
        private readonly AttorneyService _attorneyService;
        private readonly ProcessRecordsService _processRecordsService;
        private readonly ISessao _isessao;
        private readonly DepartmentService _departmentService;

        public ActiviesController(ClockTrackContext context, ClientService clientService, ProcessRecordsService processRecordsService, AttorneyService attorneyService, ISessao isessao, DepartmentService departmentService)
        {
            _context = context;
            _clientService = clientService;
            _processRecordsService = processRecordsService;
            _attorneyService = attorneyService;
            _isessao = isessao;
            _departmentService = departmentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProcessRecords()
        {
            var records = await _context.ProcessRecord
                .Include(pr => pr.Attorney)
                .Include(pr => pr.Client)
                .Include(pr => pr.Department)
                .ToListAsync();

            return Json(records);
        }

    }
}
