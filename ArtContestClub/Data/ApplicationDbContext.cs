using ArtContestClub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArtContestClub.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<ContestComment> ContestComments { get; set; }
        public DbSet<ContestParticipant> ContestParticipants { get; set; }
        public DbSet<ContestSubmission> ContestSubmissions { get; set; }
        public DbSet<SubmissionComment> SubmissionComments { get; set; }
        public DbSet<AboutMe> AboutMe { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Rank> Ranks { get; set; }
    }
}