﻿using System;
using System.Collections.Generic;
using System.Linq;
using Afterburn.Extensions;
using Afterburn.Model;
using GalaSoft.MvvmLight;

namespace Afterburn.ViewModel
{
    public class AnalysisViewModel : ViewModelBase
    {
        private IEnumerable<TaskViewModel> tasks;
        private double hoursPerDay;
        private bool skipWeekends;
        private double totalEstimatedHours;
        private List<DayUpdate> updates;

        public TaskTotalViewModel AnalysisDistractions { get; set; }
        public TaskTotalViewModel AnalysisRemainingHours { get; set; }
        public TaskTotalViewModel AnalysisProjectedTotal { get; set; }
        public TaskTotalViewModel AnalysisTotalWorked { get; set; }
        public TaskTotalViewModel AnalysisAverageExtrapolation { get; set; }

        public TaskTotalViewModel Distractions { get; set; }
        public TaskTotalViewModel RemainingHours { get; set; }
        public TaskTotalViewModel ProjectedTotal { get; set; }
        public TaskTotalViewModel TotalWorked { get; set; }

        public FeatureSpreadViewModel FeatureSpread { get; set; }

        public AnalysisViewModel()
        {
            this.FeatureSpread = new FeatureSpreadViewModel();

            this.Distractions = new TaskTotalViewModel();
            this.RemainingHours = new TaskTotalViewModel();
            this.ProjectedTotal = new TaskTotalViewModel();
            this.TotalWorked = new TaskTotalViewModel();

            this.AnalysisDistractions = new TaskTotalViewModel();
            this.AnalysisRemainingHours = new TaskTotalViewModel();
            this.AnalysisProjectedTotal = new TaskTotalViewModel();
            this.AnalysisTotalWorked = new TaskTotalViewModel();
            this.AnalysisAverageExtrapolation = new TaskTotalViewModel();

            this.FeatureSpread = new FeatureSpreadViewModel();
        }

        public void CalculateTotals(IEnumerable<TaskViewModel> tasks,
            double hoursPerDay, bool skipWeekends)
        {
            this.tasks = tasks;
            this.hoursPerDay = hoursPerDay;
            this.skipWeekends = skipWeekends;
            this.totalEstimatedHours = tasks.Sum(t => t.Hours);
            this.updates = this.GetDayUpdates();

            if (!tasks.Any())
                return;

            this.Distractions.Updates.Clear();
            this.RemainingHours.Updates.Clear();
            this.TotalWorked.Updates.Clear();

            CreateRemainingAndTotalAndDistractions();

            CreateIdealBurndown();

            GenerateChartFriendlyValues();

            CreateAverageExtrapolation();

            CreateFeatureSpread();
        }

        private void CreateFeatureSpread()
        {
            FeatureSpread.Clear();
            FeatureSpread.Features.AddRange(GetFeatureHours());
        }

        private IEnumerable<FeatureTotalViewModel> GetFeatureHours()
        {
            if (!tasks.Any() || !tasks.All(t => t.Updates.Any()))
                return Enumerable.Empty<FeatureTotalViewModel>();

            return tasks.GroupBy(t => t.Feature)
                         .Select(g => new FeatureTotalViewModel
                         {
                             Name = g.Key,
                             Hours = g.Sum(t => t.Updates.Last().Hours)
                        }).ToList();
        }

        private void CreateAverageExtrapolation()
        {
            if (!tasks.First().Updates.Any())
                return;

            this.AnalysisAverageExtrapolation.Updates.Clear();
            var currrentDay = tasks.First().Updates.Last().Date;
            var remainingTotal = RemainingHours.Updates.Last().Hours;

            var avg = TotalWorked.Updates.Average(u => u.Hours);
            if (avg <= 0)
                return;

            while (true)
            {
                var update = new TaskTotalUpdateViewModel
                {
                    Hours = remainingTotal,
                    Date = currrentDay
                };
                this.AnalysisAverageExtrapolation.Updates.Add(update);

                currrentDay = AddDays(currrentDay, 1);
                remainingTotal -= avg;
                
                if (update.Hours <= 0)
                    return;
            }
        }

        private void GenerateChartFriendlyValues()
        {
            if (tasks.Any() && tasks.First().Updates.Any())
            {
                this.AnalysisProjectedTotal.Updates.Clear();
                this.AnalysisRemainingHours.Updates.Clear();
                this.AnalysisTotalWorked.Updates.Clear();
                this.AnalysisDistractions.Updates.Clear();

                //add day one to the analysis
                var dayOne = this.GetDayUpdates().First().Date;
                var dayZero = AddDays(dayOne, -1);

                this.AnalysisProjectedTotal.Updates.Add(new TaskTotalUpdateViewModel
                {
                    Hours = tasks.Sum(t => t.Hours),
                    Date = dayZero
                });

                this.AnalysisRemainingHours.Updates.Add(new TaskTotalUpdateViewModel
                {
                    Hours = tasks.Sum(t => t.Hours),
                    Date = dayZero
                });

                this.AnalysisDistractions.Updates.AddRange(this.Distractions.Updates);
                this.AnalysisProjectedTotal.Updates.AddRange(this.ProjectedTotal.Updates);
                this.AnalysisRemainingHours.Updates.AddRange(this.RemainingHours.Updates);
                this.AnalysisTotalWorked.Updates.AddRange(this.TotalWorked.Updates);
            }
        }

        private void CreateIdealBurndown()
        {
            //generate ideal burndown
            this.ProjectedTotal.Updates.Clear();
            var remainingTotal = totalEstimatedHours;
            var currrentDay = DateTime.Today;

            if (tasks.First().Updates.Any())
            {
                currrentDay = tasks.First().Updates.First().Date;
            }

            while (true)
            {
                remainingTotal -= hoursPerDay;
                var update = new TaskTotalUpdateViewModel
                {
                    Hours = remainingTotal,
                    Date = currrentDay
                };
                this.ProjectedTotal.Updates.Add(update);
                currrentDay = AddDays(currrentDay, 1);
                if (update.Hours <= 0)
                    return;
            }
        }

        private DayUpdate CreateRemainingAndTotalAndDistractions()
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

                this.RemainingHours.Updates.Add(new TaskTotalUpdateViewModel
                {
                    Date = update.Date.Date,
                    Hours = update.Hours
                });

                this.TotalWorked.Updates.Add(new TaskTotalUpdateViewModel
                {
                    Date = update.Date.Date,
                    Hours = worked
                });

                this.Distractions.Updates.Add(new TaskTotalUpdateViewModel
                {
                    Date = update.Date.Date,
                    Hours = remaining
                });
            }
            return previousUpdate;
        }

        private List<DayUpdate> GetDayUpdates()
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

        public static DateTime AddDays(DateTime date, int days)
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
    }
}
