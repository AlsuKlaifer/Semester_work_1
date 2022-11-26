using ORIS.week10.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORIS.week10.Interfaces
{
    public interface IDAO<T>
    {
        void Insert(T t);

        //void Update(T t, string newPassword);

        void Delete(T t);

        List<T> GetAll();

        T GetById(int id);
    }
}
