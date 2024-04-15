using System.ComponentModel.DataAnnotations;

namespace WebEnterprise.ViewModels.Contribution
{
    public class ContributionForCoo
    {
        public List<GetContributionModel> Contributions { get; set; } = new List<GetContributionModel>();
        public int MegazineId { get; set; }
        [Required(ErrorMessage = "Title cannot be empty !!")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Id cannot be empty !!")]
        public int Id { get; set; }
    }
}
