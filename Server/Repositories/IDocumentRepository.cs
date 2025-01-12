using Server.DTOs.Document;
using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Repositories
{
    public interface IDocumentRepository
    {
        Task<bool> CreateDocument(DocumentCreateDto document);
        Task<Document> GetDocumentById(int documentId);
        Task<(List<DocumentResponseDto> documents, int totalPages)> GetDocuments(GetDocumentRequestDto req);
        Task<(bool success, string message)> DisableDocument(int documentId);
        Task<DocumentResponseDto> GetDocumentByPath(int documentId); 
    }
}
