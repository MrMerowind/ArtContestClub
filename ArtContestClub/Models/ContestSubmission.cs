namespace ArtContestClub.Models
{
    public class ContestSubmission
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Title { get; set; }
        public string? ArtLink { get; set; }
        public int? ContestId { get; set; }
        public Contest? Contest { get; set; }
        public DateTime? Submited { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsBanned { get; set; }
        public ICollection<SubmissionComment>? SubmissionComments { get; set; }
    }
}
