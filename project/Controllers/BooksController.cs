using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models.Data;
using project.Models;
using Microsoft.AspNetCore.Identity;
using project.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace project.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> IndividualIndex()
        {
            var userId = _userManager.GetUserId(User);
            var books = await _context.Books.Where(e=>e.UserId==userId).ToListAsync();
            return View(books);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

       
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Author,Genre,Description,PublishedDate")] Book book, IFormFile PhotoUrl)
        {

            var userId = _userManager.GetUserId(User);

            book.UserId = userId;

            if (PhotoUrl != null && PhotoUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + PhotoUrl.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await PhotoUrl.CopyToAsync(fileStream);
                    }

                    book.PhotoUrl = "/uploads/" + uniqueFileName;
                }

                _context.Add(book);
                await _context.SaveChangesAsync();

            return RedirectToAction("Index");

        

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return PartialView("_EditBookForm", book);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Book model, IFormFile? photoUrl)
        {
            //if (ModelState.IsValid)
            //{
                var book = _context.Books.Find(model.BookId);
                if (book == null)
                {
                    return NotFound();
                }

                // Update book properties
                book.Title = model.Title;
                book.Author = model.Author;
                book.Genre = model.Genre;
                book.PublishedDate = model.PublishedDate;

                if (photoUrl != null)
                {
                    // Save the uploaded file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var fileName = Path.GetFileName(photoUrl.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoUrl.CopyToAsync(stream);
                    }

                    // Save the URL of the uploaded file
                    book.PhotoUrl = $"/uploads/{fileName}";
                }

                _context.Books.Update(book);
                await _context.SaveChangesAsync();

                 return RedirectToAction("IndividualIndex");
            //}

            //return Json(new { success = false, error = "Validation error" });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndividualIndex));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
        [Authorize]
        public IActionResult ReviewRating(int bookId)
        {
            var book = _context.Books
                .Include(b => b.Reviews)
                .ThenInclude(r => r.User)  // Make sure User is included
                .FirstOrDefault(b => b.BookId == bookId);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [Authorize]
        [HttpPost]
        public IActionResult SubmitReview(int bookId, int rating, string comment)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);  // Get the current user's ID

            // Check if the user has already submitted a review for this book
            var existingReview = _context.Reviews.FirstOrDefault(r => r.BookId == bookId && r.UserId == userId);

            if (existingReview != null)
            {
                // Update the existing review
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.DatePosted = DateTime.Now;
            }
            else
            {
                // Add a new review
                var newReview = new Review
                {
                    BookId = bookId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    DatePosted = DateTime.Now
                };

                _context.Reviews.Add(newReview);
            }

            _context.SaveChanges();

            return RedirectToAction("BookDetails", new { Id = bookId });
        }





        public IActionResult BookShowcase()
        {
            var books = _context.Books
                .Include(b => b.Reviews)
                .ToList();  // Fetch all books into memory

            var booksWithRatings = books
                .Select(b => new BookViewModel
                {
                    Book = b,
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = b.Reviews.Count
                })
                .OrderByDescending(b => b.AverageRating)  // Order by highest average rating
                .ToList();

            // Get the list of distinct genres for the checkboxes
            ViewBag.Genres = _context.Books
                .Select(b => b.Genre)
                .Distinct()
                .ToList();

            return View(booksWithRatings);
        }




        public IActionResult BookDetails(int id)
        {
            var book = _context.Books
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)  // Include reviewer details
                .FirstOrDefault(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            var bookViewModel = new BookViewModel
            {
                Book = book,
                AverageRating = book.Reviews.Any() ? book.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = book.Reviews.Count
            };

            return View(bookViewModel);
        }





    }
}

