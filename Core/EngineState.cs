using System;
using System.Collections.Generic;
using System.Text;

namespace KeyEngine.Core
{
    public enum EngineState
    {
        Stopped,
        Initializing,
        Running,
        ShuttingDown,
        Error
    }
}
