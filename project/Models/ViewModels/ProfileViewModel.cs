namespace project.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public int BooksPublished { get; set; }
        public int ReviewsWritten { get; set; }
        public double AverageRating { get; set; }
        public List<ReviewViewModel> Reviews { get; set; }
    }

    
}
