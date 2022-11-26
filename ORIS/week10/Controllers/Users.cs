using ORIS.week10.Attributes;
using System.Net;
using System.Text.Json;
using Scriban;
using Scriban.Runtime;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ORIS.week10.Controllers
{
    [HttpController("users")]
    public class Users
    {
        UserDAO userDAO = new UserDAO();
        HttpListenerContext _httpContent;

        public Users(HttpListenerContext httpContent)
        {
            _httpContent = httpContent;
        }

        [HttpGET("getById")]
        public User GetUserById(int id)
        {
            return userDAO.GetById(id);
        }

        [HttpGET("getUser")]
        public string GetCurrentUser()
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

            var data = File.ReadAllText("./Site/post_profile.html");
            StringBuilder sb = new StringBuilder();

            var tpl = Template.Parse(data);
            User user = GetUserById(sessionId.Id);

            var scriptObject = new ScriptObject();
            scriptObject.Add("name", user.Name);
            scriptObject.Add("login", user.Login);
            scriptObject.Add("number", user.Number);

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            var res = tpl.Render(context);

            sb.Append(res);
            sb.Replace("%40", "@");
            sb.Replace("%2B", "+");
            return sb.ToString();
        }       

        [HttpPOST("saveUser")]
        public void SaveAccount(string name, string number, string login, string password)
        {
            Regex phoneRegex = new Regex(@"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$");
            MatchCollection matches = phoneRegex.Matches(number);

            Regex loginRegex = new Regex(@"^(.+)@(\ S+)$");
            MatchCollection matches2 = loginRegex.Matches(login);

            if ((matches.Count > 0 && number.Length == matches[0].Length)
                && (matches2.Count > 0 && matches2[0].Length == login.Length))
            {
                var hashPassword = HashPassword(password);
                var account = new User(name, number, login, hashPassword);
                userDAO.Insert(account);
                _httpContent.Response.Redirect("/login.html");
            }
            else
                _httpContent.Response.Redirect("/sign_in.html");
        }

        [HttpPOST("postLogin")]
        public bool PostLogin(string login, string password)
        {
            var user = userDAO.GetByLogin(login);
            if(user != null && VerifyHashedPassword(user.Password, password))
            {
                _httpContent.Response.Headers.Add("Set-Cookie",
                    nameof(SessionId) + "=" + JsonSerializer.Serialize(new SessionId(true, user.Id), typeof(SessionId)) + "; " +
                    "expires=" + DateTime.Now.AddMonths(1).ToString("dddd, dd-MM-yyyy hh:mm:ss GMT") + "; " +
                    "path=/");

                _httpContent.Response.Redirect("/");
                return true;
            }
            
            _httpContent.Response.Redirect("/login.html?err=true");
            return false;
        }

        [HttpPOST("updatePassword")]
        public void UpdateUser(string password)
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
            var hashPassword = HashPassword(password);
            userDAO.Update(sessionId.Id, hashPassword);
            _httpContent.Response.Redirect("/");
        }

        [HttpPOST("exit")]
        public void ExitProfile()
        {
            SessionId sessionId = new SessionId(false, 0);
            _httpContent.Response.Headers.Add("Set-Cookie", nameof(SessionId) + "=" + 
                JsonSerializer.Serialize(sessionId, typeof(SessionId)) + "; " +
                "expires=" + DateTime.Now.AddMonths(1).ToString("dddd, dd-MM-yyyy hh:mm:ss GMT") + "; path=/");
            _httpContent.Response.Redirect("/");
        }

        [HttpPOST("delete")]
        public void DeleteProfile()
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
            userDAO.Delete(GetUserById(sessionId.Id));
            ExitProfile();
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return buffer3.SequenceEqual(buffer4);
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public User(string name, string number, string login, string password)
        {
            Name = name;
            Number = number;
            Login = login;
            Password = password;
        }

        public User() { }
    }
}