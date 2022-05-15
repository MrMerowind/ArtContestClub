namespace ArtContestClub.Models
{
    public class SubmissionComment
    {
        public int Id { get; set; }
        public string? OwnerEmail { get; set; }
        public string? Content { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsBanned { get; set; }
        public int? ContestSubmissionId { get; set; }
        public ContestSubmission? ContestSubmission { get; set; }
    }
}
