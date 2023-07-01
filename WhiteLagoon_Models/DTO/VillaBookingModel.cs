using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon_Models.DTO
{
    public class VillaBookingModel
    {
        public List<VillaBookingDetailsModel> VillaBookingDetails { get; set; }
        public List<VillaDetailsModel> VillaDetails { get; set; }
        public List<VillaPaymentDetailsModel> VillaPaymentDetails { get; set; }
    }
}
