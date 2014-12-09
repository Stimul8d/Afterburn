using Afterburn.ViewModel;
using System;
using System.Linq;

namespace Afterburn.Messages
{
    public class UpdateModifiedMessage
    {
        public UpdateModifiedMessage(TaskUpdateViewModel vm)
        {
            this.Update = vm;
        }

        public TaskUpdateViewModel Update { get; private set; }
    }
}
