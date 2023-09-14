using DataEntities;
using eHCMS.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace aEMR.Common
{
    public class CalendarDay : NotifyChangedBase
    {
        private DateTime? _CalendarDate;
        private ObservableCollection<StaffWorkingSchedule> _StaffWorkingSchedule;
        private bool _IsSelected = false;
        private ICollectionView _StaffWorkingScheduleView;
        private int _TargetNumberOfCases;
        private int _QuantityOfProcessedCases;
        private bool _IsExtendDate = false;
        private bool _IsOpened = false;
        public int Day
        {
            get
            {
                return CalendarDate == null ? 0 : CalendarDate.Value.Day;
            }
        }
        public bool IsHasValue
        {
            get
            {
                return Day > 0;
            }
        }
        public ObservableCollection<StaffWorkingSchedule> StaffWorkingSchedule
        {
            get
            {
                return _StaffWorkingSchedule;
            }
            set
            {
                if (_StaffWorkingSchedule == value)
                {
                    return;
                }
                _StaffWorkingSchedule = value;
                RaisePropertyChanged("StaffWorkingSchedule");
                StaffWorkingScheduleView = CollectionViewSource.GetDefaultView(StaffWorkingSchedule);
                StaffWorkingScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("CurrentDepartment"));
                StaffWorkingScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("CurrentDeptLocation"));
                StaffWorkingScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("ConsultationSegment"));
            }
        }
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (_IsSelected == value)
                {
                    return;
                }
                _IsSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        public ICollectionView StaffWorkingScheduleView
        {
            get
            {
                return _StaffWorkingScheduleView;
            }
            set
            {
                if (_StaffWorkingScheduleView == value)
                {
                    return;
                }
                _StaffWorkingScheduleView = value;
                RaisePropertyChanged("StaffWorkingScheduleView");
            }
        }
        public DateTime? CalendarDate
        {
            get
            {
                return _CalendarDate;
            }
            set
            {
                if (_CalendarDate == value)
                {
                    return;
                }
                _CalendarDate = value;
                RaisePropertyChanged("CalendarDate");
                RaisePropertyChanged("Day");
                RaisePropertyChanged("IsHasValue");
            }
        }
        public int TargetNumberOfCases
        {
            get
            {
                return _TargetNumberOfCases;
            }
            set
            {
                if (_TargetNumberOfCases == value)
                {
                    return;
                }
                _TargetNumberOfCases = value;
                RaisePropertyChanged("TargetNumberOfCases");
                RaisePropertyChanged("TargetNumberOfCasesRemaining");
            }
        }
        public int QuantityOfProcessedCases
        {
            get
            {
                return _QuantityOfProcessedCases;
            }
            set
            {
                if (_QuantityOfProcessedCases == value)
                {
                    return;
                }
                _QuantityOfProcessedCases = value;
                RaisePropertyChanged("QuantityOfProcessedCases");
                RaisePropertyChanged("TargetNumberOfCasesRemaining");
            }
        }
        public int TargetNumberOfCasesRemaining
        {
            get
            {
                return TargetNumberOfCases - QuantityOfProcessedCases;
            }
        }
        public bool IsExtendDate
        {
            get
            {
                return _IsExtendDate;
            }
            set
            {
                if (_IsExtendDate == value)
                {
                    return;
                }
                _IsExtendDate = value;
                RaisePropertyChanged("IsExtendDate");
            }
        }
        public bool IsOpened
        {
            get
            {
                return _IsOpened;
            }
            set
            {
                if (_IsOpened == value)
                {
                    return;
                }
                _IsOpened = value;
                RaisePropertyChanged("IsOpened");
            }
        }
    }
}
