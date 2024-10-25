using Microsoft.AspNetCore.Mvc;
using PhotoServiceApi.Interfaces;

namespace PhotoServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : Controller
    {
        private readonly IPhotoService _photoService;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPhotos()
        {
            var photos =  _photoService.GetPhotos();
            return Ok(photos);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var photo = await _photoService.UploadPhoto(file);
            return Ok(photo);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePhoto(string id)
        {
            await _photoService.DeletePhoto(id);
            return Ok();
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdatePhoto(string id, IFormFile file)
        {
            await _photoService.ReplacePhoto(id, file);
            return Ok();
        }
    }
}
