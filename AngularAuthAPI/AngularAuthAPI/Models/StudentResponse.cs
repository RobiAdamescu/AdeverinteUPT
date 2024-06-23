using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AngularAuthAPI.Models
{
    public class StudentResponse
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Responses { get; set; } 
        [Required]
        public string Reason { get; set; }
        public string TemplateName { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]
        public DateTime ResponseDate { get; set; }
        [Required]
        public string Status { get; set; } = "In asteptare";
        [ForeignKey("Template")]
        public int TemplateID { get; set; }
        public Template Template { get; set; }
        [ForeignKey("User")]
        public int StudentID { get; set; }
        public string Facultate { get; set; }
        public string An { get; set; }
        public User Student { get; set; }

    }
}
