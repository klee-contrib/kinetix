using System;
using System.Collections;
using System.Collections.Generic;
using Kinetix.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Log4Net;
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
    /// Test unitaire de la classe BeanPropertyDescriptorCollection.
    /// </summary>
    [TestFixture]
    public class BeanPropertyDescriptorCollectionTest : DIBaseTest {
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
        /// Test la valeur de la propriété IsReadOnly.
        /// </summary>
        [Test]
        public void IsReadonly() {
            BeanDefinition definition = _beanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            Assert.IsTrue(coll.IsReadOnly);
        }

        /// <summary>
        /// Test la valeur de la propriété Count.
        /// </summary>
        [Test]
        public void Count() {
            BeanDefinition definition = _beanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            Assert.AreEqual(0, coll.Count);
        }

        /// <summary>
        /// Test l'échec de la méthode Add.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Add() {
            BeanDefinition definition = _beanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            coll.Add(null);
        }

        /// <summary>
        /// Test l'échec de la méthode Clear.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Clear() {
            BeanDefinition definition = _beanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            coll.Clear();
        }

        /// <summary>
        /// Test l'échec de la méthode Remove.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Remove() {
            BeanDefinition definition = _beanDescriptor.GetDefinition(new object());
            ICollection<BeanPropertyDescriptor> coll = definition.Properties;
            coll.Remove(null);
        }
    }
}
