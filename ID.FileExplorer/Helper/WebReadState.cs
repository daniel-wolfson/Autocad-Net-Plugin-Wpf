using System.IO;
using System.Net;

namespace FileExplorer.Helper
{
    class WebReadState
    {
        public WebRequest webRequest;
        public MemoryStream memoryStream;
        public Stream readStream;
        public byte[] buffer;
    }
}