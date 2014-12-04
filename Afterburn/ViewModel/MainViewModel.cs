using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;

namespace Afterburn.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        const int num = 10;

        public ObservableCollection<TaskViewModel> Tasks { get; set; }
        public TaskViewModel Distractions { get; set; }
        public TaskViewModel TasksRollup { get; set; }

        public MainViewModel()
        {
            CreateTasks();
            CreateDistractions();
            
            TasksRollup = new TaskViewModel();
            for (int i = 0; i < num; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = num - i + 0.25
                };
                update.Hours *= 13;
                TasksRollup.Updates.Add(update);
            }
        }
  
        private void CreateDistractions()
        {
            Distractions = new TaskViewModel();
            for (int i = 0; i < num; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = num - i + 0.25
                };
                Distractions.Updates.Add(update);
            }
        }

        private void CreateTasks()
        {
            Tasks = new ObservableCollection<TaskViewModel>();
            var t = new TaskViewModel
            {
                Reference = "PRJSBO-2014",
                Feature = "Custom Coupon",
                Name = "Sort coupons by fixture start time",
                Hours = 6.25
            };

            for (int i = 0; i < num; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = num - i + 0.25
                };
                t.Updates.Add(update);
            }

            for (int i = 0; i < 13; i++)
            {
                Tasks.Add(t);
            }
        }

        /// <summary>
        /// The <see cref="HoursPerDay" /> property's name.
        /// </summary>
        public const string HoursPerDayPropertyName = "HoursPerDay";

        private double hoursPerDay = 8.5;

        /// <summary>
        /// Sets and gets the HoursPerDay property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double HoursPerDay
        {
            get
            {
                return hoursPerDay;
            }

            set
            {
                if (hoursPerDay == value)
                {
                    return;
                }

                hoursPerDay = value;
                RaisePropertyChanged(HoursPerDayPropertyName);
            }
        }
    }
}