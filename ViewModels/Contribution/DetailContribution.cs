﻿using WebEnterprise.ViewModels.Imgae;

namespace WebEnterprise.ViewModels.Contribution
{
    public class DetailContribution
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string FullName { get; set; }
        public string ProfilePicture { get; set; }
        public int numberContribution { get; set; }
        public string UserId { get; set; }
        public DateTime EndSemesterDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> imagePaths { get; set; }

        public CreateImage CreateImage { get; set; }
    }
}
