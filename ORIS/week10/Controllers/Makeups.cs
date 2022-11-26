using ORIS.week10.Attributes;
using Scriban.Runtime;
using Scriban;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ORIS.week10.Controllers
{
    [HttpController("makeup")]
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

            for (int i = 0; i < posts.Count; i++)
            {
                int col = i / 3 + 1;
                int raw = i % 3 + 1;

                if (i % 3 == 0)
                    sb.Append("<div style=\"width: 1200px; margin: 0 auto; display: block;\">");

                if (col > 1 && i % 3 == 2)
                    sb.Append("</div><div style=\"width: 1200px; margin: 0 auto; display: block;\">");

                Makeup d = posts[i];

                var tpl = Template.Parse(data);

                var scriptObject = new ScriptObject();
                scriptObject.Add("category", d.Category);
                scriptObject.Add("description", d.Description);
                scriptObject.Add("image", d.Image);

                UserDAO userDAO = new UserDAO();
                scriptObject.Add("author", userDAO.GetById(d.Author).Name);

                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var res = tpl.Render(context);

                sb.Append(res);
            }
            sb.Append("</div>");

            return sb.ToString();
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
