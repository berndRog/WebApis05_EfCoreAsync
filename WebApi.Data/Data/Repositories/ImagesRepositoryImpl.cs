using System.IO;
using System.Threading.Tasks;
using PeopleApi.Core;
namespace PeopleApi.Persistence.Repositories;

internal class ImagesRepositoryImpl() : ImagesRepository {
    
    public async Task<(byte[], string, string)> LoadImageFile(
        string filePath, 
        string contentType
    ) {
        var readAllBytesAsync = await File.ReadAllBytesAsync(filePath);
        return (readAllBytesAsync, contentType, Path.GetFileName(filePath));
    }
    
    public async Task<string?> StoreImageFile(
        string root,
        byte[] fileContent, // image as byte array
        string fileExt      // image file extension
    ) {
        // combine a random file name with the given file extension
        var fileName = Path.GetRandomFileName();
        fileName = Path.ChangeExtension(fileName, fileExt);
        
        // ensure that the folder for the images exits
        var path = Path.Combine(root, "images");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        // save the image file to the local file system
        var fullPath = Path.Combine(path, fileName);
        await File.WriteAllBytesAsync(fullPath, fileContent);
        return fileName;
    }
}