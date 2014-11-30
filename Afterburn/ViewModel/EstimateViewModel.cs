using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace Afterburn.ViewModel
{

    public class EstimateViewModel : ViewModelBase
    {
        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        public EstimateViewModel()
        {
            Tasks = new ObservableCollection<TaskViewModel>();
            var t = new TaskViewModel
            {
                Reference = "PRJSBO-2014",
                Feature = "Custom Coupon",
                Name = "Sort coupons by fixture start time",
                Hours = 6.25
            };

            for (int i = 0; i < 25; i++)
            {
                Tasks.Add(t); 
            }
        }
    }
}