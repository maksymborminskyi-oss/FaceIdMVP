# FaceIdMVP

A minimum viable face identification system built with ASP.NET Core, Blazor, PostgreSQL, and Python.

The project demonstrates a multi-component application that combines a web interface, a REST API, persistent data storage, and a separate face-processing service.

> **Project status:** MVP

## Overview

FaceIdMVP is an information system prototype for managing users and supporting face-based identification.

The application is divided into several components, each responsible for a specific part of the system.

## Tech Stack

| Technology | Purpose |
|---|---|
| C# | Main application development |
| ASP.NET Core Web API | REST API and application backend |
| Blazor | Web interface |
| Entity Framework Core | Database access and ORM |
| PostgreSQL | Persistent data storage |
| Python | Face-processing service |
| FastAPI | HTTP interface for the Python service |
| OpenCV | Image-processing support |
| NumPy | Numerical operations |
| InsightFace | Face analysis and embedding extraction |
| Swagger / OpenAPI | API exploration and documentation |

## Architecture

The application consists of three main components.

### 1. FaceId.Web

The web application provides the user-facing interface.

### 2. FaceId.Api

The ASP.NET Core Web API handles application requests and interacts with PostgreSQL through Entity Framework Core.

The API includes functionality for managing users and integrates with the Python service for face-processing operations.

### 3. FaceId.Python

The Python service provides face-processing functionality through an HTTP interface.

The ASP.NET Core API communicates with this service using `HttpClient`.

The service exposes the following endpoints:

- `GET /health` — health check.
- `POST /extract-embedding` — processes an image and returns face embedding data.

### Request flow

```text
┌─────────────────────┐
│    FaceId.Web       │
│   Web Interface     │
└──────────┬──────────┘
           │ HTTP
           ▼
┌─────────────────────┐
│     FaceId.Api      │
│   ASP.NET Core      │
└──────┬────────┬─────┘
       │        │
       │        │ HTTP
       ▼        ▼
┌────────────┐ ┌─────────────────────┐
│ PostgreSQL │ │    FaceId.Python    │
│  Database  │ │   FastAPI Service   │
└────────────┘ └─────────────────────┘
```

## Main Features

- User data management through a REST API.
- PostgreSQL integration using Entity Framework Core.
- Face-processing integration with a separate Python service.
- HTTP communication between application components.
- API exploration through Swagger UI.
- Separation of the web interface, backend API, and face-processing service.

The exact availability of individual features depends on the current implementation.

## Project Structure

```text
FaceIdMvp/
├── FaceId.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   └── ...
├── FaceId.Web/
├── FaceId.Python/
├── FaceIdMvp.sln
├── .gitignore
└── README.md
```

## Requirements

To run the application locally, you will need:

- .NET 8 SDK.
- PostgreSQL.
- Python and the dependencies required by the Python service.
- The project source code.

## Configuration

### Database

The API uses the `DefaultConnection` connection string to connect to PostgreSQL.

Configure it using .NET User Secrets rather than committing credentials to source control.

From the repository root:

```bash
dotnet user-secrets init --project FaceId.Api/FaceId.Api.csproj
```

Set the connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
"Host=localhost;Port=5432;Database=faceid_mvp;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD" \
--project FaceId.Api/FaceId.Api.csproj
```

Replace the example database credentials with your local PostgreSQL configuration.

Ensure the database exists and apply the project's Entity Framework Core migrations if required.

### Python service

The ASP.NET Core API expects the Python service to be available at:

```text
http://127.0.0.1:8001
```

Start the Python service using the entry point and dependency instructions provided by the Python project.

## Running the Application

### 1. Start the Python service

Start the FastAPI application and verify that the service is accessible on port `8001`.

### 2. Start the REST API

From the repository root:

```bash
dotnet run --project FaceId.Api/FaceId.Api.csproj
```

The actual HTTP address and port depend on the application's launch configuration.

### 3. Explore the API

Open the Swagger UI at the API's configured Swagger address in your browser.

You can also test the users endpoint using `curl`:

```bash
curl -i http://localhost:5048/api/Users
```

Replace the port with the one used by your API.

### 4. Start the web application

Run the `FaceId.Web` project using its configured launch profile.

Ensure that the API is running and that the web application's API address is configured correctly.

## Current Limitations

FaceIdMVP is an MVP rather than a production-ready identity management platform.

The project is intended to demonstrate application integration and backend development fundamentals. Production deployment would require additional work on security, error handling, operational monitoring, and other reliability requirements.

## Learning Objectives

This project provides practical experience with:

- Building REST APIs using ASP.NET Core.
- Working with PostgreSQL and Entity Framework Core.
- Separating application responsibilities across components.
- Integrating .NET applications with Python services over HTTP.
- Working with image-processing and face-embedding technologies.
- Documenting and publishing a software project on GitHub.

## License

No license has been specified yet.