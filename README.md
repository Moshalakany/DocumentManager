# Document Manager API

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Prerequisites](#prerequisites)
- [Setup and Installation (Local)](#setup-and-installation-local)
  - [1. Clone Repository](#1-clone-repository)
  - [2. Configure Application Settings](#2-configure-application-settings)
  - [3. Database Setup](#3-database-setup)
  - [4. Run the Application](#4-run-the-application)
- [Running with Docker (Optional)](#running-with-docker-optional)
  - [1. Navigate to Solution Directory](#1-navigate-to-solution-directory)
  - [2. Build the Docker Image](#2-build-the-docker-image)
  - [3. Run the Docker Container](#3-run-the-docker-container)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)

## Introduction
The Document Manager API is a comprehensive backend solution built with ASP.NET Core for managing documents, user access, and related functionalities. It provides a secure and organized way to handle digital documents, incorporating features like versioning, tagging, access control, and more.

## Features
- **User Authentication & Authorization:** Secure user registration, login (with JWT), token refresh, and role-based access (Admin/User).
- **Document Management:** Upload, download, retrieve details, and list documents specific to users.
- **File Validation:** Server-side validation for uploaded files (type, size).
- **Tagging System:** Create, assign, and manage tags for efficient document organization.
- **Folder Management:** Organize documents within a folder structure.
- **Access Control:** Fine-grained permissions for document access (e.g., view, edit, download, share, delete, annotate) assignable to individual users and groups.
- **Annotations:** Functionality to add and manage annotations on documents.
- **API Documentation:** Integrated Swagger (OpenAPI) for easy API exploration and testing.

## Technologies Used
- **Backend Framework:** C#, ASP.NET Core (.NET 9)
- **Databases:**
  - Microsoft SQL Server (Primary data store)
  - MongoDB (Configured for potential use with specific data types or features)
- **Object-Relational Mapper (ORM):** Entity Framework Core
- **Authentication:** JSON Web Tokens (JWT)
- **API Documentation:** Swagger / OpenAPI
- **Containerization:** Docker
- **Global Usings & Implicit Usings:** Leverages modern C# features for cleaner code.

## Prerequisites
Before you begin, ensure you have the following installed:
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Microsoft SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (e.g., Express, Developer, or any other edition)
- (Optional) [MongoDB](https://www.mongodb.com/try/download/community)
- (Optional) [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- A code editor like Visual Studio Code or Visual Studio.
- Git for cloning the repository.
- [dotnet-ef tool](https://docs.microsoft.com/ef/core/cli/dotnet) (Install via `dotnet tool install --global dotnet-ef`)

## Setup and Installation (Local)

### 1. Clone Repository
Clone the repository to your local machine.
```bash
git clone <your-repository-url>
cd "Document Manager/Document Manager" # Navigate to the project directory
```
(Assuming `<your-repository-url>` points to the root of the solution, and the project is in a subdirectory `Document Manager/Document Manager` or adjust path accordingly if your structure is different. The target directory is `e:\my Own projects repo\c#\Document Manager\Document Manager`)

### 2. Configure Application Settings
Application settings are managed in `appsettings.json` and `appsettings.Development.json` within the project directory (`e:\my Own projects repo\c#\Document Manager\Document Manager`). You may need to create `appsettings.Development.json` if it doesn't exist or modify `appsettings.json`.

**Example `appsettings.json` or `appsettings.Development.json` structure:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SqlConnectionStrings": "Server=(localdb)\\mssqllocaldb;Database=DocumentManagerDB;Trusted_Connection=True;MultipleActiveResultSets=true",
  "MongoConnectionStrings": "mongodb://localhost:27017",
  "MongoDbName": "DocumentManagerMongoDev",
  "AppSettings": {
    "Token": "YOUR_VERY_STRONG_AND_SECRET_JWT_SIGNING_KEY_REPLACE_THIS",
    "Issuer": "https://localhost:your_port", // e.g., https://localhost:7001
    "Audience": "https://localhost:your_port" // e.g., https://localhost:7001
  }
}
```
**Important Configuration Notes:**
- **SQL Server:** Update `SqlConnectionStrings` to point to your SQL Server instance.
- **MongoDB:** Update `MongoConnectionStrings` and `MongoDbName` if you are using MongoDB and it's not running on the default local port or you prefer a different database name.
- **JWT Settings:**
    - Replace `AppSettings:Token` with a strong, unique secret key.
    - Update `AppSettings:Issuer` and `AppSettings:Audience` to match the URL where your API will be hosted.
- **User Secrets:** For sensitive data like API keys or connection strings during development, it's recommended to use .NET User Secrets. This project is configured with a UserSecretsId (`86ca2901-fc14-4368-bfc4-ba72b98ac935`).
  To set a user secret (from the project directory `e:\my Own projects repo\c#\Document Manager\Document Manager`):
  ```bash
  dotnet user-secrets set "SqlConnectionStrings" "your_development_sql_connection_string"
  dotnet user-secrets set "AppSettings:Token" "your_development_secret_token"
  ```

### 3. Database Setup
This project uses Entity Framework Core for database migrations with SQL Server.
dotnet ef database update
```
This command applies pending migrations to your SQL Server database, creating the schema. The `FileTypeSeedService` will also run on application startup to seed initial file validation types.

### 4. Run the Application
From the project directory (`e:\my Own projects repo\c#\Document Manager\Document Manager`):
```bash
dotnet run
```
The API will start, and the console output will show the URLs it's listening on (e.g., `https://localhost:7001`, `http://localhost:5001`).
Access the Swagger UI for API documentation and testing, typically at `/swagger` or the root URL in development (e.g., `https://localhost:7001/swagger` or `https://localhost:7001/index.html`).

## Running with Docker (Optional)
The project includes a `Dockerfile` for containerization. The Dockerfile is located at `e:\my Own projects repo\c#\Document Manager\Document Manager\Dockerfile`.
The Docker build context is expected to be the solution directory (one level up from the project directory, e.g., `e:\my Own projects repo\c#\Document Manager\`).

### 1. Navigate to Solution Directory


### 2. Build the Docker Image
```bash
docker build -f "Document Manager/Dockerfile" -t document-manager-api .
```
(The `-f "Document Manager/Dockerfile"` flag specifies the path to the Dockerfile relative to the current directory (the solution directory). The `.` at the end signifies the current directory as the build context.)

### 3. Run the Docker Container
```bash
docker run -d -p 8080:8080 -p 8081:8081 \
  -e SqlConnectionStrings="your_sql_server_connection_string_for_docker" \
  -e MongoConnectionStrings="your_mongodb_connection_string_for_docker" \
  -e MongoDbName="your_mongodb_database_name_for_docker" \
  -e "AppSettings:Token"="YOUR_DOCKER_JWT_SIGNING_KEY" \
  -e "AppSettings:Issuer"="http://localhost:8080" \
  -e "AppSettings:Audience"="http://localhost:8080" \
  --name document-manager-container document-manager-api
```
**Notes for Docker:**
- Adjust port mappings (`-p host_port:container_port`) if needed. The Dockerfile exposes ports 8080 and 8081.
- Provide necessary environment variables (`-e`) for connection strings and JWT settings.
  - For SQL Server connection strings in Docker, if SQL Server is running on your host machine, you might use `host.docker.internal` (Windows/Mac) or your host's IP address.
- Ensure your SQL Server and MongoDB instances are accessible from within the Docker container.
- Database migrations should ideally be applied to your database before running the container if the database is external.

## API Endpoints
The API provides a range of endpoints for various functionalities:
- **Authentication:** `/api/Auth` (e.g., Register, Login, Refresh Token, Logout)
- **Documents:** `/api/Documents` (e.g., Upload, Download, Get Document, List User's Documents)
- **Document Permissions:** `/api/DocumentPermissions` (e.g., Assign/Revoke document permissions for users/groups)
- **Tags:** `/api/Tags` (e.g., Create Tag, Get Tag, List Tags)
- **Folders:** (Endpoints for folder management, likely under `/api/Folders` - verify implementation details via Swagger)

For a comprehensive list of endpoints, request/response models, and to interactively test the API, please refer to the **Swagger UI**. This is typically available at `/swagger` or the root path (e.g., `https://localhost:your_port/index.html`) when the application is running in a development environment.

## Project Structure
A brief overview of the main directories within the project (`e:\my Own projects repo\c#\Document Manager\Document Manager`):
- `Controllers/`: Contains API controllers that handle incoming HTTP requests and route them to appropriate services.
- `Services/`: Houses the business logic of the application.
  - `Interfaces/`: Defines contracts (interfaces) for the services.
- `Data/`: Includes Entity Framework Core `DbContext` (`AppDbContextSQL`, `AppDbContextMongo`) and database migration files.
- `Models/`: Contains domain models (entities) and Data Transfer Objects (DTOs). (DTOs might be in a subfolder or a separate `DTOs/` directory).
- `Migrations/`: Stores Entity Framework Core database migration scripts.
- `DTOs/`: (If present as a top-level folder) Data Transfer Objects used for API request and response payloads.
- `Program.cs`: The main entry point of the application, responsible for configuring services and the HTTP request pipeline.
- `Dockerfile`: Instructions for building the Docker image for the application.

## Contributing
Contributions are welcome! If you have suggestions for improvements, bug fixes, or new features, please feel free to:
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

Please ensure your code adheres to the existing coding style and includes tests where appropriate.

## License
This project is typically licensed under an open-source license like MIT. You should add a `LICENSE` file to your repository specifying the terms.
Example: "This project is licensed under the MIT License - see the LICENSE.md file for details."

