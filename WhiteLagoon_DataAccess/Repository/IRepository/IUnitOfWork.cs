using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon_DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IVillaRepository Villa { get; }
        void Save();
    }
}
