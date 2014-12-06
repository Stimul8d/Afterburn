using Afterburn.ViewModel;
using System;
using System.Linq;

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
