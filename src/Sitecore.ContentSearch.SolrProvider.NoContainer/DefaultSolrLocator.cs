using System;
using System.Collections.Generic;
using SolrNet;
using SolrNet.Impl;
using SolrNet.Impl.DocumentPropertyVisitors;
using SolrNet.Impl.FacetQuerySerializers;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Impl.QuerySerializers;
using SolrNet.Impl.ResponseParsers;
using SolrNet.Mapping;
using SolrNet.Mapping.Validation;
using SolrNet.Mapping.Validation.Rules;
using SolrNet.Schema;

namespace Sitecore.ContentSearch.SolrProvider.NoContainer
{
    /// <summary>
    /// This class seems over complicated but its just so it can support the 'GetService' paradigm from the ServiceLocator.
    /// </summary>
    /// <typeparam name="T">The type you want your Solr data mapped back to. For Sitecore this is *always* Dictionary{string,object}</typeparam>
    public class DefaultSolrLocator<T>
    {
        private readonly Dictionary<string, object> _internalServiceCollection = new Dictionary<string, object>();
        private readonly Dictionary<string, Dictionary<string, object>> _internalKeyedServiceCollection = new Dictionary<string, Dictionary<string, object>>();
        private readonly List<SolrCore> _coreCollection = new List<SolrCore>();

        public IReadOnlyMappingManager MappingManager
        {
            get { return GetService<IReadOnlyMappingManager>(); }
            set { _internalServiceCollection[typeof(IReadOnlyMappingManager).Name] = value; }
        }

        public ISolrFieldParser FieldParser
        {
            get { return GetService<ISolrFieldParser>(); }
            set { _internalServiceCollection[typeof(ISolrFieldParser).Name] = value; }
        }

        public ISolrDocumentPropertyVisitor DocumentPropertyVisitor
        {
            get { return GetService<ISolrDocumentPropertyVisitor>(); }
            set { _internalServiceCollection[typeof(ISolrDocumentPropertyVisitor).Name] = value; }
        }

        public ISolrDocumentResponseParser<T> DocumentResponseParser
        {
            get { return GetService<ISolrDocumentResponseParser<T>>(); }
            set { _internalServiceCollection[typeof(ISolrDocumentResponseParser<T>).Name] = value; }
        }

        public ISolrAbstractResponseParser<T> ResponseParser
        {
            get { return GetService<ISolrAbstractResponseParser<T>>(); }
            set { _internalServiceCollection[typeof(ISolrAbstractResponseParser<T>).Name] = value; }
        }

        public ISolrSchemaParser SchemaParser
        {
            get { return GetService<ISolrSchemaParser>(); }
            set { _internalServiceCollection[typeof(ISolrSchemaParser).Name] = value; }
        }

        public ISolrHeaderResponseParser HeaderParser
        {
            get { return GetService<ISolrHeaderResponseParser>(); }
            set { _internalServiceCollection[typeof(ISolrHeaderResponseParser).Name] = value; }
        }

        public ISolrDIHStatusParser DihStatusParser
        {
            get { return GetService<ISolrDIHStatusParser>(); }
            set { _internalServiceCollection[typeof(ISolrDIHStatusParser).Name] = value; }
        }

        public ISolrExtractResponseParser ExtractResponseParser
        {
            get { return GetService<ISolrExtractResponseParser>(); }
            set { _internalServiceCollection[typeof(ISolrExtractResponseParser).Name] = value; }
        }

        public ISolrFieldSerializer FieldSerializer
        {
            get { return GetService<ISolrFieldSerializer>(); }
            set { _internalServiceCollection[typeof(ISolrFieldSerializer).Name] = value; }
        }

        public ISolrQuerySerializer QuerySerializer
        {
            get { return GetService<ISolrQuerySerializer>(); }
            set { _internalServiceCollection[typeof(ISolrQuerySerializer).Name] = value; }
        }

        public ISolrFacetQuerySerializer FacetQuerySerializer
        {
            get { return GetService<ISolrFacetQuerySerializer>(); }
            set { _internalServiceCollection[typeof(ISolrFacetQuerySerializer).Name] = value; }
        }

        public ISolrMoreLikeThisHandlerQueryResultsParser<T> MlthResultParser
        {
            get { return GetService<ISolrMoreLikeThisHandlerQueryResultsParser<T>>(); }
            set { _internalServiceCollection[typeof(ISolrMoreLikeThisHandlerQueryResultsParser<T>).Name] = value; }
        }

        public ISolrDocumentSerializer<T> DocumentSerializer
        {
            get { return GetService<ISolrDocumentSerializer<T>>(); }
            set { _internalServiceCollection[typeof(ISolrDocumentSerializer<T>).Name] = value; }
        }

        public IMappingValidator MappingValidator
        {
            get { return GetService<IMappingValidator>(); }
            set { _internalServiceCollection[typeof(IMappingValidator).Name] = value; }
        }

        public ISolrStatusResponseParser StatusResponseParser
        {
            get { return GetService<ISolrStatusResponseParser>(); }
            set { _internalServiceCollection[typeof(ISolrStatusResponseParser).Name] = value; }
        }

        public ISolrCache HttpCache
        {
            get { return GetService<ISolrCache>(); }
            set { _internalServiceCollection[typeof(ISolrCache).Name] = value; }
        }

        public ISolrCoreAdmin CoreAdmin
        {
            get { return GetService<ISolrCoreAdmin>(); }
            set { _internalServiceCollection[typeof(ISolrCoreAdmin).Name] = value; }
        }

