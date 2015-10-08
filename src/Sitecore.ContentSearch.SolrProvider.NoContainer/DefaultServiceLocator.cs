using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Sitecore.ContentSearch.SolrProvider.NoContainer
{
    public class DefaultServiceLocator<T> : ServiceLocatorImplBase
    {
        private readonly DefaultSolrLocator<T> _operations;

        public DefaultServiceLocator(DefaultSolrLocator<T> operations)
        {
            _operations = operations;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (key != null)
            {
                return _operations.GetService(serviceType, key);
            }

            return _operations.GetService(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            throw new System.NotImplementedException();
        }
    }
}