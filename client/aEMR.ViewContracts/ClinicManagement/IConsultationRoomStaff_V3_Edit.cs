using aEMR.Common;
using DataEntities;
using System;
using System.Collections.ObjectModel;

namespace aEMR.ViewContracts
{
    public interface IConsultationRoomStaff_V3_Edit
    {
        StaffWorkingSchedule SelectedStaffWorkingSchedule { get; set; }
        StaffWorkingSchedule NewStaffWorkingSchedule { get; set; }
        CalendarDay cDay { get; set; }
        //ObservableCollection<StaffWorkingSchedule> ListStaffWorkingSchedule { get; set; }
        bool IsConfirmed { get; set; }
        void RemoveSelectedStaffWorkingSchedule();

    }
}