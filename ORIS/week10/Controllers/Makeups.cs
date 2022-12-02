using ORIS.week10.Attributes;
using Scriban.Runtime;
using Scriban;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ORIS.week10.Controllers
{
    [HttpController("makeups")]
    internal class Makeups
    {
        MakeupDAO makeupDAO = new MakeupDAO();
        HttpListenerContext _httpContent;

        public Makeups(HttpListenerContext httpContent)
        {
            _httpContent = httpContent;
        }

        [HttpPOST("createPost")]
        public void CreatePost(string category, string description, string image)
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
                var makeup = new Makeup(category, description, image, sessionId.Id);
                makeupDAO.Insert(makeup);
            }

            _httpContent.Response.Redirect("/beauty.html");
        }

        [HttpGET("getPosts")]
        public string GetPosts()
        {
            List<Makeup> posts = makeupDAO.GetAll();

            var data = File.ReadAllText("./Site/post_beauty.html");
            StringBuilder sb = new StringBuilder();

            sb.Append("<div style=\"display: flex;flex-direction: row;flex-wrap: wrap;justify-content: center;\">");

            for (int i = 0; i < posts.Count; i++)
            {
                Makeup d = posts[i];

                var tpl = Template.Parse(data);

                var scriptObject = new ScriptObject();
                scriptObject.Add("id", d.Id);
                scriptObject.Add("category", d.Category);
                scriptObject.Add("description", d.Description);
                scriptObject.Add("image", d.Image);

                UserDAO userDAO = new UserDAO();
                scriptObject.Add("author", userDAO.GetById(d.Author).Name);

                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var res = tpl.Render(context);

                sb.Append(res); if (i % 3 == 2)
                    sb.Append("</div><div style=\"display: flex;flex-direction: row;flex-wrap: wrap;justify-content: center;\">");
            }
            sb.Append("</div>");

            return sb.ToString();
        }
        [HttpPOST("updatePost")]
        public void UpdatePost(int id, string category, string description, string image)
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
                Makeup makeup = makeupDAO.GetById(id);
                if (makeup.Author == sessionId.Id)
                {
                    makeupDAO.Update(id, category, description, image, makeup.Author);
                    _httpContent.Response.Redirect("/beauty.html");
                    return;
                }
            }

            _httpContent.Response.Redirect("/beauty.html?wrongAuthor=true");
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

            Makeup makeup = makeupDAO.GetById(id);
            if (makeup.Author == sessionId.Id)
            {
                makeupDAO.Delete(makeup);
                _httpContent.Response.Redirect("/beauty.html");
                return;
            }
            _httpContent.Response.Redirect("/beauty.html?wrongAuthor=true");
        }
    }

    public class Makeup
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public DateTime Date { get; set; }
        public int Author { get; set; }

        public Makeup(string category, string description, string image, int authorId)
        {
            Category = category;
            Description = description;
            Image = image;
            Date = DateTime.Now;
            Author = authorId;
        }

        public Makeup() { }
    }
}
