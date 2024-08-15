using Microsoft.AspNetCore.Identity;

namespace project.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public byte[]? Photo { get; set; }

        public ICollection<Book> Books { get; set; }

    }
}
