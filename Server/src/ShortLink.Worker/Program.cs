using ShortLink.Application.Clicks;
using ShortLink.Infrastructure;
using ShortLink.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ClickService>();
builder.Services.AddMessaging(builder.Configuration, x => x.AddConsumer<LinkClickedConsumer>());

var host = builder.Build();
host.Run();
