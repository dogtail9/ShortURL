using Microsoft.EntityFrameworkCore;

namespace ShortUrl.UrlManagementApi.DataAccess
{
    public class UrlDbContext : DbContext
    {
        public UrlDbContext(DbContextOptions<UrlDbContext> options)
            : base(options)
        {
        }

        public DbSet<ShortUrlModel> ShortUrl { get; set; }
    }
}
