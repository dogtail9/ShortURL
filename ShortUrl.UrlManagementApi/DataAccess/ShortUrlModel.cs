using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShortUrl.UrlManagementApi.DataAccess
{
    public class ShortUrlModel
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(10)]
        public string Key { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Url { get; set; }
    }
}
