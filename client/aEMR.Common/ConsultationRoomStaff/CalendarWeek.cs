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
    public class CalendarWeek : NotifyChangedBase
    {
        private CalendarDay[] _CalendarDayCollection = new CalendarDay[7] { new CalendarDay(), new CalendarDay(), new CalendarDay(), new CalendarDay(), new CalendarDay(), new CalendarDay(), new CalendarDay() };
        public CalendarDay[] CalendarDayCollection
        {
            get
            {
                return _CalendarDayCollection;
            }
            set
            {
                if (_CalendarDayCollection == value)
                {
                    return;
                }
                _CalendarDayCollection = value;
                RaisePropertyChanged("CalendarDayCollection");
                RaisePropertyChanged("IsHasValue");
            }
        }
        public bool IsHasValue
        {
            get
            {
                return CalendarDayCollection.Sum(x => x.Day) > 0;
            }
        }
    }
}
