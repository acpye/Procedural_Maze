using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGGameLibrary.FSM
{
    public interface IStateTransition
    {
        bool ToTransition();
    }
}
