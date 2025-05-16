using System.ComponentModel.DataAnnotations;

namespace EmailSenderAPI.Models
{
    public class EmailDto
    {
        [Required]
        public string To { get; set; } = string.Empty;
        [Required]
        public string ContactName { get; set; } = string.Empty;
        [Required]
        public string Body { get; set; } = string.Empty;
    }
}