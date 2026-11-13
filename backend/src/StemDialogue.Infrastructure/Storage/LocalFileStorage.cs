using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StemDialogue.Application.Abstractions;

namespace StemDialogue.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string Section = "Storage";

    /// <summary>Folder for uploads; relative paths resolve against the content root. Mount it as a persistent volume.</summary>
    public string RootPath { get; set; } = "App_Data/media";

    /// <summary>URL prefix the files are served from (by the API or a CDN), e.g. <c>/media</c> or <c>https://cdn.example/media</c>.</summary>
    public string PublicBasePath { get; set; } = "/media";
}

/// <summary>Stores uploads on the local file system (or a mounted volume).</summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly string _publicBase;

    public LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment environment)
    {
        var configured = options.Value.RootPath;
        _root = Path.GetFullPath(Path.IsPathRooted(configured) ? configured : Path.Combine(environment.ContentRootPath, configured));
        _publicBase = options.Value.PublicBasePath.TrimEnd('/');
        Directory.CreateDirectory(_root);
    }

    public string RootPath => _root;

    public async Task SaveAsync(Stream content, string relativePath, CancellationToken ct)
    {
        var path = Resolve(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(file, ct);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct)
    {
        var path = Resolve(relativePath);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string relativePath) => $"{_publicBase}/{relativePath.Replace('\\', '/')}";

    private string Resolve(string relativePath)
    {
        var full = Path.GetFullPath(Path.Combine(_root, relativePath));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid storage path.");
        return full;
    }
}
