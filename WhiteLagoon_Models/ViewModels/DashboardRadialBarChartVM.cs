using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon_Models.ViewModels
{
    public class DashboardRadialBarChartVM
    {
        public decimal TotalCount { get; set; }
        public decimal IncreaseDecreaseRatio { get; set; }
        public bool IsRatioIncrease { get; set; }
        public decimal[] Series { get; set; }

    }
}
