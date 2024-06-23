using AngularAuthAPI.Context;
using AngularAuthAPI.Models;
using AngularAuthAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using Mammoth;
using System.Text.Json;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
 
        private readonly AppDbContext _context;

        public TemplateController(IWebHostEnvironment environment, AppDbContext context)
        {
            _context = context;

        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTemplate([FromForm] TemplateDto templateDto)
        {
            if (templateDto == null || templateDto.File == null)
                return BadRequest();
            if (Path.GetExtension(templateDto.File.FileName).ToLower() != ".docx")
                return BadRequest(new { Message = "Only .docx files are allowed." });
            var specializationsArray = templateDto.Specializations.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var fieldsArray = templateDto.Fields;
            var template = new Template
            {
                Name = templateDto.Name,
                Specializations = specializationsArray,
                Fields = fieldsArray,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
            };
            var uploadsFolderPath = Path.Combine("uploads", "templates");
            if (!Directory.Exists(uploadsFolderPath))
                Directory.CreateDirectory(uploadsFolderPath);
            var fileName = $"{templateDto.Name}.docx";
            var filePath = Path.Combine(uploadsFolderPath, fileName);
            int counter = 1;
            while (System.IO.File.Exists(filePath))
            {
                fileName = $"{templateDto.Name}_{counter}.docx";
                filePath = Path.Combine(uploadsFolderPath, fileName);
                counter++;
            }
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await templateDto.File.CopyToAsync(stream);
            }
            template.FilePath = filePath;
            await _context.Templates.AddAsync(template);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Template created successfully!" });
        }

        [HttpGet]
        public async Task<ActionResult<Template>> GetAllTemplates()
        {
            return Ok(await _context.Templates.ToListAsync());
        }

        [HttpGet("download/{fileName}")]
        public IActionResult DownloadTemplate(string fileName)
        {
            var uploadsRootFolderPath = @"D:\UPT\Licenta\angular_auth\AngularAuthAPI\AngularAuthAPI\uploads\templates";
            var fullPath = Path.Combine(uploadsRootFolderPath, fileName);
            if (!Directory.Exists(uploadsRootFolderPath))
            {
                return NotFound(new { Message = "Directory does not exist." });
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { Message = "File does not exist." });
            }

            try
            {
                var bytes = System.IO.File.ReadAllBytes(fullPath);
                return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error reading file." });
            }
        }

        [HttpGet("view/{fileName}")]
        public async Task<IActionResult> ViewTemplate(string fileName)
        {
            var uploadsRootFolderPath = @"D:\UPT\Licenta\angular_auth\AngularAuthAPI\AngularAuthAPI\uploads\templates";
            var filePath = Path.Combine(uploadsRootFolderPath, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { Message = "File not found." });
            }
            try
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string htmlContent;
                using (var stream = new MemoryStream(fileBytes))
                {
                    var converter = new DocumentConverter();
                    var result = converter.ConvertToHtml(stream);
                    htmlContent = result.Value;
                }
                return Ok(new { Content = htmlContent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error reading file." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplateById(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound(new { Message = "Template not found." });
            }
            return Ok(template);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromForm] TemplateUpdateModel updateModel)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound(new { Message = "Template not found." });
            }
            if (updateModel.File != null)
            {
                if (Path.GetExtension(updateModel.File.FileName).ToLower() != ".docx")
                    return BadRequest(new { Message = "Only .docx files are allowed." });
                var uploadsFolderPath = Path.Combine("uploads", "templates");
                if (!Directory.Exists(uploadsFolderPath))
                    Directory.CreateDirectory(uploadsFolderPath);
                var fileName = $"{updateModel.Name}.docx";
                var filePath = Path.Combine(uploadsFolderPath, fileName);
                int counter = 1;
                while (System.IO.File.Exists(filePath))
                {
                    fileName = $"{updateModel.Name}_{counter}.docx";
                    filePath = Path.Combine(uploadsFolderPath, fileName);
                    counter++;
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updateModel.File.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(template.FilePath) && System.IO.File.Exists(template.FilePath))
                {
                    System.IO.File.Delete(template.FilePath);
                }
                template.FilePath = filePath;
            }
            else if (template.Name != updateModel.Name)
            {
                var uploadsFolderPath = Path.Combine("uploads", "templates");
                var newFileName = $"{updateModel.Name}{Path.GetExtension(template.FilePath)}";
                var newFilePath = Path.Combine(uploadsFolderPath, newFileName);
                if (System.IO.File.Exists(template.FilePath) && template.FilePath != newFilePath)
                {
                    System.IO.File.Move(template.FilePath, newFilePath);
                    template.FilePath = newFilePath;
                }
            }
            template.Name = updateModel.Name;
            template.Specializations = updateModel.Specializations;
            template.Fields = updateModel.Fields;
            template.UpdateDate = DateTime.UtcNow;
            _context.Templates.Update(template);
            await _context.SaveChangesAsync();
            return Ok(template);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound(new { Message = "Template not found." });
            }
            if (!string.IsNullOrEmpty(template.FilePath) && System.IO.File.Exists(template.FilePath))
            {
                System.IO.File.Delete(template.FilePath);
            }
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Template deleted successfully!" });
        }

        [HttpGet("{id}/fields")]
        public async Task<IActionResult> GetFieldsByTemplateId(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound(new { Message = "Template not found." });
            }

            
            return Ok(new { Fields = template.Fields });
        }

    }
}
