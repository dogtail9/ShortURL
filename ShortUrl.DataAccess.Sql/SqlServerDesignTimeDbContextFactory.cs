using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShortUrl.DataAccess.Sql
{
    public class SqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<UrlDbContext>
    {
        public UrlDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Server = localhost; Database = ShortUrl; User Id = sa; Password = P@ssw0rd;";
            var optionsBuilder = new DbContextOptionsBuilder<UrlDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new UrlDbContext(optionsBuilder.Options);
        }
    }
}
