using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using WebApiApi.Core;
namespace WebApi.Controllers;

[ApiVersion("2")]
[Route("carshop/v{version:apiVersion}")]

[ApiController]
public class ImagesController(
   IWebHostEnvironment webHostingEnvironment,
   ImagesRepository repository
) : ControllerBase {
   private static readonly Dictionary<string, string> MimeTypeMap = new() {
      { "image/jpeg", "jpg" },
      { "image/png", "png" },
      { "image/gif", "gif" },
      { "image/bmp", "bmp" },
      { "image/tiff", "tiff" },
      { "image/webp", "webp" }
   };

   private static string? GetExtension(string mimeType) =>
      MimeTypeMap.TryGetValue(mimeType, out var extension) ? extension : null;

   private static string? GetMimeType(string fileExt) =>
      MimeTypeMap.FirstOrDefault(x => x.Value == fileExt).Key;

   // Download an image file http://localhost:5200/carshop/v2/images/{filename}
   [HttpGet("images/{filename}")]
   [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   [ProducesDefaultResponseType]
   public async Task<IActionResult> Download(
      [FromRoute] string filename
   ) {
      var request = HttpContext.Request;
      // local folder: Environment.SpecialFolder.UserProfile/banking_files/v2/images
      var path = Path.Combine(webHostingEnvironment.WebRootPath, "images", filename);

      if (!System.IO.File.Exists(path))
         return NotFound("File not found.");

      var fileExt = Path.GetExtension(filename).Substring(1);
      var mimeType = GetMimeType(fileExt);
      var (fileBytes, contentType, name) = await repository.LoadImageFile(path, mimeType);

      var imageUrl = $"{request.Scheme}://{request.Host}{request.Path}";
      // FileContentResult(fileBytes, contentType, remoteImageUrl) 
      return File(fileBytes, contentType, imageUrl);
   }

   /// <summary>
   // Upload an image file http://localhost:5200/carshop/v2/images
   [HttpPost("images")]
   [Consumes("multipart/form-data")]
   [Produces(MediaTypeNames.Application.Json)]
   [ProducesResponseType(StatusCodes.Status201Created)]
   [ProducesResponseType(StatusCodes.Status400BadRequest)]
   [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
   [ProducesDefaultResponseType]
   public async Task<IActionResult> Upload() {
      
      var request = HttpContext.Request;

      // Check if request is multipart/form-data
      if (!request.HasFormContentType)
         return new UnsupportedMediaTypeResult();

      // Initialize a MultipartReader to read the request stream
      // Parse the boundary from the content type
      var mediaType = MediaTypeHeaderValue.Parse(request.ContentType);
      var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value; // Convert StringSegment to string
      if (string.IsNullOrEmpty(boundary))
         return new UnsupportedMediaTypeResult();

      // Create a MultipartReader to read the request stream   
      var reader = new MultipartReader(boundary, request.Body);
      var section = await reader.ReadNextSectionAsync();
      
      string? fileName = null;
      string? fileExt = null;
      byte[]? fileContent = null;
      // Read each section of the multipart form data
      while (section != null) {
         // Check if this is a file section
         var hasContentDisposition = ContentDispositionHeaderValue.TryParse(
            section.ContentDisposition, out var contentDisposition);
         if (hasContentDisposition &&
             contentDisposition.DispositionType.Equals("form-data") &&
             !string.IsNullOrEmpty(contentDisposition.FileName.Value)) {
            // This is the image part
            fileName = contentDisposition.FileName.Value;
            // Check if the file extension is valid
            fileExt = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(fileExt) || !fileExt.StartsWith("."))
               return new UnsupportedMediaTypeResult();
            // Read the file content to a byte array
            await using (var memoryStream = new MemoryStream()) {
               await section.Body.CopyToAsync(memoryStream);
               fileContent = memoryStream.ToArray(); // Read file content
            }
         }
         section = await reader.ReadNextSectionAsync();
      }
      if (fileContent == null || string.IsNullOrEmpty(fileName))
         return BadRequest("No image data found.");

      // Store image file with random name to the local file system
      var root = webHostingEnvironment.WebRootPath;
      fileName = await repository.StoreImageFile(root, fileContent, fileExt);
      if (fileName == null)
         return BadRequest("Error storing image file.");
      
      var imageUrl = $"{request.Scheme}://{request.Host}{request.Path}/{fileName}";
      var uri = new Uri(imageUrl, UriKind.Absolute);
      return Created(uri, imageUrl);
   }

   // Check if an image file exists http://localhost:5200/carshop/v2/images/exists/{filename}
   [HttpGet("images/exists/{filename}")]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public IActionResult CheckImageExists(
      [FromQuery] string filename
   ) {
      // local folder: Environment.SpecialFolder.UserProfile/banking_files/v2/images
      var path = Path.Combine(webHostingEnvironment.WebRootPath, filename);
      if (System.IO.File.Exists(path))
         return Ok(new { exists = true });

      return NotFound(new { exists = false });
   }
   
   // Delete an image file http://localhost:5200/carshop/v2/images/{filename}
   [HttpDelete("images/{filename}")] 
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   [ProducesDefaultResponseType]
   public async Task<IActionResult> Delete(
      [FromRoute] string filename
   ) {
      
      if (IsPercentEncoded(filename))
         return BadRequest("Filename is percent encoded.");
      
      // var uri = new Uri(filename);
      // var decodedPath = Uri.UnescapeDataString(uri.AbsolutePath);
      // var imageName = Path.GetFileNameWithoutExtension(decodedPath);
      // var extension = Path.GetExtension(decodedPath);
      
      var imagePath = Path.Combine(webHostingEnvironment.WebRootPath, "images", filename);
      if (!System.IO.File.Exists(imagePath))
         return NotFound("File not found.");

      System.IO.File.Delete(imagePath);
      return NoContent();
   }
   
   public static bool IsPercentEncoded(string filename) {
      var decodedFilename = Uri.UnescapeDataString(filename);
      return !string.Equals(filename, decodedFilename, StringComparison.Ordinal);
   }
}