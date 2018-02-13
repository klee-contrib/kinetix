using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Log4Net;
using Kinetix.Test;

#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Test du BeanDescriptor.
    /// </summary>
    [TestFixture]
    public class BeanDescriptorTest : DIBaseTest {

        private BeanDescriptor _beanDescriptor;

        /// <summary>
        /// Initialise l'application avec un domaine LIBELLE_COURT.
        /// </summary>
        [SetUp]
        public void Init() {
            
        }

        public override void Register()
        {
            base.Register();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<BeanDescriptor>();
            serviceCollection.AddSingleton<IDomainManager, DomainManager<object>>();
            serviceCollection.AddLogging(builder => builder.AddLog4Net("log4net.config"));

            BuildServiceProvider(serviceCollection);
            _beanDescriptor = GetServiceProvider().GetService<BeanDescriptor>();
        }

        /// <summary>
        /// Test la récupération des propriétés d'un type null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDefinitionByTypeNull() {
            BeanDefinition beanDefinition = _beanDescriptor.GetDefinition((Type)null);
        }

        /// <summary>
        /// Test la récupération des propriétés d'une collection de bean nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDefinitionCollectionNull() {
            BeanDefinition beanDefinition = _beanDescriptor.GetCollectionDefinition(null);
        }

        /// <summary>
        /// Test la récupération des propriétés avec une valeur nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDefinitionNull() {
            _beanDescriptor.GetDefinition((object)null);
        }

 
        /// <summary>
        /// Test la récupération des propriétés pour un type invalide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionInvalidPropertyType() {
            _beanDescriptor.GetDefinition(new BeanInvalidPropertyType());
        }

        /// <summary>
        /// Test la récupération des propriétés pour un type généric invalide.
        /// </summary>
        [Test]
        public void GetDefinitionInvalidGenericType() {
            _beanDescriptor.GetDefinition(new BeanInvalidGenericType());
        }

        /// <summary>
        /// Test la récupération des propriétés pour un type non supporté.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetDefinitionUnsupportedType() {
            BeanDefinition definition = _beanDescriptor.GetDefinition(new BeanInvalidUnsupportedType());
            BeanPropertyDescriptor property = definition.Properties["OtherId"];
            property.ValidConstraints(3, null);
        }

        /// <summary>
        /// Test CheckAll : paramètre collection null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCheckAllNullCollection() {
            _beanDescriptor.CheckAll<int>(null, false);
        }
    }
}
