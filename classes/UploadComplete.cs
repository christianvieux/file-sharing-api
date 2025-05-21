using System.Globalization;

public class UploadCompleteRequest
{
    public string FileCode { get; set; } = string.Empty;
    public string FileS3Key { get; set; } = string.Empty;
}
