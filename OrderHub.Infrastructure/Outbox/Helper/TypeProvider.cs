using OrderHub.Shared.Events;

namespace OrderHub.Infrastructure.Outbox.Helpers;

public static class TypeProvider
{
    private static readonly Dictionary<string, Type> _types = new()
    {
        { nameof(OrderCreatedEvent), typeof(OrderCreatedEvent) }
        // Gerekirse diğer event tiplerini de buraya ekle
    };

    public static Type? Get(string typeName)
    {
        return _types.TryGetValue(typeName, out var type) ? type : null;
    }
}

