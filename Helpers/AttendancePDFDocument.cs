using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RollCall.Models;
using System;
using System.Collections.Generic;

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
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generated on ").FontSize(9);
                        text.Span($"{DateTime.Now:g}").FontSize(9).SemiBold();
                        text.Line(" ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Roll Call System")
                    .FontSize(20).Bold().AlignCenter();

                column.Item().Text($"Attendance Report: {_session.Course?.Name ?? "N/A"}")
                    .FontSize(14).SemiBold().AlignCenter();

                var end = _session.EndTime ?? DateTime.UtcNow;
                column.Item().Text($"Session: {_session.StartTime:g} - {end:g}")
                    .FontSize(10).AlignCenter();

                column.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Item().PaddingBottom(6).Text($"Total Students: {_records.Count}")
                    .FontSize(13).Bold();

                column.Item().PaddingVertical(6).Table(table =>
                {
                    // ===== Column Definitions =====
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);    // #
                        columns.ConstantColumn(100);   // Student ID
                        columns.RelativeColumn(2);     // Name
                        columns.RelativeColumn(1);     // Section
                        columns.RelativeColumn(2);     // Email
                        columns.RelativeColumn(2);     // Marked At
                    });

                    // ===== Header Row =====
                    table.Header(header =>
                    {
                        header.Cell().Element(cell =>
                            cell.Background(Colors.Grey.Darken1)
                                .Padding(6)
                                .Border(1).BorderColor(Colors.Black)
                                .AlignCenter()
                                .Text("#").FontColor(Colors.White).SemiBold());

                        header.Cell().Element(cell =>
                            cell.Background(Colors.Grey.Darken1)
                                .Padding(6)
                                .Border(1).BorderColor(Colors.Black)
                                .AlignCenter()
                                .Text("Student ID").FontColor(Colors.White).SemiBold());

                        header.Cell().Element(cell =>
                            cell.Background(Colors.Grey.Darken1)
                                .Padding(6)
                                .Border(1).BorderColor(Colors.Black)
                                .AlignCenter()
                                .Text("Name").FontColor(Colors.White).SemiBold());

                        header.Cell().Element(cell =>
                            cell.Background(Colors.Grey.Darken1)
                                .Padding(6)
                                .Border(1).BorderColor(Colors.Black)
                                .AlignCenter()
                                .Text("Section").FontColor(Colors.White).SemiBold());

                        header.Cell().Element(cell =>
                            cell.Background(Colors.Grey.Darken1)
                                .Padding(6)
                                .Border(1).BorderColor(Colors.Black)
                                .AlignCenter()
                                .Text("Email").FontColor(Colors.White).SemiBold());

                        header.Cell().Element(cell =>
                            cell.Background(Colors.Grey.Darken1)
                                .Padding(6)
                                .Border(1).BorderColor(Colors.Black)
                                .AlignCenter()
                                .Text("Marked At").FontColor(Colors.White).SemiBold());
                    });

                    // ===== Data Rows =====
                    for (int i = 0; i < _records.Count; i++)
                    {
                        var record = _records[i];
                        var student = record.Student;

                        // # (center)
                        table.Cell().Element(cell =>
                            cell.Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).AlignMiddle().AlignCenter()
                                .Text((i + 1).ToString()));

                        // Student ID
                        table.Cell().Element(cell =>
                            cell.Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).AlignMiddle()
                                .Text(student?.StudentId ?? "N/A"));

                        // Name (left)
                        table.Cell().Element(cell =>
                            cell.Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).AlignMiddle()
                                .Text(student?.Name ?? "N/A"));

                        // Section (center-ish)
                        table.Cell().Element(cell =>
                            cell.Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).AlignMiddle().AlignCenter()
                                .Text(student?.Section ?? "N/A"));

                        // Email (left)
                        table.Cell().Element(cell =>
                            cell.Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).AlignMiddle()
                                .Text(student?.Email ?? "N/A"));

                        // Marked At (center)
                        table.Cell().Element(cell =>
                            cell.Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).AlignMiddle().AlignCenter()
                                .Text(record.MarkedAt.ToString("g")));
                    }
                });
            });
        }
    }
}
