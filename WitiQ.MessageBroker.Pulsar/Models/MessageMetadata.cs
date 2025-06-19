namespace WitiQ.MessageBroker.Pulsar.Core.Models;

public class MessageMetadata
{
    public string? Key { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
    public DateTime? EventTime { get; set; }
    public int? SequenceId { get; set; }
}