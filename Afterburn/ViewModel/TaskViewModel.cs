﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Afterburn.ViewModel
{
    public class TaskViewModel : ViewModelBase
    {
        public ObservableCollection<TaskUpdateViewModel> Updates { get; set; }

        public TaskViewModel()
        {
            Updates = new ObservableCollection<TaskUpdateViewModel>();
        }

        #region INPC
        /// <summary>
        /// The <see cref="Reference" /> property's name.
        /// </summary>
        public const string ReferencePropertyName = "Reference";

        private string reference = "";

        /// <summary>
        /// Sets and gets the Reference property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Reference
        {
            get
            {
                return reference;
            }

            set
            {
                if (reference == value)
                {
                    return;
                }

                reference = value;
                RaisePropertyChanged(ReferencePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Feature" /> property's name.
        /// </summary>
        public const string FeaturePropertyName = "Feature";

        private string feature = "";

        /// <summary>
        /// Sets and gets the Feature property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Feature
        {
            get
            {
                return feature;
            }

            set
            {
                if (feature == value)
                {
                    return;
                }

                feature = value;
                RaisePropertyChanged(FeaturePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string name = "";

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name == value)
                {
                    return;
                }

                name = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Hours" /> property's name.
        /// </summary>
        public const string HoursPropertyName = "Hours";

        private double hours = 0;

        /// <summary>
        /// Sets and gets the Hours property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Hours
        {
            get
            {
                return hours;
            }

            set
            {
                if (hours == value)
                {
                    return;
                }

                hours = value;
                RaisePropertyChanged(HoursPropertyName);
            }
        }
        #endregion
    }
}