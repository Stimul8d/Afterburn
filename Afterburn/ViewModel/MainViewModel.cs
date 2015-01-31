using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Afterburn.Messages;
using Afterburn.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Afterburn.Extensions;
using Ploeh.AutoFixture;
using System.Diagnostics;

namespace Afterburn.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private bool enableCalculation = true;

        public AnalysisViewModel Analysis { get; set; }
        public EditDateViewModel EditDate { get; set; }

        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        public RelayCommand AddTaskCommand { get; set; }
        public RelayCommand AddDayCommand { get; set; }
        public RelayCommand NewCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand LoadCommand { get; set; }
        public RelayCommand SortFeaturesCommand { get; set; }

        public MainViewModel()
        {
            Analysis = new AnalysisViewModel();
            EditDate = new EditDateViewModel();
            Tasks = new ObservableCollection<TaskViewModel>();

#if DEBUG
            var fixture = new Fixture();
            fixture.RepeatCount = 15;
            var random = new Random(DateTime.Now.Millisecond);
            var file = fixture.Create<AfterburnFile>();

            for (int t = 0; t < fixture.RepeatCount; t++)
            {
                var task = file.Tasks[t];
                for (int u = 0; u < fixture.RepeatCount; u++)
                {
                    var update = task.Updates[u];
                    update.Date = DateTime.Today.AddDays(u);
                }
            }

            LoadState(file);
#else
            AddDummyTask();
            CalculateTotals();
#endif


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

            Messenger.Default.Register<EditDateMessage>(this, (m) =>
            {
                var dates = Tasks.First().Updates.Select(u => u.Date).ToList();
                Messenger.Default.Send<BlacklistDatesMessage>(
                    new BlacklistDatesMessage(dates));
            });

            Messenger.Default.Register<ConfirmDateChangedMessage>(this, (m) =>
            {
                ChangeDate(m.From, m.To);
            });

            Messenger.Default.Register<FeatureNameUpdatedMessage>(this, (m) =>
            {
                CalculateTotals();
            });

            Messenger.Default.Register<DeleteDateMessage>(this,
                (m) =>
                {
                    DeleteDate(m);
                });

            Messenger.Default.Register<EstimateUpdatedMessage>(this,
                (m) =>
                {
                    UpdateEstimates(m);
                });

            Messenger.Default.Register<UpdateModifiedMessage>(this,
                (m) =>
                {
                    CalculateTotals();
                });
        }

        private void ChangeDate(DateTime from, DateTime to)
        {
            DisableCalculation();
            foreach (var task in Tasks)
            {
                var updates = task.Updates.ToList();
                task.Updates.Clear();
                updates.Single(u => u.Date == from).Date = to;
                task.Updates.AddRange(updates.OrderBy(u => u.Date));
            }
            AllowCalculation();
            CalculateTotals();
        }

        private void DisableCalculation()
        {
            Debug.WriteLine("Disabling Calc");
            enableCalculation = false;
        }

        private void SetupCommands()
        {
            NewCommand = new RelayCommand(() => { Reset(); AddDummyTask(); });

            SaveCommand = new RelayCommand(() => { Messenger.Default.Send<SaveMessage>(new SaveMessage()); });

            LoadCommand = new RelayCommand(() => { Messenger.Default.Send<LoadMessage>(new LoadMessage()); });

            AddTaskCommand = new RelayCommand(AddTask);

            AddDayCommand = new RelayCommand(AddDay);

            SortFeaturesCommand = new RelayCommand(SortFeatures);
        }

        private void SortFeatures()
        {
            Tasks.Sort(x => x.Feature);
        }

        private void UpdateEstimates(EstimateUpdatedMessage m)
        {
            DisableCalculation();
            if (m.Task.Updates.Any() &&
                m.Task.Updates.First().Hours > 0)
            {
                foreach (var update in m.Task.Updates)
                {
                    update.Hours += m.Task.Hours - m.Previous;
                }
                AllowCalculation();
                CalculateTotals();
                return;
            }

            foreach (var update in m.Task.Updates)
            {
                update.Hours = m.Task.Hours;
            }
            AllowCalculation();
            CalculateTotals();
        }

        private void DeleteDate(DeleteDateMessage m)
        {
            DisableCalculation();
            foreach (var task in Tasks)
            {
                var updateToRemove = task.Updates
                                         .SingleOrDefault(t => t.Date == m.Date);

                if (updateToRemove == null)
                {
                    continue;
                }
                task.Updates.Remove(updateToRemove);
            }
            AllowCalculation();
            CalculateTotals();
            ShowHideAnalysis();
        }

        private void DeleteTask(DeleteTaskMessage m)
        {
            DisableCalculation();
            Tasks.Remove(m.Task);

            if (Tasks.Count == 0)
            {
                AddDummyTask();
            }

            AllowCalculation();
            CalculateTotals();
        }

        private void AddTask()
        {
            DisableCalculation();
            var newTask = new TaskViewModel();
            if (Tasks.Any(t => t.Updates.Any()))
            {
                var existingTask = Tasks.First(t => t.Updates.Any());
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
            AllowCalculation();
            Tasks.Add(newTask);
            CalculateTotals();
        }

        private void AllowCalculation()
        {
            Debug.WriteLine("Enable Calc");
            enableCalculation = true;
        }

        private void AddDay()
        {
            DisableCalculation();
            foreach (var task in Tasks)
            {
                task.AllowEdits = true;
                var lasthours = task.Updates.LastOrDefault() == null
                                ? task.Hours
                                : task.Updates.Last().Hours;

                var lastDate = AddDays(task.Updates.LastOrDefault() == null
                                       ? DateTime.Today.AddDays(-1)
                                       : task.Updates.Last().Date, 1, skipWeekends);

                task.Updates.Add(new TaskUpdateViewModel
                {
                    Date = lastDate,
                    Hours = lasthours
                });
            }
            AllowCalculation();
            CalculateTotals();
        }

        private void Reset()
        {
            Analysis.Clear();
            Tasks.Clear();
            CalculateTotals();
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
            AnalysisVisibility = Tasks.Any()
                                      ? (Tasks.First().Updates.Any()
                                         ? Visibility.Visible
                                         : Visibility.Collapsed)
                                      : Visibility.Collapsed;
            SelectedTabIndex = 0;
        }

        private void CalculateTotals()
        {
            if (!enableCalculation) return;
            Analysis.CalculateTotals(Tasks, HoursPerDay, SkipWeekends);
            TotalEstimatedHours = Tasks.Sum(t => t.Hours);
            ShowHideAnalysis();
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
            Tasks.Add(t);
        }

        internal void LoadState(AfterburnFile file)
        {
            DisableCalculation();

            foreach (var task in file.Tasks)
            {
                var vm = new TaskViewModel
                {
                    Feature = task.Feature,
                    Name = task.Name,
                    Hours = task.Hours
                };

                Tasks.Add(vm);

                foreach (var update in task.Updates)
                {
                    vm.Updates.Add(new TaskUpdateViewModel
                    {
                        Date = update.Date,
                        Hours = update.Hours
                    });
                }
            }
            AllowCalculation();
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
                return allowEstimateEdits;
            }

            set
            {
                if (allowEstimateEdits == value)
                {
                    return;
                }

                allowEstimateEdits = value;
                RaisePropertyChanged(AllowEstimateEditsPropertyName);
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
                return selectedTabIndex;
            }

            set
            {
                if (selectedTabIndex == value)
                {
                    return;
                }

                selectedTabIndex = value;
                RaisePropertyChanged(SelectedTabIndexPropertyName);
            }
        }

        #endregion
    }
}