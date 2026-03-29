
using p4w.Core.Models;

namespace p4w.Core.Interfaces.Repositories.MediaRepo
{
    public interface IMediaRepository
    {
        Task<Media> GetByIdAsync(Guid mediaId);
        Task CreateAsync(Media media);
        Task UpdateAsync(Media media);
        Task DeleteAsync(Guid mediaId);
    }
}