using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static partial class GlobalUtilities
    {
        /// <summary>
        /// Get the image bytes from a web request to an image at the provided URL
        /// </summary>
        /// <param name="url">The URL of the image</param>
        /// <returns>A byte array </returns>
        public static byte[] GetExternalImageBytes(string url)
        {
            WebResponse result = null;
            var bytes = new byte[0];

            try
            {
                WebRequest request  = WebRequest.Create(url);
                request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
                result              = request.GetResponse();
                Stream stream       = result.GetResponseStream();
                BinaryReader br     = new BinaryReader(stream);
                byte[] rBytes       = br.ReadBytes(1000000);
                br.Close();
                result.Close();
                bytes               = new MemoryStream(rBytes, 0, rBytes.Length).ToArray();
            }
            catch
            {

            }
            finally
            {
                if (result != null) result.Close();
            }

            return bytes;
        }

        /// <summary>
        /// Get the bytes from a stream
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <returns>A byte array </returns>
        public static byte[] GetBytesFromStream(Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)bytes.Length);

            return bytes;
        }

        /// <summary>
        /// Get the image bytes from a web request to an image at the provided URL
        /// </summary>
        /// <param name="url">The URL of the image</param>
        /// <returns>A byte array </returns>
        public static string GetCustomerAvatarUrl(int customerID, AvatarType type = AvatarType.Default, bool cache = true)
        {
            var cachekey = (!cache) ? "?nocache={0}".FormatWith(Guid.NewGuid()) : string.Empty;
            var filename = "avatar";
            switch (type)
            {
                case AvatarType.Tiny: filename += "-xs"; break;
                case AvatarType.Small: filename += "-sm"; break;
                case AvatarType.Large: filename += "-lg"; break;
            }

            return "http://api.exigo.com/4.0/{0}/images/customers/{1}/{2}.png{3}".FormatWith(GlobalSettings.Exigo.Api.CompanyKey, customerID, filename, cachekey);
        }

        /// <summary>
        /// Get the uncropped, raw image bytes of the customer's avatar image
        /// </summary>
        /// <param name="url">The URL of the image</param>
        /// <returns>A byte array </returns>
        public static string GetUncroppedCustomerAvatarUrl(int customerID)
        {
            var cachekey = "?nocache={0}".FormatWith(Guid.NewGuid());
            return "http://api.exigo.com/4.0/{0}/images/customers/{1}/avatar-raw.png{2}".FormatWith(GlobalSettings.Exigo.Api.CompanyKey, customerID, cachekey);
        }

        /// <summary>
        /// Provides either a Gravatar image URL or a placeholder based on the provided email address
        /// </summary>
        /// <param name="email">The email address tied to the Gravatar account</param>
        /// <param name="size">The width and height of the desired image. Default is 50.</param>
        /// <returns>The image URL.</returns>
        public static string GetGravatarUrl(string email, int size = 50)
        {
            if (string.IsNullOrEmpty(email))
            {
                return "http://placehold.it/{0}x{0}".FormatWith(size);
            }

            email = email.ToLower();
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(email.Trim()));
            var hashString = string.Join("", hash.Select(x => x.ToString("x2")));
            var imageUrl = string.Format("http://www.gravatar.com/avatar/{0}.jpg?s={1}&d=mm&r=g", hash, size);

            return imageUrl;
        }

        /// <summary>
        /// Resize an image so that the image dimensions fall between the provided maximum width and height.
        /// </summary>
        /// <param name="imageBytes">The image bytes</param>
        /// <param name="maxWidth">The maximum width of the image</param>
        /// <param name="maxHeight">The maximum height of the image</param>
        /// <returns></returns>
        public static byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            using (var ms = new MemoryStream(imageBytes))
            using (var bmp = new Bitmap(ms))
            {
                ImageFormat format = bmp.RawFormat;
                decimal ratio;
                int newWidth = 0;
                int newHeight = 0;

                if (bmp.Width > maxWidth || bmp.Height > maxHeight)
                {
                    if (bmp.Width > bmp.Height)
                    {
                        ratio = (decimal)maxWidth / bmp.Width;
                        newWidth = maxWidth;
                        decimal lnTemp = bmp.Height * ratio;
                        newHeight = (int)lnTemp;
                    }
                    else
                    {
                        ratio = (decimal)maxHeight / bmp.Height;
                        newHeight = maxHeight;
                        decimal lnTemp = bmp.Width * ratio;
                        newWidth = (int)lnTemp;
                    }
                }

                if (newWidth == 0) newWidth = bmp.Width;
                if (newHeight == 0) newHeight = bmp.Height;

                using (var bmpOut = new Bitmap(newWidth, newHeight))
                using (var msOut = new MemoryStream())
                {
                    Graphics g = Graphics.FromImage(bmpOut);

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                    g.DrawImage(bmp, 0, 0, newWidth, newHeight);

                    bmpOut.Save(msOut, ImageFormat.Jpeg);

                    return (msOut.ToArray());
                }
            }
        }

        /// <summary>
        /// Crops an image found at the provided URL to the provided dimensions, starting at the provided X and Y position.
        /// </summary>
        /// <param name="imageUrl">The URL where the image can be found at</param>
        /// <param name="width">The desired width of the cropped image</param>
        /// <param name="height">The desired height of the cropped image</param>
        /// <param name="X">The X position where the cropping will begin</param>
        /// <param name="Y">The Y position where the cropping will begin</param>
        /// <returns></returns>
        public static byte[] Crop(string imageUrl, int width, int height, int X, int Y)
        {
            // Download the image
            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(imageUrl);

            return Crop(imageBytes, width, height, X, Y);
        }

        /// <summary>
        /// Crops a provided image to the provided dimensions, starting at the provided X and Y position.
        /// </summary>
        /// <param name="imageBytes">The image to be cropped</param>
        /// <param name="width">The desired width of the cropped image</param>
        /// <param name="height">The desired height of the cropped image</param>
        /// <param name="X">The X position where the cropping will begin</param>
        /// <param name="Y">The Y position where the cropping will begin</param>
        /// <returns></returns>
        public static byte[] Crop(byte[] imageBytes, int width, int height, int X, int Y)
        {
            // Convert the bytes into an Image
            MemoryStream ms = new MemoryStream(imageBytes);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);

            using (System.Drawing.Image OriginalImage = returnImage)
            {
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height))
                {
                    bmp.SetResolution(OriginalImage.HorizontalResolution, OriginalImage.VerticalResolution);
                    using (System.Drawing.Graphics Graphic = System.Drawing.Graphics.FromImage(bmp))
                    {
                        Graphic.SmoothingMode = SmoothingMode.HighQuality;
                        Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        Graphic.DrawImage(OriginalImage, new System.Drawing.Rectangle(0, 0, width, height), X, Y, width, height, System.Drawing.GraphicsUnit.Pixel);
                        MemoryStream nms = new MemoryStream();
                        bmp.Save(nms, OriginalImage.RawFormat);
                        return nms.GetBuffer();
                    }
                }
            }
        }
    }
}