using System.ComponentModel.DataAnnotations;

namespace AngularAuthAPI.Models.Dto
{
    public class StudentResponseDto
    {
        [Required]
        public string Responses { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public int TemplateID { get; set; }

        [Required]
        public int StudentID { get; set; }
        [Required]
        public string Facultate { get; set; }
        [Required]
        public string An { get;set; }

    }
}
