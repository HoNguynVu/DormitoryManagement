using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }      // Dữ liệu của trang hiện tại
        public int TotalItems { get; set; }     // Tổng số bản ghi (để tính số trang)
        public int PageIndex { get; set; }      // Trang hiện tại
        public int PageSize { get; set; }       // Kích thước trang
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public PagedResult(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalItems = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}
