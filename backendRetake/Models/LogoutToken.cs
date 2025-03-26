using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class LogoutToken
    {
        [Key]
        public string Token { get; set; }
    }
}
