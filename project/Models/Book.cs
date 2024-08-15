using System.ComponentModel.DataAnnotations;

namespace project.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        public string Genre { get; set; }

        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; }

        public string PhotoUrl { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();


        public double AverageRating
        {
            get
            {
                return Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
            }
        }

        public int ReviewCount => Reviews.Count;

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }


        public string Description { get; set; }
    }
}
