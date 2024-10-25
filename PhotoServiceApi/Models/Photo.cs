using System.ComponentModel.DataAnnotations;

namespace PhotoServiceApi.Models
{
    public class Photo
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
