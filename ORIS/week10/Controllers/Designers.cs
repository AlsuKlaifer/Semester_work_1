using ORIS.week10.Attributes;
using Scriban.Runtime;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                var designer = new Designer(name, country, brand, image);
                designerDAO.Insert(designer);
            }

            _httpContent.Response.Redirect("/designers.html");
        }

        [HttpGET("getPosts")]
        public string GetPosts()
        {
            List<Designer> posts = designerDAO.GetAll();

            var data = File.ReadAllText("./Site/post.html");
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < posts.Count; i++)
            {
                Designer d = posts[i];

                var tpl = Template.Parse(data);

                var scriptObject = new ScriptObject();
                scriptObject.Add("name", d.Name);
                scriptObject.Add("country", d.Country);
                scriptObject.Add("brand", d.Brand);
                scriptObject.Add("image", d.Image);

                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var res = tpl.Render(context);

                sb.Append(res);
            }

            return sb.ToString();
        }
    }

    public class Designer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Brand { get; set; }
        public string Image { get; set; }

        public Designer(string name, string country, string brand, string image)
        {
            Name = name;
            Country = country;
            Brand = brand;
            Image = image;
        }

        public Designer() { }
    }
}
