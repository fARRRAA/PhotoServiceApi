using PhotoServiceApi.Models;

namespace PhotoServiceApi.Interfaces
{
    public interface IPhotoService
    {
        Task<Photo> UploadPhoto(IFormFile file);
        Task DeletePhoto(string photoId);
        Task<Photo> ReplacePhoto(string photoId, IFormFile file);
        List<Photo> GetPhotos();

    }
}
