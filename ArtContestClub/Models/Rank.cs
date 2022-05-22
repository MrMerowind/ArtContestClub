namespace ArtContestClub.Models
{
    public class Rank
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime Expires { get; set; }
        public string User { get; set; }

    }
}
