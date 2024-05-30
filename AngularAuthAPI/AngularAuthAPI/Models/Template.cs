using System.ComponentModel.DataAnnotations;

namespace AngularAuthAPI.Models
{
    public class Template
    {
        [Key]
        public int templateID { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string[] Specializations { get; set; }
        public string[] Fields { get; set; }

    }
}
