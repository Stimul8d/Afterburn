using Afterburn.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afterburn.ViewModel
{
    public class EditDateViewModel : ViewModelBase
    {
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand ChangeDateCommand { get; set; }

        public EditDateViewModel()
        {
            Messenger.Default.Register<EditDateMessage>(this,
            (m) =>
            {
                TaskUpdateViewModel = m.Vm;
                NewDate = m.Vm.Date;
                ShowEditDate = true;
            });

            Messenger.Default.Register<DeleteDateMessage>(this,
            (m) =>
            {
                ShowEditDate = false;
            });

            CancelCommand = new RelayCommand(() =>
            {
                ShowEditDate = false;
            });

            ChangeDateCommand = new RelayCommand(() =>
            {
                TaskUpdateViewModel.Date = NewDate;
                ShowEditDate = false;
            });
        }

        #region INPC
        /// <summary>
        /// The <see cref="ShowEditDate" /> property's name.
        /// </summary>
        public const string ShowEditDatePropertyName = "ShowEditDate";

        private bool showEditDate = false;

        /// <summary>
        /// Sets and gets the ShowEditDate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowEditDate
        {
            get
            {
                return showEditDate;
            }

            set
            {
                if (showEditDate == value)
                {
                    return;
                }

                showEditDate = value;
                RaisePropertyChanged(ShowEditDatePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="NewDate" /> property's name.
        /// </summary>
        public const string NewDatePropertyName = "NewDate";

        private DateTime newDate = DateTime.Today;

        /// <summary>
        /// Sets and gets the NewDate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DateTime NewDate
        {
            get
            {
                return newDate;
            }

            set
            {
                if (newDate == value)
                {
                    return;
                }

                newDate = value;
                RaisePropertyChanged(NewDatePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TaskUpdateViewModel" /> property's name.
        /// </summary>
        public const string TaskUpdateViewModelPropertyName = "TaskUpdateViewModel";

        private TaskUpdateViewModel taskUpdateViewModel = null;

        /// <summary>
        /// Sets and gets the TaskUpdateViewModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TaskUpdateViewModel TaskUpdateViewModel
        {
            get
            {
                return taskUpdateViewModel;
            }

            set
            {
                if (taskUpdateViewModel == value)
                {
                    return;
                }

                taskUpdateViewModel = value;
                RaisePropertyChanged(TaskUpdateViewModelPropertyName);
            }
        }
        #endregion
    }
}
