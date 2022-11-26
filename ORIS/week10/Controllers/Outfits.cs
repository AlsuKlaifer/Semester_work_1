using System.Data.SqlClient;
using ORIS.week10.Attributes;
using System.Net;
using System.Net.Http.Headers;
using System.Collections.Specialized;
using System.Text.Json;
using Scriban;
using System.Diagnostics.Metrics;
using System.Xml.Linq;
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

        [HttpGET("getById")]
        public Outfit GetOutfitById(int id)
        {
            return outfitDAO.GetById(id);
        }

        [HttpGET("getList")]
        public HttpResponseMessage GetOutfits()
        {
            if(_httpContent.Request.Cookies["SessionId_IsAuthorize"] == null)
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            string cookie_IsAuthorize = _httpContent.Request.Cookies["SessionId_IsAuthorize"].Value;
            string cookie_Id = _httpContent.Request.Cookies["SessionId_Id"].Value;
            if (bool.Parse(cookie_IsAuthorize))
            {
                HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                responseMessage.Content = new StringContent(String.Join(", ", outfitDAO.GetAll()));
                return responseMessage;
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
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

            var data = File.ReadAllText("./Site/post.html");
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < posts.Count; i++)
            {
                Outfit d = posts[i];

                var tpl = Template.Parse(data);

                var scriptObject = new ScriptObject();
                scriptObject.Add("season", d.Season);
                scriptObject.Add("style", d.Style);
                scriptObject.Add("image", d.Image);

                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var res = tpl.Render(context);

                sb.Append(res);
            }

            return sb.ToString();
        }
    }

    public class Outfit
    {
        public int Id { get; set; }
        public string Season { get; set; }
        public string Style { get; set; }
        public string Image { get; set; }
        public DateTime Date { get; set; }
        public string Author { get; set; }

        public Outfit(string season, string style, string image, int authorId)
        {
            Season = season;
            Style = style;
            Image = image;
            Date = DateTime.Now;
            UserDAO userDAO = new UserDAO();
            Author = userDAO.GetById(authorId).Name;
        }

        public Outfit() { }
    }
}

