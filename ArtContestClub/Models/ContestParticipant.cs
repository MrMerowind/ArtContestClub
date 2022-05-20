namespace ArtContestClub.Models
{
    public class ContestParticipant
    {
        public int Id { get; set; }
        public string UserIdentity { get; set; }
        public int? ContestId { get; set; }
        public Contest? Contest { get; set; }
    }
}
