var builder = DistributedApplication.CreateBuilder(args);

var rabbitmqUser = builder.AddParameter("rabbitmq-user", "bellight");
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", "bellight", secret: true);

var rabbitmq = builder.AddRabbitMQ("rabbitmq", userName: rabbitmqUser, password: rabbitmqPassword)
    .WithImage("library/rabbitmq", "4-management")
    .WithImageRegistry("docker.io")
    .WithBindMount("./rabbitmq-definitions.json", "/etc/rabbitmq/definitions.json", isReadOnly: true)
    .WithBindMount("./rabbitmq-load-definitions.conf", "/etc/rabbitmq/conf.d/20-load-definitions.conf", isReadOnly: true);

builder.AddProject<Projects.MessageBusPublisher>("publisher")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.MessageBusSubscriber>("subscriber")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

await builder.Build().RunAsync();
