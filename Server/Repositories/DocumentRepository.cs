using Server.Data;
using Server.DTOs.Document;
using Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Server.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DatabaseContext db;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Uploads");

        public DocumentRepository(DatabaseContext db)
        {
            this.db = db;
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }

        public async Task<bool> CreateDocument(DocumentCreateDto document)
        {
            var filePath = await SaveFile(document.File);
            var newDocument = new Document
            {
                Name = document.Name,
                FilePath = filePath,
                UploadDate = DateTime.UtcNow,
                Status = document.Status
            };

            db.Documents.Add(newDocument);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<Document> GetDocumentById(int documentId)
        {
            return await db.Documents.SingleOrDefaultAsync(d => d.Id == documentId);
        }

        public async Task<(List<DocumentResponseDto> documents, int totalPages)> GetDocuments(GetDocumentRequestDto req)
        {
            try
            {
                var pageSize = 10;
                var pageNumber = req.PageNumber > 0 ? req.PageNumber : 1;
                var documents = db.Documents.AsQueryable();

                if (req.Status.HasValue)
                {
                    documents = documents.Where(d => d.Status == req.Status);
                }

                if (!string.IsNullOrEmpty(req.SearchValue))
                {
                    var searchValueLower = req.SearchValue.ToLower();
                    documents = documents.Where(d => d.Name.ToLower().Contains(searchValueLower));
                }

                var totalDocuments = await documents.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalDocuments / pageSize);

                var paginatedDocuments = await documents
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new DocumentResponseDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        FilePath = d.FilePath,
                        UploadDate = d.UploadDate,
                        Status = d.Status
                    })
                    .ToListAsync();

                return (paginatedDocuments, totalPages);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting documents: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool success, string message)> DisableDocument(int documentId)
        {
            var document = await db.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
            {
                return (false, "Document does not exist!");
            }

            document.Status = false;
            await db.SaveChangesAsync();
            return (true, "Disable document successfully!");
        }

        public async Task<DocumentResponseDto> GetDocumentByPath(int documentId)
        {
            var document = await GetDocumentById(documentId);
            if (document == null)
            {
                return null;
            }

            return new DocumentResponseDto
            {
                Id = document.Id,
                Name = document.Name,
                FilePath = Path.Combine(_uploadPath, document.FilePath),
                UploadDate = document.UploadDate,
                Status = document.Status
            };
        }
    }
}
