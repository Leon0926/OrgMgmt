namespace OrgMgmt.Models
{
    /// <summary>
    /// Validates and applies a photo upload / replace / remove for a <see cref="Person"/>.
    /// </summary>
    public static class PhotoUpload
    {
        public const long MaxBytes = 4 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes =
        {
            "image/png", "image/jpeg", "image/gif", "image/webp", "image/bmp"
        };

        /// <summary>
        /// Returns an error message when the submitted file cannot be accepted, otherwise null.
        /// An absent or empty file is valid: it simply means "leave the photo alone".
        /// </summary>
        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            if (file.Length > MaxBytes)
            {
                return $"The photo must be {MaxBytes / (1024 * 1024)} MB or smaller.";
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return "The photo must be a PNG, JPEG, GIF, WebP or BMP image.";
            }

            return null;
        }

        /// <summary>
        /// Mutates <paramref name="target"/> to reflect the submitted form. Removing wins over
        /// uploading; when neither is requested the existing photo is left untouched.
        /// Call <see cref="Validate"/> first.
        /// </summary>
        public static async Task ApplyAsync(Person target, IFormFile? file, bool remove)
        {
            if (remove)
            {
                target.Photo = null;
                target.PhotoContentType = null;
                return;
            }

            if (file == null || file.Length == 0)
            {
                return;
            }

            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            target.Photo = buffer.ToArray();
            target.PhotoContentType = file.ContentType;
        }
    }
}
