using DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aEMR.Common
{
    public class StaffWorkingSchedule
    {
        public DeptLocation CurrentDeptLocation { get; set; }
        public RefDepartment CurrentDepartment { get; set; }
        public ConsultationTimeSegments ConsultationSegment { get; set; }
        public Staff DoctorStaff { get; set; }
        public IList<RefMedicalServiceItem> ServiceItemCollection { get; set; }
        public long ConsultationRoomStaffAllocationServiceListID { get; set; }
        public long CRSAWeekID { get; set; }
        public string CRSANote { get; set; }
        public long V_TimeSegmentType { get; set; }
    }
}
