using System;
using System.Linq;
using Afterburn.ViewModel;

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
