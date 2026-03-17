var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Users_API>("users-api");

builder.AddProject<Projects.Books_API>("books-api");

builder.Build().Run();
