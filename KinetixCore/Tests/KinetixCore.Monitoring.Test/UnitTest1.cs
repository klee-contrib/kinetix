using Kinetix.Test;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
