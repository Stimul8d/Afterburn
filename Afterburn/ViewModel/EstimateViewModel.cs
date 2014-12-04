using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;

namespace Afterburn.ViewModel
{

    public class EstimateViewModel : ViewModelBase
    {
        public ObservableCollection<TaskViewModel> Tasks { get; set; }
        public TaskViewModel Distractions { get; set; }
        public TaskViewModel TasksRollup { get; set; }

        public EstimateViewModel()
        {
            CreateTasks();
            CreateDistractions();
            
            TasksRollup = new TaskViewModel();
            for (int i = 0; i < 15; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = 15 - i + 0.25
                };
                update.Hours *= 13;
                TasksRollup.Updates.Add(update);
            }
        }
  
        private void CreateDistractions()
        {
            Distractions = new TaskViewModel();
            for (int i = 0; i < 15; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = 15 - i + 0.25
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

            for (int i = 0; i < 15; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var update = new TaskUpdateViewModel
                {
                    Date = date,
                    Hours = 15 - i + 0.25
                };
                t.Updates.Add(update);
            }

            for (int i = 0; i < 13; i++)
            {
                Tasks.Add(t);
            }
        }
    }
}