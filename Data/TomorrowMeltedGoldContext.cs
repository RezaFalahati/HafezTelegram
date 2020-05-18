using HafezTelegram.Models;
using Microsoft.EntityFrameworkCore;

namespace HafezTelegram.Data
{
    public class TomorrowMeltedGoldContext : DbContext
    {
        public TomorrowMeltedGoldContext(DbContextOptions<TomorrowMeltedGoldContext> options)
            : base(options)
        {
        }

        public DbSet<TomorrowMeltedGold> TomorrowMeltedGold { get; set; }
    }
}