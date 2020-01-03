using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortUrl.RedirectApi.DataAccess
{
    public interface IUrlRepository
    {
        Task<string> GetUrlAsync(string key);
    }
}
