using AppLearningEnglish.Business.Services.IServices;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AppLearningEnglish.Services
{
    public static class ProgressPdfBuilder
    {
        public static byte[] Build(
            string learnerName,
            IReadOnlyList<ProgressExportRow> rows)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Calibri).FontSize(11));

                    page.Header().Column(header =>
                    {
                        header.Item().Text("AppLearningEnglish").FontSize(12)
                            .FontColor(Colors.Grey.Darken1);
                        header.Item().Text($"Tiến độ học — {learnerName}")
                            .FontSize(18).Bold().FontColor(Colors.Indigo.Darken2);
                        header.Item().Text($"Xuất ngày {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                                .Text("Khóa").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                                .Text("Bài").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                                .Text("%").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                                .Text("Trạng thái").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                                .Text("Từ").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                                .Text("Nghe").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Darken2).Padding(6)
                                .Text("BT").FontColor(Colors.White).Bold();
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(row.Course);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(row.Lesson);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text($"{row.Progress:0}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(row.Status);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(row.Vocab);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(row.Listening);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(row.Exercise);
                        }
                    });
                });
            }).GeneratePdf();
        }
    }
}
