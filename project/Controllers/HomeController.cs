using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using project.Models;
using project.Models.Data;
using System.Diagnostics;

namespace project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
       

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var genres = _context.Books.Select(b => b.Genre).Distinct().ToList();

            ViewBag.Genres = genres;
            return View();
        }


        public IActionResult Search(string query, string genre)
        {
            var genres = _context.Books
        .Select(b => b.Genre)
        .Distinct()
        .ToList();

            ViewBag.Genres = genres;

            var results = _context.Books
                .AsQueryable();  

            if (!string.IsNullOrEmpty(query))
            {
                results = results.Where(b => b.Title.Contains(query) || b.Author.Contains(query));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                results = results.Where(b => b.Genre == genre);
            }

            return View(results.ToList());
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
