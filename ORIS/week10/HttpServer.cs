using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ORIS.week10.Attributes;
using Scriban;
using Scriban.Runtime;
using HttpMultipartParser;

namespace ORIS.week10
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener listener;
        public ServerStatus Status = ServerStatus.Stop;
        private ServerSettings serverSettings;

        public HttpServer()
        {
            serverSettings = ServerSettings.Deserialize();
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:" + serverSettings.Port + "/");
        }

        public void Start()
        {
            if (Status == ServerStatus.Start)
            {
                Console.WriteLine("Сервер уже запущен!");
            }
            else
            {
                Console.WriteLine("Запуск сервера");
                listener.Start();
                Console.WriteLine("Сервер запущен");
                Status = ServerStatus.Start;
            }
            Receive();
        }

        public void Stop()
        {
            if (Status == ServerStatus.Start)
            {
                Console.WriteLine("Остановка сервера...");
                listener.Stop();
                Console.WriteLine("Сервер остановлен");
                Status = ServerStatus.Stop;
            }
            else
                Console.WriteLine("Сервер уже остановлен");
        }

        private async void Receive()
        {
            while (listener.IsListening)
            {
                var context = await listener.GetContextAsync();

                if (!MethodHandler(context)) //находим метод
                    StaticFiles(context.Request, context.Response); //метод не найден, ищем в файлах
            }
        }

        private void StaticFiles(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] buffer;

            if (Directory.Exists(serverSettings.Path))
            {
                string url = request.RawUrl.Replace("%20", " ");
                int index = url.IndexOf('?');
                if (index != -1) 
                    url = url.Remove(index, url.Length - index); //удаляем аттрибуты запроса, если они есть

                //Задаю расширения для файлов
                Files.GetExtension(ref response, "." + url);

                if (response.ContentType.Equals("text/html")) //если запрашиваем html
                {
                    SessionId? sessionId = null;
                    if (request.Headers.Get("Cookie") != null)
                    {
                        string[] cookies = request.Headers.Get("Cookie").Split("; ");
                        foreach (string c in cookies)
                        {
                            string[] splitted = c.Split("=");
                            if (splitted[0].Equals(nameof(SessionId)))
                            {
                                sessionId = JsonSerializer.Deserialize<SessionId>(splitted[1]);
                                break;
                            }
                        }
                    }
                    
                    bool isAuth = sessionId != null;
                    if (isAuth)
                        isAuth = sessionId.IsAuthorize;

                    string data = getHtml(url);
                    var tpl = Template.Parse(data);

                    var scriptObject = new ScriptObject();
                    scriptObject.Add("isAuth", isAuth ? "hidden=\"hidden\"" : "");
                    scriptObject.Add("isNotAuth", isAuth ? "" : "hidden=\"hidden\"");

                    var context = new TemplateContext();
                    context.PushGlobal(scriptObject);

                    var res = tpl.Render(context);

                    buffer = Encoding.UTF8.GetBytes(res);
                }
                else
                {
                    buffer = getFile(url);

                    if (buffer == null)
                    {
                        response.Headers.Set("Content-Type", "text/plain");

                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        string err = "404 - not found";

                        buffer = Encoding.UTF8.GetBytes(err);
                    }
                }
            }
            else
            {
                string err = $"Directory '{serverSettings.Path}' not found";

                buffer = Encoding.UTF8.GetBytes(err);
            }

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // закрываем поток
            output.Close();
        }

        private byte[] getFile(string rawUrl)
        {
            byte[] buffer = null;
            var filePath = serverSettings.Path + rawUrl;

            if (Directory.Exists(filePath))
            {
                // Каталог
                filePath = filePath + "/index.html";
                if (File.Exists(filePath))
                {
                    buffer = File.ReadAllBytes(filePath);
                }
            }
            else if (File.Exists(filePath))
            {
                // Файл
                buffer = File.ReadAllBytes(filePath);
            }

            return buffer;
        }

        private string getHtml(string rawUrl)
        {
            string buffer = null;
            var filePath = serverSettings.Path + rawUrl;

            if (Directory.Exists(filePath))
            {
                // Каталог
                filePath = filePath + "/index.html";
                if (File.Exists(filePath))
                {
                    buffer = File.ReadAllText(filePath);
                }
            }
            else if (File.Exists(filePath))
            {
                // Файл
                buffer = File.ReadAllText(filePath);
            }

            return buffer;
        }

        public void Dispose()
        {
            listener.Stop();
        }

        private bool MethodHandler(HttpListenerContext context)
        {
            //объект запроса
            HttpListenerRequest request = context.Request;

            //объект ответа
            HttpListenerResponse response = context.Response;

            Console.WriteLine("URL: " + request.Url);
            Console.WriteLine("Http Method: " + request.HttpMethod);
            Console.WriteLine("Content Type: " + request.ContentType);
            Console.WriteLine("Content Length: " + request.ContentLength64);
            Console.WriteLine("Content (Encoded): " + request.ContentEncoding);
            Console.WriteLine("URL Segments: " + string.Join(", ", request.Url.Segments));

            if (request.Url.Segments.Length < 2)
                return false;

            string controllerName = request.Url.Segments[1].Replace("/", "");

            //string[] strparams = request.url
            //    .segments
            //    .skip(2)
            //    .select(s => s.replace("/", ""))
            //    .toarray();

            var assembly = Assembly.GetExecutingAssembly();

            var controller = assembly.GetTypes().FirstOrDefault(t =>
            {
                var myAttribute = (HttpController)Attribute.GetCustomAttribute(t, typeof(HttpController));

                if (myAttribute == null)
                    return false;

                return myAttribute.ControllerName.Equals(controllerName);
            });

            if (controller == null) return false;

            string methodURI = request.Url.Segments[2].Replace("/", "");

            var method = controller
                .GetMethods()
                .Where(t => t.GetCustomAttributes(true)
                    .Any(attr => attr.GetType().Name == $"Http{request.HttpMethod}"))
                .FirstOrDefault(x => request.HttpMethod switch
                {
                    "GET" => x.GetCustomAttribute<HttpGET>()?.MethodURI == methodURI,
                    "POST" => x.GetCustomAttribute<HttpPOST>()?.MethodURI == methodURI
                });

            List<object?> attr = new List<object?>();

            if (request.HttpMethod == "GET")
            {
                foreach (string? s in request.QueryString.AllKeys)
                    attr.Add(request.QueryString.Get(s)); //преобразовываем аттрибуты запроса в список объектов
            }
            else if (request.HttpMethod == "POST")
            {
                if (request.HasEntityBody)
                {
                    if (request.ContentType.Contains("multipart/form-data"))
                    {
                        var parser = MultipartFormDataParser.Parse(request.InputStream);

                        foreach (var parameter in parser.Parameters)
                            attr.Add(parser.GetParameterValue(parameter.Name));

                        var file = parser.Files.First();
                        string filename = file.FileName;
                        Stream data = file.Data;

                        var memoryStream = new MemoryStream();
                        data.CopyTo(memoryStream);
                        byte[] byteArray = memoryStream.ToArray();
                        string imageData = Convert.ToBase64String(byteArray); //для фронта

                        attr.Add(imageData);
                    }
                    else
                    {
                        string s;
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            s = reader.ReadToEnd();
                        }

                        string[] parameters = s.Split("&");
                        foreach (string parameter in parameters)
                        {
                            string[] parSplit = parameter.Split('=');
                            attr.Add(parSplit[1]);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No client data was sent with the request.");
                }
            }

            object?[] queryParams = method.GetParameters()
                .Select((p, i) => Convert.ChangeType(attr[i], p.ParameterType))
                .ToArray();

            Console.WriteLine("MethodName: " + method.Name);
            Console.WriteLine("MethodURI: " + methodURI);
            Console.WriteLine("QueryParams: " + queryParams);

            var ret = method.Invoke(Activator.CreateInstance(controller, new object[] { context }), queryParams);

            byte[] buffer;

            response.ContentType = "Application/json";

            buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // закрываем поток
            output.Close();

            return true;
        }
    }
}