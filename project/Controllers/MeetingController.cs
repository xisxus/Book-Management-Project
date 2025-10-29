using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using project.Models.Data;
using project.Models;

namespace project.Controllers
{
    [Authorize]  // Requires login
    public class MeetingController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MeetingController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Join(string room = null)
        {
            if (string.IsNullOrEmpty(room))
            {
                room = Guid.NewGuid().ToString();  // Dynamically generate unique room if not provided
            }
            ViewBag.Room = room;
            return View();
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file, string room)
        {
            if (file != null && !string.IsNullOrEmpty(room))
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", room);  // Dynamic folder per room
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, file.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok(new { FileName = file.FileName });
            }
            return BadRequest();
        }

        [HttpGet("Download/{room}/{fileName}")]
        public IActionResult Download(string room, string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", room, fileName);
            if (System.IO.File.Exists(path))
            {
                return PhysicalFile(path, "application/octet-stream", fileName);
            }
            return NotFound();
        }

        [HttpGet("ListFiles/{room}")]
        public IActionResult ListFiles(string room)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", room);
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir).Select(f => Path.GetFileName(f)).ToList();
                return Ok(files);
            }
            return Ok(new List<string>());
        }
    }
}
