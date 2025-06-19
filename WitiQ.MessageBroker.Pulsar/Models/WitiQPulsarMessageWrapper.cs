using System;
using System.Collections.Generic;
using DotPulsar.Abstractions;

namespace WitiQ.MessageBroker.Pulsar.Core.Models;

internal class WitiQPulsarMessageWrapper<T>
{
    public WitiQPulsarMessage<T> Message { get; set; } = null!;
    public IMessage<ReadOnlyMemory<byte>> OriginalMessage { get; set; } = null!;
}