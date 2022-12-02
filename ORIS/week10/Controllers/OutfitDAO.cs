using ORIS.week10.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORIS.week10.Controllers
{
    internal class OutfitDAO : IDAO<Outfit>
    {
        //const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        MyORM orm = new MyORM();

        public void Delete(Outfit outfit)
        {
            orm.Delete<Outfit>(outfit);
        }

        public List<Outfit> GetAll()
        {
            return orm.Select<Outfit>().ToList();
        }

        public List<Outfit> GetPosts(int count)
        {
            return orm.ExecuteQuery<Outfit>("SELECT * FROM dbo.Outfits LIMIT " + count).ToList();
        }

        public Outfit GetById(int id)
        {
            return orm.AddParameter("@id", id).ExecuteQuery<Outfit>("SELECT * FROM dbo.Outfits WHERE Id = @id").FirstOrDefault();
        }

        public void Insert(Outfit outfit)
        {
            orm.Insert<Outfit>(outfit);
        }
        
        public void Update(int id, string season, string style, string image, int authorId)
        {
            Outfit outfit = new Outfit(season, style, image, authorId);

            orm.Update(id, outfit);
        }
    }
}
