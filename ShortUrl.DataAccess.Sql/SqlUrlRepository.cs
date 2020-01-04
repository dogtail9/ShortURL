using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortUrl.DataAccess.Sql
{
    public class SqlUrlRepository : IUrlRepository
    {
        private const string KeyNotFound = "Key not found.";
        private const string AgumentNeedsToHaveAValue = "Argument needs to have a value.";

        private readonly UrlDbContext _context;

        public SqlUrlRepository(UrlDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ShortUrlModel>> GetAll()
        {
            var shortUrlModels = await _context.ShortUrl.ToListAsync();

            return shortUrlModels;
        }

        public async Task<ShortUrlModel> AddUrl(string key, string url)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(AgumentNeedsToHaveAValue, nameof(key));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException(AgumentNeedsToHaveAValue, nameof(url));

            var shortUrlModel = new ShortUrlModel { Key = key, Url = url };
            _context.Add(shortUrlModel);
            await _context.SaveChangesAsync();

            return shortUrlModel;
        }

        public async Task DeleteUrl(long? id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var shortUrlModel = await _context.ShortUrl.FirstOrDefaultAsync(m => m.Id == id);
            
            if (shortUrlModel is null)
                throw new ArgumentException(KeyNotFound);

            _context.ShortUrl.Remove(shortUrlModel);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetUrlAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var shortUrlModel = await _context.ShortUrl.FirstOrDefaultAsync(m => m.Key == key);

            if(shortUrlModel is null)
                throw new ArgumentException(KeyNotFound);

            return shortUrlModel.Url;
        }

        public async Task<string> GetUrlAsync(long? id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var shortUrlModel = await _context.ShortUrl.FirstOrDefaultAsync(m => m.Id == id);

            if (shortUrlModel is null)
                throw new ArgumentException(KeyNotFound);

            return shortUrlModel.Url;
        }
    }
}
