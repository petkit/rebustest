using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;
using SagaLib.InternalMessages;
using SagaStateTestApp;
using TestMessages;

namespace SagaLib
{
    public class AlternativeSaga : Saga<TestSagaData>, IAmInitiatedBy<SomeMessage>, IHandleMessages<SomeReply>, IHandleMessages<TimeOutMessage>, IHandleMessages<EndSaga>
    {
        private readonly IBus _bus;

        public AlternativeSaga(IBus bus)
        {
            _bus = bus;
        }

        protected override void CorrelateMessages(ICorrelationConfig<TestSagaData> config)
        {
            config.Correlate<SomeMessage>(s => s.Identifier, d => d.OriginalMessageIdentifier);
            config.Correlate<SomeMessage>(s => s.Tag, d => d.CorrelationIdentifier);
            config.Correlate<SomeReply>(s => s.Tag, d => d.CorrelationIdentifier);
            config.Correlate<TimeOutMessage>(s => s.Tag, d => d.CorrelationIdentifier);
            config.Correlate<EndSaga>(s => s.Tag, d => d.CorrelationIdentifier);
        }

        public async Task Handle(SomeMessage message)
        {
            if (!IsNew)
                return;

            Data.CorrelationIdentifier = message.Tag;
            Data.OriginalMessageIdentifier = message.Identifier;
            Data.ReplyReceived = false;
            await _bus.Send(new SomeRequest {Tag = message.Tag});
            await _bus.Defer(TimeSpan.FromSeconds(5), new TimeOutMessage() {Tag = message.Tag});
        }

        public async Task Handle(SomeReply message)
        {
            Data.ReplyReceived = true;

            if (!Data.TimeoutReceived)
            {                
                Console.WriteLine($"Saga for operation {message.Tag} got reply");
                Console.WriteLine($"Operation {message.Tag} continuing regular operation...");
                await _bus.SendLocal(new StartRegularAsyncOperations() { Tag = message.Tag });
            }            
        }

        public async Task Handle(TimeOutMessage message)
        {
            Data.TimeoutReceived = true;

            Console.WriteLine($"Saga for operation {message.Tag} timed out!");
            // ...this, DoStuffIfTimeout below is always called
            // since state is preserved from the _bus.Defer call. Is this the expected behavior?
            if (Data.ReplyReceived)
            {
                Console.WriteLine($"Saga for operation {message.Tag} timed out but we have already reached regular operations. Exiting timeout operations.");
            }
            else
            {                
                Console.WriteLine($"Operation {message.Tag} doing time out things!");
                await _bus.SendLocal(new StartTimeoutOperations() { Tag = message.Tag });
            }
        }

        public async Task Handle(EndSaga message)
        {
            Console.WriteLine($"Saga for operation {message.Tag} ended.");
            MarkAsComplete();
        }
    }
}
