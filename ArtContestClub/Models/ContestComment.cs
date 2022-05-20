namespace ArtContestClub.Models
{
    public class ContestComment
    {
        public int Id { get; set; }
        public string UserIdentity { get; set; }
        public string Content { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsBanned { get; set; }
        public int? ContestId { get; set; }
        public Contest? Contest { get; set; }
    }
}