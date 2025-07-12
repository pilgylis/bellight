var builder = DistributedApplication.CreateBuilder(args);
var mongo = builder.AddMongoDB("mongo")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithArgs("--keyFile", "/data/mongodb.keyfile", "--replSet", "rs0", "--bind_ip_all")
    .WithBindMount("./MongoDB/init.sh", "/data/init.sh", isReadOnly: true)
    .WithBindMount("./MongoDB/setup.sh", "/data/setup.sh", isReadOnly: true)
    .WithBindMount("./MongoDB/mongodb.keyfile", "/temp/mongodb.keyfile", isReadOnly: true)
    .WithBindMount("./MongoDB/mongodb.pem", "/temp/mongodb.pem", isReadOnly: true)
    .WithEntrypoint("/data/init.sh");

_ = mongo.AddDatabase("mongodb");

await builder.Build().RunAsync();
