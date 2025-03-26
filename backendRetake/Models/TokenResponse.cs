using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class TokenResponse
    {
        [Required]
        public string Token { get; set; }
    }
}
