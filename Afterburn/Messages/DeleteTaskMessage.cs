using System;
using System.Linq;
using Afterburn.ViewModel;

namespace Afterburn.Messages
{
    public class DeleteTaskMessage
    {
        public DeleteTaskMessage(TaskViewModel tvm)
        {
            this.Task = tvm;
        }

        public TaskViewModel Task { get; private set; }
    }
}
