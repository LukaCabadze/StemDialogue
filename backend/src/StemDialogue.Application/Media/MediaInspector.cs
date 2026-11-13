using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;
using StemDialogue.Domain.Media;

namespace StemDialogue.Application.Media;

public sealed class MediaOptions
{
    public const string Section = "Media";

    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxDocumentBytes { get; set; } = 50 * 1024 * 1024;
}

public sealed record InspectedFile(string Extension, string ContentType, MediaKind Kind, int? Width, int? Height);

/// <summary>
/// Server-side type check of uploads (spec §11): the extension must be allowed <em>and</em> the file content
/// must match it. SVG is accepted only without scripts or event handlers.
/// </summary>
public static partial class MediaInspector
{
    private sealed record FileType(string ContentType, MediaKind Kind, Func<byte[], bool> Matches);

    private static readonly byte[] Zip = [0x50, 0x4B, 0x03, 0x04];

    private static readonly Dictionary<string, FileType> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new("image/jpeg", MediaKind.Image, h => StartsWith(h, [0xFF, 0xD8, 0xFF])),
        [".jpeg"] = new("image/jpeg", MediaKind.Image, h => StartsWith(h, [0xFF, 0xD8, 0xFF])),
        [".png"] = new("image/png", MediaKind.Image, h => StartsWith(h, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])),
        [".gif"] = new("image/gif", MediaKind.Image, h => StartsWith(h, "GIF87a"u8.ToArray()) || StartsWith(h, "GIF89a"u8.ToArray())),
        [".webp"] = new("image/webp", MediaKind.Image, h => StartsWith(h, "RIFF"u8.ToArray()) && h.Length >= 12 && h.AsSpan(8, 4).SequenceEqual("WEBP"u8)),
        [".svg"] = new("image/svg+xml", MediaKind.Image, LooksLikeSvg),
        [".pdf"] = new("application/pdf", MediaKind.Document, h => StartsWith(h, "%PDF-"u8.ToArray())),
        [".docx"] = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document", MediaKind.Document, h => StartsWith(h, Zip)),
        [".pptx"] = new("application/vnd.openxmlformats-officedocument.presentationml.presentation", MediaKind.Presentation, h => StartsWith(h, Zip)),
        [".ppsx"] = new("application/vnd.openxmlformats-officedocument.presentationml.slideshow", MediaKind.Presentation, h => StartsWith(h, Zip)),
        [".xlsx"] = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", MediaKind.Spreadsheet, h => StartsWith(h, Zip)),
    };

    public static IReadOnlyCollection<string> AllowedExtensions => Allowed.Keys;

    /// <summary>Returns the detected type, or an error message.</summary>
    public static (InspectedFile? File, string? Error) Inspect(string fileName, ReadOnlySpan<byte> head, long length, MediaOptions options)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!Allowed.TryGetValue(extension, out var type))
            return (null, $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", Allowed.Keys)}. Video should be linked from a video platform.");

        if (length <= 0) return (null, "The file is empty.");

        var limit = type.Kind == MediaKind.Image ? options.MaxImageBytes : options.MaxDocumentBytes;
        if (length > limit) return (null, $"The file is too large. The limit for this type is {limit / (1024 * 1024)} MB.");

        var bytes = head.ToArray();
        if (!type.Matches(bytes)) return (null, "The file content does not match its extension.");

        if (extension == ".svg" && !IsSafeSvg(bytes))
            return (null, "SVG files must not contain scripts, event handlers or external entities.");

        var (width, height) = type.Kind == MediaKind.Image ? ImageHeaderReader.ReadSize(bytes) : (null, null);
        return (new InspectedFile(extension, type.ContentType, type.Kind, width, height), null);
    }

    private static bool StartsWith(byte[] data, byte[] prefix) => data.Length >= prefix.Length && data.AsSpan(0, prefix.Length).SequenceEqual(prefix);

    private static bool LooksLikeSvg(byte[] head)
    {
        var text = Encoding.UTF8.GetString(head).TrimStart('﻿', ' ', '\t', '\r', '\n');
        return (text.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) || text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            && text.Contains("<svg", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafeSvg(byte[] content)
    {
        var text = Encoding.UTF8.GetString(content);
        return !UnsafeSvg().IsMatch(text);
    }

    // Blocks scripts, handlers, entities and external references; allows in-document (#id) and embedded raster images.
    [GeneratedRegex(@"<\s*script|<\s*foreignObject|<!ENTITY|javascript\s*:|\son[a-z]+\s*=|(?:xlink:)?href\s*=\s*[""']\s*(?!#|data:image/(?:png|jpeg|gif|webp);)", RegexOptions.IgnoreCase)]
    private static partial Regex UnsafeSvg();
}

/// <summary>Reads pixel dimensions from image headers so the UI can reserve space and keep logo proportions.</summary>
public static class ImageHeaderReader
{
    public static (int? Width, int? Height) ReadSize(byte[] data)
    {
        try
        {
            if (data.Length >= 24 && data[0] == 0x89 && data[1] == 0x50)
                return (BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(16)), BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(20)));

            if (data.Length >= 10 && data[0] == 'G' && data[1] == 'I' && data[2] == 'F')
                return (BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(6)), BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(8)));

            if (data.Length >= 30 && data[0] == 'R' && data[8] == 'W')
                return ReadWebp(data);

            if (data.Length >= 4 && data[0] == 0xFF && data[1] == 0xD8)
                return ReadJpeg(data);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Truncated or malformed header: dimensions are optional.
        }

        return (null, null);
    }

    private static (int?, int?) ReadWebp(byte[] d)
    {
        var chunk = Encoding.ASCII.GetString(d, 12, 4);
        return chunk switch
        {
            "VP8 " => (BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(26)) & 0x3FFF, BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(28)) & 0x3FFF),
            "VP8L" => (1 + (((d[22] & 0x3F) << 8) | d[21]), 1 + (((d[24] & 0x0F) << 10) | (d[23] << 2) | ((d[22] & 0xC0) >> 6))),
            "VP8X" => (1 + (d[24] | (d[25] << 8) | (d[26] << 16)), 1 + (d[27] | (d[28] << 8) | (d[29] << 16))),
            _ => (null, null),
        };
    }

    private static (int?, int?) ReadJpeg(byte[] d)
    {
        var i = 2;
        while (i + 9 < d.Length)
        {
            if (d[i] != 0xFF) { i++; continue; }
            var marker = d[i + 1];
            if (marker == 0xFF) { i++; continue; }
            if (marker is 0xD8 or 0x01 || marker is >= 0xD0 and <= 0xD7) { i += 2; continue; }

            var length = BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan(i + 2));
            var isStartOfFrame = marker is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC;
            if (isStartOfFrame)
                return (BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan(i + 7)), BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan(i + 5)));

            i += 2 + length;
        }

        return (null, null);
    }
}
