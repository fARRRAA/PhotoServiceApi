
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

        public Stream GetPhotoByName(string name)
        {
            var filePath = Path.Combine(_storagePath, name);
            return System.IO.File.Exists(filePath) ? File.OpenRead(filePath) : null;
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
                Name = fileName,
                Url = $"https://localhost:7270/photos/{fileName}",
                UploadedAt = DateTime.Now
            };
            await _context.Photos.AddAsync(photo);
            await _context.SaveChangesAsync();
            return photo;

        }

        public async Task DeletePhoto(string name)
        {
            var filePath = Path.Combine(_storagePath, name);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var photo = await _context.Photos.FirstOrDefaultAsync(p=>p.Name == name);

            _context.Remove(photo);
            _context.SaveChanges();
        }

        public async Task<Photo> ReplacePhoto(string name, IFormFile file)
        {
            var photo = await UploadPhoto(file);
            var temp = await _context.Photos.FirstOrDefaultAsync(p => p.Name == name);
            var id = $"{Guid.NewGuid()}_{photo.Name}";
            temp.Url = photo.Url;
            temp.UploadedAt = DateTime.Now;
            temp.Name=photo.Name;
            await DeletePhoto(name);
            await _context.SaveChangesAsync();
            return photo;
        }

        public List<Photo> ALlFromFolder()
        {
            var photos = new List<Photo>();
            var files = Directory.GetFiles(_storagePath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                photos.Add(new Photo { Name = fileName, Url = $"/photos/{fileName}" });
            }

            return photos;
        }
        public  List<Photo> GetPhotos()
        {
            var photos =  _context.Photos.ToList();
            return photos;
        }
        
    }
}
