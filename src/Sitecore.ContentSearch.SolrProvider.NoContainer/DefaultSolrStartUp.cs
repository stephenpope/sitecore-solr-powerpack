using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch.SolrProvider.DocumentSerializers;
using SolrNet;
using SolrNet.Impl;

namespace Sitecore.ContentSearch.SolrProvider.NoContainer
{
    public class DefaultSolrStartUp : ISolrStartUp
    {
        private DefaultSolrLocator<Dictionary<string, object>> _operations;

        public void AddCore(string coreId, Type documentType, string coreUrl)
        {
            _operations.AddCore(coreId, documentType, coreUrl);
        }

        public void Initialize()
        {
            if (!SolrContentSearchManager.IsEnabled)
            {
                throw new InvalidOperationException("Solr configuration is not enabled. Please check your settings and include files.");
            }

            _operations = new DefaultSolrLocator<Dictionary<string, object>>();

            // Override the document serializer to support boosting.
            _operations.DocumentSerializer = new SolrFieldBoostingDictionarySerializer(_operations.FieldSerializer);

            foreach (var index in SolrContentSearchManager.Cores)
            {
                AddCore(index, typeof(Dictionary<string, object>), $"{SolrContentSearchManager.ServiceAddress}/{index}");
            }

            if (SolrContentSearchManager.EnableHttpCache)
            {
                _operations.HttpCache = new HttpRuntimeCache();
            }

            _operations.RegisterCores();
            _operations.CoreAdmin = BuildCoreAdmin();

            //Register the service locator (yes, its a horrible pattern but this means we dont have to change anything internally in Sitecore for now).
            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => new DefaultServiceLocator<Dictionary<string, object>>(_operations));

            SolrContentSearchManager.SolrAdmin = _operations.CoreAdmin;
            SolrContentSearchManager.Initialize();
        }

        private ISolrCoreAdmin BuildCoreAdmin()
        {
            var connection = new SolrConnection(SolrContentSearchManager.ServiceAddress);
            return _operations.BuildCoreAdmin(connection);
        }

        public bool IsSetupValid()
        {
            if (!SolrContentSearchManager.IsEnabled)
            {
                return false;
            }

            var admin = BuildCoreAdmin();

            return SolrContentSearchManager.Cores.Select(defaultIndex => admin.Status(defaultIndex).First()).All(status => status.Name != null);
        }
    }
}
