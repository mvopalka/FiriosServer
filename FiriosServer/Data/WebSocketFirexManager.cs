using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Firios.Data;

public class WebSocketFirexManager
{
    private ConcurrentDictionary<Guid, WebSocket> _subscriptions = new ConcurrentDictionary<Guid, WebSocket>();

    public ICollection<WebSocket> GetAll() => _subscriptions.Values;
    public void AddNew(Guid id, WebSocket webSocket) => _subscriptions.TryAdd(id, webSocket);
}