using Microsoft.AspNetCore.Mvc;
using p4w.Core.Interfaces.Services.Cloudinary;
using p4w.Core.Paginations;

namespace p4w.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadFileController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public UploadFileController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("image")]
        public async Task<ApiResponse<string>> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ApiResponse<string>
                {
                    Code = 400,
                    Success = false,
                    Message = "No file provided",
                    Data = null
                };

            var media = await _cloudinaryService.UploadImageAsync(file);
            return new ApiResponse<string>
            {
                Code = 200,
                Success = true,
                Message = "File uploaded successfully",
                Data = media.Url
            };

        // [HttpDelete("{publicId}")]
        // public async Task<IActionResult> DeleteImage(string publicId)
        // {
        //     var success = await _cloudinaryService.DeleteImageAsync(publicId);
        //     return success ? Ok("Deleted") : BadRequest("Delete failed");
        // }
    }
    }
}
