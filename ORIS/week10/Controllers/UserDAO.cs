using ORIS.week10.Interfaces;
using ORIS.week10.Controllers;

namespace ORIS.week10.Controllers
{
    public class UserDAO : IDAO<User>
    {
        //const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        MyORM orm = new MyORM();

        public void Delete(User user)
        {
            orm.Delete<User>(user);
        }

        public List<User> GetAll()
        { 
            return orm.Select<User>().ToList();
        }

        public List<User> GetPosts(int count)
        {
            return orm.ExecuteQuery<User>("SELECT * FROM dbo.Posts LIMIT " + count).ToList();
        }

        public User GetById(int id)
        {
            return orm.AddParameter("@id", id).ExecuteQuery<User>("SELECT * FROM dbo.Users WHERE Id = @id").FirstOrDefault();
        }

        public User GetByLogin(string login)
        {
            return orm.AddParameter("@login", login).ExecuteQuery<User>("SELECT * FROM dbo.Users WHERE Login = @login").FirstOrDefault();
        }

        public void Insert(User user)
        {
            orm.Insert<User>(user);
        }

        public void Update(int id, string newPassword)
        {
            orm.AddParameter("@id", id)
                .AddParameter("@password", newPassword)
                .ExecuteNonQuery("UPDATE dbo.Users SET Password = @password WHERE Id = @id");
        }
    }
}
