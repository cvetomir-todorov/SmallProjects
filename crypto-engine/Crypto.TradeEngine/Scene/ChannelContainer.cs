using System.Threading.Channels;

namespace Crypto.TradeEngine.Scene;

public sealed class ChannelContainer
{
    // TODO: consider using bounded channels in order to limit memory consumption but find how not to lose trades

    public Channel<TradeInfo> TradeBackend { get; } = Channel.CreateUnbounded<TradeInfo>(new UnboundedChannelOptions
    {
        SingleWriter = true, SingleReader = true
    });

    public Channel<TradeInfo> VolumeSpikeBackend { get; } = Channel.CreateUnbounded<TradeInfo>(new UnboundedChannelOptions
    {
        SingleWriter = true, SingleReader = true
    });

    public Channel<TradeInfo> LocalFile { get; } = Channel.CreateUnbounded<TradeInfo>(new UnboundedChannelOptions
    {
        SingleWriter = true, SingleReader = true
    });

    public Channel<TradeInfo> VolumeSpike { get; } = Channel.CreateUnbounded<TradeInfo>(new UnboundedChannelOptions
    {
        SingleWriter = true, SingleReader = true
    });
}
