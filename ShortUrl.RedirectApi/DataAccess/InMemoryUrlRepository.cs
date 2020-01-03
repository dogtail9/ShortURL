using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortUrl.RedirectApi.DataAccess
{
    public class InMemoryUrlRepository : IUrlRepository
    {
        private const string KeyNotFound = "Key not found.";

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
    }
}
