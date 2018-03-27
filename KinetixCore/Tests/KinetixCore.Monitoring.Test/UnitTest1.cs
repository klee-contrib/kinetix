using Kinetix.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Log4Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KinetixCore.Monitoring;
using System.Threading;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;

namespace KinetixCore.Monitoring.Test
{
    [TestClass]
    public class UnitTest1 : DIBaseTest
    {

        public override void Register()
        {
            base.Register();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            //serviceCollection.AddLogging(builder => builder.AddLog4Net("log4net.config"));

            serviceCollection.AddMonitoring();
            serviceCollection.AddRemoteSocketConnectorMonitoring();

            WebHost.CreateDefaultBuilder().Build();

            BuildServiceProvider(serviceCollection);
        }

    }
}
