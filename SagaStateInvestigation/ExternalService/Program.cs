using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Logging;
using TestMessages;

namespace ExternalService
{
    class Program
    {
        static void Main()
        {
            using (var adapter = new BuiltinHandlerActivator())
            {
                adapter.Handle<SomeRequest>(async (bus, request) =>                
                    await HandleRequest(bus, request));

                Configure.With(adapter)
                    .Logging(l => l.ColoredConsole(minLevel: LogLevel.Warn))
                    .Transport(t => t.UseRabbitMq("amqp://localhost", "externalservice.input"))
                    .Start();

                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
            }
        }

        static async Task HandleRequest(IBus bus, SomeRequest request)
        {
            Console.WriteLine($"Request for operation {request.Tag} is being handled...");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine($"Request for operation {request.Tag} was handled. Sending reply...");
            await bus.Reply(new SomeReply {Tag = request.Tag});
        }
    }
}
