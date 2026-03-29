

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using p4w.Core.Interfaces.Repositories.MediaRepo;
using p4w.Core.Interfaces.Services.Cloudinary;
using p4w.Core.Models;
using p4w.Core.Settings;

namespace p4w.Service.Services.CloudinaryService
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IMediaRepository _mediaRepository;

        public CloudinaryService(IOptions<CloudinarySetting> config, IMediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }
        }
       public async Task<Media> UploadImageAsync(IFormFile file)
{
    using var stream = file.OpenReadStream();
    var uploadParams = new ImageUploadParams
    {
        File = new FileDescription(file.FileName, stream),
        Folder = "uploads"
    };
    var result = await _cloudinary.UploadAsync(uploadParams);

    var media = new Media
    {
        Id        = Guid.NewGuid(),
        Url       = result.SecureUrl.ToString(),
        MimeType  = file.ContentType,
        Size      = file.Length,
        Status    = 1,
        CreatedAt = DateTime.UtcNow
    };

    await _mediaRepository.CreateAsync(media);

    return media;
}

        // public async Task<bool> DeleteImageAsync(string publicId)
        // {
        //     var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        //     return result.Result == "ok";
        // }
    }
}