using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Grasshopper.Kernel;

namespace Pawper.Grasshopper;

/// <summary>Original vector-drawn icons, designed on the Grasshopper 24 px grid.</summary>
public static class PawperIcons
{
    static readonly Color Ink = Color.FromArgb(20, 78, 74);
    static readonly Color Paper = Color.FromArgb(255, 253, 245);
    static readonly Color Accent = Color.FromArgb(221, 116, 66);

    public static Bitmap Create(string key, int size = 24)
    {
        if (size is < 16 or > 1024) throw new ArgumentOutOfRangeException(nameof(size));
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.ScaleTransform(size / 24f, size / 24f);
        using var outline = new Pen(Ink, 1.45f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var ink = new SolidBrush(Ink);
        using var paper = new SolidBrush(Paper);
        using var accent = new SolidBrush(Accent);
        using var pale = new SolidBrush(Color.FromArgb(211, 232, 220));
        PointF[] page = [new(4, 2), new(15, 2), new(21, 8), new(21, 22), new(4, 22)];
        g.FillPolygon(paper, page); g.DrawPolygon(outline, page);
        g.FillPolygon(pale, (PointF[])[new(15, 2), new(15, 8), new(21, 8)]);
        g.DrawLines(outline, (PointF[])[new(15, 2), new(15, 8), new(21, 8)]);
        void Line(float x1, float y1, float x2, float y2) => g.DrawLine(outline, x1, y1, x2, y2);
        void Text(string value, float points = 9, float x = 6, float y = 9)
        {
            using var font = new Font("Segoe UI", points, FontStyle.Bold, GraphicsUnit.Pixel);
            g.DrawString(value, font, ink, x, y, StringFormat.GenericTypographic);
        }
        void Grid(bool cell = false)
        {
            g.FillRectangle(pale, 7, 10, 11, 3);
            g.DrawRectangle(outline, 7, 10, 11, 9);
            Line(7, 13, 18, 13); Line(7, 16, 18, 16); Line(11, 10, 11, 19); Line(15, 10, 15, 19);
            if (cell) g.FillRectangle(accent, 11.6f, 13.6f, 2.8f, 1.8f);
        }
        key = key.Replace("Component", "", StringComparison.Ordinal);
        switch (key)
        {
            case "Pawper":
                g.FillEllipse(ink, 8.5f, 14.5f, 8, 5.5f);
                g.FillEllipse(ink, 7, 11.6f, 3.2f, 4);
                g.FillEllipse(ink, 10.3f, 9.5f, 3.2f, 4);
                g.FillEllipse(ink, 14, 10, 3.1f, 4);
                g.FillEllipse(ink, 17, 12.4f, 2.8f, 3.8f);
                break;
            case "NewDocument": case "Document":
                Line(8, 14, 17, 14); Line(12.5f, 9.5f, 12.5f, 18.5f); break;
            case "Compose": case "ContentBlock":
                g.FillRectangle(pale, 7, 10, 11, 3); g.FillRectangle(ink, 7, 14, 11, 2); g.FillRectangle(accent, 7, 18, 11, 2); break;
            case "TextBlocks": Text("#", 9, 6, 8); Line(13, 12, 18, 12); Line(7, 16, 18, 16); Line(7, 19, 15, 19); break;
            case "QuickDocument": Text("+", 13, 7, 7); g.FillEllipse(accent, 15, 17, 4, 4); break;
            case "FillFields": Text("=", 11, 8, 8); g.FillEllipse(accent, 7, 18, 3, 3); Line(12, 19, 18, 19); break;
            case "ParagraphFormat": Line(7, 11, 18, 11); Line(9, 15, 16, 15); Line(7, 19, 18, 19); g.FillEllipse(accent, 3, 13, 3, 3); break;
            case "TableFormat": Grid(); g.FillRectangle(accent, 7.5f, 10.5f, 10, 2); break;
            case "PagePreset": Text("A4", 8, 6, 12); break;
            case "TextRun": Text("Ab"); break;
            case "Paragraph": Line(7, 11, 18, 11); Line(7, 14.5f, 18, 14.5f); Line(7, 18, 14, 18); break;
            case "Heading": Text("H", 12, 8, 8); break;
            case "List":
                foreach (float y in new[] { 11f, 15f, 19f }) { g.FillEllipse(accent, 7, y - 1, 2, 2); Line(11, y, 18, y); } break;
            case "Style": Text("A", 12, 7, 8); g.FillEllipse(accent, 16, 17, 3, 3); break;
            case "Table": case "TableRows": Grid(); if (key == "TableRows") g.FillRectangle(accent, 7.5f, 17, 10, 1.2f); break;
            case "QuickTable": Grid(); g.FillPolygon(accent, (PointF[])[new(3, 7), new(7, 3), new(6, 7), new(9, 7), new(4, 12), new(5, 8)]); break;
            case "TextTable": Grid(); g.FillRectangle(paper, 1, 7, 9, 6); Text("T", 7, 2, 6); Line(6, 10, 9, 10); break;
            case "Cell": Grid(true); break;
            case "Image":
                g.FillEllipse(accent, 14, 10, 3, 3); g.FillPolygon(ink, (PointF[])[new(7, 19), new(11, 13), new(14, 17), new(16, 15), new(19, 19)]); break;
            case "CaptureView":
                g.DrawRectangle(outline, 7, 12, 11, 7); g.FillRectangle(ink, 9, 10, 5, 2); g.DrawEllipse(outline, 10, 13.5f, 4.5f, 4.5f); break;
            case "Equation": Text("Σ", 13, 8, 7); break;
            case "Number": Text("#", 13, 8, 7); break;
            case "Layout": case "PageLayout":
                g.DrawRectangle(outline, 7, 10, 11, 10); g.FillRectangle(pale, 9, 12, 7, 6); break;
            case "Section": Line(7, 10, 18, 10); Line(7, 15, 18, 15); g.FillRectangle(accent, 7, 17, 11, 3); break;
            case "Break": Line(7, 11, 18, 11); Line(7, 19, 18, 19); Line(7, 15, 10, 15); Line(15, 15, 18, 15); break;
            case "Field": Text("{#}", 8, 6, 10); break;
            case "Bookmark":
                g.FillPolygon(accent, (PointF[])[new(9, 10), new(16, 10), new(16, 20), new(12.5f, 17), new(9, 20)]); break;
            case "Note": Line(7, 12, 18, 12); Line(7, 17, 14, 17); Line(7, 20, 18, 20); Text("1", 6, 16, 15); break;
            case "Comment":
                g.FillRectangle(pale, 7, 10, 11, 7); g.DrawPolygon(outline, (PointF[])[new(7, 10), new(18, 10), new(18, 17), new(11, 17), new(8, 20), new(8, 17), new(7, 17)]); break;
            case "Control": case "Target": case "TemplateTarget":
                g.DrawRectangle(outline, 7, 10, 11, 9); g.FillEllipse(accent, 10.5f, 12.5f, 4, 4); break;
            case "FillTemplate": Text("{ }", 10, 6, 9); g.FillEllipse(accent, 11.3f, 14, 2.5f, 2.5f); break;
            case "Repeat":
                g.DrawArc(outline, 8, 10, 9, 9, 45, 270); g.FillPolygon(accent, (PointF[])[new(17, 9), new(18, 14), new(13, 12)]); break;
            case "Conditional": Text("?", 12, 9, 8); break;
            case "Metadata": Text("i", 13, 10, 7); break;
            case "Inspector":
                g.DrawEllipse(outline, 7, 10, 7, 7); Line(13, 16, 18, 21); break;
            case "Save": case "Batch":
                g.FillRectangle(ink, 7, 10, 11, 10); g.FillRectangle(paper, 9, 10, 6, 3); g.FillRectangle(paper, 9, 16, 7, 4);
                if (key == "Batch") { g.FillEllipse(accent, 16, 16, 6, 6); } break;
            case "Import": Line(6, 14, 17, 14); Line(13, 10, 17, 14); Line(13, 18, 17, 14); break;
            case "OpenResult": Line(8, 18, 17, 10); Line(11, 10, 17, 10); Line(17, 10, 17, 16); break;
            case "RelativePath":
                g.DrawLines(outline, (PointF[])[new(7, 19), new(7, 14), new(17, 14), new(17, 10)]); g.FillEllipse(accent, 5.5f, 17.5f, 3, 3); g.FillEllipse(ink, 15.5f, 8.5f, 3, 3); break;
            default: throw new ArgumentException("Icona Pawper non definita: " + key, nameof(key));
        }
        // Small terracotta seal visually links the functional icons to the Pawper family.
        if (key != "Pawper") g.FillEllipse(accent, 1, 18, 4.5f, 4.5f);
        return bitmap;
    }
}

public sealed class PawperPriority : GH_AssemblyPriority
{
    public override GH_LoadingInstruction PriorityLoad()
    {
        global::Grasshopper.Instances.ComponentServer.AddCategoryIcon("Pawper", PawperIcons.Create("Pawper", 16));
        global::Grasshopper.Instances.ComponentServer.AddCategoryShortName("Pawper", "Paw");
        global::Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Pawper", 'P');
        Gpc.PluginChecks.PluginBuildCheck.Start(typeof(PawperPriority).Assembly, "Pawper");
        return GH_LoadingInstruction.Proceed;
    }
}
