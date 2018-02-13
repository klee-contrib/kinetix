using Kinetix.Caching;
using Kinetix.Caching.Config;
using Kinetix.Search;
using Kinetix.Search.Config;
using Kinetix.Search.Elastic;
using Kinetix.Search.Test.Dum;
using Kinetix.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Log4Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using static Kinetix.Search.ServiceExtensions;

namespace KinetixCore.Caching.Test
{
    public class AbstractSearchTest : DIBaseTest
    {
        private const string DataSourceName = "default";
        private const string NodeUri = "http://docker-vertigo.part.klee.lan.net:9200/";
        private const string IndexName = "kinetix_core_search_test";

        protected SearchManager _searchManager;
        protected ElasticManager _elasticManager;

        public override void Register()
        {
            base.Register();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging(builder => builder.AddLog4Net("log4net.config"));
            serviceCollection.AddSearch(DataSourceName);

            serviceCollection.Configure<SearchConfig>(sc =>
            {
                sc.Servers = new Dictionary<string, SearchConfigItem>();
                var sci = new SearchConfigItem()
                {
                    NodeUri = NodeUri,
                    IndexName = IndexName
                };
                sc.Servers.Add(DataSourceName, sci);
            });

            BuildServiceProvider(serviceCollection);

            SetUp();
        }

        private void SetUp()
        {
            var sp = GetServiceProvider();
            _searchManager = sp.GetService<SearchManager>();
            _elasticManager = sp.GetService<ElasticManager>();

            /* Créé l'index. */
            if (_elasticManager.ExistIndex(DataSourceName))
            {
                _elasticManager.DeleteIndex(DataSourceName);
            }

            _elasticManager.InitIndex(DataSourceName, new IndexConfigurator());

            /* Créé le type de document. */
            var broker = _searchManager.GetBroker<PersonneDocument>();
            broker.CreateDocumentType();

            /* Ajoute des documents. */
            var doc1 = new PersonneDocument
            {
                Id = 7,
                Nom = "TOUTLEMONDE",
                NomSort = "TOUTLEMONDE",
                Prenom = "Robert",
                Text = "Robert TOUTLEMONDE",
                DepartementList = "92 75",
                Genre = "M"
            };

            var doc2 = new PersonneDocument
            {
                Id = 8,
                Nom = "MARCHAND",
                NomSort = "MARCHAND",
                Prenom = "Camille",
                Text = "Camille MARCHAND",
                DepartementList = "01 02",
                Genre = "F"
            };

            var doc3 = new PersonneDocument
            {
                Id = 9,
                Nom = "RODRIGEZ",
                NomSort = "RODRIGEZ",
                Prenom = "Clémentine",
                Text = "Clémentine RODRIGEZ",
                DepartementList = "03 04",
                Genre = "F"
            };

            var doc4 = new PersonneDocument
            {
                Id = 10,
                Nom = "BUCHE",
                NomSort = "BUCHE",
                Prenom = "Géraldine",
                Text = "Géraldine BUCHE",
                DepartementList = "99 98",
                Genre = null
            };

            var doc5 = new PersonneDocument
            {
                Id = 11,
                Nom = "RAY",
                NomSort = "RAY",
                Prenom = "Jean-Baptiste",
                Text = "Jean-Baptiste RAY",
                DepartementList = "92 75",
                Genre = "M"
            };

            var doc6 = new PersonneDocument
            {
                Id = 12,
                Nom = "D'ALEMBERT",
                NomSort = "D'ALEMBERT",
                Prenom = "Roger",
                Text = "Roger D'ALEMBERT",
                DepartementList = "92 75",
                Genre = "M"
            };

            broker.Put(doc1);
            broker.Put(doc2);
            broker.Put(doc3);
            broker.Put(doc4);
            broker.Put(doc5);
            broker.Put(doc6);

            /* Attends que les documents soit disponibles. */
            Thread.Sleep(1000);
        }

        [TestCleanup]
        private void TearDown()
        {
            try
            {
                /* Supprime l'index. */
                if (_elasticManager.ExistIndex(DataSourceName))
                {
                    _elasticManager.DeleteIndex(DataSourceName);
                }
            }
            catch
            {
                // RAS.
            }
        }

    }
}
