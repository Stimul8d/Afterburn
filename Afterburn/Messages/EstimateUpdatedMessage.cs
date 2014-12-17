using System;
using System.Linq;
using Afterburn.ViewModel;

namespace Afterburn.Messages
{
    public class EstimateUpdatedMessage
    {
        public EstimateUpdatedMessage(TaskViewModel task, double previous)
        {
            this.Task = task;
            this.Previous = previous;
        }

        public TaskViewModel Task { get; private set; }

        public double Previous { get; private set; }
    }
}
