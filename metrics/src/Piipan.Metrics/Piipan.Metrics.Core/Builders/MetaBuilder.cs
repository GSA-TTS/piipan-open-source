using Microsoft.AspNetCore.WebUtilities;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Metrics.Core.Builders
{
    public class MetaBuilder : IMetaBuilder
    {
        private Meta _meta = new Meta();
        private ParticipantUploadRequestFilter _filter;

        public MetaBuilder()
        {
        }

        public Meta Build()
        {
            SetPageParams();
            return _meta;
        }

        public IMetaBuilder SetFilter(ParticipantUploadRequestFilter filter)
        {
            _meta.PerPage = filter.PerPage;
            _filter = filter;
            return this;
        }

        public IMetaBuilder SetTotal(long total)
        {
            _meta.Total = total;
            return this;
        }

        private void SetPageParams()
        {
            string result = "";
            result = SetFilterStrings(result);
            result = QueryHelpers.AddQueryString(result, "PerPage", _meta.PerPage.ToString());
            _meta.PageQueryParams = result;
        }

        private string SetFilterStrings(string result)
        {
            if(_filter == null)
            {
                return result;
            }
            if (!string.IsNullOrEmpty(_filter.State))
                result = QueryHelpers.AddQueryString(result, nameof(_filter.State), _filter.State);
            if (_filter.StartDate != null)
                result = QueryHelpers.AddQueryString(result, nameof(_filter.StartDate), _filter.StartDate.Value.ToString("yyyy-MM-dd"));
            if (_filter.EndDate != null)
                result = QueryHelpers.AddQueryString(result, nameof(_filter.EndDate), _filter.EndDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(_filter.Status))
                result = QueryHelpers.AddQueryString(result, nameof(_filter.Status), _filter.Status);
            return result;
        }
    }
}