using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RollCall.Models;
using System;

namespace RollCall.Helpers
{
    public class AttendancePDFDocument
    {
      
        static AttendancePDFDocument()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private readonly AttendanceSession _session;
        private readonly List<AttendanceRecord> _records;

        public AttendancePDFDocument(AttendanceSession session, List<AttendanceRecord> records)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _records = records ?? throw new ArgumentNullException(nameof(records));
        }

        public byte[] Generate()
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(ComposeContent);

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error generating PDF: {ex.Message}");
                throw; 
            }
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Roll Call System").FontSize(20).Bold();
                    column.Item().Text($"Attendance Report: {_session.Course.Name}").FontSize(14);
                    column.Item().Text($"Session: {_session.StartTime:g} - {(_session.EndTime ?? DateTime.UtcNow):g}").FontSize(10);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Item().Text($"Total Students: {_records.Count}").FontSize(14).Bold();

                column.Item().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    
                    table.Header(header =>
                    {
                        header.Cell().Text("#");
                        header.Cell().Text("Student ID");
                        header.Cell().Text("Name");
                        header.Cell().Text("Email");
                        header.Cell().Text("Marked At");

                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5);
                    });

                    
                    for (int i = 0; i < _records.Count; i++)
                    {
                        var record = _records[i];
                        table.Cell().Text((i + 1).ToString());
                        table.Cell().Text(record.Student.StudentId ?? "N/A");
                        table.Cell().Text(record.Student.Name);
                        table.Cell().Text(record.Student.Email);
                        table.Cell().Text(record.MarkedAt.ToString("g"));

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                    }
                });
            });
        }
    }
}