        public DefaultSolrLocator()
        {
            MappingManager = new MemoizingMappingManager(new AttributesMappingManager());
            FieldParser = new DefaultFieldParser();
            DocumentPropertyVisitor = new DefaultDocumentVisitor(MappingManager, FieldParser);

            if (typeof(T) == typeof(Dictionary<string, object>))
            {
                DocumentResponseParser =
                    (ISolrDocumentResponseParser<T>)new SolrDictionaryDocumentResponseParser(FieldParser);
            }
            else
            {
                DocumentResponseParser = new SolrDocumentResponseParser<T>(MappingManager, DocumentPropertyVisitor,
                    new SolrDocumentActivator<T>());
            }

            ResponseParser = new DefaultResponseParser<T>(DocumentResponseParser);
            SchemaParser = new SolrSchemaParser();
            HeaderParser = new HeaderResponseParser<string>();
            DihStatusParser = new SolrDIHStatusParser();
            ExtractResponseParser = new ExtractResponseParser(HeaderParser);
            FieldSerializer = new DefaultFieldSerializer();

            QuerySerializer = new DefaultQuerySerializer(FieldSerializer);
            FacetQuerySerializer = new DefaultFacetQuerySerializer(QuerySerializer, FieldSerializer);
            MlthResultParser = new SolrMoreLikeThisHandlerQueryResultsParser<T>(new[] { ResponseParser });
            StatusResponseParser = new SolrStatusResponseParser();

            if (typeof(T) == typeof(Dictionary<string, object>))
            {
                DocumentSerializer = (ISolrDocumentSerializer<T>)new SolrDictionarySerializer(FieldSerializer);
            }
            else
            {
                DocumentSerializer = new SolrDocumentSerializer<T>(MappingManager, FieldSerializer);
            }

            HttpCache = new NullCache();
        }

        public ISolrBasicOperations<T> GetBasicServer(ISolrConnection connection)
        {
            var executor = new SolrQueryExecuter<T>(ResponseParser, connection, QuerySerializer, FacetQuerySerializer, MlthResultParser);
            return new SolrBasicServer<T>(connection, executor, DocumentSerializer, SchemaParser, HeaderParser, QuerySerializer, DihStatusParser, ExtractResponseParser);
        }

        public ISolrBasicReadOnlyOperations<T> GetBasicReadOnlyServer(ISolrConnection connection)
        {
            return GetBasicServer(connection);
        }

        public ISolrOperations<T> GetServer(ISolrConnection connection)
        {
            var basicServer = GetBasicServer(connection);

            MappingValidator = new MappingValidator(MappingManager, new IValidationRule[]
            {
                new MappedPropertiesIsInSolrSchemaRule(),
                new RequiredFieldsAreMappedRule(),
                new UniqueKeyMatchesMappingRule(),
                new MultivaluedMappedToCollectionRule()
            });

            ISolrOperations<T> server = new SolrServer<T>(basicServer, MappingManager, MappingValidator);

            return server;
        }

        public void AddCore(string coreId, Type documentType, string coreUrl)
        {
            _coreCollection.Add(new SolrCore(coreId, documentType, coreUrl));
        }

        public ISolrCoreAdmin BuildCoreAdmin(SolrConnection connection)
        {
            if (SolrContentSearchManager.EnableHttpCache)
            {
                connection.Cache = HttpCache;
            }

            return new SolrCoreAdmin(connection, HeaderParser, StatusResponseParser);
        }
        public void RegisterCores()
        {
            foreach (var core in _coreCollection)
            {
                RegisterCore(core);
            }
        }

        internal object GetService(Type serviceType)
        {
            if (_internalServiceCollection.ContainsKey(serviceType.Name))
            {
                return _internalServiceCollection[serviceType.Name];
            }

            throw new ApplicationException($"Cannot locate service [{serviceType}].");
        }

        internal object GetService(Type serviceType, string key)
        {
            if (_internalKeyedServiceCollection.ContainsKey(key))
            {
                if (_internalKeyedServiceCollection[key].ContainsKey(serviceType.Name))
                {
                    return _internalKeyedServiceCollection[key][serviceType.Name];
                }
            }

            throw new ApplicationException($"Cannot locate service [{serviceType}] with the key [ {key} ].");
        }

        /// <summary>
        /// This is not strictly correct as it only supports cores of the same type (i.e. T cannot change per core).
        /// This is not a problem for Sitecore (as we always use <see cref="Dictionary{TKey,TValue}"/>  but not very useful anywhere else.
        /// </summary>
        /// <param name="core"></param>
        private void RegisterCore(SolrCore core)
        {
            var coreConnectionId = core.Id; // + typeof(ISolrConnection);

            var connection = new SolrConnection(core.Url);

            _internalKeyedServiceCollection.Add(coreConnectionId, new Dictionary<string, object>());

            _internalKeyedServiceCollection[coreConnectionId].Add(typeof(ISolrConnection).Name, connection);
            _internalKeyedServiceCollection[coreConnectionId].Add(typeof(ISolrBasicOperations<T>).Name, GetBasicServer(connection));
            _internalKeyedServiceCollection[coreConnectionId].Add(typeof(ISolrOperations<T>).Name, GetServer(connection));
        }

        private TS GetService<TS>()
        {
            return (TS)GetService(typeof(TS));
        }

        private TS GetService<TS>(string key)
        {
            return (TS)GetService(typeof(TS), key);
        }
    }
}