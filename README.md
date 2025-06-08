# File Sharing API

The server-side API for the file sharing application. Built with .NET Core, this API handles file operations using AWS services (S3 for storage and DynamoDB for database).

## 🌐 Live App

Visit the live application [here.](http://44.203.74.69:3006/home)

## Tech Stack

- **.NET Core 9.0** - Web API framework
- **AWS SDK**
  - S3 - File storage
  - DynamoDB - Database
- **Entity Framework Core** - Database ORM
- **DotNetEnv** - Environment variable management

## Prerequisites

- [.NET Core SDK 9.0](https://dotnet.microsoft.com/download) or higher
- AWS Account with:
  - S3 bucket
  - DynamoDB table
  - IAM user with appropriate permissions

> **Note:** Setting up the AWS resources (S3, DynamoDB, IAM) can be time-consuming and may require careful configuration. Please ensure you have experience with AWS or allocate sufficient time for the setup process.

## Setup

1. Clone the repository:
   > **Note:** Skip this step if you already cloned this project folder from the mono repo

    ```sh
    git clone https://github.com/christianvieux/file-sharing-api.git
    ```


2. Create a `.env` file in the root directory with:

    ```sh
    AWS_ACCESS_KEY_ID=your_access_key
    AWS_SECRET_ACCESS_KEY=your_secret_key
    AWS_REGION=your_region
    S3_BUCKET_NAME=your_bucket_name
    DYNAMODB_TABLE_NAME=your_table_name
    FILE_EXPIRATION_IN_MINUTES=60
    ```

3. Install dependencies:

    ```sh
    dotnet restore
    ```

4. Run the API:

    ```sh
    dotnet run
    ```

The API will start at `http://localhost:5138` by default. If url doesn't work, check the console.

## Development

To run in development mode with hot reload:

```sh
dotnet watch run
```

## API Endpoints

- `GET /files` - List all files
- `GET /download-url/{FileCode}` - Get download URL for a file
- `GET /generate-upload-url` - Generate upload URL
- `POST /upload-complete` - Mark upload as complete

## Environment Variables

- `AWS_ACCESS_KEY_ID` - AWS access key
- `AWS_SECRET_ACCESS_KEY` - AWS secret key
- `AWS_REGION` - AWS region (e.g., us-east-1)
- `S3_BUCKET_NAME` - S3 bucket for file storage
- `DYNAMODB_TABLE_NAME` - DynamoDB table for metadata
- `FILE_EXPIRATION_IN_MINUTES` - File link expiration time
