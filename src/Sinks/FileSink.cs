using Core.Abstractions;
using Core.Events;

namespace Cli.Sinks;

public class FileSink(string filePath) : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e)
    {
        File.AppendAllText(filePath, $"[FileSink] {e.At:HH:mm:ss} {e.Kind} товару {e.Id}\n");
    }
}