using System;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;

namespace Afterburn.ViewModel
{
    [DebuggerDisplay("{Date},{Hours}")]
    public class TaskTotalUpdateViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="Date" /> property's name.
        /// </summary>
        public const string DatePropertyName = "Date";

        /// <summary>
        /// The <see cref="Hours" /> property's name.
        /// </summary>
        public const string HoursPropertyName = "Hours";

        private DateTime date = DateTime.Now;

        private double hours = 0;

        public RelayCommand DeleteDateCommand { get; set; }

        /// <summary>
        /// Sets and gets the Date property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DateTime Date
        {
            get
            {
                return this.date;
            }

            set
            {
                if (this.date == value)
                {
                    return;
                }

                this.date = value;
                this.RaisePropertyChanged(DatePropertyName);
                this.RaisePropertyChanged("DateString");
            }
        }

        public string DateString
        {
            get
            {
                return this.Date.ToString("dd/MM");
            }
        }

        /// <summary>
        /// Sets and gets the Hours property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Hours
        {
            get
            {
                return hours;
            }

            set
            {
                if (hours == value)
                {
                    return;
                }
                hours = value;

                if (hours < 0)
                    hours = 0;
                this.RaisePropertyChanged(HoursPropertyName);
            }
        }
    }
}
