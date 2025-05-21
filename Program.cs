using DotNetEnv; // Import the library
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc; // needed for the [FromBody]
// Storage/S3 (AWS)
using Amazon;
using Amazon.S3; // For S3 buckets
using Amazon.Extensions.NETCore.Setup;
// Database (AWS)
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

// Load .env file
DotNetEnv.Env.Load(); // Or Env.Load("path/to/.env");

// Build :P
var builder = WebApplication.CreateBuilder(args);


// The following (2 lines below) adds the database context to the dependency injection (DI) container and enables displaying database-related exceptions
// builder.Services.AddDbContext<FileDB>(opt => opt.UseInMemoryDatabase("FileList"));
// builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// ! I removed the stuff above ^^ this was from the microsoft tutorial page. Don't think I need it no more :P

// S3 BUCKET
builder.Services.AddSingleton<S3Service>();
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var regionName = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

    // Convert region string to RegionEndpoint enum
    var region = RegionEndpoint.GetBySystemName(regionName);

    var config = new AmazonS3Config
    {
        RegionEndpoint = region
    };

    var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
    var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
    return new AmazonS3Client(accessKey, secretKey, config);
});
//DynamoDB
builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var regionName = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
    var region = RegionEndpoint.GetBySystemName(regionName);

    var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
    var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

    var config = new AmazonDynamoDBConfig
    {
        RegionEndpoint = region
    };

    return new AmazonDynamoDBClient(accessKey, secretKey, config);
});
builder.Services.AddSingleton<DynamoDbService>();
// Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// --------------------------------[[ BUILD ]]------------------------------------

var app = builder.Build();


// --------------------------------[[ TEST endpoints S3/DynamoDB ]]------------------------------------
app.MapGet("/test-s3-bucket", async (IAmazonS3 s3) =>
{
    try
    {
        var response = await s3.GetBucketLocationAsync(new Amazon.S3.Model.GetBucketLocationRequest
        {
            BucketName = "file-share-app-uploads"
        });
        return Results.Ok($"Bucket location: {response.Location}");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/files", async (DynamoDbService dynamoService) =>
{
    var files = await dynamoService.ListAllFilesAsync();
    return Results.Ok(files);
});


// --------------------------------[[ DEFAULT ]]------------------------------------
app.MapGet("/", () => "Hello World!");


// --------------------------------[[ GET ]]------------------------------------
app.MapGet("/download-url/{FileCode}", async (string FileCode, DynamoDbService dynamoService, S3Service s3Service) =>
{
    var file = await dynamoService.GetFile(FileCode);

    // Check if the file exists
    if (file == null)
        return Results.NotFound(new { error = "File not found." });

    // Check if the timestap value (createdAt) is valid or not
    if (!file.TryGetValue("createdAt", out var createdAtStr) ||
    !DateTime.TryParseExact(createdAtStr, "o", System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
        out var createdAt))
        return Results.BadRequest(new { error = "Invalid file timestamp." });

    // Check if it's expired
    var expiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("FILE_EXPIRATION_IN_MINUTES"), out var minutes) ? minutes : 60; // env variable or 60 (default)
    Console.WriteLine(DateTime.UtcNow);
    Console.WriteLine(createdAt.AddMinutes(expiryMinutes));
    if (DateTime.UtcNow > createdAt.AddMinutes(expiryMinutes))
        return Results.BadRequest(new { error = "File has expired." });

    // Get a presigned (meaning authored) url
    var s3Key = file["s3Key"];
    var downloadUrl = s3Service.GeneratePreSignedDownloadURL(s3Key);

    return Results.Ok(new { downloadUrl });
});

app.MapGet("/generate-upload-url", (string FileName, S3Service s3Service) =>
{
    // TODO: this works but it's flawed, use a better way to generate code.
    var FileCode = Guid.NewGuid().ToString("N").Substring(0, 6); // A unique ID being generated 
    var FileS3Key = $"{FileCode}_{FileName}";
    var UploadUrl = s3Service.GeneratePreSignedUploadURL(FileS3Key);

    return Results.Ok(new { FileCode, FileS3Key, UploadUrl });
});

// --------------------------------[[ POST ]]------------------------------------
app.MapPost("/upload-complete", async ([FromBody] UploadCompleteRequest request, DynamoDbService dynamoService) =>
{
    var fileCode = await dynamoService.StoreFileOnDB(request.FileCode, request.FileS3Key);

    return Results.Ok(new { fileCode });
});

// --------------------------------[[ CORS (Settings) ]]------------------------------------
app.UseCors("AllowAll");

// --------------------------------[[ RUN ]]------------------------------------
app.Run();






// DELETE

// app.MapGet("/fileitems", async (FileDB db) =>
//     await db.Files.ToListAsync());

// app.MapGet("/fileitems/complete", async (FileDB db) =>
//     await db.Files.Where(t => t.IsComplete).ToListAsync());

// app.MapGet("/fileitems/{id}", async (int id, FileDB db) =>
//     await db.Files.FindAsync(id)
//         is File file
//             ? Results.Ok(file)
//             : Results.NotFound());



// app.MapPut("/fileitems/{id}", async (int id, File inputFile, FileDB db) =>
// {
//     var file = await db.Files.FindAsync(id);

//     if (file is null) return Results.NotFound();

//     file.Name = inputFile.Name;
//     file.IsComplete = inputFile.IsComplete;

//     await db.SaveChangesAsync();

//     return Results.NoContent();
// });

// app.MapDelete("/fileitems/{id}", async (int id, FileDB db) =>
// {
//     if (await db.Files.FindAsync(id) is File file)
//     {
//         db.Files.Remove(file);
//         await db.SaveChangesAsync();
//         return Results.NoContent();
//     }

//     return Results.NotFound();
// });
