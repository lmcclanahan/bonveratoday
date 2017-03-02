using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public interface IMediaFile
    {
        string FileName { get; set; }
        string Url { get; set; }
    }
}
