using Afterburn.ViewModel;
using System;
using System.Linq;

namespace Afterburn.Messages
{
    public class EstimateUpdatedMessage
    {
        public EstimateUpdatedMessage(TaskViewModel task)
        {
            this.Task = task;
        }

        public TaskViewModel Task { get; private set; }
    }
}
