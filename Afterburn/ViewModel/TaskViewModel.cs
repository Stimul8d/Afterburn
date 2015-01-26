using System;
using System.Collections.ObjectModel;
using System.Linq;
using Afterburn.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Afterburn.ViewModel
{
    public class TaskViewModel : ViewModelBase
    {
        public Guid Id { get; private set; }

        public ObservableCollection<TaskUpdateViewModel> Updates { get; set; }

        public RelayCommand DeleteTaskCommand { get; set; }

        public TaskViewModel()
            : this("FEATURE", "NAME", 0)
        {
        }

        public TaskViewModel(string feature,
            string name, double hours)
        {
            this.Id = Guid.NewGuid();
            this.feature = feature;
            this.name = name;
            this.hours = hours;

            this.Updates = new ObservableCollection<TaskUpdateViewModel>();

            this.DeleteTaskCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send<DeleteTaskMessage>(
                    new DeleteTaskMessage(this));
            });
        }

        #region INPC

        /// <summary>
        /// The <see cref="AllowEdits" /> property's name.
        /// </summary>
        public const string AllowEditsPropertyName = "AllowEdits";

        private bool allowEdits = true;

        /// <summary>
        /// Sets and gets the AllowEdits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AllowEdits
        {
            get
            {
                return this.allowEdits;
            }

            set
            {
                if (this.allowEdits == value)
                {
                    return;
                }

                this.allowEdits = value;
                this.RaisePropertyChanged(AllowEditsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Feature" /> property's name.
        /// </summary>
        public const string FeaturePropertyName = "Feature";

        private string feature = "";

        /// <summary>
        /// Sets and gets the Feature property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Feature
        {
            get
            {
                return this.feature;
            }

            set
            {
                if (this.feature == value)
                {
                    return;
                }

                this.feature = value;
                Messenger.Default.Send<FeatureNameUpdatedMessage>(new FeatureNameUpdatedMessage());

                this.RaisePropertyChanged(FeaturePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string name = "";

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.name == value)
                {
                    return;
                }

                this.name = value;
                this.RaisePropertyChanged(NamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Hours" /> property's name.
        /// </summary>
        public const string HoursPropertyName = "Hours";

        private double hours = 0;

        /// <summary>
        /// Sets and gets the Hours property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Hours
        {
            get
            {
                return this.hours;
            }

            set
            {
                if (this.hours == value)
                {
                    return;
                }
                var previous = this.hours;
                this.hours = Math.Round(value, 2);

                this.RaisePropertyChanged(HoursPropertyName);
                Messenger.Default.Send<EstimateUpdatedMessage>(new EstimateUpdatedMessage(this, previous));
            }
        }
        #endregion
    }
}
