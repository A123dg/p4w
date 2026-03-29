using Microsoft.EntityFrameworkCore;
using p4w.Core.Interfaces.Repositories.MediaRepo;
using p4w.Core.Models;
using p4w.Data.Persistence;

namespace p4w.Core.Interfaces.Repositories.MediaRepo
{
    public class MediaRepository : IMediaRepository
    {
        private readonly AppDbContext _context;
        public MediaRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateAsync(Media media)
        {
            await _context.Media.AddAsync(media);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid mediaId)
        {
            var media = await _context.Media.FirstOrDefaultAsync(m => m.Id == mediaId);
            if (media != null){
                media.Status =0;
                _context.Media.Update(media);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Media> GetByIdAsync(Guid mediaId)
        {
            return await _context.Media
                .FirstOrDefaultAsync(m => m.Id == mediaId) ;
        }

          public async Task UpdateAsync(Media media)
        {
            _context.Media.Update(media);
            await _context.SaveChangesAsync();
        }
    }
}