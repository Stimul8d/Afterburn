using Afterburn.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afterburn.Messages
{
    public class EditDateMessage
    {
        public EditDateMessage(TaskUpdateViewModel vm)
        {
            this.Vm = vm;
        }

        public TaskUpdateViewModel Vm { get; private set; }
    }
}
