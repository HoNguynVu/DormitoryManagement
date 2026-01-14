using BusinessObject.DTOs.ConfirmDTOs;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace API.Services.Helpers
{
    public class PdfService
    {
        public PdfService()
        {
            // 1. Xác định đường dẫn file Font
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "times.ttf");

            // 2. Kiểm tra file có tồn tại không để báo lỗi rõ ràng
            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException($"Không tìm thấy file font tại: {fontPath}. Vui lòng kiểm tra thư mục 'Resources' và set 'Copy to Output Directory'.");
            }
            FontManager.RegisterFont(File.OpenRead(fontPath));
        }

        public byte[] GenerateExtensionContractPdf(DormRenewalSuccessDto dto)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    // SỬ DỤNG FONT TIMES NEW ROMAN
                    page.DefaultTextStyle(x => x.FontSize(13).FontFamily("Times New Roman"));

                    // --- HEADER ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                                .Bold().FontSize(14).AlignCenter();

                            col.Item().Text("Độc lập - Tự do - Hạnh phúc")
                                .Bold().FontSize(14).AlignCenter();

                            col.Item().Text("-------------------").AlignCenter();

                            col.Item().PaddingTop(10)
                                .Text($"PHỤ LỤC GIA HẠN HỢP ĐỒNG KÝ TÚC XÁ")
                                .Bold().FontSize(16).FontColor(Colors.Blue.Medium).AlignCenter();

                            col.Item().PaddingTop(5).Text($"Số: {dto.ContractCode}/PL-GH").Italic().AlignCenter();
                        });
                    });

                    // --- CONTENT ---
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        var culture = new CultureInfo("vi-VN");

                        col.Item().Text($"Hôm nay, ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}, chúng tôi gồm:");

                        // BÊN A
                        col.Item().PaddingTop(10).Text("BÊN A: BAN QUẢN LÝ KÝ TÚC XÁ").Bold();

                        // BÊN B
                        col.Item().PaddingTop(10).Text("BÊN B: SINH VIÊN (NGƯỜI THUÊ)").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(120);
                                columns.RelativeColumn();
                            });

                            table.Cell().Text("Họ và tên:");
                            table.Cell().Text(dto.StudentName).Bold();

                            table.Cell().Text("Email:");
                            table.Cell().Text(dto.StudentEmail);
                        });

                        col.Item().PaddingTop(15).Text("Hai bên thống nhất ký phụ lục gia hạn hợp đồng thuê chỗ ở với nội dung sau:");

                        // Thông tin chi tiết (Box có viền)
                        col.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(box =>
                        {
                            box.Item().PaddingBottom(5).Text($"1. Phòng ở: {dto.RoomName} - Tòa nhà: {dto.BuildingName}");

                            box.Item().PaddingBottom(5).Text($"2. Thời gian gia hạn: Từ ngày {dto.NewStartDate:dd/MM/yyyy} đến ngày {dto.NewEndDate:dd/MM/yyyy}");

                            box.Item().Text(text =>
                            {
                                text.Span("3. Số tiền đã thanh toán: ");
                                text.Span($"{dto.TotalAmountPaid.ToString("N0", culture)} VNĐ")
                                    .Bold().FontColor(Colors.Red.Medium);
                            });
                        });

                        col.Item().PaddingTop(10).Text("Các điều khoản khác của hợp đồng gốc vẫn giữ nguyên giá trị pháp lý.");
                        col.Item().Text("Phụ lục này được lập thành văn bản và là một phần không thể tách rời của hợp đồng thuê chỗ ở.");
                    });

                    // --- FOOTER ---
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ĐẠI DIỆN BÊN B").Bold().AlignCenter();
                            col.Item().Text("(Đã xác nhận điện tử)").Italic().FontSize(10).AlignCenter();
                            col.Item().PaddingTop(30).Text(dto.StudentName).Bold().AlignCenter();
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ĐẠI DIỆN BÊN A").Bold().AlignCenter();
                            col.Item().Text("(Đã ký)").Italic().FontSize(10).AlignCenter();
                            col.Item().PaddingTop(30).Text("Ban Quản Lý KTX").Bold().AlignCenter();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}