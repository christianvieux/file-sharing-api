using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;


// var secret = Environment.GetEnvironmentVariable("MY_SECRET_KEY");

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration config)
    {
        _s3Client = s3Client;
        _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "BUCKET_MISSING";
    }

    public string GeneratePreSignedUploadURL(string objectKey, int expiryInMinutes = 15)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
            // ContentType = "application/octet-stream"
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public string GeneratePreSignedDownloadURL(string objectKey, int expiryInMinutes = 15)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes)
        };

        return _s3Client.GetPreSignedURL(request);
    }
}