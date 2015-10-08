using System;

namespace Sitecore.ContentSearch.SolrProvider.NoContainer
{
    internal class SolrCore
    {
        public string Id { get; private set; }

        public Type DocumentType { get; private set; }

        public string Url { get; private set; }

        public SolrCore(string id, Type documentType, string url)
        {
            Id = id;
            Url = url;
            DocumentType = documentType;
        }

        public SolrCore(Type documentType, string url): this(Guid.NewGuid().ToString(), documentType, url)
        {
        }
    }
}