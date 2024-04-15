using System.ComponentModel.DataAnnotations;

namespace WebEnterprise.ViewModels.Contribution
{
    public class EditContribution
    {
        [Required(ErrorMessage = "Title cannot be empty !!")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Id cannot be empty !!")]
        public int Id { get; set; }
        public int MegazineId { get; set; }
    }
}
