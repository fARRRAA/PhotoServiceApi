
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhotoServiceApi.dbContext;
using PhotoServiceApi.Interfaces;
using PhotoServiceApi.Models;
using System.IO;

namespace PhotoServiceApi.Services
{


    public class PhotoService : IPhotoService
    {
        private readonly string _storagePath;
        private PhotoServiceDb _context;
        public PhotoService(string storagePath, PhotoServiceDb context)
        {
            _storagePath = storagePath;
            _context = context;

            // Создание каталога, если он не существует.
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }
        public async Task<Photo> UploadPhoto(IFormFile file)
        {
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(_storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var id = $"{Guid.NewGuid()}_{fileName}";
            var photo = new Photo
            {
                Id = fileName,
                Name = fileName,
                Url = $"https://localhost:7270/photos/{fileName}",
                UploadedAt = DateTime.Now
            };
            await _context.Photos.AddAsync(photo);
            await _context.SaveChangesAsync();
            return photo;

        }

        public async Task DeletePhoto(string photoId)
        {
            var filePath = Path.Combine(_storagePath, photoId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var photo = await _context.Photos.FirstOrDefaultAsync(p=>p.Id == photoId);

            _context.Remove(photo);
            _context.SaveChanges();
        }

        public async Task<Photo> ReplacePhoto(string photoId, IFormFile file)
        {

            await DeletePhoto(photoId);
            var photo = await UploadPhoto(file);
            var temp = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId);
            temp.Url = photo.Url;
            temp.UploadedAt = DateTime.Now;
            temp.Name=photo.Name;
            temp.Id = photo.Id;
            await _context.SaveChangesAsync();
            return photo;
        }

        public List<Photo> GetPhotos()
        {
            var photos = new List<Photo>();
            var files = Directory.GetFiles(_storagePath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                photos.Add(new Photo { Id = fileName, Name = fileName, Url = $"/photos/{fileName}" });
            }

            return photos;
        }

    }
}
