using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon_Models.ViewModels
{
    public class DashboardLineChartVM
    {
        //public string NewBookingData { get; set; }
        //public string NewCustomerData { get; set; }

        public List<ChartData> ChartData { get; set; }
        public string[] Categories { get; set; }
    }

    public class ChartData
    {
        public string Name { get; set; }
        public int[] Data { get; set; }
    }

}
