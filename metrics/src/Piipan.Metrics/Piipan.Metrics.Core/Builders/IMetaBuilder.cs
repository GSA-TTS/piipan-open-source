using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.Builders
{
    public interface IMetaBuilder
    {
        Meta Build();
        IMetaBuilder SetFilter(ParticipantUploadRequestFilter filter);
        IMetaBuilder SetTotal(long total);
    }
}