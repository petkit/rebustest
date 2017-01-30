using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;
using Rebus.Threading.SystemThreadingTimer;
using TestMessages;

namespace SagaStateTestApp
{
    public class TestSagaData : ISagaData
    {
        public Guid Id { get; set; }
        public int Revision { get; set; }

        public Guid OriginalMessageIdentifier { get; set; }
        public Guid CorrelationIdentifier { get; set; }
        public bool ReplyReceived { get; set; }
    }

    public class TestSaga : Saga<TestSagaData>, IAmInitiatedBy<SomeMessage>, IHandleMessages<SomeReply>, IHandleMessages<TimeOutMessage>
    {
        private readonly IBus _bus;
        public TestSaga(IBus bus)
        {
            _bus = bus;
        }

        protected override void CorrelateMessages(ICorrelationConfig<TestSagaData> config)
        {
            config.Correlate<SomeMessage>(s => s.Identifier, d => d.OriginalMessageIdentifier);
            config.Correlate<SomeMessage>(s => s.Tag, d => d.CorrelationIdentifier);
            config.Correlate<SomeReply>(s => s.Tag, d => d.CorrelationIdentifier);
            config.Correlate<TimeOutMessage>(s => s.Tag, d => d.CorrelationIdentifier);
        }

        public async Task Handle(SomeMessage message)
        {
            if (!IsNew)
                return;

            Data.CorrelationIdentifier = message.Tag;
            Data.OriginalMessageIdentifier = message.Identifier;
            Data.ReplyReceived = false;
            await _bus.Send(new SomeRequest { Tag = message.Tag });
            await _bus.Defer(TimeSpan.FromSeconds(5), new TimeOutMessage() {Tag = message.Tag});
        }

        public async Task Handle(SomeReply message)
        {
            // Even if we would get here loooong before...
            Data.ReplyReceived = true;
            await DoStuffIfNotTimedout();
        }

        public async Task Handle(TimeOutMessage message)
        {
            // ...this, DoStuffIfTimeout below is always called
            // since state is preserved from the _bus.Defer call. Is this the expected behavior?
            if (!Data.ReplyReceived)
                await DoStuffIfTimedout();
        }

        private async Task DoStuffIfNotTimedout()
        {
            // some more async stuff here
            await AsyncHandler1();
            MarkAsComplete();
        }

        private async Task DoStuffIfTimedout()
        {
            // some more async stuff here
            await AsyncHandler2();
            MarkAsComplete();
        }

        private static async Task AsyncHandler1()
        {
            await Task.Run(() => System.Threading.Thread.Sleep(10000));
        }

        private static async Task AsyncHandler2()
        {
            await Task.Run(() => System.Threading.Thread.Sleep(10000));
        }
    }
}
