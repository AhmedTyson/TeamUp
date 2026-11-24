// Date: 2026-11-24
// Purpose: QuestPDF editorial design primitives matching the Obsidian (#14151C), Lime (#D3FB52), and Alabaster (#F3F5F2) print system.

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TeamUp.Infrastructure.Templates;

namespace TeamUp.Infrastructure.Reports;

internal static class EditorialPdfTheme
{
    public const string Obsidian = "#14151C";
    public const string Lime = "#D3FB52";
    public const string Paper = "#FFFFFF";
    public const string SurfaceMuted = "#F8F9F7";
    public const string Border = "#E2E6E0";
    public const string InkPrimary = "#14151C";
    public const string InkSecondary = "#4B5260";
    public const string InkMuted = "#6B7280";
    public const string ForestInk = "#15733F";
    public const string ForestTint = "#EDF8F1";
    public const string ForestBorder = "#BBE5CB";
    public const string AmberInk = "#92400E";
    public const string AmberTint = "#FEF9EC";

    static EditorialPdfTheme()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static void EnsureInitialized()
    {
        // Triggers static constructor once per process to configure Community license.
    }

    public static void ComposeMasthead(
        IContainer container,
        string classificationTag,
        string title,
        string subtitle,
        string generatedStamp)
    {
        container.Column(col =>
        {
            col.Item()
                .Background(Obsidian)
                .PaddingVertical(12)
                .PaddingHorizontal(18)
                .Row(row =>
                {
                    row.RelativeItem()
                        .AlignMiddle()
                        .AlignLeft()
                        .Height(28)
                        .Width(70)
                        .Svg(BrandAssets.LogoVectorSvg);

                    row.AutoItem()
                        .AlignMiddle()
                        .Text(classificationTag.ToUpperInvariant())
                        .FontSize(8.5f)
                        .SemiBold()
                        .FontColor("#A1A6B4");
                });

            col.Item().Height(3).Background(Lime);

            col.Item()
                .Border(1)
                .BorderColor(Border)
                .Background(Paper)
                .PaddingVertical(12)
                .PaddingHorizontal(18)
                .Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text(title).FontSize(17).Bold().FontColor(InkPrimary);
                        left.Item().PaddingTop(2).Text(subtitle).FontSize(9.5f).FontColor(InkSecondary);
                    });

                    row.AutoItem()
                        .AlignRight()
                        .AlignMiddle()
                        .Text(generatedStamp)
                        .FontSize(8.5f)
                        .FontColor(InkMuted);
                });
        });
    }

    public static void ComposeSectionHeader(IContainer container, string sectionCode, string title)
    {
        container
            .PaddingTop(14)
            .PaddingBottom(6)
            .BorderBottom(1)
            .BorderColor(Obsidian)
            .Row(row =>
            {
                row.AutoItem()
                    .PaddingRight(8)
                    .Text($"{sectionCode} /")
                    .FontSize(9)
                    .Bold()
                    .FontColor(ForestInk);

                row.RelativeItem()
                    .Text(title.ToUpperInvariant())
                    .FontSize(9.5f)
                    .Bold()
                    .FontColor(InkPrimary);
            });
    }

    public static void ComposeKpiTile(
        IContainer container,
        string label,
        string value,
        string? caption = null,
        bool accentForest = false)
    {
        container
            .Border(1)
            .BorderColor(accentForest ? ForestBorder : Border)
            .Background(accentForest ? ForestTint : SurfaceMuted)
            .Padding(10)
            .Column(col =>
            {
                col.Item().Text(label.ToUpperInvariant()).FontSize(7.5f).Bold().FontColor(InkMuted);
                col.Item()
                    .PaddingTop(3)
                    .Text(value)
                    .FontSize(18)
                    .Bold()
                    .FontColor(accentForest ? ForestInk : InkPrimary);

                if (!string.IsNullOrWhiteSpace(caption))
                {
                    col.Item().PaddingTop(2).Text(caption).FontSize(7.5f).FontColor(InkSecondary);
                }
            });
    }

    public static void ComposeTableHeaderCell(IContainer container, string label, bool alignRight = false)
    {
        var styled = container
            .Background(Obsidian)
            .PaddingVertical(6)
            .PaddingHorizontal(7);

        if (alignRight)
        {
            styled.AlignRight().Text(label.ToUpperInvariant()).FontSize(7.5f).Bold().FontColor(Paper);
        }
        else
        {
            styled.AlignLeft().Text(label.ToUpperInvariant()).FontSize(7.5f).Bold().FontColor(Paper);
        }
    }

    public static void ComposeTableCell(
        IContainer container,
        string value,
        bool alternate = false,
        bool bold = false,
        bool alignRight = false,
        string? textColor = null)
    {
        var styled = container
            .BorderBottom(1)
            .BorderColor(Border)
            .Background(alternate ? SurfaceMuted : Paper)
            .PaddingVertical(5)
            .PaddingHorizontal(7);

        var text = (alignRight ? styled.AlignRight() : styled.AlignLeft())
            .Text(value)
            .FontSize(8.5f)
            .FontColor(textColor ?? InkPrimary);

        if (bold)
            text.SemiBold();
    }

    public static void ComposeFooter(IContainer container)
    {
        container
            .PaddingTop(8)
            .BorderTop(1)
            .BorderColor(Border)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text("TeamUp Platform Governance · Immutable Administrative Ledger")
                    .FontSize(8)
                    .FontColor(InkMuted);

                row.AutoItem().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(InkMuted));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
    }
}
