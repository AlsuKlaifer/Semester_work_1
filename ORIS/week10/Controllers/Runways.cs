using ORIS.week10.Attributes;
using System.Net;
using System.Text.Json;
using System.Text;
using Scriban;
using Scriban.Runtime;
using System.Diagnostics.Metrics;
using static System.Net.Mime.MediaTypeNames;

namespace ORIS.week10.Controllers
{
    [HttpController("runways")]
    public class Runways
    {
        RunwayDAO runwayDAO = new RunwayDAO();
        HttpListenerContext _httpContent;
        public Runways(HttpListenerContext httpContent)
        {
            _httpContent = httpContent;
        }

        [HttpPOST("createPost")]
        public void CreatePost(string modelName, string country, string brand, string collection, string image)
        {
            SessionId sessionId = null;
            string[] cookies = _httpContent.Request.Headers.Get("Cookie").Split("; ");
            foreach (string c in cookies)
            {
                string[] splitted = c.Split("=");
                if (splitted[0].Equals(nameof(SessionId)))
                {
                    sessionId = JsonSerializer.Deserialize<SessionId>(splitted[1]);
                    break;
                }
            }

            if(sessionId != null)
            {
                var runway = new Runway(modelName, country, brand, collection, image, sessionId.Id);
                runwayDAO.Insert(runway);
            }
            
            _httpContent.Response.Redirect("/runway.html");
        }

        [HttpGET("getPosts")]
        public string GetPosts()
        {            
            List<Runway> posts = runwayDAO.GetAll();
            
            var data = File.ReadAllText("./Site/post.html");
            StringBuilder sb = new StringBuilder();

            sb.Append("<div style=\"display: flex;flex-direction: row;flex-wrap: wrap;justify-content: center;\">");
            for (int i = 0; i < posts.Count; i++)
            {                
                Runway r = posts[i];

                var tpl = Template.Parse(data);

                var scriptObject = new ScriptObject();

                scriptObject.Add("id", r.Id);
                scriptObject.Add("name", r.ModelName);
                scriptObject.Add("country", r.Country);
                scriptObject.Add("brand", r.Brand);
                scriptObject.Add("collection", r.Collection);
                scriptObject.Add("image", r.Image);

                UserDAO userDAO = new UserDAO();
                scriptObject.Add("author", userDAO.GetById(r.Author).Name);

                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var res = tpl.Render(context);

                sb.Append(res);

                if (i % 3 == 2)
                    sb.Append("</div><div style=\"display: flex;flex-direction: row;flex-wrap: wrap;justify-content: center;\">");
            }
            sb.Append("</div>");

            return sb.ToString();
        }

        [HttpPOST("updatePost")]
        public void UpdatePost(int id, string modelName, string country, string brand, string collection, string image)
        {
            SessionId sessionId = null;
            string[] cookies = _httpContent.Request.Headers.Get("Cookie").Split("; ");
            foreach (string c in cookies)
            {
                string[] splitted = c.Split("=");
                if (splitted[0].Equals(nameof(SessionId)))
                {
                    sessionId = JsonSerializer.Deserialize<SessionId>(splitted[1]);
                    break;
                }
            }

            if (sessionId != null)
            {
                Runway runway = runwayDAO.GetById(id);
                if (runway.Author == sessionId.Id)
                {
                    runwayDAO.Update(id, modelName, country, brand, collection, image, runway.Author);

                    _httpContent.Response.Redirect("/runway.html");
                    return;
                }
            }

            _httpContent.Response.Redirect("/runway.html?wrongAuthor=true");
        }

        [HttpPOST("deletePost")]
        public void DeletePost(int id)
        {
            SessionId sessionId = null;
            string[] cookies = _httpContent.Request.Headers.Get("Cookie").Split("; ");
            foreach (string c in cookies)
            {
                string[] splitted = c.Split("=");
                if (splitted[0].Equals(nameof(SessionId)))
                {
                    sessionId = JsonSerializer.Deserialize<SessionId>(splitted[1]);
                    break;
                }
            }

            Runway runway = runwayDAO.GetById(id);
            if (runway.Author == sessionId.Id)
            {
                runwayDAO.Delete(runway);
                _httpContent.Response.Redirect("/runway.html");
                return;
            }

            _httpContent.Response.Redirect("/runway.html?wrongAuthor=true");
        }
    }

    public class Runway
    {
        public int Id { get; set; }
        public string ModelName { get; set; }
        public string Country { get; set; }
        public string Brand { get; set; }
        public string Collection { get; set; }
        public string Image { get; set; }
        public DateTime Date { get; set; }
        public int Author { get; set; }

        public Runway(string modelName, string country, string brand, string collection, string image, int authorId)
        {
            ModelName = modelName;
            Country = country;
            Brand = brand;
            Collection = collection;
            Image = image;
            Date = DateTime.Now;
            Author = authorId;
        }

        public Runway() { }
    }
}