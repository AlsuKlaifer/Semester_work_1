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
        const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        MyORM orm = new MyORM(connectionString);

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

        public void Insert(Runway user)
        {
            orm.Insert<Runway>(user);
        }
        public void Update(int id, string modelName, string country, string brand, string collection, string image)
        {
            orm.AddParameter("@id", id)
                .AddParameter("@modelName", modelName)
                .AddParameter("@country", country)
                .AddParameter("@brand", brand)
                .AddParameter("@collection", collection)
                .AddParameter("@image", image)
                .ExecuteNonQuery("UPDATE dbo.Runways SET ModelName = @modelName, Country = @country, " +
                "Brand = @brand, Collection = @collection, Image = @image WHERE Id = @id");
        }
    }
}
