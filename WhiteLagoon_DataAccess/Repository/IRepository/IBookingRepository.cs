using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_Models;

namespace WhiteLagoon_DataAccess.Repository.IRepository
{
    public interface IBookingRepository : IRepository<BookingDetail>
    {
        void Update(BookingDetail entity);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        void UpdateStatus(int bookingId, string orderStatus);
    }
}
