using Kinetix.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Log4Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixCore.ComponentModel.Test
{
    public class AbstractComponentModelTest : DIBaseTest {

        public override void Register()
        {
            base.Register();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging(builder => builder.AddLog4Net("log4net.config"));

            BuildServiceProvider(serviceCollection);
        }
    }
}
