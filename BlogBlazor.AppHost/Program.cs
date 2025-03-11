var builder = DistributedApplication.CreateBuilder(args);



var sql = builder.AddSqlServer("sql").AddDatabase("sqldata");

var api = builder.AddProject<Projects.BlogBlazor_ApiService>("apiservice")
    .WithReference(sql)
    .WaitFor(sql); 

builder.AddProject<Projects.BlogBlazor_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
