using GalaSoft.MvvmLight;
using System;
using System.Linq;

namespace Afterburn.ViewModel
{
    public class TaskUpdateViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="Date" /> property's name.
        /// </summary>
        public const string DatePropertyName = "Date";

        private DateTime date = DateTime.Now;

        /// <summary>
        /// Sets and gets the Date property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DateTime Date
        {
            get
            {
                return date;
            }

            set
            {
                if (date == value)
                {
                    return;
                }

                date = value;
                RaisePropertyChanged(DatePropertyName);
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
                RaisePropertyChanged(HoursPropertyName);
            }
        }
    }
}
