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
        IVillaNumberRepository VillaNumber { get; }
        IAmenityRepository Amenity { get; }
        IBookingRepository Booking { get; }
        IUserRepository User { get; }
        IDashboardRepository AdminDashboard { get; }

        void Save();
    }
}
