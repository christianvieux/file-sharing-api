using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;


public class DynamoDbService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public DynamoDbService(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME") ?? "TABLE_NOT_FOUND";
    }

    public async Task<List<Dictionary<string, string>>> ListAllFilesAsync()
    {
        var request = new ScanRequest
        {
            TableName = _tableName
        };

        var response = await _dynamoDbClient.ScanAsync(request);

        var simpleItems = new List<Dictionary<string, string>>();

        foreach (var item in response.Items)
        {
            var simpleItem = new Dictionary<string, string>();

            foreach (var kvp in item)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                // Pull only string or number values for now
                if (value.S != null) simpleItem[key] = value.S;
                else if (value.N != null) simpleItem[key] = value.N;
                else if (value.BOOL.HasValue) simpleItem[key] = value.BOOL.Value.ToString();
            }

            simpleItems.Add(simpleItem);
        }
        return simpleItems;
    }

    public async Task<string> StoreFileOnDB(string fileCode, string fileS3Key, string? createdAt = null)
    {
        createdAt ??= DateTime.UtcNow.ToString("o");
        var item = new Dictionary<string, AttributeValue>

        {
            { "fileCode", new AttributeValue { S = fileCode } }, // S stands for 'String' :P
            { "s3Key", new AttributeValue { S = fileS3Key } },
            { "createdAt", new AttributeValue { S = createdAt } }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDbClient.PutItemAsync(request);

        return fileCode;
    }

    public async Task<Dictionary<string, string>?> GetFile(string fileCode)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "fileCode = :v_code",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_code", new AttributeValue { S = fileCode } }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request);

        // If no item found, return null
        var item = response.Items.FirstOrDefault();
        if (item == null)
        {
            return null;
        }


        var result = new Dictionary<string, string>();
    
        foreach (var kvp in item)
        {
            // Only extract string values (from `S`), assuming the value type is always string for this case
            if (kvp.Value.S != null)
            {
                result[kvp.Key] = kvp.Value.S;
            }
        }

        return result;
    }
}