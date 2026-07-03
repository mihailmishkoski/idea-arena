using System.Text.Json;
using BusinessIdea.Domain.Entities;

namespace BusinessIdea.Application.Common.Outbox;

/// <summary>
/// Builds <see cref="OutboxMessage"/> rows with a consistently serialized
/// payload. Handlers add the result to the DbContext inside the same
/// transaction as the business change (transactional outbox pattern).
/// </summary>
public static class OutboxMessageFactory
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public static OutboxMessage Create<TPayload>(string type, TPayload payload) => new()
    {
        Type = type,
        PayloadJson = JsonSerializer.Serialize(payload, SerializerOptions),
    };

    public static TPayload Deserialize<TPayload>(OutboxMessage message) =>
        JsonSerializer.Deserialize<TPayload>(message.PayloadJson, SerializerOptions)
        ?? throw new InvalidOperationException(
            $"Outbox message {message.Id} has an unreadable '{message.Type}' payload.");
}
