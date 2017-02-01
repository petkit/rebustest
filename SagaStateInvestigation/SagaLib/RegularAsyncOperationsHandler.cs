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
    public class RegularAsyncOperationsHandler : IHandleMessages<StartRegularAsyncOperations>
    {
        private readonly IBus _bus;

        public RegularAsyncOperationsHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(StartRegularAsyncOperations message)
        {
            await Asyncoperation();
            await _bus.SendLocal(new EndSaga {Tag = message.Tag});
        }

        private async Task Asyncoperation()
        {
            await Task.Delay(200);            
        }
    }
}
