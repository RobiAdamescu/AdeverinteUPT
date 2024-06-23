using AngularAuthAPI.Context;
using AngularAuthAPI.Models;
using AngularAuthAPI.Models.Dto;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Mammoth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xceed.Words.NET;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentResponseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentResponseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudentResponse([FromBody] StudentResponseDto studentResponseDto)
        {
            try
            {
                var student = await _context.Users.FindAsync(studentResponseDto.StudentID);
                if (student == null)
                {
                    return NotFound(new { Message = "Student not found." });
                }
                    if (studentResponseDto == null)
                {
                    return BadRequest(new { Message = "Request data is null." });
                }
                var template = await _context.Templates.FindAsync(studentResponseDto.TemplateID);
                if (template == null)
                {
                    return NotFound(new { Message = "Template not found." });
                }
                var responses = studentResponseDto.Responses;
                var filePath = GenerateFilePath(template, studentResponseDto);
                var studentResponse = new StudentResponse
                {
                    Responses = responses,
                    Reason = studentResponseDto.Reason,
                    FilePath = filePath,
                    ResponseDate = DateTime.UtcNow,
                    Status = "In asteptare",
                    TemplateID = studentResponseDto.TemplateID,
                    StudentID = studentResponseDto.StudentID,
                    TemplateName = template.Name,
                    Facultate = student.Facultate, 
                    An = student.An, 
                };
                await _context.StudentResponses.AddAsync(studentResponse);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Student response created successfully!" });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

        private string GenerateFilePath(Template template, StudentResponseDto studentResponseDto)
        {
            var uploadsFolderPath = Path.Combine("uploads", "adeverinte_completate");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var fileName = $"{template.Name}_{studentResponseDto.StudentID}_{DateTime.UtcNow.Ticks}.docx";
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var document = DocX.Load(template.FilePath))
            {
                var responses = JsonSerializer.Deserialize<Dictionary<string, string>>(studentResponseDto.Responses);

                foreach (var response in responses)
                {
                    document.ReplaceText($"{{{response.Key}}}", response.Value);
                }

                document.SaveAs(filePath);
            }

            return filePath;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentResponsesByStudentId(int studentId)
        {
            var studentResponses = await _context.StudentResponses
                .Where(sr => sr.StudentID == studentId)
                .ToListAsync();

            if (studentResponses == null || studentResponses.Count == 0)
            {
                return NotFound(new { Message = "No responses found for this student." });
            }

            return Ok(studentResponses);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentResponse(int id)
        {
            var studentResponse = await _context.StudentResponses.FindAsync(id);

            if (studentResponse == null)
            {
                return NotFound(new { Message = "Student response not found." });
            }

            _context.StudentResponses.Remove(studentResponse);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Student response deleted successfully." });
        }

        [HttpGet("view/{fileName}")]
        public async Task<IActionResult> ViewCompletedCertificate(string fileName)
        {
            var uploadsRootFolderPath = @"D:\UPT\Licenta\angular_auth\AngularAuthAPI\AngularAuthAPI\uploads\adeverinte_completate";
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

        [HttpGet("download/{fileName}")]
        public IActionResult DownloadCompletedCertificate(string fileName)
        {
            var uploadsRootFolderPath = @"D:\UPT\Licenta\angular_auth\AngularAuthAPI\AngularAuthAPI\uploads\adeverinte_completate";
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudentResponse(int id, [FromForm] StudentResponseUpdateModel updateModel)
        {
            var studentResponse = await _context.StudentResponses.FindAsync(id);
            if (studentResponse == null)
            {
                return NotFound(new { Message = "Student response not found." });
            }
            var responsesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(updateModel.Responses);
            if (responsesDict == null)
            {
                return BadRequest(new { Message = "Invalid responses format." });
            }
            var template = await _context.Templates.FindAsync(studentResponse.TemplateID);
            if (template == null)
            {
                return NotFound(new { Message = "Template not found." });
            }
            var uploadsFolderPath = Path.Combine("uploads", "adeverinte_completate");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var newFileName = $"{template.Name}_{studentResponse.StudentID}_{DateTime.UtcNow.Ticks}.docx";
            var newFilePath = Path.Combine(uploadsFolderPath, newFileName);

            try
            {
                using (var templateDoc = WordprocessingDocument.Open(template.FilePath, false))
                {
                    using (var newDoc = WordprocessingDocument.Create(newFilePath, WordprocessingDocumentType.Document))
                    {
                        foreach (var part in templateDoc.Parts)
                        {
                            newDoc.AddPart(part.OpenXmlPart, part.RelationshipId);
                        }

                        var body = newDoc.MainDocumentPart.Document.Body;
                        foreach (var entry in responsesDict)
                        {
                            var key = entry.Key;
                            var value = entry.Value;
                            foreach (var text in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                            {
                                if (text.Text.Contains($"{{{key}}}"))
                                {
                                    text.Text = text.Text.Replace($"{{{key}}}", value);
                                }
                            }
                        }
                        newDoc.MainDocumentPart.Document.Save();
                    }
                }
            }


            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error creating new DOCX file: {ex.Message}" });
            }
            studentResponse.Responses = updateModel.Responses;
            studentResponse.Reason = updateModel.Reason;
            studentResponse.FilePath = newFilePath;
            _context.StudentResponses.Update(studentResponse);
            await _context.SaveChangesAsync();
            return Ok(studentResponse);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllStudentResponses()
        {
            var responses = await _context.StudentResponses.ToListAsync();
            return Ok(responses);
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudentDetails()
        {
            var studentDetails = await _context.Users
                .Select(u => new
                {
                    u.userID,
                    StudentName = u.Nume,
                    StudentFaculty = u.Facultate,
                    StudentSpecialization = u.Specializare
                })
                .ToListAsync();

            return Ok(studentDetails);
        }

        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptStudentResponse(int id)
        {
            var studentResponse = await _context.StudentResponses.FindAsync(id);
            if (studentResponse == null)
            {
                return NotFound(new { Message = "Student response not found." });
            }
            studentResponse.Status = "Acceptat";
            await _context.SaveChangesAsync();
            return Ok(studentResponse);
        }

        [HttpPut("{id}/refuse")]
        public async Task<IActionResult> RefuseStudentResponse(int id)
        {
            var studentResponse = await _context.StudentResponses.FindAsync(id);
            if (studentResponse == null)
            {
                return NotFound(new { Message = "Student response not found." });
            }
            studentResponse.Status = "Refuzat";
            await _context.SaveChangesAsync();
            return Ok(studentResponse);
        }

        [HttpPut("{id}/revert")]
        public async Task<IActionResult> RevertStudentResponse(int id)
        {
            var studentResponse = await _context.StudentResponses.FindAsync(id);
            if (studentResponse == null)
            {
                return NotFound(new { Message = "Student response not found." });
            }
            studentResponse.Status = "In asteptare";
            await _context.SaveChangesAsync();
            return Ok(studentResponse);
        }


    }
}

   
