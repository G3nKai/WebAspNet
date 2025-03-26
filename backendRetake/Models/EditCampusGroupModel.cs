using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class EditCampusGroupModel
    {
        [Required(ErrorMessage = "Campus group name is required.")]
        public string Name { get; set; }
    }
}
