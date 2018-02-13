using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Transactions;
using System.Xml;

namespace Kinetix.Test
{
    /// <summary>
    /// Defines methods needed in tests.
    /// </summary>
    public abstract class DIBaseTest
    {
        #region .Net Core DI

        private static IServiceProvider _serviceProvider;

 
        public static void BuildServiceProvider(IServiceCollection serviceCollection)
        {
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public static IServiceProvider GetServiceProvider()
        {
            return _serviceProvider;
        }
        #endregion

        protected static TransactionScope _scope;
        //private TestSecurityContext _context;

        /// <summary>
        /// Test cleanup.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                //_context.Dispose();
            }
            catch
            {
                // RAS.
            }

            try
            {
                if (Transaction.Current != null)
                {
                    Transaction.Current.Rollback();
                }

                _scope.Dispose();
                _scope = null;
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Test initialize.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.Register();

            /* Initialise la configuration. */
            //ConfigManager.Init();

            _scope = new TransactionScope();
        }


        public virtual void Register()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }
    }

}
