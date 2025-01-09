using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DatabaseContext _context;

        public CommentRepository(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<Comment>> GetCommentsByReportIdAsync(int reportId)
        {
            return await _context.Comments
                .Where(c => c.ReportId == reportId)
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<int> GetCommentsCountByReportIdAsync(int reportId)
        {
            return await _context.Comments
                .Where(c => c.ReportId == reportId)
                .CountAsync();
        }
    }
}
