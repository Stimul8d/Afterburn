using System;
using System.Collections.Generic;
using System.Linq;
using Afterburn.Extensions;
using Afterburn.Model;
using GalaSoft.MvvmLight;

namespace Afterburn.ViewModel
{
    public class AnalysisViewModel : ViewModelBase
    {
        public TaskViewModel AnalysisDistractions { get; set; }
        public TaskViewModel AnalysisRemainingHours { get; set; }
        public TaskViewModel AnalysisProjectedTotal { get; set; }
        public TaskViewModel AnalysisTotalWorked { get; set; }

        public TaskViewModel Distractions { get; set; }
        public TaskViewModel RemainingHours { get; set; }
        public TaskViewModel ProjectedTotal { get; set; }
        public TaskViewModel TotalWorked { get; set; }

        public AnalysisViewModel()
        {
            this.Distractions = new TaskViewModel();
            this.RemainingHours = new TaskViewModel();
            this.ProjectedTotal = new TaskViewModel();
            this.TotalWorked = new TaskViewModel();

            this.AnalysisDistractions = new TaskViewModel();
            this.AnalysisRemainingHours = new TaskViewModel();
            this.AnalysisProjectedTotal = new TaskViewModel();
            this.AnalysisTotalWorked = new TaskViewModel();
        }

        public void CalculateTotals(IEnumerable<TaskViewModel> tasks,
            double hoursPerDay, bool skipWeekends)
        {
            if (!tasks.Any())
                return;

            var totalEstimatedHours = tasks.Sum(t => t.Hours);

            this.Distractions.Updates.Clear();
            this.RemainingHours.Updates.Clear();
            this.TotalWorked.Updates.Clear();

            var updates = this.GetDayUpdates(tasks);

            CalculateAverages(hoursPerDay);

            CreateRemainingAndTotalAndDistractions(updates, tasks, hoursPerDay);

            CreateIdealBurndown(totalEstimatedHours, tasks, hoursPerDay, skipWeekends);

            GenerateChartFriendlyValues(tasks, skipWeekends);
        }

        private void CalculateAverages(double hoursPerDay)
        {
            if (!TotalWorked.Updates.Any())
                return;
            AverageWorked = TotalWorked.Updates.Average(u => u.Hours);
            AverageWorkedMinusOneStdDev = AverageWorked - TotalWorked.Updates
                .Select(u => hoursPerDay - u.Hours).StdDev();
            AverageWorkedPlusOneStdDev = AverageWorked + TotalWorked.Updates
                .Select(u => hoursPerDay - u.Hours).StdDev();
        }

        private void GenerateChartFriendlyValues
            (IEnumerable<TaskViewModel> tasks, bool skipWeekends)
        {
            if (tasks.Any() && tasks.First().Updates.Any())
            {
                this.AnalysisDistractions.Updates.Clear();
                this.AnalysisProjectedTotal.Updates.Clear();
                this.AnalysisRemainingHours.Updates.Clear();
                this.AnalysisTotalWorked.Updates.Clear();

                //add day one to the analysis
                var dayOne = this.GetDayUpdates(tasks).First().Date;
                var dayZero = AddDays(dayOne, -1, skipWeekends);
                this.AnalysisDistractions.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Date = dayZero
                });

                this.AnalysisProjectedTotal.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Hours = tasks.Sum(t => t.Hours),
                    Date = dayZero
                });

                this.AnalysisRemainingHours.Updates.Add(new TaskUpdateViewModel(false)
                {
                    Hours = tasks.Sum(t => t.Hours),
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
        }

        private void CreateIdealBurndown(double totalEstimatedHours, IEnumerable<TaskViewModel> tasks, double hoursPerDay, bool skipWeekends)
        {
            //generate ideal burndown
            this.ProjectedTotal.Updates.Clear();
            var remainingTotal = totalEstimatedHours;
            var currrentDay = DateTime.Today;

            if (tasks.First().Updates.Any())
            {
                currrentDay = tasks.First().Updates.First().Date;
            }

            while (remainingTotal > -hoursPerDay)
            {
                remainingTotal -= hoursPerDay;
                var update = new TaskUpdateViewModel(false)
                {
                    Hours = remainingTotal,
                    Date = currrentDay
                };
                this.ProjectedTotal.Updates.Add(update);
                currrentDay = AddDays(currrentDay, 1, skipWeekends);
            }
        }

        private DayUpdate CreateRemainingAndTotalAndDistractions(List<DayUpdate> updates, IEnumerable<TaskViewModel> tasks, double hoursPerDay)
        {
            DayUpdate previousUpdate = null;

            for (int ix = 0; ix < updates.Count; ix++)
            {
                if (ix == 0)
                {
                    previousUpdate = new DayUpdate
                    {
                        Hours = tasks.Sum(t => t.Hours)
                    };
                }
                else
                {
                    previousUpdate = updates[ix - 1];
                }

                var update = updates[ix];
                var worked = previousUpdate.Hours - update.Hours;
                var remaining = hoursPerDay - worked;

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
            return previousUpdate;
        }

        private List<DayUpdate> GetDayUpdates(IEnumerable<TaskViewModel> tasks)
        {
            var dateTotals = tasks.SelectMany(t => t.Updates)
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

        /// <summary>
        /// The <see cref="AverageWorked" /> property's name.
        /// </summary>
        public const string AverageWorkedPropertyName = "AverageWorked";

        private double averageWorked = 0;

        /// <summary>
        /// Sets and gets the AverageWorked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double AverageWorked
        {
            get
            {
                return averageWorked;
            }

            set
            {
                if (averageWorked == value)
                {
                    return;
                }

                averageWorked = value;
                RaisePropertyChanged(AverageWorkedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AverageWorkedPlusOneStdDev" /> property's name.
        /// </summary>
        public const string AverageWorkedPlusOneStdDevPropertyName = "AverageWorkedPlusOneStdDev";

        private double averageWorkedPlusOneStdDev = 0;

        /// <summary>
        /// Sets and gets the AverageWorkedPlusOneStdDev property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double AverageWorkedPlusOneStdDev
        {
            get
            {
                return averageWorkedPlusOneStdDev;
            }

            set
            {
                if (averageWorkedPlusOneStdDev == value)
                {
                    return;
                }

                averageWorkedPlusOneStdDev = value;
                RaisePropertyChanged(AverageWorkedPlusOneStdDevPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AverageWorkedMinusOneStdDev" /> property's name.
        /// </summary>
        public const string AverageWorkedMinusOneStdDevPropertyName = "AverageWorkedMinusOneStdDev";

        private double averageWorkedMinusOneStdDev = 0;

        /// <summary>
        /// Sets and gets the AverageWorkedMinusOneStdDev property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double AverageWorkedMinusOneStdDev
        {
            get
            {
                return averageWorkedMinusOneStdDev;
            }

            set
            {
                if (averageWorkedMinusOneStdDev == value)
                {
                    return;
                }

                averageWorkedMinusOneStdDev = value;
                RaisePropertyChanged(AverageWorkedMinusOneStdDevPropertyName);
            }
        }
    }
}
