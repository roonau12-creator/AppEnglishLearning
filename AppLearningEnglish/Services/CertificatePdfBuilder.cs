using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AppLearningEnglish.Services
{
    public static class CertificatePdfBuilder
    {
        public static byte[] Build(
            string learnerName,
            string courseName,
            string? courseLevel,
            DateTime completedAt,
            string certificateCode)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Calibri).FontSize(14));

                    page.Content().Border(3).BorderColor(Colors.Indigo.Medium).Padding(30).Column(column =>
                    {
                        column.Spacing(14);

                        column.Item().AlignCenter().Text("AppLearningEnglish")
                            .FontSize(14).FontColor(Colors.Grey.Darken1);

                        column.Item().AlignCenter().Text("CERTIFICATE OF COMPLETION")
                            .FontSize(30).Bold().FontColor(Colors.Indigo.Darken2);

                        column.Item().AlignCenter().Text("This certifies that");

                        column.Item().AlignCenter().Text(learnerName)
                            .FontSize(24).Bold();

                        column.Item().AlignCenter().Text("has successfully completed the course");

                        column.Item().AlignCenter().Text(courseName)
                            .FontSize(20).Bold().FontColor(Colors.Indigo.Medium);

                        if (!string.IsNullOrWhiteSpace(courseLevel))
                        {
                            column.Item().AlignCenter().Text($"Level: {courseLevel}")
                                .FontColor(Colors.Grey.Darken1);
                        }

                        column.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("Ngày hoàn thành").FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                                left.Item().Text(completedAt.ToString("dd/MM/yyyy")).Bold();
                            });

                            row.RelativeItem().AlignRight().Column(right =>
                            {
                                right.Item().AlignRight().Text("Mã chứng chỉ").FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                                right.Item().AlignRight().Text(certificateCode).Bold();
                            });
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
