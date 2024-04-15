using System.ComponentModel.DataAnnotations;

namespace WebEnterprise.ViewModels.User
{
    public class EditProfileModel
    {
            [Required]
            public string FullName { set; get; }

            [Required]
            [EmailAddress]
            public string Email { set; get; }
            [Required]
            public string? PhoneNumber { set; get; }

            public IFormFile? ProfilePicture { get; set; }

            public string? ProfilePictureUrl { get; set; }
        
    }
}
