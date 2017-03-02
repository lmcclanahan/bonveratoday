using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class CustomerImage : IMediaFile
    {
        public int CustomerID { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl
        {
            get 
            {
                var urlParts = this.Url.Split('/');
                var path = string.Join("/", urlParts.Take(urlParts.Length - 1));
                return string.Format("{0}/thumbs/{1}", path, this.FileName);
            }
        }
        public int FileSize { get; set; }
    }
}
