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
        DbSet<Contest> Contests { get; set; }
        DbSet<ContestComment> ContestComments { get; set; }
        DbSet<ContestParticipant> ContestParticipants { get; set; }
        DbSet<ContestSubmission> ContestSubmissions { get; set; }
        DbSet<SubmissionComment> SubmissionComments { get; set; }
    }
}