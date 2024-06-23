namespace AngularAuthAPI.Models.Dto
{
    public class TemplateUpdateModel
    {
        public string Name { get; set; }
        public string[] Specializations { get; set; }
        public IFormFile File { get; set; }
        public string[] Fields { get; set; }
    }
}
