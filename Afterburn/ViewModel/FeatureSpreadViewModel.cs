using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Afterburn.ViewModel
{
    public class FeatureSpreadViewModel : ViewModelBase
    {
        public ObservableCollection<FeatureTotalViewModel> Features { get; set; }

        public FeatureSpreadViewModel()
        {
            Features = new ObservableCollection<FeatureTotalViewModel>();
        }

        public void Clear()
        {
            Features.Clear();
        }

        public void Add(FeatureTotalViewModel feature)
        {
            Features.Add(feature);
        }
    }
}
