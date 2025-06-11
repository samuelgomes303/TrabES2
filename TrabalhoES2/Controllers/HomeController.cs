using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoES2.Models;           // Ajusta para o teu namespace de DbContext


namespace TrabalhoES2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly projetoPraticoDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            projetoPraticoDbContext context,
            UserManager<Utilizador> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            // 1) Buscar utilizador actual
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                ViewBag.IsAdmin = true;
                return View(); // Passa sem carteira
            }

            // 2) Eager‐load da carteira + utilizador + ativos + cada tipo + banco
            var carteira = await _context.Carteiras
                .Include(c => c.Utilizador)
                .Include(c => c.Ativofinanceiros)
                    .ThenInclude(a => a.Depositoprazo)
                        .ThenInclude(d => d.Banco)
                .Include(c => c.Ativofinanceiros)
                    .ThenInclude(a => a.Fundoinvestimento)
                        .ThenInclude(f => f.Banco)
                .Include(c => c.Ativofinanceiros)
                    .ThenInclude(a => a.Imovelarrendado)
                        .ThenInclude(i => i.Banco)
                .FirstOrDefaultAsync(c => c.UtilizadorId == user.Id);

            // 3) Se não existir, criamos uma carteira vazia
            if (carteira == null)
            {
                carteira = new Carteira
                {
                    Nome = "Carteira Principal",
                    UtilizadorId = user.Id,
                    Ativofinanceiros = new List<Ativofinanceiro>()
                };
                _context.Carteiras.Add(carteira);
                await _context.SaveChangesAsync();
            }

            // 4) Passa para a view
            return View(carteira);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
