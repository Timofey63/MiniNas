using Microsoft.AspNetCore.Http.Features;

// ngrok http 5000

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

var app = builder.Build();

// var storagePath = @"/home/td63d/storage";
var storagePath = @"D:\StorageNas";

if (!Directory.Exists(storagePath))
    Directory.CreateDirectory(storagePath);

app.UseStaticFiles();

app.MapPost("/upload", async (HttpRequest request) =>
{
    var files = request.Form.Files;

    long totalSize = files.Sum(f => f.Length);

    var drive = new DriveInfo(Path.GetPathRoot(storagePath)!);

    if (drive.AvailableFreeSpace < totalSize)
    {
        return Results.BadRequest("Not enough disk space");
    }

    foreach (var file in files)
    {
        if (file.Length == 0)
            continue;
        
        var originalName = Path.GetFileNameWithoutExtension(file.FileName);
        originalName = string.Concat(originalName.Split(Path.GetInvalidFileNameChars()));

        if (string.IsNullOrWhiteSpace(originalName))
            continue;

        var name = originalName.Length > 20 ? originalName[..20] : originalName;
        var timeCreate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var extension = Path.GetExtension(file.FileName);
        var safeFileName = $"{name}_{timeCreate}{extension}";

        var filePath = Path.Combine(storagePath, safeFileName);

        using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);
    }

    return Results.Ok("Uploaded");
});

app.MapGet("/download/{filename}", (string filename) =>
{
    var safeFileName = Path.GetFileName(filename);

    if (string.IsNullOrWhiteSpace(safeFileName))
        return Results.BadRequest();

    var filePath = Path.Combine(storagePath, safeFileName);

    if (!File.Exists(filePath))
        return Results.NotFound();

    return Results.File(filePath, "application/octet-stream", safeFileName);
});

app.MapGet("/files", () =>
{
    var files = Directory.GetFiles(storagePath)
        .Select(Path.GetFileName);

    return Results.Json(files);
});

app.MapDelete("/delete/{filename}", (string filename) =>
{
    var safeFileName = Path.GetFileName(filename);

    if (string.IsNullOrWhiteSpace(safeFileName))
        return Results.BadRequest();

    var filePath = Path.Combine(storagePath, safeFileName);

    if (!File.Exists(filePath))
        return Results.NotFound("Файл не найден");

    File.Delete(filePath);

    return Results.Ok("Удалено");
});

app.MapFallbackToFile("index.html");
app.Run("http://0.0.0.0:5000");