using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MatrixCalculator.Models;

namespace MatrixCalculator.Services;

public static partial class ExcelMatrixService
{
    private static readonly XNamespace Spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PackageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";

    public static Matrix ReadFirstSheet(string filePath, CultureInfo culture)
    {
        using var archive = ZipFile.OpenRead(filePath);
        var sharedStrings = ReadSharedStrings(archive);
        var sheetPath = GetFirstSheetPath(archive);
        var sheetEntry = archive.GetEntry(sheetPath) ?? throw new InvalidDataException("Worksheet was not found.");
        using var stream = sheetEntry.Open();
        var document = XDocument.Load(stream);

        var values = new Dictionary<(int Row, int Column), double>();
        var maxRow = 0;
        var maxColumn = 0;

        foreach (var cell in document.Descendants(Spreadsheet + "c"))
        {
            var reference = (string?)cell.Attribute("r");
            if (string.IsNullOrWhiteSpace(reference) || !TryParseCellReference(reference, out var row, out var column))
            {
                continue;
            }

            var text = ReadCellText(cell, sharedStrings);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (!TryParseNumber(text, culture, out var value))
            {
                throw new FormatException($"Cell {reference} contains a non-numeric value: \"{text}\".");
            }

            values[(row, column)] = value;
            maxRow = Math.Max(maxRow, row);
            maxColumn = Math.Max(maxColumn, column);
        }

        if (maxRow == 0 || maxColumn == 0)
        {
            throw new InvalidDataException("The first worksheet does not contain numeric matrix data.");
        }

        var matrix = new Matrix(maxRow, maxColumn);
        foreach (var (position, value) in values)
        {
            matrix[position.Row - 1, position.Column - 1] = value;
        }

        return matrix;
    }

    public static void WriteMatrix(string filePath, Matrix matrix, CultureInfo culture)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
        WriteEntry(archive, "[Content_Types].xml", BuildContentTypes());
        WriteEntry(archive, "_rels/.rels", BuildPackageRelationships());
        WriteEntry(archive, "xl/workbook.xml", BuildWorkbook());
        WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationships());
        WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheet(matrix, culture));
    }

    private static string[] ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        return document
            .Descendants(Spreadsheet + "si")
            .Select(item => string.Concat(item.Descendants(Spreadsheet + "t").Select(text => text.Value)))
            .ToArray();
    }

    private static string GetFirstSheetPath(ZipArchive archive)
    {
        var workbookEntry = archive.GetEntry("xl/workbook.xml") ?? throw new InvalidDataException("Workbook was not found.");
        using var workbookStream = workbookEntry.Open();
        var workbook = XDocument.Load(workbookStream);
        var firstSheet = workbook.Descendants(Spreadsheet + "sheet").FirstOrDefault() ?? throw new InvalidDataException("Workbook does not contain worksheets.");
        var relationshipId = (string?)firstSheet.Attribute(Relationships + "id") ?? throw new InvalidDataException("Worksheet relationship was not found.");

        var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels") ?? throw new InvalidDataException("Workbook relationships were not found.");
        using var relsStream = relsEntry.Open();
        var rels = XDocument.Load(relsStream);
        var target = rels
            .Descendants(PackageRelationships + "Relationship")
            .FirstOrDefault(item => (string?)item.Attribute("Id") == relationshipId)
            ?.Attribute("Target")
            ?.Value;

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new InvalidDataException("Worksheet target was not found.");
        }

        return target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)
            ? target
            : $"xl/{target.TrimStart('/')}";
    }

    private static string ReadCellText(XElement cell, IReadOnlyList<string> sharedStrings)
    {
        var type = (string?)cell.Attribute("t");
        if (type == "s")
        {
            var indexText = cell.Element(Spreadsheet + "v")?.Value ?? string.Empty;
            return int.TryParse(indexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
                && index >= 0
                && index < sharedStrings.Count
                ? sharedStrings[index]
                : string.Empty;
        }

        if (type == "inlineStr")
        {
            return string.Concat(cell.Descendants(Spreadsheet + "t").Select(text => text.Value));
        }

        return cell.Element(Spreadsheet + "v")?.Value ?? string.Empty;
    }

    private static bool TryParseCellReference(string reference, out int row, out int column)
    {
        var match = CellReferenceRegex().Match(reference);
        if (!match.Success)
        {
            row = 0;
            column = 0;
            return false;
        }

        column = 0;
        foreach (var letter in match.Groups["column"].Value)
        {
            column = (column * 26) + char.ToUpperInvariant(letter) - 'A' + 1;
        }

        row = int.Parse(match.Groups["row"].Value, CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryParseNumber(string text, CultureInfo culture, out double value)
    {
        var normalized = text.Trim();
        return double.TryParse(normalized, NumberStyles.Float, culture, out value)
            || double.TryParse(normalized, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(normalized.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static string BuildWorksheet(Matrix matrix, CultureInfo culture)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        builder.Append("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>""");

        for (var row = 0; row < matrix.Rows; row++)
        {
            builder.Append(CultureInfo.InvariantCulture, $"""<row r="{row + 1}">""");
            for (var column = 0; column < matrix.Columns; column++)
            {
                var reference = $"{ColumnName(column + 1)}{row + 1}";
                var value = matrix[row, column].ToString("0.############", CultureInfo.InvariantCulture);
                builder.Append(CultureInfo.InvariantCulture, $"""<c r="{reference}"><v>{value}</v></c>""");
            }

            builder.Append("</row>");
        }

        builder.Append("</sheetData></worksheet>");
        return builder.ToString();
    }

    private static string ColumnName(int column)
    {
        var name = string.Empty;
        while (column > 0)
        {
            column--;
            name = (char)('A' + column % 26) + name;
            column /= 26;
        }

        return name;
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(content.TrimStart());
    }

    private static string BuildContentTypes()
    {
        return """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
              <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
            </Types>
            """;
    }

    private static string BuildPackageRelationships()
    {
        return """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
            </Relationships>
            """;
    }

    private static string BuildWorkbook()
    {
        return """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets>
                <sheet name="Result" sheetId="1" r:id="rId1"/>
              </sheets>
            </workbook>
            """;
    }

    private static string BuildWorkbookRelationships()
    {
        return """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
            </Relationships>
            """;
    }

    [GeneratedRegex(@"^(?<column>[A-Za-z]+)(?<row>\d+)$")]
    private static partial Regex CellReferenceRegex();
}
