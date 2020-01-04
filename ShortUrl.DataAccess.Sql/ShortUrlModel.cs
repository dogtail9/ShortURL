using System.ComponentModel.DataAnnotations;

namespace ShortUrl.DataAccess.Sql
{
    public class ShortUrlModel
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Key { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Url { get; set; }
    }
}
