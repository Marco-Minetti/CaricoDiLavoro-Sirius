using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaconAPITests.classes.Mock
{
    internal class MockLogger
    {
        public static ILogger<T> Create<T>()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(logging => logging
                    .AddFilter("Microsoft", LogLevel.None)
                    .AddFilter("Smyt", LogLevel.Trace)
                    .AddConsole())
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            if (factory == null)
                throw new Exception("Can't create logger");

            return factory.CreateLogger<T>();
        }
    }
}
