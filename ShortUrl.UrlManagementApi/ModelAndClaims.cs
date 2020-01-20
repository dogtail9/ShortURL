using ShortUrl.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ShortUrl.UrlManagementApi.Controllers.ManagementController;

namespace ShortUrl.UrlManagementApi
{
    public class ModelsAndClaims
    {
        public IEnumerable<ShortUrlModel> ShortUrlModels { get; set; }
        public IEnumerable<MyClaim> MyClaims { get; set; }
    }
}
