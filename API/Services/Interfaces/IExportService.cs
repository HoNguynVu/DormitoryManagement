using ClosedXML.Excel;
using System.Text;

namespace API.Services.Interfaces
{
    public interface IExportService
    {
        byte[] CreateExcel(Action<XLWorkbook> populateWorkbook);
        byte[] CreateCsv(string content, Encoding? encoding = null);
    }
}
