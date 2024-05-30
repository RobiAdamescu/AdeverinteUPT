using AngularAuthAPI.Context;
using AngularAuthAPI.Models;
using AngularAuthAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TemplateController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTemplate([FromForm] TemplateDto templateDto)
        {
            if (templateDto == null || templateDto.File == null)
                return BadRequest();

            // Validare extensie fișier
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

            var filePath = Path.Combine(uploadsFolderPath, templateDto.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await templateDto.File.CopyToAsync(stream);
            }

            template.FilePath = filePath;

            // Aici poți adăuga codul pentru a utiliza docxtemplater pentru a manipula fișierul docx, dacă este necesar

            await _context.Templates.AddAsync(template);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Template created successfully!" });
        }
    }
}
