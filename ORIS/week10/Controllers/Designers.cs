using ORIS.week10.Attributes;
using Scriban.Runtime;
using Scriban;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ORIS.week10.Controllers
{
    [HttpController("designers")]
    internal class Designers
    {
        DesignerDAO designerDAO = new DesignerDAO();
        HttpListenerContext _httpContent;

        public Designers(HttpListenerContext httpContent)
        {
            _httpContent = httpContent;
        }

        [HttpPOST("createPost")]
        public void CreatePost(string name, string country, string brand, string image)
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
                var designer = new Designer(name, country, brand, image, sessionId.Id);
                designerDAO.Insert(designer);
            }

            _httpContent.Response.Redirect("/designers.html");
        }

        [HttpGET("getPosts")]
        public string GetPosts()
        {
            List<Designer> posts = designerDAO.GetAll();

            var data = File.ReadAllText("./Site/post_designer.html");
            StringBuilder sb = new StringBuilder();

            sb.Append("<div style=\"display: flex;flex-direction: row;flex-wrap: wrap;justify-content: center;\">");
            for (int i = 0; i < posts.Count; i++)
            {
                Designer d = posts[i];

                var tpl = Template.Parse(data);

                var scriptObject = new ScriptObject();
                scriptObject.Add("id", d.Id);
                scriptObject.Add("name", d.Name);
                scriptObject.Add("country", d.Country);
                scriptObject.Add("brand", d.Brand);
                scriptObject.Add("image", d.Image);

                UserDAO userDAO = new UserDAO();
                scriptObject.Add("author", userDAO.GetById(d.Author).Name);

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
        public void UpdatePost(int id, string name, string country, string brand, string image)
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
                Designer designer = designerDAO.GetById(id);
                if (designer.Author == sessionId.Id)
                {
                    designerDAO.Update(id, name, country, brand, image, designer.Author);
                    _httpContent.Response.Redirect("/designers.html");
                    return;
                }
            }

            _httpContent.Response.Redirect("/designers.html?wrongAuthor=true");
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

            Designer designer = designerDAO.GetById(id);
            if (designer.Author == sessionId.Id)
            {
                designerDAO.Delete(designer);
                _httpContent.Response.Redirect("/designers.html");
                return;
            }
            _httpContent.Response.Redirect("/designers.html?wrongAuthor=true");
        }
    }

    public class Designer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Brand { get; set; }
        public string Image { get; set; }
        public DateTime Date { get; set; }
        public int Author { get; set; }

        public Designer(string name, string country, string brand, string image, int authorId)
        {
            Name = name;
            Country = country;
            Brand = brand;
            Image = image;
            Date = DateTime.Now;
            Author = authorId;
        }

        public Designer() { }
    }
}
