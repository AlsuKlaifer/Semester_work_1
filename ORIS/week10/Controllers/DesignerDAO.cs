using ORIS.week10.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORIS.week10.Controllers
{
    internal class DesignerDAO : IDAO<Designer>
    {
        //const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        MyORM orm = new MyORM();

        public void Delete(Designer designer)
        {
            orm.Delete<Designer>(designer);
        }

        public List<Designer> GetAll()
        {
            return orm.Select<Designer>().ToList();
        }

        public List<Designer> GetPosts(int count)
        {
            return orm.ExecuteQuery<Designer>("SELECT * FROM dbo.Designers LIMIT " + count).ToList();
        }

        public Designer GetById(int id)
        {
            return orm.AddParameter("@id", id).ExecuteQuery<Designer>("SELECT * FROM dbo.Designers WHERE Id = @id").FirstOrDefault();
        }

        public void Insert(Designer designer)
        {
            orm.Insert<Designer>(designer);
        }
        public void Update(int id, string name, string country, string brand, string image, int authorId)
        {
            Designer designer = new Designer(name, country, brand, image, authorId);

            orm.Update(id, designer);
        }
    }
}
