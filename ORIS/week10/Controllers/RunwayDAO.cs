using ORIS.week10.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORIS.week10.Controllers
{
    internal class RunwayDAO : IDAO<Runway>
    {
        //const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        MyORM orm = new MyORM();

        public void Delete(Runway runway)
        {
            orm.Delete<Runway>(runway);
        }

        public List<Runway> GetAll()
        {
            return orm.Select<Runway>().ToList();
        }

        public List<Runway> GetPosts(int count)
        {
            return orm.ExecuteQuery<Runway>("SELECT * FROM dbo.Runways LIMIT " + count).ToList();
        }

        public Runway GetById(int id)
        {
            return orm.AddParameter("@id", id).ExecuteQuery<Runway>("SELECT * FROM dbo.Runways WHERE Id = @id").FirstOrDefault();
        }

        public void Insert(Runway runway)
        {
            orm.Insert<Runway>(runway);
        }

        public void Update(int id, string modelName, string country, string brand, string collection, string image, int authorId)
        {
            Runway runway = new Runway(modelName, country, brand, collection, image, authorId);

            orm.Update(id, runway);
        }
    }
}
