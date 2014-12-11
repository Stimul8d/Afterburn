using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Afterburn.Messages;

namespace Afterburn.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        const int NumOfUpdates = 3;
        const int NumOfTasks = 1;

        static readonly Random rand = new Random();

        public ObservableCollection<TaskViewModel> Tasks { get; set; }
        public TaskViewModel Distractions { get; set; }
        public TaskViewModel RemainingHours { get; set; }
        public TaskViewModel ProjectedTotal { get; set; }
        public TaskViewModel ProjectedTotalMinusDistractions { get; set; }
        public TaskViewModel TotalWorked { get; set; }

        public RelayCommand AddTaskCommand { get; set; }
        public RelayCommand AddDayCommand { get; set; }

        public MainViewModel()
        {
            Tasks = new ObservableCollection<TaskViewModel>();
            Distractions = new TaskViewModel();
            RemainingHours = new TaskViewModel();
            ProjectedTotal = new TaskViewModel();
            ProjectedTotalMinusDistractions = new TaskViewModel();
            TotalWorked = new TaskViewModel();

            //AddDummyTask();

            CreateTasks();
            CalculateTotals();
            //CreateRollup();
            //CreateProjectedTotal();
            //CreateProjectedTotalMinusDistractions();
            //CreateTotalWorked();
            

            AddTaskCommand = new RelayCommand(() =>
                {
                    Tasks.Add(new TaskViewModel());
                });

            Messenger.Default.Register<DeleteTaskMessage>(this,
                (m) =>
                {
                    Tasks.Remove(m.Task);
                    if (Tasks.Count == 0)
                        AddDummyTask();

                    CalculateTotals();
                });

            Messenger.Default.Register<DeleteDateCommand>(this,
                (m) =>
                {
                    foreach (var task in Tasks)
                    {
                        var updateToRemove = task.Updates
                            .SingleOrDefault(t => t.Date == m.Date);

                        if (updateToRemove == null)
                            continue;
                        task.Updates.Remove(updateToRemove);
                    }
                    CalculateTotals();
                    ShowHideAnalysis();
                });

            Messenger.Default.Register<UpdateModifiedMessage>(this,
                (m) =>
                {
                    CalculateTotals();
                });

            AddDayCommand = new RelayCommand(AddDay);

        }

        private void AddDay()
        {
            foreach (var task in Tasks)
            {
                var lasthours = task.Updates.LastOrDefault() == null
                                ? task.Hours
                                : task.Updates.Last().Hours;

                var lastDate = AddDays(task.Updates.LastOrDefault() == null
                                       ? DateTime.Today
                                       : task.Updates.Last().Date, 1, skipWeekends);

                task.Updates.Add(new TaskUpdateViewModel
                {
                    Date = lastDate,
                    Hours = lasthours
                });
            }

            CalculateTotals();
            ShowHideAnalysis();
        }

        void ShowHideAnalysis()
        {
            AnalysisVisibility = Tasks.Any()
                ? (Tasks.First().Updates.Any()
                    ? Visibility.Visible
                    : Visibility.Collapsed)
                : Visibility.Collapsed;
        }

        private void CalculateTotals()
        {
            Distractions.Updates.Clear();
            RemainingHours.Updates.Clear();
            TotalWorked.Updates.Clear();
            var updates = GetDayUpdates();
            DayUpdate previousUpdate = null;
            
            for (int ix = 0; ix < updates.Count; ix++)
			{
                if (ix == 0)
                {
                    previousUpdate = new DayUpdate
                    {
                        Hours = Tasks.Sum(t => t.Hours)
                    };
                }
                else
                {
                    previousUpdate = updates[ix - 1];
                }
            
                var update = updates[ix];
                var worked = previousUpdate.Hours - update.Hours;
                var remaining = HoursPerDay - worked;

                RemainingHours.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = update.Date.Date,
                    Hours = update.Hours
                });

                TotalWorked.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = update.Date.Date,
                    Hours = worked
                });

                Distractions.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = update.Date.Date,
                    Hours = remaining
                });

                TotalEstimatedHours = Tasks.Sum(t => t.Hours);
            }
        }

        public static DateTime AddDays(DateTime date, int days, bool ignoreWeekends)
        {
            if (!ignoreWeekends)
                return date.AddDays(days);

            if (days < 0)
            {
                throw new ArgumentException("days cannot be negative", "days");
            }

            if (days == 0) return date;

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
                days -= 1;
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
                days -= 1;
            }

            date = date.AddDays(days / 5 * 7);
            int extraDays = days % 5;

            if ((int)date.DayOfWeek + extraDays > 5)
            {
                extraDays += 2;
            }

            return date.AddDays(extraDays);

        }

        private void AddDummyTask()
        {
            var t = new TaskViewModel()
            {
                Reference = "Task-1",
                Feature = "Homepage",
                Name = "Complete the login form",
                Hours = rand.Next(2, 16)
            };
            Tasks.Add(t);
        }

        private void CreateTotalWorked()
        {
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
            for (int i = 0; i < NumOfTasks; i++)
            {
                var t = new TaskViewModel()
                {
                    Reference = "PRJSBO-" + rand.Next(10),
                    Feature = "Custom Coupon",
                    Name = "Sort coupons by fixture start time",
                    Hours = rand.Next(2, 16)
                };

                if (IsInDesignMode)
                {
                    for (int j = 0; j < NumOfUpdates; j++)
                    {
                        var previousHours =
                            j == 0 ? t.Hours : t.Updates[j - 1].Hours;

                        var date = DateTime.Now.AddDays(j - NumOfUpdates);
                        var update = new TaskUpdateViewModel
                        {
                            Date = date,
                            Hours = previousHours - rand.Next(0, 3)
                        };
                        t.Updates.Add(update);
                    }
                }
                Tasks.Add(t);
            }

            CreateRollup();
        }

        private void CreateRollup()
        {
            var dateTotals = GetDayUpdates();

            foreach (var item in dateTotals)
            {
                RemainingHours.Updates.Add(new TaskUpdateViewModel
                {
                    Date = item.Date,
                    Hours = item.Hours
                });
            }
        }

        private List<DayUpdate> GetDayUpdates()
        {
            var dateTotals = Tasks.SelectMany(t => t.Updates)
                                  .GroupBy(t => t.Date.Date)
                                  .Select(g => new DayUpdate
                                         {
                                             Date = g.Key,
                                             Hours = g.Sum(x => x.Hours)
                                         })
                                  .OrderBy(x => x.Date)
                                  .ToList();
            return dateTotals;
        }

        private void CreateDistractions()
        {
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
        /// The <see cref="SkipWeekends" /> property's name.
        /// </summary>
        public const string SkipWeekendsPropertyName = "SkipWeekends";

        private bool skipWeekends = true;

        /// <summary>
        /// Sets and gets the SkipWeekends property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool SkipWeekends
        {
            get
            {
                return skipWeekends;
            }

            set
            {
                if (skipWeekends == value)
                {
                    return;
                }

                skipWeekends = value;
                RaisePropertyChanged(SkipWeekendsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="HoursPerDay" /> property's name.
        /// </summary>
        public const string HoursPerDayPropertyName = "HoursPerDay";

        private double hoursPerDay = 8.0;

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
                CalculateTotals();
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

        /// <summary>
        /// The <see cref="AnalysisVisibility" /> property's name.
        /// </summary>
        public const string AnalysisVisibilityPropertyName = "AnalysisVisibility";

        private Visibility analysisVisibility = Visibility.Collapsed;

        /// <summary>
        /// Sets and gets the AnalysisVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility AnalysisVisibility
        {
            get
            {
                return analysisVisibility;
            }

            set
            {
                if (analysisVisibility == value)
                {
                    return;
                }

                analysisVisibility = value;
                RaisePropertyChanged(AnalysisVisibilityPropertyName);
            }
        }

        class DayUpdate
        {
            public DateTime Date { get; set; }
            public double Hours { get; set; }
        }
    }
}