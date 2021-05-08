using System;
using System.Collections.Generic;
using System.Text;

namespace CodeExecutionOrchestrator.Core
{
    public class InstanceDescriptor
    {
        public InstanceDescriptor(Type type, object instance)
        {
            Type = type;
            Instance = instance;
        }

        public Type Type { get; set; }

        public object Instance { get; set; }
    }
}
