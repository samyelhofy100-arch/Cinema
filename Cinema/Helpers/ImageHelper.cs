
using Microsoft.AspNetCore.Http;

namespace Cinema.Helpers
{
    public static class ImageHelper
    {
        private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public const long MaxBytes = 5 * 1024 * 1024;

        public static bool IsValid(IFormFile? file, out string error)
        {
            error = string.Empty;

            if (file == null || file.Length == 0)
            {
                error = "The file is empty.";
                return false;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Allowed.Contains(ext))
            {
                error = "Only JPG, PNG, WEBP or GIF images are allowed.";
                return false;
            }

            if (file.Length > MaxBytes)
            {
                error = "Each image must be 5 MB or smaller.";
                return false;
            }

            return true;
        }

        public static string Save(IFormFile file, string webRoot, string folder)
        {
            var dir = Path.Combine(webRoot, "Images", folder);
            Directory.CreateDirectory(dir);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var name = Guid.NewGuid().ToString("N") + ext;

            using var stream = File.Create(Path.Combine(dir, name));
            file.CopyTo(stream);

            return name;
        }

        public static void Delete(string webRoot, string folder, string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var safeName = Path.GetFileName(fileName);
            var path = Path.Combine(webRoot, "Images", folder, safeName);

            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // Ignore file deletion errors
                }
            }
        }
    }
}
