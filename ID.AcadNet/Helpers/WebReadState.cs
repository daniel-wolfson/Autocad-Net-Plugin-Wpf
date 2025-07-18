using System.IO;
using System.Net;

namespace Intellidesk.AcadNet.Helpers
{
    class WebReadState
    {
        public WebRequest webRequest;
        public MemoryStream memoryStream;
        public Stream readStream;
        public byte[] buffer;
    }
}