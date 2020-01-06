using ShortUrl.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortUrl.RedirectApi.DataAccess
{
    public class InMemoryUrlRepository : IUrlRepository
    {
        private const string KeyNotFound = "Key not found.";

        public Task<ShortUrlModel> AddUrl(string key, string url)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUrl(long? id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ShortUrlModel>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetUrlAsync(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (key == "ms")
                return "http://www.microsoft.com";

            if (key == "ibm")
                return "http://www.ibm.com";

            throw new ArgumentException(KeyNotFound);
        }

        public Task<ShortUrlModel> GetUrlAsync(long? id)
        {
            throw new NotImplementedException();
        }
    }
}
