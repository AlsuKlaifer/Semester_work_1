using System.Net;

namespace ORIS.week10
{
	public class Files
	{
        public static void GetExtension(ref HttpListenerResponse response, string path)
        {
            if (Directory.Exists(path))
                path += "/index.html";

            response.ContentType = Path.GetExtension(path) switch
            {
                ".html" => "text/html",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".svg" => "image/svg+xml",
                ".gif" => "image/gif",
                ".js" => "text/javascript",
                ".css" => "text/css",
                ".ico" => "image/x-icon",
                _ => "text/plain",
            };
        }
    }
}

