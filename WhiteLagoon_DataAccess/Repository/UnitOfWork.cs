using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_DataAccess.Repository.IRepository;

namespace WhiteLagoon_DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IVillaRepository Villa { get; private set; }
        public IAmenityRepository Amenity { get; private set; }
        public IVillaNumberRepository VillaNumber { get; private set; }
        public IBookingRepository Booking { get; private set; }
        public IUserRepository User { get; private set; }
        public IDashboardRepository AdminDashboard { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Villa = new VillaRepository(_db);
            VillaNumber = new VillaNumberRepository(_db);
            Amenity = new AmenityRepository(_db);
            Booking = new BookingRepository(_db);
            User = new UserRepository(_db);
            AdminDashboard = new DashboardRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
