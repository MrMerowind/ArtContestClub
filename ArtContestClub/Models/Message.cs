namespace ArtContestClub.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool IsDeleted { get; set; }
    }
}
