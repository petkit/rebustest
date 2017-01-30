using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Handlers;
using SagaLib.InternalMessages;

namespace SagaLib
{
    public class TimeoutAsyncOperationsHandler : IHandleMessages<StartTimeoutOperations>
    {
        private readonly IBus _bus;

        public TimeoutAsyncOperationsHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(StartTimeoutOperations message)
        {
            await Asyncoperation();
            await _bus.SendLocal(new EndSaga() { Tag = message.Tag });
        }

        private async Task Asyncoperation()
        {
            await Task.Run(() => System.Threading.Thread.Sleep(10000));
        }
    }
}
