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

var storagePath = @"/home/td63d/storage";

if (!Directory.Exists(storagePath))
    Directory.CreateDirectory(storagePath);

app.UseStaticFiles();

app.MapPost("/upload", async (HttpRequest request) =>
{
    var files = request.Form.Files;

    foreach (var file in files)
    {
        var safeFileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(storagePath, safeFileName);

        using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);
    }

    return Results.Ok("Uploaded");
});

app.MapGet("/download/{filename}", (string filename) =>
{
    var safeFileName = Path.GetFileName(filename);
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
    var filePath = Path.Combine(storagePath, safeFileName);

    if (!File.Exists(filePath))
        return Results.NotFound("Файл не найден");

    File.Delete(filePath);

    return Results.Ok("Удалено");
});

app.MapFallbackToFile("index.html");
app.Run("http://0.0.0.0:5000");