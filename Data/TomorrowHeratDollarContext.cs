using HafezTelegram.Models;
using Microsoft.EntityFrameworkCore;

namespace HafezTelegram.Data
{
    public class TomorrowHeratDollarContext : DbContext
    {
        public TomorrowHeratDollarContext(DbContextOptions<TomorrowHeratDollarContext> options)
            : base(options)
        {
        }

        public DbSet<TomorrowHeratDollar> TomorrowHeratDollar { get; set; }
    }
}