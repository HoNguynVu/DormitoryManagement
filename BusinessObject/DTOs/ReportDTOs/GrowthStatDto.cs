using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ReportDTOs
{
    public class GrowthStatDto
    {
        public int TotalValue { get; set; }      // Tổng số lượng hiện tại
        public double GrowthPercent { get; set; } // % tăng giảm so với tháng trước
        public bool IsIncrease { get; set; }
        public decimal TotalMoney { get; set; }
    }
}
