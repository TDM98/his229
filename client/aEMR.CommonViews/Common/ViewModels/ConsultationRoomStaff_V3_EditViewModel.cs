using System.ComponentModel.Composition;
using aEMR.ViewContracts;
using aEMR.Common.BaseModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using aEMR.Common.Collections;
using aEMR.Infrastructure;
using System.Threading;
using aEMR.ServiceClient;
using eHCMSLanguage;
using DataEntities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using aEMR.CommonTasks;
using Caliburn.Micro;
using System.Xml.Linq;
using aEMR.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;

namespace aEMR.Common.ViewModels
{
    [Export(typeof(IConsultationRoomStaff_V3_Edit)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConsultationRoomStaff_V3_EditViewModel : ViewModelBase, IConsultationRoomStaff_V3_Edit
    {
        public ConsultationRoomStaff_V3_EditViewModel()
        {
            GetAllConsultationTimeSegments();
        }
        private StaffWorkingSchedule _SelectedStaffWorkingSchedule;
        public StaffWorkingSchedule SelectedStaffWorkingSchedule
        {
            get
            {
                return _SelectedStaffWorkingSchedule;
            }
            set
            {
                if (_SelectedStaffWorkingSchedule == value)
                    return;
                _SelectedStaffWorkingSchedule = value;
            }
        }
        private StaffWorkingSchedule _NewStaffWorkingSchedule;
        public StaffWorkingSchedule NewStaffWorkingSchedule
        {
            get
            {
                return _NewStaffWorkingSchedule;
            }
            set
            {
                if (_NewStaffWorkingSchedule == value)
                    return;
                _NewStaffWorkingSchedule = value;
            }
        }
        private CalendarDay _cDay;
        public CalendarDay cDay
        {
            get
            {
                return _cDay;
            }
            set
            {
                if (_cDay == value)
                    return;
                _cDay = value;
            }
        }
        private ObservableCollection<StaffWorkingSchedule> _ListStaffWorkingSchedule;
        public ObservableCollection<StaffWorkingSchedule> ListStaffWorkingSchedule
        {
            get
            {
                return _ListStaffWorkingSchedule;
            }
            set
            {
                if (_ListStaffWorkingSchedule == value)
                    return;
                _ListStaffWorkingSchedule = value;
                NotifyOfPropertyChange(() => ListStaffWorkingSchedule);
            }
        }
        private ObservableCollection<ConsultationTimeSegments> _lstConsultationTimeSegments;
        public ObservableCollection<ConsultationTimeSegments> lstConsultationTimeSegments
        {
            get
            {
                return _lstConsultationTimeSegments;
            }
            set
            {
                if (_lstConsultationTimeSegments == value)
                    return;
                _lstConsultationTimeSegments = value;
                NotifyOfPropertyChange(() => lstConsultationTimeSegments);
            }
        }
        private ConsultationTimeSegments _SelectedConsultationTimeSegments;
        public ConsultationTimeSegments SelectedConsultationTimeSegments
        {
            get
            {
                return _SelectedConsultationTimeSegments;
            }
            set
            {
                if (_SelectedConsultationTimeSegments == value)
                    return;
                _SelectedConsultationTimeSegments = value;
                NotifyOfPropertyChange(() => SelectedConsultationTimeSegments);
            }
        }
        private bool _IsConfirmed = false;
        public bool IsConfirmed
        {
            get
            {
                return _IsConfirmed;
            }
            set
            {
                if (_IsConfirmed == value)
                    return;
                _IsConfirmed = value;
                NotifyOfPropertyChange(() => IsConfirmed);
            }
        }
        private string _CRSANote;
        public string CRSANote
        {
            get
            {
                return _CRSANote;
            }
            set
            {
                if (_CRSANote == value)
                    return;
                _CRSANote = value;
                NotifyOfPropertyChange(() => CRSANote);
            }
        }
        public void RemoveSelectedStaffWorkingSchedule()
        {
            CRSANote = SelectedStaffWorkingSchedule.CRSANote;
            //cDay.StaffWorkingSchedule.Remove(SelectedStaffWorkingSchedule);
            ListStaffWorkingSchedule = cDay.StaffWorkingSchedule.DeepCopy();
            ListStaffWorkingSchedule.Remove(ListStaffWorkingSchedule.Where(x=> x.ConsultationSegment != null
                && x.ConsultationSegment.StartTime == SelectedStaffWorkingSchedule.ConsultationSegment.StartTime
                && x.ConsultationSegment.EndTime == SelectedStaffWorkingSchedule.ConsultationSegment.EndTime
                && x.DoctorStaff != null
                && x.DoctorStaff.StaffID == SelectedStaffWorkingSchedule.DoctorStaff.StaffID
                && x.V_TimeSegmentType == SelectedStaffWorkingSchedule.V_TimeSegmentType).FirstOrDefault());
        }
        public void btnSave()
        {
            string MessageErrorSum = "";
            DateTime StartTime = cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.StartTime.Hour).AddMinutes(SelectedConsultationTimeSegments.StartTime.Minute);
            DateTime EndTime = cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.EndTime.Hour).AddMinutes(SelectedConsultationTimeSegments.EndTime.Minute);
            DateTime? StartTime2 = SelectedConsultationTimeSegments.StartTime2 == null ? SelectedConsultationTimeSegments.StartTime2 :
                        cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.StartTime2.Value.Hour).AddMinutes(SelectedConsultationTimeSegments.StartTime2.Value.Minute);
            DateTime? EndTime2 = SelectedConsultationTimeSegments.EndTime2 == null ? SelectedConsultationTimeSegments.EndTime2 :
                        cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.EndTime2.Value.Hour).AddMinutes(SelectedConsultationTimeSegments.EndTime2.Value.Minute);
            if (!CheckValid(cDay, StartTime, EndTime, StartTime2, EndTime2, out string MessageError))
            {
                MessageErrorSum += MessageError + Environment.NewLine;
                if (!string.IsNullOrWhiteSpace(MessageErrorSum))
                {
                    MessageBox.Show(MessageErrorSum);
                }
                return;
            }
            IsConfirmed = true;
            NewStaffWorkingSchedule = new StaffWorkingSchedule{ 
                CurrentDeptLocation = SelectedStaffWorkingSchedule.CurrentDeptLocation.DeepCopy(),
                CurrentDepartment = SelectedStaffWorkingSchedule.CurrentDepartment.DeepCopy(),
                ConsultationSegment = new ConsultationTimeSegments
                {
                    ConsultationTimeSegmentID = SelectedConsultationTimeSegments.ConsultationTimeSegmentID,
                    SegmentName = SelectedConsultationTimeSegments.SegmentName,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    StartTime2 = StartTime2,
                    EndTime2 = EndTime2,
                },
                DoctorStaff = SelectedStaffWorkingSchedule.DoctorStaff.DeepCopy(),
                CRSAWeekID = SelectedStaffWorkingSchedule.CRSAWeekID,
                CRSANote = CRSANote,
                V_TimeSegmentType = SelectedConsultationTimeSegments.V_TimeSegmentType
            };
            this.TryClose();
        }

        public void btnClose()
        { 
            this.TryClose();
        }
        private void GetAllConsultationTimeSegments()
        {
            var t = new Thread(() =>
            {
                this.ShowBusyIndicator();
                using (var serviceFactory = new ClinicManagementServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginGetAllConsultationTimeSegments(Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            var results = contract.EndGetAllConsultationTimeSegments(asyncResult);
                            if (results != null && results.Count > 0)
                            {
                                lstConsultationTimeSegments = new ObservableCollection<ConsultationTimeSegments>();
                                foreach (var consTimeSeg in results)
                                {
                                    lstConsultationTimeSegments.Add(consTimeSeg);
                                }
                                if(SelectedStaffWorkingSchedule!= null)
                                {
                                    SelectedConsultationTimeSegments = SelectedStaffWorkingSchedule.ConsultationSegment;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                        }
                        finally
                        {
                            this.HideBusyIndicator();
                        }
                    }), null);
                }
            });
            t.Start();
        }
        private bool CheckValid(CalendarDay cDay, DateTime StartTime, DateTime EndTime, DateTime? StartTime2, DateTime? EndTime2, out string MessageError)
        {
            MessageError = "";
            Staff SelectedStaff = SelectedStaffWorkingSchedule.DoctorStaff;
            DeptLocation SelectedLocation = SelectedStaffWorkingSchedule.CurrentDeptLocation == null ? null : SelectedStaffWorkingSchedule.CurrentDeptLocation;
            RefDepartment SelectedDepartment = SelectedStaffWorkingSchedule.CurrentDepartment;
            // Cùng bác sĩ, 2 loại khung giờ
            if (ListStaffWorkingSchedule.Any(x => x.ConsultationSegment != null

                && x.DoctorStaff != null
                && x.DoctorStaff.StaffID == SelectedStaff.StaffID
                && x.V_TimeSegmentType != SelectedConsultationTimeSegments.V_TimeSegmentType))
            {
                StaffWorkingSchedule addSchedule = ListStaffWorkingSchedule.Where(x => x.ConsultationSegment != null
                       && x.DoctorStaff != null
                       && x.DoctorStaff.StaffID == SelectedStaff.StaffID
                       && x.V_TimeSegmentType != SelectedConsultationTimeSegments.V_TimeSegmentType).FirstOrDefault();
                MessageError = "Ngày " + cDay.CalendarDate.Value.Date.ToString("dd/MM/yyyy") + " " + SelectedStaff.PrintTitle + " "
                    + SelectedStaff.FullName + " đang có lịch " + addSchedule.ConsultationSegment.SegmentName + " tại "
                    + addSchedule.CurrentDepartment.DeptName + " "
                    + (addSchedule.CurrentDeptLocation == null ? "" : addSchedule.CurrentDeptLocation.Location.LocationName);
                return false;
            }

            // Cùng giờ, cùng bác sĩ
            if (ListStaffWorkingSchedule.Any(x => x.ConsultationSegment != null
                //&& x.ConsultationSegment.ConsultationTimeSegmentID == SelectedConsultationTimeSegments.ConsultationTimeSegmentID
                && ((x.ConsultationSegment.StartTime.TimeOfDay < StartTime.TimeOfDay && StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                    || (x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay && EndTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                    || (StartTime.TimeOfDay < x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay)
                    || (StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay < EndTime.TimeOfDay)
                    || (StartTime.TimeOfDay == x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay == EndTime.TimeOfDay))
                && x.V_TimeSegmentType == SelectedConsultationTimeSegments.V_TimeSegmentType
                && x.DoctorStaff != null
                && x.DoctorStaff.StaffID == SelectedStaff.StaffID))
            {
                StaffWorkingSchedule addSchedule = ListStaffWorkingSchedule.Where(x => x.ConsultationSegment != null
                        //&& x.ConsultationSegment.ConsultationTimeSegmentID == SelectedConsultationTimeSegments.ConsultationTimeSegmentID
                        && ((x.ConsultationSegment.StartTime.TimeOfDay < StartTime.TimeOfDay && StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                            || (x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay && EndTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                            || (StartTime.TimeOfDay < x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay)
                            || (StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay < EndTime.TimeOfDay)
                            || (StartTime.TimeOfDay == x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay == EndTime.TimeOfDay))
                        && x.V_TimeSegmentType == SelectedConsultationTimeSegments.V_TimeSegmentType
                        && x.DoctorStaff != null
                        && x.DoctorStaff.StaffID == SelectedStaff.StaffID).FirstOrDefault();
                MessageError = "Ngày " + cDay.CalendarDate.Value.Date.ToString("dd/MM/yyyy") + " " + SelectedStaff.PrintTitle + " "
                    + SelectedStaff.FullName + " đang có lịch " + addSchedule.ConsultationSegment.SegmentName + " tại "
                    + addSchedule.CurrentDepartment.DeptName + " "
                    + (addSchedule.CurrentDeptLocation == null ? "" : addSchedule.CurrentDeptLocation.Location.LocationName);
                return false;
            }

            // Cùng giờ, cùng phòng
            //if (SelectedLocation == null || SelectedLocation.LID == 0)
            //{
            //    if (cDay.StaffWorkingSchedule.Any(x => x.ConsultationSegment != null
            //        //&& x.ConsultationSegment.ConsultationTimeSegmentID == SelectedConsultationTimeSegments.ConsultationTimeSegmentID
            //        && ((x.ConsultationSegment.StartTime.TimeOfDay < StartTime.TimeOfDay && StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
            //            || (x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay && EndTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
            //            || (StartTime.TimeOfDay < x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay)
            //            || (StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay < EndTime.TimeOfDay)
            //            || (StartTime.TimeOfDay == x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay == EndTime.TimeOfDay))
            //        && x.V_TimeSegmentType == SelectedConsultationTimeSegments.V_TimeSegmentType
            //        && x.DoctorStaff != null
            //        && x.DoctorStaff.StaffID != SelectedStaff.StaffID
            //        && x.CurrentDepartment.DeptID == SelectedDepartment.DeptID
            //        ))
            //    {
            //        Staff addDoctor = cDay.StaffWorkingSchedule.Where(x => x.ConsultationSegment != null
            //            //&& x.ConsultationSegment.ConsultationTimeSegmentID == SelectedConsultationTimeSegments.ConsultationTimeSegmentID
            //            && ((x.ConsultationSegment.StartTime.TimeOfDay < StartTime.TimeOfDay && StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
            //                || (x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay && EndTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
            //                || (StartTime.TimeOfDay < x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay)
            //                || (StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay < EndTime.TimeOfDay)
            //                || (StartTime.TimeOfDay == x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay == EndTime.TimeOfDay))
            //            && x.V_TimeSegmentType == SelectedConsultationTimeSegments.V_TimeSegmentType
            //            && x.DoctorStaff != null
            //            && x.DoctorStaff.StaffID != SelectedStaff.StaffID
            //            && x.CurrentDepartment.DeptID == SelectedDepartment.DeptID).FirstOrDefault().DoctorStaff;
            //        MessageError = "Ngày " + cDay.CalendarDate.Value.Date.ToString("dd/MM/yyyy") + " tại " + SelectedDepartment.DeptName + " "
            //            + (SelectedLocation == null ? "" : SelectedLocation.Location.LocationName) + " có " + addDoctor.PrintTitle + " "
            //            + addDoctor.FullName + "; " + SelectedStaff.PrintTitle + " " + SelectedStaff.FullName + " đang trùng lịch làm";
            //        return false;
            //    }
            //}
            //else
            if (SelectedLocation != null && SelectedLocation.LID > 0)
            {
                if (ListStaffWorkingSchedule.Any(x => x.ConsultationSegment != null
                   //&& x.ConsultationSegment.ConsultationTimeSegmentID == SelectedConsultationTimeSegments.ConsultationTimeSegmentID
                   && ((x.ConsultationSegment.StartTime.TimeOfDay < StartTime.TimeOfDay && StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                       || (x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay && EndTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                       || (StartTime.TimeOfDay < x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay)
                       || (StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay < EndTime.TimeOfDay)
                       || (StartTime.TimeOfDay == x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay == EndTime.TimeOfDay))
                   && x.V_TimeSegmentType == SelectedConsultationTimeSegments.V_TimeSegmentType
                   && x.DoctorStaff != null
                   && x.DoctorStaff.StaffID != SelectedStaff.StaffID
                   && x.CurrentDepartment.DeptID == SelectedDepartment.DeptID
                   && x.CurrentDeptLocation.Location.LID == SelectedLocation.Location.LID
                   ))
                {
                    Staff addDoctor = ListStaffWorkingSchedule.Where(x => x.ConsultationSegment != null
                        //&& x.ConsultationSegment.ConsultationTimeSegmentID == SelectedConsultationTimeSegments.ConsultationTimeSegmentID
                        && ((x.ConsultationSegment.StartTime.TimeOfDay < StartTime.TimeOfDay && StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                            || (x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay && EndTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay)
                            || (StartTime.TimeOfDay < x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.StartTime.TimeOfDay < EndTime.TimeOfDay)
                            || (StartTime.TimeOfDay < x.ConsultationSegment.EndTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay < EndTime.TimeOfDay)
                            || (StartTime.TimeOfDay == x.ConsultationSegment.StartTime.TimeOfDay && x.ConsultationSegment.EndTime.TimeOfDay == EndTime.TimeOfDay))
                        && x.V_TimeSegmentType == SelectedConsultationTimeSegments.V_TimeSegmentType
                        && x.DoctorStaff != null
                        && x.DoctorStaff.StaffID != SelectedStaff.StaffID
                        && x.CurrentDepartment.DeptID == SelectedDepartment.DeptID
                        && x.CurrentDeptLocation.Location.LID == SelectedLocation.Location.LID).FirstOrDefault().DoctorStaff;
                    MessageError = "Ngày " + cDay.CalendarDate.Value.Date.ToString("dd/MM/yyyy") + " tại " + SelectedDepartment.DeptName + " "
                        + (SelectedLocation == null ? "" : SelectedLocation.Location.LocationName) + " có " + addDoctor.PrintTitle + " "
                        + addDoctor.FullName + "; " + SelectedStaff.PrintTitle + " " + SelectedStaff.FullName + " đang trùng lịch làm";
                    return false;
                }
            }
            return true;
        }
    }
}
