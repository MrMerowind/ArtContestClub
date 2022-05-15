namespace ArtContestClub.Models
{
    public class ContestParticipant
    {
        public int Id { get; set; }
        public string? ParticipantEmail { get; set; }
        public int? ContestId { get; set; }
        public Contest? Contest { get; set; }
    }
}
