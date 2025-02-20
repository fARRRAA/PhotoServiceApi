
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhotoServiceApi.dbContext;
using PhotoServiceApi.Interfaces;
using PhotoServiceApi.Models;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.AspNetCore.WebUtilities;
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

            if (!System.IO.File.Exists(filePath))
            {
                return null; // Возвращаем null, если файл не существует
            }

            return System.IO.File.OpenRead(filePath); // Открываем файл для чтения
        }

        public async Task<Photo> UploadPhoto(IFormFile file)
        {
            // Разрешенные расширения файлов
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".webp", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            // Проверка расширения файла
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"Недопустимое расширение файла: {fileExtension}");
            }

            // Проверка размера файла
            if (file.Length > 5 * 1024 * 1024) // 5 MB
            {
                throw new ArgumentException("Файл слишком большой.");
            }

            // Проверка пустого файла
            if (file.Length == 0)
            {
                throw new ArgumentException("Файл пустой.");
            }

            // Путь к файлу
            var fileName = Path.GetFileNameWithoutExtension(file.FileName); // Имя без расширения
            var originalExtension = Path.GetExtension(file.FileName).ToLower();
            var newFileName = $"{fileName}.webp"; // Новое имя файла с расширением .webp
            var filePath = Path.Combine(_storagePath, newFileName);

            // Проверка существования директории
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            Console.WriteLine($"Сохраняем файл: {filePath}");

            // Конвертация в .webp, если файл не в формате .webp
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream); // Копируем содержимое файла в MemoryStream
                memoryStream.Position = 0; // Сбрасываем позицию потока

                if (originalExtension != ".webp")
                {
                    // Конвертируем изображение в .webp
                    using (var image = await SixLabors.ImageSharp.Image.LoadAsync(memoryStream)) // Используем MemoryStream напрямую
                    {
                        using (var webpStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.SaveAsync(webpStream, new WebpEncoder());
                        }
                    }
                }
                else
                {
                    // Если файл уже в формате .webp, просто сохраняем его
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await memoryStream.CopyToAsync(fileStream);
                    }
                }
            }

            // Проверка успешности сохранения
            if (File.Exists(filePath))
            {
                Console.WriteLine($"Файл успешно сохранен: {filePath}");
            }
            else
            {
                throw new InvalidOperationException("Не удалось сохранить файл.");
            }

            // Создание объекта Photo
            var id = $"{Guid.NewGuid()}_{newFileName}";
            var photo = new Photo
            {
                Name = newFileName,
                Url = $"https://localhost:7270/api/Photos/photo/{newFileName}",
                UploadedAt = DateTime.Now
            };

            // Проверка, что файл еще не существует в базе данных
            var all = _context.Photos.ToList();
            if (!all.Any(x => x.Name == photo.Name))
            {
                await _context.Photos.AddAsync(photo);
                await _context.SaveChangesAsync();
            }

            return photo;
        }
        public async Task DeletePhoto(string name)
        {
            var filePath = Path.Combine(_storagePath, name);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Name == name);

            _context.Remove(photo);
            _context.SaveChanges();
        }

        public async Task<Photo> ReplacePhoto(string name, IFormFile file)
        {
            var newPhoto = await UploadPhoto(file);

            if (newPhoto == null)
            {
                throw new InvalidOperationException("Не удалось загрузить новый файл.");
            }

            var oldPhoto = await _context.Photos.FirstOrDefaultAsync(p => p.Name == name);

            if (oldPhoto != null)
            {
                var filePath = Path.Combine(_storagePath, oldPhoto.Name);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.Photos.Remove(oldPhoto);
            }

            await _context.SaveChangesAsync();
            return newPhoto;
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
        public List<Photo> GetPhotos()
        {
            var photos = _context.Photos.ToList();
            return photos;
        }

    }
}
