var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql").AddDatabase("sqldata");

var server = builder.AddProject<Projects.BlogServer>("server").WithReference(sql).WaitFor(sql);

var api = builder
    .AddProject<Projects.BlogBlazor_ApiService>("apiservice")
    .WithReference(sql)
    .WaitFor(server);

builder
    .AddProject<Projects.BlogBlazor_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
