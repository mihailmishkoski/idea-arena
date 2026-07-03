using BusinessIdea.Application.Features.Outbox;
using BusinessIdea.Infrastructure;
using BusinessIdea.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Load the shared secret store regardless of environment, so `dotnet run`
// works without extra environment variables.
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddOutboxSideEffectServices(builder.Configuration);

// One processor per outbox event type; the dispatcher indexes them by Type.
builder.Services.AddScoped<IOutboxProcessor, IdeaCreatedProcessor>();
builder.Services.AddScoped<IOutboxProcessor, ChatRequestedProcessor>();
builder.Services.AddScoped<IOutboxProcessor, ChatAcceptedProcessor>();
builder.Services.AddScoped<IOutboxProcessor, CofounderAppliedProcessor>();

builder.Services.AddHostedService<OutboxProcessorService>();

var host = builder.Build();
host.Run();
