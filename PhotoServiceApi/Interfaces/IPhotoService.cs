using Microsoft.AspNetCore.Mvc;
using PhotoServiceApi.Models;

namespace PhotoServiceApi.Interfaces
{
    public interface IPhotoService
    {
        Task<Photo> UploadPhoto(IFormFile file);
        Task DeletePhoto(string name);
        Task<Photo> ReplacePhoto(string name, IFormFile file);
        List<Photo> GetPhotos();
        Stream GetPhotoByName(string name);
        public List<Photo> ALlFromFolder();
    }
}
