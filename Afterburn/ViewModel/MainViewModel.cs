using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace Afterburn.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        const int NumOfUpdates = 5;
        const int NumOfTasks = 3;

        static readonly Random rand = new Random();

        public ObservableCollection<TaskViewModel> Tasks { get; set; }
        public TaskViewModel Distractions { get; set; }
        public TaskViewModel TasksRollup { get; set; }
        public TaskViewModel ProjectedTotal { get; set; }
        public TaskViewModel ProjectedTotalMinusDistractions { get; set; }
        public TaskViewModel TotalWorked { get; set; }

        public MainViewModel()
        {
            CreateTasks();
            CreateDistractions();
            CreateRollup();

            TotalEstimatedHours = Tasks.Sum(t => t.Hours);

            CreateProjectedTotal();
            CreateProjectedTotalMinusDistractions();
            CreateTotalWorked();
        }
  
        private void CreateTotalWorked()
        {
            TotalWorked = new TaskViewModel();
            for (int i = 0; i < NumOfUpdates; i++)
            {
                var date = DateTime.Now.AddDays(i).Date;
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = rand.Next(0, (int)HoursPerDay)
                };
                TotalWorked.Updates.Add(update);
            }
        }

        private void CreateTasks()
        {
            Tasks = new ObservableCollection<TaskViewModel>();

            for (int i = 0; i < NumOfTasks; i++)
            {
                var t = new TaskViewModel
                {
                    Reference = "PRJSBO-" + rand.Next(10),
                    Feature = "Custom Coupon",
                    Name = "Sort coupons by fixture start time",
                    Hours = rand.Next(2, 16)
                };
                for (int j = 0; j < NumOfUpdates; j++)
                {
                    var previousHours =
                        j == 0 ? t.Hours : t.Updates[j - 1].Hours;

                    var date = DateTime.Now.AddDays(j);
                    var update = new TaskUpdateViewModel
                    {
                        Date = date,
                        Hours = previousHours - rand.Next(0, 3)
                    };
                    t.Updates.Add(update);
                }
                Tasks.Add(t);
            }

            CreateRollup();
        }

        private void CreateRollup()
        {
            TasksRollup = new TaskViewModel();
            var dateTotals = Tasks.SelectMany(t => t.Updates)
                .GroupBy(t => t.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.Hours)
                }).OrderBy(x => x.Date).ToList();

            foreach (var item in dateTotals)
            {
                TasksRollup.Updates.Add(new TaskUpdateViewModel
                {
                    Date = item.Date,
                    Hours = item.Total
                });
            }
        }

        private void CreateDistractions()
        {
            Distractions = new TaskViewModel();
            for (int i = 0; i < NumOfUpdates; i++)
            {
                var date = DateTime.Now.AddDays(i).Date;
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = rand.Next(0, (int)HoursPerDay)
                };
                Distractions.Updates.Add(update);
            }
        }

        private void CreateProjectedTotal()
        {
            ProjectedTotal = new TaskViewModel();
            var total = TotalEstimatedHours - HoursPerDay;
            for (int i = 0; i < NumOfUpdates; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = total
                };
                update.Hours *= NumOfTasks;
                ProjectedTotal.Updates.Add(update);
                total -= HoursPerDay;
            }
        }

        private void CreateProjectedTotalMinusDistractions()
        {
            ProjectedTotalMinusDistractions = new TaskViewModel();
            for (int i = 0; i < NumOfUpdates; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = NumOfUpdates - i
                };
                update.Hours *= NumOfTasks;
                ProjectedTotalMinusDistractions.Updates.Add(update);
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

        /// <summary>
        /// The <see cref="TotalEstimatedHours" /> property's name.
        /// </summary>
        public const string TotalEstimatedHoursPropertyName = "TotalEstimatedHours";

        private double totalEstimatedHours = 0;

        /// <summary>
        /// Sets and gets the TotalEstimatedHours property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double TotalEstimatedHours
        {
            get
            {
                return totalEstimatedHours;
            }

            set
            {
                if (totalEstimatedHours == value)
                {
                    return;
                }

                totalEstimatedHours = value;
                RaisePropertyChanged(TotalEstimatedHoursPropertyName);
            }
        }
    }
}