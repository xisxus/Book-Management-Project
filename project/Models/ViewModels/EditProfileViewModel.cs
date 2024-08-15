using System.ComponentModel.DataAnnotations;

namespace project.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public IFormFile Photo { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        // Display the existing photo
        public string ExistingPhotoPath { get; set; }
    }
}
