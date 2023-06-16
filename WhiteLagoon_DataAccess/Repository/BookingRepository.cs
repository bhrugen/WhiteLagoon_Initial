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
using WhiteLagoon_Utility;

namespace WhiteLagoon_DataAccess.Repository
{
    public class BookingRepository : Repository<BookingDetail>, IBookingRepository
    {
        private ApplicationDbContext _db;
        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(BookingDetail entity)
        {
            _db.Update(entity);
        }

        public void UpdateStatus(int bookingId, string orderStatus, int villaNumber=0)
        {
            var orderFromDb = _db.BookingDetails.FirstOrDefault(u => u.Id == bookingId);
            if (orderFromDb != null)
            {
                orderFromDb.Status = orderStatus;
                if (orderStatus == SD.StatusCheckedIn && villaNumber > 0)
                {
                    orderFromDb.VillaNumber = villaNumber;
                    orderFromDb.ActualCheckInDate = DateTime.Now;
                }
                if (orderStatus == SD.StatusCompleted)
                {
                    orderFromDb.ActualCheckOutDate=DateTime.Now;
                }
                
            }
            
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var bookingFromDb = _db.BookingDetails.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                bookingFromDb.StripeSessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                bookingFromDb.StripePaymentIntentId = paymentIntentId;
                bookingFromDb.PaymentDate = DateTime.Now;
                bookingFromDb.IsPaymentSuccessful= true;
            }
        }

    }
}
