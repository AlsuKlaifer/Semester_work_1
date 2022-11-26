using ORIS.week10.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORIS.week10.Controllers
{
    internal class MakeupDAO : IDAO<Makeup>
    {
        const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        MyORM orm = new MyORM(connectionString);

        public void Delete(Makeup makeup)
        {
            orm.Delete<Makeup>(makeup);
        }

        public List<Makeup> GetAll()
        {
            return orm.Select<Makeup>().ToList();
        }

        public List<Makeup> GetPosts(int count)
        {
            return orm.ExecuteQuery<Makeup>("SELECT * FROM dbo.Designers LIMIT " + count).ToList();
        }

        public Makeup GetById(int id)
        {
            return orm.AddParameter("@id", id).ExecuteQuery<Makeup>("SELECT * FROM dbo.Designers WHERE Id = @id").FirstOrDefault();
        }

        public void Insert(Makeup makeup)
        {
            orm.Insert<Makeup>(makeup);
        }
    }
}
