namespace ArtContestClub.Models
{
    public class ConfigurationManager
    {
        public readonly IConfiguration Configuration;

        public ConfigurationManager(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
