using ORIS.week10.Interfaces;

namespace ORIS.week10.Controllers
{
    internal class MakeupDAO : IDAO<Makeup>
    {
        //const string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ArizonaDB;Integrated Security=True";
        MyORM orm = new MyORM();

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
            return orm.ExecuteQuery<Makeup>("SELECT * FROM dbo.Makeups LIMIT " + count).ToList();
        }

        public Makeup GetById(int id)
        {
            return orm.AddParameter("@id", id).ExecuteQuery<Makeup>("SELECT * FROM dbo.Makeups WHERE Id = @id").FirstOrDefault();
        }

        public void Insert(Makeup makeup)
        {
            orm.Insert<Makeup>(makeup);
        }
        public void Update(int id, string category, string description, string image, int authorId)
        {
            Makeup makeup = new Makeup(category, description, image, authorId);

            orm.Update(id, makeup);
        }
    }
}
