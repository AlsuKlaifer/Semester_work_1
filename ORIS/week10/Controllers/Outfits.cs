using ORIS.week10.Attributes;
using System.Net;
using System.Text.Json;
using Scriban;
using Scriban.Runtime;
using System.Text;

namespace ORIS.week10.Controllers
{
    [HttpController("outfits")]
    public class Outfits
    {
        OutfitDAO outfitDAO = new OutfitDAO();
        HttpListenerContext _httpContent;
        public Outfits(HttpListenerContext httpContent)
        {
            _httpContent = httpContent;
        }

        [HttpPOST("createPost")]
        public void CreatePost(string season, string style, string image)
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
                var outfit = new Outfit(season, style, image, sessionId.Id);
                outfitDAO.Insert(outfit);
            }

            _httpContent.Response.Redirect("/outfits.html");
        }

        [HttpGET("getPosts")]
        public string GetPosts()
        {
            List<Outfit> posts = outfitDAO.GetAll();

            var data = File.ReadAllText("./Site/post_outfit.html");
            StringBuilder sb = new StringBuilder();

            sb.Append("<div style=\"display: flex;flex-direction: row;flex-wrap: wrap;justify-content: center;\">");
            for (int i = 0; i < posts.Count; i++)
            {
                Outfit d = posts[i];

                var tpl = Template.Parse(data);

                var scriptObject = new ScriptObject();
                scriptObject.Add("id", d.Id);
                scriptObject.Add("season", d.Season);
                scriptObject.Add("style", d.Style);
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
        public void UpdatePost(int id, string season, string style, string image)
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
                Outfit outfit = outfitDAO.GetById(id);
                if (outfit.Author == sessionId.Id)
                {
                    outfitDAO.Update(id, season, style, image, outfit.Author);
                    _httpContent.Response.Redirect("/outfits.html");
                    return;
                }
            }

            _httpContent.Response.Redirect("/outfits.html?wrongAuthor=true");
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

            Outfit outfit = outfitDAO.GetById(id);
            if (outfit.Author == sessionId.Id)
            {
                outfitDAO.Delete(outfit);
                _httpContent.Response.Redirect("/outfits.html");
                return;
            }
            _httpContent.Response.Redirect("/outfits.html?wrongAuthor=true");
        }
    }

    public class Outfit
    {
        public int Id { get; set; }
        public string Season { get; set; }
        public string Style { get; set; }
        public string Image { get; set; }
        public DateTime Date { get; set; }
        public int Author { get; set; }

        public Outfit(string season, string style, string image, int authorId)
        {
            Season = season;
            Style = style;
            Image = image;
            Date = DateTime.Now;
            Author = authorId;
        }

        public Outfit() { }
    }
}

