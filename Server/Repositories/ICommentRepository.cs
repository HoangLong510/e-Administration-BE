using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByReportIdAsync(int reportId);
        Task<int> GetCommentsCountByReportIdAsync(int reportId);
    }
}
