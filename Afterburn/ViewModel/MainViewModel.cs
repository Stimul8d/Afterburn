using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Afterburn.Extensions;
using Afterburn.Messages;
using Afterburn.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Afterburn.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        public TaskViewModel Distractions { get; set; }

        public TaskViewModel RemainingHours { get; set; }

        public TaskViewModel ProjectedTotal { get; set; }

        public TaskViewModel TotalWorked { get; set; }

        public TaskViewModel AnalysisDistractions { get; set; }

        public TaskViewModel AnalysisRemainingHours { get; set; }

        public TaskViewModel AnalysisProjectedTotal { get; set; }

        public TaskViewModel AnalysisTotalWorked { get; set; }

        public RelayCommand AddTaskCommand { get; set; }

        public RelayCommand AddDayCommand { get; set; }

        public RelayCommand NewCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand LoadCommand { get; set; }

        public MainViewModel()
        {
            this.Tasks = new ObservableCollection<TaskViewModel>();

            this.Distractions = new TaskViewModel();
            this.RemainingHours = new TaskViewModel();
            this.ProjectedTotal = new TaskViewModel();
            this.TotalWorked = new TaskViewModel();

            this.AnalysisDistractions = new TaskViewModel();
            this.AnalysisRemainingHours = new TaskViewModel();
            this.AnalysisProjectedTotal = new TaskViewModel();
            this.AnalysisTotalWorked = new TaskViewModel();

            this.AddDummyTask();
            this.CalculateTotals();

            this.NewCommand = new RelayCommand(() =>
            {
                this.Reset();
            });

            this.SaveCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send<SaveMessage>(new SaveMessage());
            });

            this.LoadCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send<LoadMessage>(new LoadMessage());
            });

            this.AddTaskCommand = new RelayCommand(() =>
            {
                var newTask = new TaskViewModel();
                if (this.Tasks.Any(t => t.Updates.Any()))
                {
                    var existingTask = this.Tasks.First(t => t.Updates.Any());
                    {
                        foreach (var update in existingTask.Updates)
                        {
                            newTask.Updates.Add(new TaskUpdateViewModel
                            {
                                Date = update.Date,
                                Hours = 0
                            });
                        }
                    }
                }

                this.Tasks.Add(newTask);
            });

            Messenger.Default.Register<DeleteTaskMessage>(this,
                (m) =>
                {
                    this.Tasks.Remove(m.Task);
                    if (this.Tasks.Count == 0)
                    {
                        this.AddDummyTask();
                    }

                    this.CalculateTotals();
                });

            Messenger.Default.Register<DeleteDateMessage>(this,
                (m) =>
                {
                    foreach (var task in this.Tasks)
                    {
                        var updateToRemove = task.Updates
                                                 .SingleOrDefault(t => t.Date == m.Date);

                        if (updateToRemove == null)
                        {
                            continue;
                        }
                        task.Updates.Remove(updateToRemove);
                    }
                    this.CalculateTotals();
                    this.ShowHideAnalysis();
                });

            Messenger.Default.Register<EstimateUpdatedMessage>(this,
                (m) =>
                {
                    if (!m.Task.Updates.Any())
                    {
                        return;
                    }

                    if (m.Task.Updates.First().Hours > 0)
                    {
                        foreach (var update in m.Task.Updates)
                        {
                            update.Hours += m.Task.Hours - m.Previous;
                        }
                        return;
                    }

                    foreach (var update in m.Task.Updates)
                    {
                        update.Hours = m.Task.Hours;
                    }
                    this.CalculateTotals();
                });

            Messenger.Default.Register<UpdateModifiedMessage>(this,
                (m) =>
                {
                    this.CalculateTotals();
                });

            this.AddDayCommand = new RelayCommand(this.AddDay);
        }

        private void Reset()
        {
            this.Tasks.Clear();
            this.CalculateTotals();
            this.Tasks.Clear();
        }

        private void AddDay()
        {
            foreach (var task in this.Tasks)
            {
                task.AllowEdits = true;
                var lasthours = task.Updates.LastOrDefault() == null
                                ? task.Hours
                                : task.Updates.Last().Hours;

                var lastDate = AddDays(task.Updates.LastOrDefault() == null
                                       ? DateTime.Today.AddDays(-1)
                                       : task.Updates.Last().Date, 1, this.skipWeekends);

                task.Updates.Add(new TaskUpdateViewModel
                {
                    Date = lastDate,
                    Hours = lasthours
                });
            }

            this.CalculateTotals();
        }

        private void ShowHideAnalysis()
        {
            this.AnalysisVisibility = this.Tasks.Any()
                                      ? (this.Tasks.First().Updates.Any()
                                         ? Visibility.Visible
                                         : Visibility.Collapsed)
                                      : Visibility.Collapsed;
            this.SelectedTabIndex = 0;
        }

        private void CalculateTotals()
        {
            if (!this.Tasks.Any())
            {
                this.AddDummyTask();
            }

            this.TotalEstimatedHours = this.Tasks.Sum(t => t.Hours);
            this.Distractions.Updates.Clear();
            this.RemainingHours.Updates.Clear();
            this.TotalWorked.Updates.Clear();
            var updates = this.GetDayUpdates();
            DayUpdate previousUpdate = null;

            for (int ix = 0; ix < updates.Count; ix++)
            {
                if (ix == 0)
                {
                    previousUpdate = new DayUpdate
                    {
                        Hours = this.Tasks.Sum(t => t.Hours)
                    };
                }
                else
                {
                    previousUpdate = updates[ix - 1];
                }

                var update = updates[ix];
                var worked = previousUpdate.Hours - update.Hours;
                var remaining = this.HoursPerDay - worked;

                this.RemainingHours.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = update.Date.Date,
                    Hours = update.Hours
                });

                this.TotalWorked.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = update.Date.Date,
                    Hours = worked
                });

                this.Distractions.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = update.Date.Date,
                    Hours = remaining
                });
            }

            //generate ideal burndown
            this.ProjectedTotal.Updates.Clear();
            var remainingTotal = this.TotalEstimatedHours;
            var currrentDay = DateTime.Today;

            if (this.Tasks.First().Updates.Any())
            {
                currrentDay = this.Tasks.First().Updates.First().Date;
            }

            while (remainingTotal > -this.HoursPerDay)
            {
                remainingTotal -= this.HoursPerDay;
                var update = new TaskUpdateViewModel(false)
                {
                    Hours = remainingTotal,
                    Date = currrentDay
                };
                this.ProjectedTotal.Updates.Add(update);
                currrentDay = AddDays(currrentDay, 1, this.SkipWeekends);
            }

            if (this.Tasks.Any() && this.Tasks.First().Updates.Any())
            {
                this.AnalysisDistractions.Updates.Clear();
                this.AnalysisProjectedTotal.Updates.Clear();
                this.AnalysisRemainingHours.Updates.Clear();
                this.AnalysisTotalWorked.Updates.Clear();

                //add day one to the analysis
                var dayOne = this.GetDayUpdates().First().Date;
                var dayZero = AddDays(dayOne, -1, this.skipWeekends);
                this.AnalysisDistractions.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = dayZero
                });

                this.AnalysisProjectedTotal.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Hours = this.Tasks.Sum(t => t.Hours),
                    Date = dayZero
                });

                this.AnalysisRemainingHours.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Hours = this.Tasks.Sum(t => t.Hours),
                    Date = dayZero
                });

                this.AnalysisTotalWorked.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Hours = 0,
                    Date = dayZero
                });

                this.AnalysisDistractions.Updates.AddRange(this.Distractions.Updates);
                this.AnalysisProjectedTotal.Updates.AddRange(this.ProjectedTotal.Updates);
                this.AnalysisRemainingHours.Updates.AddRange(this.RemainingHours.Updates);
                this.AnalysisTotalWorked.Updates.AddRange(this.TotalWorked.Updates);
            }
            this.ShowHideAnalysis();
        }

        public static DateTime AddDays(DateTime date, int days, bool skipWeekends)
        {
            DateTime tmpDate = date;
            while (days != 0)
            {
                var sign = Math.Sign(days);

                tmpDate = tmpDate.AddDays(sign);
                if ((tmpDate.DayOfWeek < DayOfWeek.Saturday &&
                     tmpDate.DayOfWeek > DayOfWeek.Sunday))
                {
                    days -= sign;
                }
            }
            return tmpDate;
        }

        public DateTime AddWorkdays(DateTime originalDate, int workDays)
        {
            DateTime tmpDate = originalDate;
            while (workDays > 0)
            {
                tmpDate = tmpDate.AddDays(1);
                if (tmpDate.DayOfWeek < DayOfWeek.Saturday &&
                    tmpDate.DayOfWeek > DayOfWeek.Sunday)
                {
                    workDays--;
                }
            }
            return tmpDate;
        }

        private void AddDummyTask()
        {
            var t = new TaskViewModel();
            this.Tasks.Add(t);
        }

        private List<DayUpdate> GetDayUpdates()
        {
            var dateTotals = this.Tasks.SelectMany(t => t.Updates)
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

        #region INPC
        
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
                return this.skipWeekends;
            }
            
            set
            {
                if (this.skipWeekends == value)
                {
                    return;
                }
                
                this.skipWeekends = value;
                this.RaisePropertyChanged(SkipWeekendsPropertyName);
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
                return this.hoursPerDay;
            }
            
            set
            {
                if (this.hoursPerDay == value)
                {
                    return;
                }
                
                this.hoursPerDay = value;
                this.RaisePropertyChanged(HoursPerDayPropertyName);
                this.CalculateTotals();
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
                return this.totalEstimatedHours;
            }
            
            set
            {
                if (this.totalEstimatedHours == value)
                {
                    return;
                }
                
                this.totalEstimatedHours = value;
                this.RaisePropertyChanged(TotalEstimatedHoursPropertyName);
            }
        }
        
        /// <summary>
        /// The <see cref="AllowEstimateEdits" /> property's name.
        /// </summary>
        public const string AllowEstimateEditsPropertyName = "AllowEstimateEdits";
        
        private bool allowEstimateEdits = true;
        
        /// <summary>
        /// Sets and gets the AllowEstimateEdits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AllowEstimateEdits
        {
            get
            {
                return this.allowEstimateEdits;
            }
            
            set
            {
                if (this.allowEstimateEdits == value)
                {
                    return;
                }
                
                this.allowEstimateEdits = value;
                this.RaisePropertyChanged(AllowEstimateEditsPropertyName);
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
                return this.analysisVisibility;
            }
            
            set
            {
                if (this.analysisVisibility == value)
                {
                    return;
                }
                
                this.analysisVisibility = value;
                this.RaisePropertyChanged(AnalysisVisibilityPropertyName);
            }
        }
        
        /// <summary>
        /// The <see cref="SelectedTabIndex" /> property's name.
        /// </summary>
        public const string SelectedTabIndexPropertyName = "SelectedTabIndex";
        
        private int selectedTabIndex = 0;
        
        /// <summary>
        /// Sets and gets the SelectedTabIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SelectedTabIndex
        {
            get
            {
                return this.selectedTabIndex;
            }
            
            set
            {
                if (this.selectedTabIndex == value)
                {
                    return;
                }
                
                this.selectedTabIndex = value;
                this.RaisePropertyChanged(SelectedTabIndexPropertyName);
            }
        }
        
        #endregion
        
        internal void LoadState(AfterburnFile file)
        {
            this.Reset();
            foreach (var task in file.Tasks)
            {
                var vm = new TaskViewModel
                {
                    Reference = task.Reference,
                    Feature = task.Feature,
                    Name = task.Name,
                    Hours = task.Hours
                };
                
                this.Tasks.Add(vm);
                
                foreach (var update in task.Updates)
                {
                    vm.Updates.Add(new TaskUpdateViewModel
                    {
                        Date = update.Date,
                        Hours = update.Hours
                    });
                }
            }
            CalculateTotals();
        }
    }
}