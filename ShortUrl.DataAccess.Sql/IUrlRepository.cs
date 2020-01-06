using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShortUrl.DataAccess.Sql
{
    public interface IUrlRepository
    {
        Task<IEnumerable<ShortUrlModel>> GetAll();

        Task<string> GetUrlAsync(string key);

        Task<ShortUrlModel> GetUrlAsync(long? id);

        Task<ShortUrlModel> AddUrl(string key, string url);

        Task DeleteUrl(long? id);
    }
}
