using Core.Abstractions;
using Core.Events;

namespace Cli.Sinks;

public class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e)
    {
        Console.WriteLine($"[ConsoleSink] {e.At:HH:mm:ss} {e.Kind} товару {e.Id}");
    }
}