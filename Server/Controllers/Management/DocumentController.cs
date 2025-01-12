using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.DTOs.Document;
using Server.Repositories;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/management/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository documentRepo;

        public DocumentController(IDocumentRepository documentRepo)
        {
            this.documentRepo = documentRepo;
        }

        [HttpPost("create-document")]
        public async Task<ActionResult> CreateDocument([FromForm] DocumentCreateDto document)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(document.Name))
            {
                errors["name"] = "Name is required";
            }

            if (document.File == null)
            {
                errors["file"] = "File is required";
            }
            else if (document.File.ContentType != "application/pdf")
            {
                errors["file"] = "Invalid file format. Only PDF is allowed.";
            }

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = "Invalid document information! Please check the errors of the fields again."
                });
            }

            try
            {
                var result = await documentRepo.CreateDocument(document);
                if (!result)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Create document failed!"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Document created successfully!"
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating document: {ex.Message}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while creating the document."
                });
            }
        }

        [HttpPost("get-documents")]
        public async Task<ActionResult> GetDocuments(GetDocumentRequestDto req)
        {
            req.Status = true;
            var (documents, totalPages) = await documentRepo.GetDocuments(req);

            return Ok(new
            {
                Success = true,
                Documents = documents,
                TotalPages = totalPages
            });
        }

        [HttpPut("disable-document/{id}")]
        public async Task<ActionResult> DisableDocument(int id)
        {

            var (result, message) = await documentRepo.DisableDocument(id);
            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = message
                });
            }

            return Ok(new
            {
                Success = true,
                Message = message
            });
        }

        [HttpGet("download-document/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await documentRepo.GetDocumentByPath(id);
            if (document == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Document not found!"
                });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
            var contentType = "application/pdf"; // Thiết lập kiểu nội dung là PDF
            var fileName = $"{document.Name}.pdf"; // Tên tệp tải về

            return File(fileBytes, contentType, fileName);
        }

    }
}
