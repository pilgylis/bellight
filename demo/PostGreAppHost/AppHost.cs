var builder = DistributedApplication.CreateBuilder(args);

var postgreSQL = builder.AddPostgres("postgreSQL")
    .WithImage("library/postgres", "17-alpine")
    .WithImageRegistry("docker.io");
_ = postgreSQL.AddDatabase("postgres");

await builder.Build().RunAsync();
