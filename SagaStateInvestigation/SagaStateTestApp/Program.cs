//#define USE_ORIGINAL
//#define USE_SEPARATE_PRODUCER_QUEUES
using System;
using System.Threading;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
using SagaLib;
using TestMessages;


namespace SagaStateTestApp
{
    class Program
    {
        static IBus _bus;
        static void Main()
        {
            
            using (var adapter = new BuiltinHandlerActivator())
            {

#if USE_ORIGINAL
                adapter.Register(() => new TestSaga(_bus));
#else
                adapter.Register(() => new AlternativeSaga(_bus));
                adapter.Register(() => new RegularAsyncOperationsHandler(_bus));
                adapter.Register(() => new TimeoutAsyncOperationsHandler(_bus));
#endif
                _bus = Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
#if USE_SEPARATE_PRODUCER_QUEUES
                    .Transport(t => t.UseRabbitMq("amqp://localhost", Guid.NewGuid().ToString()))
#else
                    .Transport(t => t.UseRabbitMq("amqp://localhost", "oneandonlyproducerqueue"))
#endif                
                    .Routing(r => r.TypeBased().MapAssemblyOf<SomeRequest>("externalservice.input"))
                    .Options(o => o.SetMaxParallelism(5))
                    .Start();

                Timer timer = new Timer(SendRequest, null, 1000, 5000);

                Console.WriteLine("Press Q to quit or any other key to produce an extra message");

                while (true)
                {
                    var keyChar = char.ToLower(Console.ReadKey(true).KeyChar);

                    switch (keyChar)
                    {
                        case 'q':
                            goto quit;

                        default:
                            _bus.SendLocal(new SomeMessage { Tag = Guid.NewGuid() }).Wait();
                            break;
                    }
                }

                quit:
                Console.WriteLine("Quitting...");
                timer.Dispose();
            }
        }



        static void SendRequest(object state)
        {
            _bus.SendLocal(new SomeMessage { Tag = Guid.NewGuid(), Identifier = Guid.NewGuid() }).Wait();
        }
    }
}
