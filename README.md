# File Sharing API

The backend for a simple file-sharing app. Built with **.NET Core**, this API lets users upload and download files using **Amazon S3** for storage and **DynamoDB** for tracking file metadata.

---

## 🌐 Live App  
Visit the live application [here](http://44.215.35.137:3006/home)

---

## 🛠️ Tech Stack
- **.NET Core 9.0** – Web API framework  
- **AWS SDK for .NET**  
  - **S3** – File storage  
  - **DynamoDB** – Tracks uploaded files  
- **Entity Framework Core** – ORM  
- **DotNetEnv** – Loads environment variables from `.env`

---

## ✅ Prerequisites

You’ll need:

- [.NET Core SDK 9.0](https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian?tabs=dotnet9)
- An **AWS Account** with:
  - 📁 S3 Bucket
  - 🗃️ DynamoDB Table
  - 🔐 IAM User

---

## 🚀 AWS Setup

### 1. IAM User

- Go to the AWS Console → **IAM** → **Users**
- Create a user with **programmatic access**
- Attach these permissions:
  - `AmazonS3FullAccess`
  - `AmazonDynamoDBFullAccess`
- After creating the user, **copy the Access Key ID and Secret Access Key**
  > These go into your `.env` file (see below)

---

### 2. S3 Bucket

- Create a new S3 bucket (e.g., `your-s3-bucket-name`)
- Set region (e.g., `us-east-1`)
- Go to **Permissions → CORS** and paste:

```json
[
  {
    "AllowedHeaders": ["*"],
    "AllowedMethods": ["GET", "PUT", "POST", "DELETE", "HEAD"],
    "AllowedOrigins": ["http://your-frontend-domain.com"],
    "ExposeHeaders": ["ETag"],
    "MaxAgeSeconds": 3000
  }
]
````

> Replace `"http://your-frontend-domain.com"` with your actual frontend URL

---

### 3. DynamoDB Table

* Table name: `your-dynamodb-table-name`
* **Partition key**: `fileCode` (String)

#### Required Field:

* `fileCode` – Unique file ID (used as the primary key)

#### Optional Fields:

* `s3Key` – File path in the S3 bucket
* `createdAt` – Upload timestamp in ISO 8601 format

---

## ⚙️ Setup

1. Clone the backend:

```bash
git clone https://github.com/christianvieux/file-sharing-api.git
cd file-sharing-api
```

2. Install dependencies:

```bash
dotnet restore
```

3. Create a `.env` file in the root directory:

```env
AWS_ACCESS_KEY_ID=your_access_key_here
AWS_SECRET_ACCESS_KEY=your_secret_key_here
AWS_REGION=your_aws_region
S3_BUCKET_NAME=your_s3_bucket_name
DYNAMODB_TABLE_NAME=your_dynamodb_table_name
FILE_EXPIRATION_IN_MINUTES=60
```

4. Run the API:

```bash
dotnet run
# Or with a custom port:
dotnet run --urls "http://0.0.0.0:3005"
```

> The API will start on `http://localhost:5138` by default unless configured otherwise.

---

## 🧪 Development Mode

To run with hot reload:

```bash
dotnet watch run
# Or with a custom port:
dotnet watch run --urls "http://0.0.0.0:3005"
```

---

## 📡 API Endpoints

| Method | Endpoint                   | Description                |
| ------ | -------------------------- | -------------------------- |
| GET    | `/files`                   | List all uploaded files    |
| GET    | `/download-url/{FileCode}` | Generate download link     |
| GET    | `/generate-upload-url`     | Generate upload link       |
| POST   | `/upload-complete`         | Confirm upload is finished |

---

## 🔐 Environment Variables

| Key                          | Purpose                           |
| ---------------------------- | --------------------------------- |
| `AWS_ACCESS_KEY_ID`          | IAM user’s access key             |
| `AWS_SECRET_ACCESS_KEY`      | IAM user’s secret key             |
| `AWS_REGION`                 | AWS region (e.g. `us-east-1`)     |
| `S3_BUCKET_NAME`             | Your S3 bucket name               |
| `DYNAMODB_TABLE_NAME`        | Your DynamoDB table name          |
| `FILE_EXPIRATION_IN_MINUTES` | Link expiration time (in minutes) |

---

## 📂 Related Repositories

* [Main Project (Monorepo)](https://github.com/christianvieux/GA_Project_Final_File-Sharing-App)
* [Frontend Application](https://github.com/christianvieux/file-sharing-frontend)
