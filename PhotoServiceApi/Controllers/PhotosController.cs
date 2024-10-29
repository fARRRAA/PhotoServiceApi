﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoServiceApi.Interfaces;
using PhotoServiceApi.Models;

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
        [Authorize(Roles = "admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetPhotos()
        {
            var photos =  _photoService.GetPhotos();
            return Ok(photos);
        }
        [Authorize]

        [HttpGet("photo/{name}")]
        public async Task<IActionResult> getPhotoByName(string name)
        {
           Stream stream = _photoService.GetPhotoByName(name);
            return File(stream, "image/png");

        }
        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm]IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var photo = await _photoService.UploadPhoto(file);
            return Ok(photo);
        }
        [Authorize]
        [HttpDelete("delete/{name}")]
        public async Task<IActionResult> DeletePhoto(string name)
        {
            await _photoService.DeletePhoto(name);
            return Ok();
        }
        [Authorize]

        [HttpPut("update/{name}")]
        public async Task<IActionResult> UpdatePhoto(string name, IFormFile file)
        {
            await _photoService.ReplacePhoto(name, file);
            return Ok();
        }
        [Authorize(Roles = "admin")]

        [HttpGet("allFromFolder")]
        public async Task<IActionResult> AllFromFolder()
        {
            var photos = _photoService.ALlFromFolder();
            return Ok(photos);
        }
    }
}
