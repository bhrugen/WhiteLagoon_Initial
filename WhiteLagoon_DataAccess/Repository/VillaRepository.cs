using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;

namespace WhiteLagoon_DataAccess.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private ApplicationDbContext _db;
        public VillaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Villa entity)
        {
            entity.VillaAmenity = null;
            _db.Update(entity);
        }

        //public async Task<bool> IsRoomBooked(int villaId, string checkInDatestr, string checkOutDatestr)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(checkOutDatestr) && !string.IsNullOrEmpty(checkInDatestr))
        //        {
        //            DateOnly checkInDate = DateOnly.FromDateTime(DateTime.ParseExact(checkInDatestr, "MM/dd/yyyy", null));
        //            DateOnly checkOutDate = DateOnly.FromDateTime(DateTime.ParseExact(checkOutDatestr, "MM/dd/yyyy", null));

        //            var existingBooking = await _db.BookingDetails.Where(x => x.VillaId == villaId && x.IsPaymentSuccessful &&
        //               //check if checkin date that user wants does not fall in between any dates for room that is booked
        //               ((checkInDate < x.CheckOutDate && checkInDate >= x.CheckInDate)
        //               //check if checkout date that user wants does not fall in between any dates for room that is booked
        //               || (checkOutDate > x.CheckInDate && checkInDate <= x.CheckInDate)
        //               )).FirstOrDefaultAsync();

        //            if (existingBooking != null)
        //            {
        //                return true;
        //            }
        //            return false;
        //        }
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }


        //}

    }
}
