using Microsoft.AspNetCore.Http;
using p4w.Core.Models;

namespace p4w.Core.Interfaces.Services.Cloudinary;
public interface ICloudinaryService
{
    Task<Media> UploadImageAsync(IFormFile file);
}