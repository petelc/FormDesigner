var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FormDesignerAPI_Web>("web");

builder.Build().Run();
