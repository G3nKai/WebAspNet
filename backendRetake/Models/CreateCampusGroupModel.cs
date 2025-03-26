using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class CreateCampusGroupModel
    {
        [Required(ErrorMessage = "Campus group name is required.")]
        public string Name { get; set; }
    }
}
