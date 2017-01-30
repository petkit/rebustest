using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaLib.InternalMessages
{
    public class EndSaga
    {
        public Guid Tag { set; get; }
    }
}
