using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Afterburn.AttachedProperties
{
    public class BlackoutDates
    {
        public static ObservableCollection<DateTime> GetBlackOutDates(DependencyObject obj)
        {
            return (ObservableCollection<DateTime>)obj.GetValue(BlackOutDatesProperty);
        }

        public static void SetBlackOutDates(DependencyObject obj, ObservableCollection<DateTime> value)
        {
            obj.SetValue(BlackOutDatesProperty, value);
        }

        public static readonly DependencyProperty BlackOutDatesProperty =
            DependencyProperty.RegisterAttached("BlackOutDates", typeof(ObservableCollection<DateTime>), typeof(BlackoutDates), new PropertyMetadata(null, OnBlackOutDatesChanged));
        
        private static Calendar control;

        private static void OnBlackOutDatesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            control = (Calendar)sender;
            var list = (ObservableCollection<DateTime>)e.NewValue;

            list.CollectionChanged +=(x,y)=>
            {
                control.BlackoutDates.Clear();
                foreach (var date in list.Where(d=>d.Date != control.SelectedDate))
                {
                    control.BlackoutDates.Add(new CalendarDateRange(date));
                }
            };

        }
    }
}
