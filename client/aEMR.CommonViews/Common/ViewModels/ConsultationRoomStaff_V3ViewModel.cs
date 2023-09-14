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
    [Export(typeof(IConsultationRoomStaff_V3)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConsultationRoomStaff_V3ViewModel : ViewModelBase, IConsultationRoomStaff_V3
    {
        private long StaffCatType = (long)V_StaffCatType.BacSi;
        public DateTime CurDateTime = DateTime.Now;
        ObservableCollection<CalendarWeek> _CalendarWeekCollection;
        public ObservableCollection<CalendarWeek> CalendarWeekCollection
        {
            get
            {
                return _CalendarWeekCollection;
            }
            set
            {
                if (_CalendarWeekCollection == value)
                {
                    return;
                }
                _CalendarWeekCollection = value;
                NotifyOfPropertyChange(() => CalendarWeekCollection);
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
        private ObservableCollection<RefStaffCategory> _allRefStaffCategory;
        public ObservableCollection<RefStaffCategory> allRefStaffCategory
        {
            get
            {
                return _allRefStaffCategory;
            }
            set
            {
                if (_allRefStaffCategory == value)
                    return;
                _allRefStaffCategory = value;
                NotifyOfPropertyChange(() => allRefStaffCategory);
            }
        }
        private ObservableCollection<Staff> _allStaff;
        public ObservableCollection<Staff> allStaff
        {
            get
            {
                return _allStaff;
            }
            set
            {
                if (_allStaff == value)
                    return;
                _allStaff = value;
                NotifyOfPropertyChange(() => allStaff);
                StaffViewCollection = CollectionViewSource.GetDefaultView(allStaff);
                StaffViewCollection.Filter = (x) => { return string.IsNullOrEmpty(FilterDocterString) || (!string.IsNullOrEmpty((x as Staff).FullName) && Globals.RemoveVietnameseString((x as Staff).FullName).ToLower().Contains(Globals.RemoveVietnameseString(FilterDocterString).ToLower())); };
            }
        }
        private ICollectionView _StaffViewCollection;
        public ICollectionView StaffViewCollection
        {
            get
            {
                return _StaffViewCollection;
            }
            set
            {
                if (_StaffViewCollection == value)
                {
                    return;
                }
                _StaffViewCollection = value;
                NotifyOfPropertyChange(() => StaffViewCollection);
            }
        }
        private RefStaffCategory _SelectedRefStaffCategory;
        public RefStaffCategory SelectedRefStaffCategory
        {
            get
            {
                return _SelectedRefStaffCategory;
            }
            set
            {
                if (_SelectedRefStaffCategory == value)
                    return;
                _SelectedRefStaffCategory = value;
                NotifyOfPropertyChange(() => SelectedRefStaffCategory);
                if (SelectedRefStaffCategory != null)
                {
                    GetAllStaff(SelectedRefStaffCategory.StaffCatgID);
                }
            }
        }
        private ObservableCollection<ConsultationRoomStaffAllocations> _tempConsRoomStaffAlloc;
        public ObservableCollection<ConsultationRoomStaffAllocations> tempConsRoomStaffAlloc
        {
            get
            {
                return _tempConsRoomStaffAlloc;
            }
            set
            {
                if (_tempConsRoomStaffAlloc == value)
                    return;
                _tempConsRoomStaffAlloc = value;
            }
        }
        private ObservableCollection<ConsultationRoomStaffAllocations> _curAllConsulRoomStaffAlloc;
        public ObservableCollection<ConsultationRoomStaffAllocations> curAllConsulRoomStaffAlloc
        {
            get
            {
                return _curAllConsulRoomStaffAlloc;
            }
            set
            {
                if (_curAllConsulRoomStaffAlloc == value)
                    return;
                _curAllConsulRoomStaffAlloc = value;
                NotifyOfPropertyChange(() => curAllConsulRoomStaffAlloc);

            }
        }
        private ObservableCollection<ConsultationRoomStaffAllocations> _allConsulRoomStaffAlloc;
        public ObservableCollection<ConsultationRoomStaffAllocations> allConsulRoomStaffAlloc
        {
            get
            {
                return _allConsulRoomStaffAlloc;
            }
            set
            {
                if (_allConsulRoomStaffAlloc == value)
                    return;
                _allConsulRoomStaffAlloc = value;
                NotifyOfPropertyChange(() => allConsulRoomStaffAlloc);
                GetCurRoomStaffAllocations();
                NotifyOfPropertyChange(() => curAllConsulRoomStaffAlloc);
            }
        }
        private Staff _SelectedStaff;
        public Staff SelectedStaff
        {
            get
            {
                return _SelectedStaff;
            }
            set
            {
                if (_SelectedStaff == value)
                    return;
                _SelectedStaff = value;
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
        private int[] _MonthCollection = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        public int[] MonthCollection
        {
            get
            {
                return _MonthCollection;
            }
            set
            {
                _MonthCollection = value;
                NotifyOfPropertyChange(() => MonthCollection);
            }
        }
        private int _CurrentMonth = DateTime.Now.Month;
        public int CurrentMonth
        {
            get
            {
                return _CurrentMonth;
            }
            set
            {
                if (_CurrentMonth == value)
                {
                    return;
                }
                _CurrentMonth = value;
                NotifyOfPropertyChange(() => CurrentMonth);
            }
        }
        private int[] _YearCollection = new int[] { DateTime.Now.Year - 1, DateTime.Now.Year, DateTime.Now.Year + 1 };
        public int[] YearCollection
        {
            get
            {
                return _YearCollection;
            }
            set
            {
                if (_YearCollection == value)
                {
                    return;
                }
                _YearCollection = value;
                NotifyOfPropertyChange(() => YearCollection);
            }
        }
        private int _CurrentYear = DateTime.Now.Year;
        public int CurrentYear
        {
            get
            {
                return _CurrentYear;
            }
            set
            {
                if (_CurrentYear == value)
                {
                    return;
                }
                _CurrentYear = value;
                NotifyOfPropertyChange(() => CurrentYear);
            }
        }
        private ObservableCollection<RefDepartment> _DepartmentCollection;
        public ObservableCollection<RefDepartment> DepartmentCollection
        {
            get
            {
                return _DepartmentCollection;
            }
            set
            {
                if (_DepartmentCollection == value)
                {
                    return;
                }
                _DepartmentCollection = value;
                NotifyOfPropertyChange(() => DepartmentCollection);
            }
        }
        private RefDepartment _SelectedDepartment;
        public RefDepartment SelectedDepartment
        {
            get
            {
                return _SelectedDepartment;
            }
            set
            {
                if (_SelectedDepartment == value)
                {
                    return;
                }
                _SelectedDepartment = value;
                NotifyOfPropertyChange(() => SelectedDepartment);
            }
        }
        private ObservableCollection<DeptLocation> _LocationCollection;
        public ObservableCollection<DeptLocation> LocationCollection
        {
            get
            {
                return _LocationCollection;
            }
            set
            {
                if (_LocationCollection == value)
                {
                    return;
                }
                _LocationCollection = value;
                NotifyOfPropertyChange(() => LocationCollection);
            }
        }
        private DeptLocation _SelectedLocation;
        public DeptLocation SelectedLocation
        {
            get
            {
                return _SelectedLocation;
            }
            set
            {
                if (_SelectedLocation == value)
                {
                    return;
                }
                _SelectedLocation = value;
                NotifyOfPropertyChange(() => SelectedLocation);
                //LoadAllContentSchedule();
            }
        }
        private ObservableCollection<Staff> _StaffCollection;
        public ObservableCollection<Staff> StaffCollection
        {
            get
            {
                return _StaffCollection;
            }
            set
            {
                if (_StaffCollection == value)
                {
                    return;
                }
                _StaffCollection = value;
                NotifyOfPropertyChange(() => StaffCollection);
            }
        }
        private Staff _FilteredStaff;
        public Staff FilteredStaff
        {
            get
            {
                return _FilteredStaff;
            }
            set
            {
                if (_FilteredStaff == value)
                {
                    return;
                }
                _FilteredStaff = value;
                NotifyOfPropertyChange(() => FilteredStaff);
                ChangedViewAfterFiltered();
            }
        }
        private RefDepartment _FilteredDepartment;
        public RefDepartment FilteredDepartment
        {
            get
            {
                return _FilteredDepartment;
            }
            set
            {
                if (_FilteredDepartment == value)
                {
                    return;
                }
                _FilteredDepartment = value;
                NotifyOfPropertyChange(() => FilteredDepartment);
                ChangedViewAfterFiltered();
            }
        }
        private bool _IsEditViewCase = true;
        public bool IsEditViewCase
        {
            get
            {
                return _IsEditViewCase;
            }
            set
            {
                if (_IsEditViewCase == value)
                {
                    return;
                }
                _IsEditViewCase = value;
                NotifyOfPropertyChange(() => IsEditViewCase);
            }
        }
        private bool _IsSuccessedCalled = false;
        public bool IsSuccessedCalled
        {
            get
            {
                return _IsSuccessedCalled;
            }
            set
            {
                if (_IsSuccessedCalled == value)
                {
                    return;
                }
                _IsSuccessedCalled = value;
                NotifyOfPropertyChange(() => IsSuccessedCalled);
            }
        }
        private long _SelectedTimeSegmentID;
        public long SelectedTimeSegmentID
        {
            get
            {
                return _SelectedTimeSegmentID;
            }
            set
            {
                if (_SelectedTimeSegmentID == value)
                {
                    return;
                }
                _SelectedTimeSegmentID = value;
                NotifyOfPropertyChange(() => SelectedTimeSegmentID);
            }
        }
        private DateTime _SelectedCalendarDate;
        public DateTime SelectedCalendarDate
        {
            get
            {
                return _SelectedCalendarDate;
            }
            set
            {
                if (_SelectedCalendarDate == value)
                {
                    return;
                }
                _SelectedCalendarDate = value;
                NotifyOfPropertyChange(() => SelectedCalendarDate);
            }
        }
        private ObservableCollection<Lookup> _SpecialistTypeCollection;
        public ObservableCollection<Lookup> SpecialistTypeCollection
        {
            get
            {
                return _SpecialistTypeCollection;
            }
            set
            {
                if (_SpecialistTypeCollection == value)
                {
                    return;
                }
                _SpecialistTypeCollection = value;
                NotifyOfPropertyChange(() => SpecialistTypeCollection);
            }
        }
        private long _V_SpecialistType;
        public long V_SpecialistType
        {
            get
            {
                return _V_SpecialistType;
            }
            set
            {
                if (_V_SpecialistType == value)
                {
                    return;
                }
                _V_SpecialistType = value;
                NotifyOfPropertyChange(() => V_SpecialistType);
            }
        }
        private ConsultationRoomStaffAllocationServiceList _ConfirmedServiceList;
        public ConsultationRoomStaffAllocationServiceList ConfirmedServiceList
        {
            get
            {
                return _ConfirmedServiceList;
            }
            set
            {
                if (_ConfirmedServiceList == value)
                {
                    return;
                }
                _ConfirmedServiceList = value;
                NotifyOfPropertyChange(() => ConfirmedServiceList);
                NotifyOfPropertyChange(() => SelectedServiceItemCollection);
                NotifyOfPropertyChange(() => SelectedServiceItemString);
            }
        }
        private CRSAWeek _CurrentCRSAWeek;
        public CRSAWeek CurrentCRSAWeek
        {
            get
            {
                return _CurrentCRSAWeek;
            }
            set
            {
                if (_CurrentCRSAWeek == value)
                {
                    return;
                }
                _CurrentCRSAWeek = value;
                NotifyOfPropertyChange(() => CurrentCRSAWeek);
            }
        }
        private DateTime _SelectedDate = Globals.GetCurServerDateTime();
        public DateTime SelectedDate
        {
            get
            {
                return _SelectedDate;
            }
            set
            {
                if (_SelectedDate == value)
                    return;
                _SelectedDate = value;
                NotifyOfPropertyChange(() => SelectedDate);
            }
        }
        public IList<RefMedicalServiceItem> SelectedServiceItemCollection
        {
            get
            {
                return ConfirmedServiceList == null || ConfirmedServiceList.ServiceCollection == null ? null : ConfirmedServiceList.ServiceCollection.Select(x => x.MedicalService).ToObservableCollection();
            }
        }
        public string SelectedServiceItemString
        {
            get
            {
                return SelectedServiceItemCollection == null || SelectedServiceItemCollection.Count == 0 ? "" : string.Join(",", SelectedServiceItemCollection.Select(x => x.MedServiceName));
            }
        }
        private void InitCalendar()
        {
            List<CalendarWeek> CurrentCalendarWeekCollection = new List<CalendarWeek> { new CalendarWeek() };
            CurrentCalendarWeekCollection.Add(new CalendarWeek());
            //int CurrentWeek = 0;
            //int CurrentDay = 0;
            //int DaysOfMonth = DateTime.DaysInMonth(CurrentYear, CurrentMonth);
            //for (int aDay = 1; aDay <= DaysOfMonth; aDay++)
            //{
            //    DateTime CurrentDate = new DateTime(CurrentYear, CurrentMonth, aDay);
            //    if (CurrentWeek == 0 && aDay == 1)
            //    {
            //        switch (CurrentDate.DayOfWeek)
            //        {
            //            case DayOfWeek.Tuesday:
            //                CurrentDay = 1;
            //                break;
            //            case DayOfWeek.Wednesday:
            //                CurrentDay = 2;
            //                break;
            //            case DayOfWeek.Thursday:
            //                CurrentDay = 3;
            //                break;
            //            case DayOfWeek.Friday:
            //                CurrentDay = 4;
            //                break;
            //            case DayOfWeek.Saturday:
            //                CurrentDay = 5;
            //                break;
            //            case DayOfWeek.Sunday:
            //                CurrentDay = 6;
            //                break;
            //        }
            //    }
            //    CurrentCalendarWeekCollection[CurrentWeek].CalendarDayCollection[CurrentDay].CalendarDate = new DateTime(CurrentYear, CurrentMonth, aDay);
            //    CurrentDay++;
            //    if (CurrentDay == 7)
            //    {
            //        CurrentDay = 0;
            //        CurrentWeek++;
            //        CurrentCalendarWeekCollection.Add(new CalendarWeek());
            //    }
            //}
            DateTime firstDayCurWeek = GetFirstDayOfWeek(CurrentYear, CurrentWeek);
            List<DateTime> ListDateOfCurWeek = Enumerable.Range(0, 7).Select(days => firstDayCurWeek.AddDays(days)).ToList();
            int CurrentDay = 0;
            foreach (var date in ListDateOfCurWeek)
            {
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        DayWeek = date.Date.ToString("dd/MM/yyyy");
                        //CurrentDay = 1;
                        break;
                    case DayOfWeek.Tuesday:
                        CurrentDay = 1;
                        break;
                    case DayOfWeek.Wednesday:
                        CurrentDay = 2;
                        break;
                    case DayOfWeek.Thursday:
                        CurrentDay = 3;
                        break;
                    case DayOfWeek.Friday:
                        CurrentDay = 4;
                        break;
                    case DayOfWeek.Saturday:
                        CurrentDay = 5;
                        break;
                    case DayOfWeek.Sunday:
                        DayWeek += " - " + date.Date.ToString("dd/MM/yyyy");
                        CurrentDay = 6;
                        break;
                }
                CurrentCalendarWeekCollection[0].CalendarDayCollection[CurrentDay].CalendarDate = date.Date;
            }
            CalendarWeekCollection = CurrentCalendarWeekCollection.ToObservableCollection();
            LoadWeekSchedule();
            //LoadAllContentSchedule();
        }
        private void GetAllConsultationTimeSegments(GenericCoRoutineTask genTask)
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
                                SelectedConsultationTimeSegments = lstConsultationTimeSegments.FirstOrDefault();
                            }
                        }
                        catch (Exception ex)
                        {
                            Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                        }
                        finally
                        {
                            this.HideBusyIndicator();
                            genTask.ActionComplete(true);
                        }
                    }), null);
                }
            });
            t.Start();
        }
        private void GetRefStaffCategories()
        {
            allRefStaffCategory = new ObservableCollection<RefStaffCategory>();
            GetRefStaffCategoriesByType((long)V_StaffCatType.BacSi);
            GetRefStaffCategoriesByType((long)V_StaffCatType.PhuTa);
        }
        private void GetRefStaffCategoriesByType(long V_StaffCatType)
        {
            var t = new Thread(() =>
            {
                this.ShowBusyIndicator();
                using (var serviceFactory = new ResourcesManagementServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginGetRefStaffCategoriesByType(V_StaffCatType, Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            var results = contract.EndGetRefStaffCategoriesByType(asyncResult);
                            if (results != null && results.Count > 0)
                            {
                                foreach (var p in results)
                                {
                                    allRefStaffCategory.Add(p);
                                }
                                SelectedRefStaffCategory = allRefStaffCategory.FirstOrDefault();
                            }
                            else
                            {
                                allRefStaffCategory.Clear();
                                SelectedRefStaffCategory = null;
                            }
                            NotifyOfPropertyChange(() => allRefStaffCategory);
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

        private void GetAllStaff(long StaffCatgID)
        {
            allStaff = Globals.AllStaffs.Where(x => !x.IsStopUsing && x.StaffCatgID == (long)StaffCatg.Bs).ToObservableCollection();
        }

        private void GetAllStaffCollection()
        {
            StaffCollection = Globals.AllStaffs.Where(x => !x.IsStopUsing && x.RefStaffCategory != null && x.StaffCatgID == (long)StaffCatg.Bs).ToObservableCollection();
        }

        private ObservableCollection<ConsultationRoomStaffAllocations> GetRoomStaffAlloByType(long StaffCatType)
        {
            ObservableCollection<ConsultationRoomStaffAllocations> tempConsRoom = new ObservableCollection<ConsultationRoomStaffAllocations>();
            if (allConsulRoomStaffAlloc != null)
            {
                foreach (var RoomStaffAllocation in allConsulRoomStaffAlloc)
                {
                    if (RoomStaffAllocation.Staff.RefStaffCategory.V_StaffCatType == StaffCatType)
                    {
                        tempConsRoom.Add(RoomStaffAllocation);
                    }
                }
            }
            return tempConsRoom;
        }

        private void GetCurRoomStaffAllocations()
        {
            curAllConsulRoomStaffAlloc = new ObservableCollection<ConsultationRoomStaffAllocations>();
            foreach (var cts in lstConsultationTimeSegments)
            {
                if (cts.ConsultationTimeSegmentID < 1)
                    continue;
                bool flag = false;
                if (allConsulRoomStaffAlloc == null ||
                    allConsulRoomStaffAlloc.Count < 1)
                {
                    return;
                }
                foreach (var crt in allConsulRoomStaffAlloc)
                {
                    if (crt.ConsultationTimeSegmentID == cts.ConsultationTimeSegmentID
                        && crt.AllocationDate.Date <= CurDateTime.Date
                        && crt.Staff.RefStaffCategory.V_StaffCatType == StaffCatType)
                    {
                        crt.Status = eHCMSResources.G2355_G1_X.ToUpper();
                        flag = true;
                        curAllConsulRoomStaffAlloc.Add(ObjectCopier.DeepCopy(crt));
                        break;
                    }
                }
                if (!flag)
                {
                    ConsultationRoomStaffAllocations crt = new ConsultationRoomStaffAllocations();
                    crt.ConsultationTimeSegments = new ConsultationTimeSegments();
                    crt.ConsultationTimeSegments.SegmentName = cts.SegmentName;
                    curAllConsulRoomStaffAlloc.Add(ObjectCopier.DeepCopy(crt));
                }
            }
            NotifyOfPropertyChange(() => curAllConsulRoomStaffAlloc);
        }

        private IEnumerator<IResult> LoadDepartments()
        {
            this.ShowBusyIndicator();
            var GetDepartmentTask = new LoadDepartmentsByV_DeptTypeOperationTask(true);
            yield return GetDepartmentTask;
            if (GetDepartmentTask.Departments != null
                && GetDepartmentTask.Departments.Count > 0)
            {
                DepartmentCollection = GetDepartmentTask.Departments.Where(x => !x.DeptName.Equals("KHOA KHÁM BỆNH THEO YÊU CẦU VIP (Lầu 2)")).ToObservableCollection();
                if (DepartmentCollection.Count > 0 && DepartmentCollection.Any(x => x.V_DeptTypeOperation == (long)V_DeptTypeOperation.KhoaNgoaiTru))
                {
                    SelectedDepartment = DepartmentCollection.First(x => x.V_DeptTypeOperation == (long)V_DeptTypeOperation.KhoaNgoaiTru);
                }
                else
                {
                    SelectedDepartment = DepartmentCollection.FirstOrDefault();
                }
                if (SelectedDepartment != null && SelectedDepartment.DeptID > 0)
                {
                    LoadLocations(SelectedDepartment.DeptID);
                }
                NotifyOfPropertyChange(() => DepartmentCollection);
            }
            else
            {
                DepartmentCollection = new ObservableCollection<RefDepartment>();
                SelectedDepartment = null;
            }
            this.HideBusyIndicator();
            yield break;
        }
        private void LoadLocations(long aDeptID)
        {
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                using (var serviceFactory = new ConfigurationManagerServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginGetAllLocationsByDeptIDOld(aDeptID, Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            var allItems = contract.EndGetAllLocationsByDeptIDOld(asyncResult);
                            ObservableCollection<DeptLocation> ItemCollection = new ObservableCollection<DeptLocation>();
                            if (allItems != null)
                            {
                                ItemCollection = allItems.ToObservableCollection();
                            }
                            ItemCollection.Insert(0, new DeptLocation { DeptLocationID = 0, Location = new Location { LocationName = string.Empty } });
                            if (SelectedLocation != null)
                            {
                                var BackupLocation = SelectedLocation.DeepCopy();
                                LocationCollection = ItemCollection;
                                //SelectedLocation = BackupLocation;
                                SelectedLocation = LocationCollection.FirstOrDefault();
                            }
                            else
                            {
                                LocationCollection = ItemCollection;
                                SelectedLocation = LocationCollection.FirstOrDefault();
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
        private string ConvertCalendarToXml(IList<CalendarWeek> aCalendarWeekCollection)
        {
            if (!aCalendarWeekCollection.Any(x => x.IsHasValue && x.CalendarDayCollection.Any(c => c.IsHasValue)))
            {
                return null;
            }
            List<CalendarDay> AllCalendarDay = new List<CalendarDay>();
            foreach (var aDay in aCalendarWeekCollection.Where(x => x.IsHasValue && x.CalendarDayCollection.Any(c => c.IsHasValue)).SelectMany(x => x.CalendarDayCollection).Where(x => x.IsHasValue).ToList())
            {
                if (aDay.StaffWorkingSchedule == null || aDay.StaffWorkingSchedule.Count == 0)
                {
                    continue;
                }
                AllCalendarDay.Add(aDay);
            }
            XDocument mDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("Calendar", AllCalendarDay.Select(aItem => new XElement("item", new XElement[] {
                    new XElement("CalendarDate", aItem.CalendarDate == null ? "" : aItem.CalendarDate.Value.ToString("yyyy-MM-dd")),
                    new XElement("WorkingSchedule", aItem.StaffWorkingSchedule.Select(aSchedule => new XElement("item", new XElement[] {
                        new XElement("SegmentID", aSchedule.ConsultationSegment.ConsultationTimeSegmentID),
                        new XElement("DeptID", aSchedule.CurrentDepartment == null ? (long?)null : aSchedule.CurrentDepartment.DeptID),
                        new XElement("DeptLocationID", aSchedule.CurrentDeptLocation == null ? (long?)null : aSchedule.CurrentDeptLocation.DeptLocationID),
                        new XElement("StaffID", aSchedule.DoctorStaff.StaffID),
                        new XElement("ConsultationRoomStaffAllocationServiceListID", aSchedule.ConsultationRoomStaffAllocationServiceListID),
                        new XElement("CRSAWeekID", aSchedule.CRSAWeekID),
                        new XElement("CRSANote", aSchedule.CRSANote),
                        new XElement("MedServiceItems", aSchedule.ServiceItemCollection == null ? null : aSchedule.ServiceItemCollection.Select(x => new XElement("Item", new XElement[] { new XElement("MedServiceID", x.MedServiceID) })).ToArray()),
                    })))
                })).ToArray()));
            return mDocument.ToString();
        }
        private void LoadWeekSchedule()
        {
            if (CurrentWeek == 0)
            {
                return;
            }
            if (CalendarWeekCollection == null
                 || !CalendarWeekCollection.Any(x => x.CalendarDayCollection != null && x.CalendarDayCollection.Any(c => c.CalendarDate.HasValue && c.CalendarDate != null)))
            {
                return;
            }
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                using (var serviceFactory = new ClinicManagementServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginGetCRSAWeek(CurrentWeek, CalendarWeekCollection.SelectMany(x => x.CalendarDayCollection).Where(c => c.CalendarDate.HasValue && c.CalendarDate != null).Min(c => c.CalendarDate.Value)
                        , CalendarWeekCollection.SelectMany(x => x.CalendarDayCollection).Where(c => c.CalendarDate.HasValue && c.CalendarDate != null).Max(c => c.CalendarDate.Value), Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                CRSAWeek week = contract.EndGetCRSAWeek(asyncResult);
                                CurrentCRSAWeek = week;
                                if (week != null)
                                {
                                    SetEnableForWeek(week);
                                    CalendarEnable = CurrentCRSAWeek.V_CRSAWeekStatus != (long)AllLookupValues.V_CRSAWeekStatus.Khoa_Lich;
                                    CRSAWeekID = week.CRSAWeekID;
                                    LoadAllContentSchedule();
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
        private void SetEnableForWeek(CRSAWeek week)
        {
            switch (week.V_CRSAWeekStatus)
            {
                case (long)AllLookupValues.V_CRSAWeekStatus.Moi:
                    SetEnableButton(true, false, false, false, false, false);
                    break;
                case (long)AllLookupValues.V_CRSAWeekStatus.Da_Luu:
                    SetEnableButton(true, true, false, false, false, false);
                    break;
                case (long)AllLookupValues.V_CRSAWeekStatus.Gui_KHTH:
                    SetEnableButton(true, false, true, false, false, false);
                    break;
                case (long)AllLookupValues.V_CRSAWeekStatus.Gui_TC:
                    SetEnableButton(false, false, false, true, true, false);
                    break;
                case (long)AllLookupValues.V_CRSAWeekStatus.Tra_KHTH:
                    SetEnableButton(true, false, true, false, false, false);
                    break;
                case (long)AllLookupValues.V_CRSAWeekStatus.Khoa_Lich:
                    SetEnableButton(false, false, false, false, false, true);
                    break;
                case (long)AllLookupValues.V_CRSAWeekStatus.Mo_Khoa_Lich:
                    SetEnableButton(true, true, false, false, false, false);
                    break;
                default:
                    SetEnableButton(true, false, false, false, false, false);
                    break;
            }
        }
        private void SetEnableButton(bool SaveEnable, bool SendGeneralPlanRoomEnable, bool SendOrganizeRoomEnable, bool ReturnGeneralPlanRoomEnable
            , bool ConfirmEnable, bool UnlockEnable)
        {
            btnSaveEnable = SaveEnable && (bWork_Schedule_KhoaPhong || bWork_Schedule_KHTH);
            //NotifyOfPropertyChange(() => btnSaveEnable);
            btnSendGeneralPlanRoomEnable = SendGeneralPlanRoomEnable && bWork_Schedule_KhoaPhong;
            //NotifyOfPropertyChange(() => btnSendGeneralPlanRoomEnable);
            btnSendOrganizeRoomEnable = SendOrganizeRoomEnable && bWork_Schedule_KHTH;
            //NotifyOfPropertyChange(() => btnSendOrganizeRoomEnable);
            btnReturnGeneralPlanRoomEnable = ReturnGeneralPlanRoomEnable && bWork_Schedule_TC;
            //NotifyOfPropertyChange(() => btnReturnGeneralPlanRoomEnable);
            btnConfirmEnable = ConfirmEnable && bWork_Schedule_TC;
            //NotifyOfPropertyChange(() => btnConfirmEnable);
            btnUnlockEnable = UnlockEnable && bWork_Schedule_TC;
            //NotifyOfPropertyChange(() => btnUnlockEnable);

        }
        private void LoadAllContentSchedule()
        {
            if (CalendarWeekCollection == null
                || !CalendarWeekCollection.Any(x => x.CalendarDayCollection != null && x.CalendarDayCollection.Any(c => c.CalendarDate.HasValue && c.CalendarDate != null)))
            {
                return;
            }
            foreach (var aWeek in CalendarWeekCollection)
            {
                if (!aWeek.CalendarDayCollection.Any(x => x.StaffWorkingSchedule != null))
                {
                    continue;
                }
                foreach (var aDay in aWeek.CalendarDayCollection.Where(x => x.StaffWorkingSchedule != null))
                {
                    aDay.StaffWorkingSchedule.Clear();
                }
            }
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                using (var serviceFactory = new ClinicManagementServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginGetStaffConsultationTimeSegmentByDate(0, CalendarWeekCollection.SelectMany(x => x.CalendarDayCollection).Where(c => c.CalendarDate.HasValue && c.CalendarDate != null).Min(c => c.CalendarDate.Value)
                        , CalendarWeekCollection.SelectMany(x => x.CalendarDayCollection).Where(c => c.CalendarDate.HasValue && c.CalendarDate != null).Max(c => c.CalendarDate.Value), Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            var results = contract.EndGetStaffConsultationTimeSegmentByDate(asyncResult);
                            if (results != null && results.Count > 0)
                            {
                                foreach (var aWeek in CalendarWeekCollection)
                                {
                                    foreach (var aDay in aWeek.CalendarDayCollection)
                                    {
                                        if (results.Any(x => x.AllocationDate == aDay.CalendarDate))
                                        {
                                            aDay.StaffWorkingSchedule = results.Where(x => x.AllocationDate == aDay.CalendarDate)
                                                .Select(x => new StaffWorkingSchedule {
                                                    ConsultationRoomStaffAllocationServiceListID = x.ConsultationRoomStaffAllocationServiceListID
                                                    , ServiceItemCollection = x.ServiceItemCollection
                                                    , CurrentDepartment = x.DeptLocation.RefDepartment
                                                    , CurrentDeptLocation = x.DeptLocation != null && x.DeptLocation.DeptLocationID == 0 ? null : x.DeptLocation
                                                    , ConsultationSegment = new ConsultationTimeSegments {
                                                        ConsultationTimeSegmentID = x.ConsultationTimeSegmentID,
                                                        SegmentName = x.ConsultationTimeSegments.SegmentName,
                                                        StartTime = x.ConsultationTimeSegments.StartTime,
                                                        EndTime = x.ConsultationTimeSegments.EndTime,
                                                    }
                                                    , DoctorStaff = x.Staff
                                                    , CRSAWeekID = x.CRSAWeekID
                                                    , CRSANote = x.CRSANote
                                                    , V_TimeSegmentType = x.V_TimeSegmentType
                                                }).ToObservableCollection();
                                            if (aDay.StaffWorkingScheduleView.Filter == null)
                                            {
                                                aDay.StaffWorkingScheduleView.Filter = CustomerFilter;
                                                //aDay.StaffWorkingScheduleView.Filter = DepartmentFilter;
                                            }
                                        }
                                    }
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
        private bool CustomerFilter(object aSchedule)
        {
            if ((FilteredStaff == null && FilteredDepartment == null) || !(aSchedule is StaffWorkingSchedule))
            {
                return true;
            }
            StaffWorkingSchedule mSchedule = aSchedule as StaffWorkingSchedule;
            if(FilteredStaff == null)
            {
                return mSchedule.CurrentDepartment != null && mSchedule.CurrentDepartment.DeptID == FilteredDepartment.DeptID;
            }
            if (FilteredDepartment == null)
            {
                return mSchedule.DoctorStaff != null && mSchedule.DoctorStaff.StaffID == FilteredStaff.StaffID;
            }
            return mSchedule.DoctorStaff != null && mSchedule.DoctorStaff.StaffID == FilteredStaff.StaffID 
                && mSchedule.CurrentDepartment != null && mSchedule.CurrentDepartment.DeptID == FilteredDepartment.DeptID;
        }
        private bool DepartmentFilter(object aSchedule)
        {
            if (FilteredDepartment == null || !(aSchedule is StaffWorkingSchedule))
            {
                return true;
            }
            StaffWorkingSchedule mSchedule = aSchedule as StaffWorkingSchedule;
            return mSchedule.CurrentDepartment != null && mSchedule.CurrentDepartment.DeptID == FilteredDepartment.DeptID;
        }
        private void ChangedViewAfterFiltered()
        {
            if (CalendarWeekCollection == null)
            {
                return;
            }
            if (!CalendarWeekCollection.Any(x => x.CalendarDayCollection != null && x.CalendarDayCollection.Length > 0))
            {
                return;
            }
            foreach (var cWeek in CalendarWeekCollection.Where(x => x.CalendarDayCollection != null && x.CalendarDayCollection.Length > 0))
            {
                foreach (var cDay in cWeek.CalendarDayCollection)
                {
                    if (cDay.StaffWorkingSchedule == null || cDay.StaffWorkingSchedule.Count == 0)
                    {
                        continue;
                    }
                    cDay.StaffWorkingScheduleView.Refresh();
                }
            }
        }
        public void ScheduleStaffs_DoubleClick(DateTime aCalendarDate, long aConsultationTimeSegmentID)
        {
            IsSuccessedCalled = true;
            SelectedCalendarDate = aCalendarDate;
            SelectedTimeSegmentID = aConsultationTimeSegmentID;
            TryClose();
        }
        #region Events
        public ConsultationRoomStaff_V3ViewModel()
        {
            authorization();
            SpecialistTypeCollection = Globals.AllLookupValueList.Where(x => x.ObjectTypeID == (long)LookupValues.V_SpecialistType).ToObservableCollection();
            V_SpecialistType = SpecialistTypeCollection.First().LookupID;
            Coroutine.BeginExecute(GetData_Routine(), null, (o, e) =>
            {
                radBacSi_Click(null, null);
            });
            CurrentWeek = GetCurrentWeek(Globals.GetCurServerDateTime());
            InitCalendar();
            Coroutine.BeginExecute(LoadDepartments());
            GetAllStaffCollection();
        }
        
        private IEnumerator<IResult> GetData_Routine()
        {
            yield return GenericCoRoutineTask.StartTask(GetAllConsultationTimeSegments);
        }
        public void radBacSi_Click(object sender, RoutedEventArgs e)
        {
            StaffCatType = (long)V_StaffCatType.BacSi;
            allRefStaffCategory = new ObservableCollection<RefStaffCategory>();
            allRefStaffCategory.Clear();
            GetRefStaffCategoriesByType(StaffCatType);
            allStaff = new ObservableCollection<Staff>();
            tempConsRoomStaffAlloc = GetRoomStaffAlloByType(StaffCatType);
            NotifyOfPropertyChange(() => tempConsRoomStaffAlloc);
            GetCurRoomStaffAllocations();
            NotifyOfPropertyChange(() => curAllConsulRoomStaffAlloc);
        }
        public void radTroLy_Click(object sender, RoutedEventArgs e)
        {
            StaffCatType = (long)V_StaffCatType.PhuTa;
            allRefStaffCategory = new ObservableCollection<RefStaffCategory>();
            allRefStaffCategory.Clear();
            GetRefStaffCategoriesByType(StaffCatType);
            allStaff = new ObservableCollection<Staff>();
            tempConsRoomStaffAlloc = GetRoomStaffAlloByType(StaffCatType);
            NotifyOfPropertyChange(() => tempConsRoomStaffAlloc);
            GetCurRoomStaffAllocations();
            NotifyOfPropertyChange(() => curAllConsulRoomStaffAlloc);
        }
        public void grdListStaffDoubleClick(object sender)
        {
            //if (SelectedServiceItemCollection == null || SelectedServiceItemCollection.Count == 0 || ConfirmedServiceList == null)
            //{
            //    Globals.ShowMessage(eHCMSResources.Z0162_G1_HayChonDV, eHCMSResources.G0442_G1_TBao);
            //    return;
            //}
            if (SelectedConsultationTimeSegments == null
                || SelectedConsultationTimeSegments.ConsultationTimeSegmentID == 0)
            {
                return;        
            }
            if (!CalendarWeekCollection.Any(x => x.CalendarDayCollection.Any(c => c.IsSelected)))
            {
                return;
            }
            if(SelectedDepartment != null 
                && SelectedDepartment.DeptID == Globals.ServerConfigSection.Hospitals.KhoaPhongKham 
                && (SelectedLocation == null || SelectedLocation.DeptLocationID == 0))
            {
                MessageBox.Show("Khoa khám bệnh bắt buộc phải chọn phòng khám");
                return;
            }
            string MessageErrorSum = "";
            foreach (var cWeek in CalendarWeekCollection.Where(x => x.CalendarDayCollection.Any(c => c.IsSelected)))
            {
                foreach (var cDay in cWeek.CalendarDayCollection.Where(c => c.IsSelected))
                {
                    DateTime StartTime = cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.StartTime.Hour).AddMinutes(SelectedConsultationTimeSegments.StartTime.Minute);
                    DateTime EndTime = cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.EndTime.Hour).AddMinutes(SelectedConsultationTimeSegments.EndTime.Minute);
                    DateTime? StartTime2 = SelectedConsultationTimeSegments.StartTime2 == null ? SelectedConsultationTimeSegments.StartTime2 :
                                cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.StartTime2.Value.Hour).AddMinutes(SelectedConsultationTimeSegments.StartTime2.Value.Minute);
                    DateTime? EndTime2 = SelectedConsultationTimeSegments.EndTime2 == null ? SelectedConsultationTimeSegments.EndTime2 :
                                cDay.CalendarDate.Value.Date.AddHours(SelectedConsultationTimeSegments.EndTime2.Value.Hour).AddMinutes(SelectedConsultationTimeSegments.EndTime2.Value.Minute);
                    if (cDay.StaffWorkingSchedule == null)
                    {
                        cDay.StaffWorkingSchedule = new ObservableCollection<StaffWorkingSchedule>();
                        cDay.StaffWorkingScheduleView.Filter = CustomerFilter;
                        //cDay.StaffWorkingScheduleView.Filter = DepartmentFilter;
                    }
                    if (!CheckValid(cDay, StartTime, EndTime, StartTime2, EndTime2, out string MessageError))
                    {
                        MessageErrorSum += MessageError + Environment.NewLine;
                        continue;
                    }
                    cDay.StaffWorkingSchedule.Add(new StaffWorkingSchedule { //ConsultationRoomStaffAllocationServiceListID = ConfirmedServiceList.ConsultationRoomStaffAllocationServiceListID,
                        CurrentDeptLocation = SelectedLocation != null && SelectedLocation.LID == 0 ? null : SelectedLocation.DeepCopy(),
                        CurrentDepartment = SelectedDepartment != null && SelectedDepartment.DeptID == 0 ? null : SelectedDepartment.DeepCopy(),
                        ConsultationSegment = new ConsultationTimeSegments {
                            ConsultationTimeSegmentID = SelectedConsultationTimeSegments.ConsultationTimeSegmentID,
                            SegmentName = SelectedConsultationTimeSegments.SegmentName,
                            StartTime = StartTime,
                            EndTime = EndTime,
                            StartTime2 = StartTime2,
                            EndTime2 = EndTime2,
                        },
                        DoctorStaff = SelectedStaff,
                        CRSAWeekID = CRSAWeekID,
                        V_TimeSegmentType = SelectedConsultationTimeSegments.V_TimeSegmentType
                    });
                    cDay.StaffWorkingScheduleView.Refresh();
                }
            }
            if (!string.IsNullOrWhiteSpace(MessageErrorSum))
            {
                MessageBox.Show(MessageErrorSum);
            }
          
        }
        private bool CheckValid(CalendarDay cDay, DateTime StartTime, DateTime EndTime, DateTime? StartTime2, DateTime? EndTime2, out string MessageError)
        {
            MessageError = "";
            // Cùng bác sĩ, 2 loại khung giờ
            if (cDay.StaffWorkingSchedule.Any(x => x.ConsultationSegment != null
                && x.DoctorStaff != null
                && x.DoctorStaff.StaffID == SelectedStaff.StaffID
                && x.V_TimeSegmentType != SelectedConsultationTimeSegments.V_TimeSegmentType))
            {
                StaffWorkingSchedule addSchedule = cDay.StaffWorkingSchedule.Where(x => x.ConsultationSegment != null
                       && x.DoctorStaff != null
                       && x.DoctorStaff.StaffID == SelectedStaff.StaffID
                       && x.V_TimeSegmentType != SelectedConsultationTimeSegments.V_TimeSegmentType).FirstOrDefault();
                MessageError = "Ngày " + cDay.CalendarDate.Value.Date.ToString("dd/MM/yyyy") + " " + SelectedStaff.PrintTitle + " "
                    + SelectedStaff.FullName + " đang có lịch " + addSchedule.ConsultationSegment.SegmentName + " tại " 
                    + addSchedule.CurrentDeptLocation.RefDepartment.DeptName + " "
                    + (addSchedule.CurrentDeptLocation == null ? "" : addSchedule.CurrentDeptLocation.Location.LocationName);
                return false;
            }

            // Cùng giờ, cùng bác sĩ
            if (cDay.StaffWorkingSchedule.Any(x => x.ConsultationSegment != null 
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
                StaffWorkingSchedule addSchedule = cDay.StaffWorkingSchedule.Where(x => x.ConsultationSegment != null
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
            //            + addDoctor.FullName + "; "+ SelectedStaff.PrintTitle + " " + SelectedStaff.FullName + " đang trùng lịch làm" ;
            //        return false;
            //    }
            //}
            //else
            if (SelectedLocation != null && SelectedLocation.LID > 0)
            {
                if (cDay.StaffWorkingSchedule.Any(x => x.ConsultationSegment != null
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
                    Staff addDoctor = cDay.StaffWorkingSchedule.Where(x => x.ConsultationSegment != null
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
        public void CalendarItem_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditViewCase)
            {
                return;
            }
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                foreach (var item in CalendarWeekCollection.Where(x => x.CalendarDayCollection.Any(c => c.IsSelected)))
                {
                    foreach (var child in item.CalendarDayCollection.Where(c => c.IsSelected))
                    {
                        child.IsSelected = false;
                    }
                }
            }
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {

            }

                ((sender as Grid).DataContext as CalendarDay).IsSelected = !((sender as Grid).DataContext as CalendarDay).IsSelected;
        }
        public void cboMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitCalendar();
        }
        public void cboDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedDepartment == null || SelectedDepartment.DeptID == 0)
            {
                LocationCollection = new ObservableCollection<DeptLocation>();
                return;
            }
            LoadLocations(SelectedDepartment.DeptID);
        }
        private string CheckAllDoctorHaveTimeSegent()
        {
            string ListDoctorWeek = "";
            foreach (var cWeek in CalendarWeekCollection.Where(x => x.IsHasValue))
            {
                foreach (var cDay in cWeek.CalendarDayCollection)
                {
                    string ListDoctorDay = "";
                    foreach (var item in allStaff)
                    {
                        if(cDay.StaffWorkingSchedule == null || !cDay.StaffWorkingSchedule.Any(x => x.ConsultationSegment != null
                           && x.DoctorStaff != null
                           && x.DoctorStaff.StaffID == item.StaffID))
                        {
                            ListDoctorDay += item.PrintTitle + " " + item.FullName + "; ";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(ListDoctorDay))
                    {
                        ListDoctorWeek += "Ngày " + cDay.CalendarDate.Value.Date.ToString("dd/MM/yyyy") + ": " 
                            + ListDoctorDay.Substring(0, ListDoctorDay.Length - 2) + " chưa có lịch làm việc/lịch nghỉ"+ Environment.NewLine;
                    }
                }
            }
            
            return ListDoctorWeek;
        }
        public void btnSave()
        {
            
            var XmlString = ConvertCalendarToXml(CalendarWeekCollection);
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                using (var serviceFactory = new ClinicManagementServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginEditStaffConsultationTimeSegments(XmlString, 0, Globals.LoggedUserAccount.Staff.StaffID, Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            var result = contract.EndEditStaffConsultationTimeSegments(asyncResult);
                            NotifyOfPropertyChange(() => allRefStaffCategory);
                            if (result)
                            {
                                CurrentCRSAWeek.V_CRSAWeekStatus = (long)AllLookupValues.V_CRSAWeekStatus.Da_Luu;
                                SetEnableForWeek(CurrentCRSAWeek);
                                MessageBox.Show("Lưu thành công");
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
        public void RemoveItemCmd(object sender, RoutedEventArgs e)
        {
            var DataGrid = (sender as Button).FindParentOfType<DataGrid>();
            if (DataGrid != null && DataGrid.DataContext is CalendarDay)
            {
                if ((sender as Button).DataContext == null && !((sender as Button).DataContext is StaffWorkingSchedule))
                {
                    return;
                }
                (DataGrid.DataContext as CalendarDay).StaffWorkingSchedule.Remove((sender as Button).DataContext as StaffWorkingSchedule);
            }
        }
        public void EditServicesOfItemCmd(object sender, RoutedEventArgs e)
        {
            var DataGrid = (sender as Button).FindParentOfType<DataGrid>();
            if (DataGrid != null && DataGrid.DataContext is CalendarDay)
            {
                if ((sender as Button).DataContext == null && !((sender as Button).DataContext is StaffWorkingSchedule))
                {
                    return;
                }
                IConsultationRoomStaff_V3_Edit DialogView = Globals.GetViewModel<IConsultationRoomStaff_V3_Edit>();
                DialogView.SelectedStaffWorkingSchedule = ((sender as Button).DataContext as StaffWorkingSchedule);
                DialogView.cDay = (DataGrid.DataContext as CalendarDay);
                DialogView.RemoveSelectedStaffWorkingSchedule();
                GlobalsNAV.ShowDialog_V3(DialogView);
                if (DialogView.IsConfirmed)
                {
                    (DataGrid.DataContext as CalendarDay).StaffWorkingSchedule.Remove((sender as Button).DataContext as StaffWorkingSchedule);
                    (DataGrid.DataContext as CalendarDay).StaffWorkingSchedule.Add(DialogView.NewStaffWorkingSchedule);
                }
            }
        }
        public void btnClearFilter()
        {
            FilteredStaff = null;
            FilteredDepartment = null;
        }
        public void gvScheduleStaffs_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsEditViewCase)
            {
                return;
            }
            if (!(sender is DataGrid))
            {
                return;
            }
            DataGrid CurrentDataGrid = sender as DataGrid;
            if (!(CurrentDataGrid.DataContext is CalendarDay))
            {
                return;
            }
            var CalendarDate = (CurrentDataGrid.DataContext as CalendarDay).CalendarDate;
            if (!CalendarDate.HasValue || CalendarDate.Value == null)
            {
                return;
            }
            if (CurrentDataGrid.SelectedItem == null || !(CurrentDataGrid.SelectedItem is StaffWorkingSchedule))
            {
                return;
            }
            if ((CurrentDataGrid.SelectedItem as StaffWorkingSchedule).ConsultationSegment == null)
            {
                return;
            }
            var ConsultationTimeSegmentID = (CurrentDataGrid.SelectedItem as StaffWorkingSchedule).ConsultationSegment.ConsultationTimeSegmentID;
            ScheduleStaffs_DoubleClick(CalendarDate.Value, ConsultationTimeSegmentID);
        }
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if (SelectedCalendarDate != null && SelectedCalendarDate > new DateTime(2010, 1, 1))
            {
                CurrentYear = SelectedCalendarDate.Year;
                CurrentMonth = SelectedCalendarDate.Month;
                cboMonth_SelectionChanged(null, null);
            }
        }
        private string _FilterDocterString;
        public string FilterDocterString
        {
            get
            {
                return _FilterDocterString;
            }
            set
            {
                if (_FilterDocterString == value)
                {
                    return;
                }
                _FilterDocterString = value;
                NotifyOfPropertyChange(() => FilterDocterString);
                if (StaffViewCollection == null)
                {
                    return;
                }
                StaffViewCollection.Refresh();
            }
        }
        public void txtDocterFiler_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        public void SelectServicesCmd()
        {
            IUCServiceListsSelection DialogView = Globals.GetViewModel<IUCServiceListsSelection>();
            GlobalsNAV.ShowDialog_V3(DialogView);
            if (!DialogView.IsConfirmed)
            {
                return;
            }
            ConfirmedServiceList = DialogView.ConfirmedServiceList;
        }
        #endregion
        private DateTime _FromDate;
        public DateTime FromDate
        {
            get
            {
                return _FromDate;
            }
            set
            {
                if (_FromDate == value)
                {
                    return;
                }
                _FromDate = value;
                NotifyOfPropertyChange(() => FromDate);
            }
        }
        private DateTime _ToDate;
        public DateTime ToDate
        {
            get
            {
                return _ToDate;
            }
            set
            {
                if (_ToDate == value)
                {
                    return;
                }
                _ToDate = value;
                NotifyOfPropertyChange(() => ToDate);
            }
        }
        public void btnSelectDay()
        {
            foreach (var item in CalendarWeekCollection)
            {
                foreach (var child in item.CalendarDayCollection.Where(c => c.CalendarDate<=ToDate && c.CalendarDate>=FromDate))
                {
                    child.IsSelected = true;
                }
            }
        }
        private int[] _WeekCollection = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
            26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 };
        public int[] WeekCollection
        {
            get
            {
                return _WeekCollection;
            }
            set
            {
                _WeekCollection = value;
                NotifyOfPropertyChange(() => WeekCollection);
            }
        }
        private int _CurrentWeek;
        public int CurrentWeek
        {
            get
            {
                return _CurrentWeek;
            }
            set
            {
                if (_CurrentWeek == value)
                {
                    return;
                }
                _CurrentWeek = value;
                NotifyOfPropertyChange(() => CurrentWeek);
            }
        }
        private string _DayWeek;
        public string DayWeek
        {
            get
            {
                return _DayWeek;
            }
            set
            {
                if (_DayWeek == value)
                    return;
                _DayWeek = value;
                NotifyOfPropertyChange(() => DayWeek);
            }
        }
        private bool _bWork_Schedule_KhoaPhong = true;
        public bool bWork_Schedule_KhoaPhong
        {
            get
            {
                return _bWork_Schedule_KhoaPhong;
            }
            set
            {
                if (_bWork_Schedule_KhoaPhong == value)
                    return;
                _bWork_Schedule_KhoaPhong = value;
            }
        }
        private bool _bWork_Schedule_KHTH = true;
        public bool bWork_Schedule_KHTH
        {
            get
            {
                return _bWork_Schedule_KHTH;
            }
            set
            {
                if (_bWork_Schedule_KHTH == value)
                    return;
                _bWork_Schedule_KHTH = value;
            }
        }
        private bool _bWork_Schedule_TC = true;
        public bool bWork_Schedule_TC
        {
            get
            {
                return _bWork_Schedule_TC;
            }
            set
            {
                if (_bWork_Schedule_TC == value)
                    return;
                _bWork_Schedule_TC = value;
            }
        }
        private bool _btnSaveEnable = true;
        public bool btnSaveEnable
        {
            get
            {
                return _btnSaveEnable;
            }
            set
            {
                if (_btnSaveEnable == value)
                    return;
                _btnSaveEnable = value;
                NotifyOfPropertyChange(() => btnSaveEnable);
            }
        }
        private bool _btnSendGeneralPlanRoomEnable = true;
        public bool btnSendGeneralPlanRoomEnable
        {
            get
            {
                return _btnSendGeneralPlanRoomEnable;
            }
            set
            {
                if (_btnSendGeneralPlanRoomEnable == value)
                    return;
                _btnSendGeneralPlanRoomEnable = value;
                NotifyOfPropertyChange(() => btnSendGeneralPlanRoomEnable);
            }
        }
        private bool _btnSendOrganizeRoomEnable = true;
        public bool btnSendOrganizeRoomEnable
        {
            get
            {
                return _btnSendOrganizeRoomEnable;
            }
            set
            {
                if (_btnSendOrganizeRoomEnable == value)
                    return;
                _btnSendOrganizeRoomEnable = value;
                NotifyOfPropertyChange(() => btnSendOrganizeRoomEnable);
            }
        }
        private bool _btnReturnGeneralPlanRoomEnable = true;
        public bool btnReturnGeneralPlanRoomEnable
        {
            get
            {
                return _btnReturnGeneralPlanRoomEnable;
            }
            set
            {
                if (_btnReturnGeneralPlanRoomEnable == value)
                    return;
                _btnReturnGeneralPlanRoomEnable = value;
                NotifyOfPropertyChange(() => btnReturnGeneralPlanRoomEnable);
            }
        }
        private bool _btnConfirmEnable = true;
        public bool btnConfirmEnable
        {
            get
            {
                return _btnConfirmEnable;
            }
            set
            {
                if (_btnConfirmEnable == value)
                    return;
                _btnConfirmEnable = value;
                NotifyOfPropertyChange(() => btnConfirmEnable);
            }
        }
        private bool _btnUnlockEnable = true;
        public bool btnUnlockEnable
        {
            get
            {
                return _btnUnlockEnable;
            }
            set
            {
                if (_btnUnlockEnable == value)
                    return;
                _btnUnlockEnable = value;
                NotifyOfPropertyChange(() => btnUnlockEnable);
            }
        }
        private long _CRSAWeekID;
        public long CRSAWeekID
        {
            get
            {
                return _CRSAWeekID;
            }
            set
            {
                if (_CRSAWeekID == value)
                    return;
                _CRSAWeekID = value;
                NotifyOfPropertyChange(() => CRSAWeekID);
            }
        }
        private bool _CalendarEnable;
        public bool CalendarEnable
        {
            get
            {
                return _CalendarEnable;
            }
            set
            {
                if (_CalendarEnable == value)
                    return;
                _CalendarEnable = value;
                NotifyOfPropertyChange(() => CalendarEnable);
            }
        }
        public void authorization()
        {
            if (!Globals.isAccountCheck)
            {
                return;
            }

            bWork_Schedule_KhoaPhong = Globals.CheckOperation(Globals.listRefModule, (int)eModules.mClinicManagement, (int)eClinicManagement.mLichLamViec
                , (int)oClinicManagementEx.mLichLamViec_KhoaPhong);
            bWork_Schedule_KHTH = Globals.CheckOperation(Globals.listRefModule, (int)eModules.mClinicManagement, (int)eClinicManagement.mLichLamViec
                , (int)oClinicManagementEx.mLichLamViec_KHTH);
            bWork_Schedule_TC = Globals.CheckOperation(Globals.listRefModule, (int)eModules.mClinicManagement, (int)eClinicManagement.mLichLamViec
                , (int)oClinicManagementEx.mLichLamViec_TC);

            btnSaveEnable = bWork_Schedule_KhoaPhong || bWork_Schedule_KHTH;
            btnSendGeneralPlanRoomEnable = bWork_Schedule_KhoaPhong;
            btnSendOrganizeRoomEnable = bWork_Schedule_KHTH;
            btnReturnGeneralPlanRoomEnable = bWork_Schedule_TC;
            btnConfirmEnable = bWork_Schedule_TC;
            btnUnlockEnable = bWork_Schedule_TC;
        }
        public void cboWeek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitCalendar();
        }
        public void cboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime jan1 = new DateTime(CurrentYear, 1, 1);
            if (jan1.DayOfWeek == DayOfWeek.Monday)
            {
                WeekCollection = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53 };
            }
            else
            {
                WeekCollection = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 };
            }
            if (CurrentWeek == 1)
            {
                InitCalendar();
            }
            else
            {
                CurrentWeek = 1;
            }
        }
        private int GetCurrentWeek(DateTime inputDate)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(inputDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
        }
        private DateTime GetFirstDayOfWeek(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Sunday - jan1.DayOfWeek;
            DateTime firstMonday = jan1.AddDays(daysOffset + 1);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstMonday, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            var weekNum = weekOfYear;
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }
            return firstMonday.AddDays(weekNum * 7);
        }
        private void ChangeCRSAWeekStatus(long V_CRSAWeekStatus)
        {
            CurrentCRSAWeek.V_CRSAWeekStatus = V_CRSAWeekStatus;
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                using (var serviceFactory = new ClinicManagementServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    contract.BeginChangeCRSAWeekStatus(CurrentCRSAWeek, Globals.LoggedUserAccount.Staff.StaffID, Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            var result = contract.EndChangeCRSAWeekStatus(asyncResult);
                            if (result)
                            {
                                SetEnableForWeek(CurrentCRSAWeek);
                                CalendarEnable = CurrentCRSAWeek.V_CRSAWeekStatus != (long)AllLookupValues.V_CRSAWeekStatus.Khoa_Lich;
                            }
                        }
                        catch (Exception ex)
                        {
                            this.HideBusyIndicator();
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
        public void btnSendGeneralPlanRoom()
        {
            string DoctorHaveNotTimeSegent = CheckAllDoctorHaveTimeSegent();
            if (!string.IsNullOrWhiteSpace(DoctorHaveNotTimeSegent))
            {
                MessageBox.Show(DoctorHaveNotTimeSegent);
                return;
            }
            ChangeCRSAWeekStatus((long)AllLookupValues.V_CRSAWeekStatus.Gui_KHTH);
        }
        public void btnSendOrganizeRoom()
        {
            ChangeCRSAWeekStatus((long)AllLookupValues.V_CRSAWeekStatus.Gui_TC);
        }
        public void btnReturnGeneralPlanRoom()
        {
            ChangeCRSAWeekStatus((long)AllLookupValues.V_CRSAWeekStatus.Tra_KHTH);
        }
        public void btnConfirm()
        {
            ChangeCRSAWeekStatus((long)AllLookupValues.V_CRSAWeekStatus.Khoa_Lich);
        }
        public void btnPreview()
        {

        }
        public void btnUnlock()
        {
            ChangeCRSAWeekStatus((long)AllLookupValues.V_CRSAWeekStatus.Mo_Khoa_Lich);
        }
        public void btnSelectDate()
        {
            if (SelectedDate == null)
            {
                return;
            }
            if (SelectedDate.Year < DateTime.Now.Year - 1 || SelectedDate.Year > DateTime.Now.Year + 1)
            {
                return;
            }
            CurrentYear = SelectedDate.Year;
            CurrentYear = SelectedDate.Year;
            CurrentWeek = GetCurrentWeek(SelectedDate);
        }
    }
}
