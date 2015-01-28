/*
In App.xaml:
<Application.Resources>
<vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:Afterburn.ViewModel"
x:Key="Locator" />
</Application.Resources>
  
In the View:
DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Afterburn.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator : ViewModelBase
    {
        /// <summary>
        /// The <see cref="Main" /> property's name.
        /// </summary>
        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]

        public const string MainPropertyName = "Main";

        private MainViewModel mainViewModel = null;

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            if (!SimpleIoc.Default.IsRegistered<ViewModelLocator>())
            {
                SimpleIoc.Default.Register<ViewModelLocator>(() => this); 
            }
        }

        /// <summary>
        /// Sets and gets the Main property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MainViewModel Main
        {
            get
            {
                if (this.mainViewModel == null)
                {
                    this.mainViewModel =
                        ServiceLocator.Current.GetInstance<MainViewModel>();
                }
                return this.mainViewModel;
            }

            set
            {
                if (this.mainViewModel == value)
                {
                    return;
                }

                this.mainViewModel = value;
                this.RaisePropertyChanged(MainPropertyName);
            }
        }
    }
}