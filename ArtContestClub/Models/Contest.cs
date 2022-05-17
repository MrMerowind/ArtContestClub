using System.ComponentModel.DataAnnotations;

namespace ArtContestClub.Models
{
    public class Contest
    {
        public int Id { get; set; }
        public string OwnerEmail { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsNsfw { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public string SkillLevel { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public string? FirstPlaceUserEmail { get; set; }
        public string? SecondPlaceUserEmail { get; set; }
        public string? ThirdPlaceUserEmail { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Branch { get; set; }
        public ICollection<ContestParticipant> ContestParticipants { get; set; }
        public ICollection<ContestComment> Comments { get; set; }
        public ICollection<ContestSubmission> ContestSubmissions { get; set; }
        

    }
}
