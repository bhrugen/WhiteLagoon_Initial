using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_Models;

namespace WhiteLagoon_DataAccess.Repository.IRepository
{
    public interface IVillaRepository : IRepository<Villa>
    {
        void Update(Villa entity);
        //public Task<bool> IsRoomBooked(int RoomId, string checkInDate, string checkOutDate);
    }
}
