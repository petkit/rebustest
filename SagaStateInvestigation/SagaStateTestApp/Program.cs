using System;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Routing.TypeBased;
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
                adapter.Register(() => new TestSaga(_bus));

                _bus = Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                    .Transport(t => t.UseRabbitMq("amqp://localhost", "testrmq"))
                    .Routing(r => r.TypeBased().MapAssemblyOf<SomeRequest>("externalservice.input"))
                    .Start();

                Console.WriteLine("Press Q to quit or any other key to produce a job");

                while (true)
                {
                    var keyChar = char.ToLower(Console.ReadKey(true).KeyChar);

                    switch (keyChar)
                    {
                        case 'q':
                            goto quit;

                        default:
                            adapter.Bus.SendLocal(new SomeMessage {Tag = Guid.NewGuid()}).Wait();
                            break;
                    }
                }

                quit:
                Console.WriteLine("Quitting...");
            }
        }
    }
}
