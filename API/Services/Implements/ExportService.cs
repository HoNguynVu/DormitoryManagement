using API.Services.Interfaces;
using ClosedXML.Excel;
using System.Text;

namespace API.Services.Implements
{
    public class ExportService : IExportService
    {
        public byte[] CreateExcel(Action<XLWorkbook> populateWorkbook)
        {
            using var wb = new XLWorkbook();
            populateWorkbook(wb);
            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[] CreateCsv(string content, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return encoding.GetBytes(content ?? string.Empty);
        }
    }
}
