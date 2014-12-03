using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afterburn.ViewModel
{
    public class AnalysisViewModel : ViewModelBase
    {
        public Dictionary<DateTime,int> HoursRemaining { get; set; }

        public AnalysisViewModel()
        {
            HoursRemaining = new Dictionary<DateTime, int>
                {
                    {DateTime.Now, 100},
                    {DateTime.Now.AddMonths(1), 130},
                    {DateTime.Now.AddMonths(2), 150},
                    {DateTime.Now.AddMonths(3), 125},
                    { DateTime.Now.AddMonths(4), 155 }
                };

        }
    }
}
