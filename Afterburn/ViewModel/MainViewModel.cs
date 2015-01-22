using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Afterburn.Messages;
using Afterburn.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Afterburn.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public AnalysisViewModel Analysis { get; set; }

        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        public RelayCommand AddTaskCommand { get; set; }
        public RelayCommand AddDayCommand { get; set; }
        public RelayCommand NewCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand LoadCommand { get; set; }

        public MainViewModel()
        {
                this.Analysis = new AnalysisViewModel();
            this.Tasks = new ObservableCollection<TaskViewModel>();

            this.AddDummyTask();
            this.CalculateTotals();

            SetupCommands();

            SetupMessages();
                
            }
  
        private void SetupMessages()
        {
            Messenger.Default.Register<DeleteTaskMessage>(this,
                (m) =>
                {
                    DeleteTask(m);
                });

            Messenger.Default.Register<DeleteDateMessage>(this,
                (m) =>
                {
                    DeleteDate(m);
                });

            Messenger.Default.Register<EstimateUpdatedMessage>(this,
                (m) =>
                {
                    if (UpdateEstimates(m))
                    {
                        return;
                    }
                });

            Messenger.Default.Register<UpdateModifiedMessage>(this,
                (m) =>
                {
                    this.CalculateTotals();
                });
        }
  
        private void SetupCommands()
        {
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

            this.AddTaskCommand = new RelayCommand(AddTask);
            this.AddDayCommand = new RelayCommand(this.AddDay);
        }

        private bool UpdateEstimates(EstimateUpdatedMessage m)
        {
            if (!m.Task.Updates.Any())
            {
                return true;
            }

            if (m.Task.Updates.First().Hours > 0)
            {
                foreach (var update in m.Task.Updates)
                {
                    update.Hours += m.Task.Hours - m.Previous;
                }
                return true;
            }

            foreach (var update in m.Task.Updates)
            {
                update.Hours = m.Task.Hours;
            }
            this.CalculateTotals();
            return false;
        }
  
        private void DeleteDate(DeleteDateMessage m)
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
        }
  
        private void DeleteTask(DeleteTaskMessage m)
        {
            this.Tasks.Remove(m.Task);
            if (this.Tasks.Count == 0)
            {
                this.AddDummyTask();
            }

            this.CalculateTotals();
        }
  
        private void AddTask()
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
            this.CalculateTotals();
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

        private void Reset()
        {
            this.Tasks.Clear();
            this.CalculateTotals();
            this.Tasks.Clear();
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
            Analysis.CalculateTotals(Tasks, HoursPerDay, SkipWeekends);
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

        internal void LoadState(AfterburnFile file)
        {
            this.Reset();
            foreach (var task in file.Tasks)
            {
                var vm = new TaskViewModel
                {
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
    }
}