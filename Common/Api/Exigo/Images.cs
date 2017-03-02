using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Common;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static ExigoImageApi Images()
        {
            return new ExigoImageApi();
        }
    }

    public sealed class ExigoImageApi
    {
        private string LoginName = GlobalSettings.Exigo.Api.LoginName;
        private string Password = GlobalSettings.Exigo.Api.Password;

        public byte[] GetCustomerAvatar(int customerID, AvatarType type, bool cache = true)
        {
            var bytes = new byte[0];


            // Try to return the image found at the avatar path
            bytes = GlobalUtilities.GetExternalImageBytes(GlobalUtilities.GetCustomerAvatarUrl(customerID, type, cache));


            // If we didn't find anything there, convert the default image (which is Base64) to a byte array.
            // We'll use that instead
            if (bytes == null || bytes.Length == 0)
            {
                Exigo.Images().SetCustomerAvatar(customerID, Convert.FromBase64String(GlobalSettings.Avatars.DefaultAvatarAsBase64));
                return GetCustomerAvatar(customerID, type, cache);
            }

            return bytes;
        }

        public bool SaveUncroppedCustomerAvatar(int customerID, byte[] bytes)
        {
            // Define the customer profile settings
            var path = "customers/{0}".FormatWith(customerID);
            var filename = "avatar-raw.png";
            var maxWidth = 500;
            var maxHeight = 500;

            // Resize the image
            var resizedBytes = GlobalUtilities.ResizeImage(bytes, maxWidth, maxHeight);

            // Save the image
            return SaveImage(path, filename, resizedBytes);
        }
        public bool SetCustomerAvatar(int customerID, byte[] bytes, bool saveToHistory = false)
        {
            // Define the customer profile settings
            var path      = "customers/{0}".FormatWith(customerID);

            // Resize and save the images for each of the sizes
            var result = false;
            result = SaveImage(path, "avatar-lg.png", GlobalUtilities.ResizeImage(bytes, 300, 300));
            result = SaveImage(path, "avatar.png", GlobalUtilities.ResizeImage(bytes, 100, 100));
            result = SaveImage(path, "avatar-sm.png", GlobalUtilities.ResizeImage(bytes, 50, 50));
            result = SaveImage(path, "avatar-xs.png", GlobalUtilities.ResizeImage(bytes, 16, 16));

            // Save the image to the avatar history if applicable
            if (result && saveToHistory)
            {
                SaveCustomerAvatarToHistory(customerID, GlobalUtilities.ResizeImage(bytes, 300, 300));
            }

            return result;
        }
        public bool SaveCustomerAvatarToHistory(int customerID, byte[] bytes)
        {
            // Define the customer profile settings
            var path = "customers/{0}/avatars".FormatWith(customerID);
            var filename = "{0}.png".FormatWith(Path.GetRandomFileName());
            var maxWidth = 300;
            var maxHeight = 300;

            // Resize the image
            var resizedBytes = GlobalUtilities.ResizeImage(bytes, maxWidth, maxHeight);

            // Determine if this avatar has already been uploaded before by looking at it's size
            var isUnique = Exigo.OData().ImageFiles
                .Where(c => c.Path == path)
                .Where(c => c.Size == resizedBytes.Length)
                .Count() == 0;

            // Save the image
            return (isUnique) ? SaveImage(path, filename, resizedBytes) : false;
        }
        public List<string> GetCustomerAvatarHistory(int customerID)
        {
            var path = "/customers/{0}/avatars".FormatWith(customerID);
            var images = Exigo.OData().ImageFiles
                .Where(c => c.Path == path)
                .ToList();
            return images.Select(c => c.Url).ToList();
        }

        public bool SaveImage(string path, string filename, byte[] bytes)
        {
            if (path.StartsWith("/")) path = path.Remove(0, 1);
            if (path.EndsWith("/")) path = path.Remove(path.Length - 1, 1);

            var url = "http://api.exigo.com/4.0/{0}/images/{1}/{2}".FormatWith(
                GlobalSettings.Exigo.Api.CompanyKey,
                path,
                filename);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(LoginName + ":" + Password)));
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            var writer = request.GetRequestStream();
            writer.Write(bytes, 0, bytes.Length);
            writer.Close();

            var response = (HttpWebResponse)request.GetResponse();

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}