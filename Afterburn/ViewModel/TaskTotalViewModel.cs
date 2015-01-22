using System;
using System.Collections.ObjectModel;
using System.Linq;
using Afterburn.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Afterburn.ViewModel
{
    public class TaskTotalViewModel : ViewModelBase
    {
        public Guid Id { get; private set; }

        public ObservableCollection<TaskTotalUpdateViewModel> Updates { get; set; }

        public TaskTotalViewModel()
        {
            this.Updates = new ObservableCollection<TaskTotalUpdateViewModel>();
        }
    }
}
