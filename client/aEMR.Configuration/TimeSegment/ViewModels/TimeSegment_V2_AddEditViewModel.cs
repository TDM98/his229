using eHCMSLanguage;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using aEMR.ServiceClient;
using aEMR.ViewContracts.Configuration;
using aEMR.Infrastructure.CachingUtils;
using Castle.Windsor;
using Castle.Core.Logging;
using Caliburn.Micro;
using DataEntities;
using aEMR.ViewContracts;
using aEMR.Common.Collections;

namespace aEMR.Configuration.TimeSegment.ViewModels
{
    [Export(typeof(ITimeSegment_V2_AddEdit)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class TimeSegment_V2_AddEditViewModel : Conductor<object>, ITimeSegment_V2_AddEdit
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public TimeSegment_V2_AddEditViewModel(IWindsorContainer container, INavigationService navigationService, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;
            TimeSegmentType = Globals.GetAllLookupValuesByType((long)LookupValues.V_TimeSegmentType).ToObservableCollection();

            FromTimeHour = Globals.GetViewModel<IMinHourControl>();
            FromTimeHour.DateTime = Globals.GetCurServerDateTime().Date;
            ToTimeHour = Globals.GetViewModel<IMinHourControl>();
            ToTimeHour.DateTime = Globals.GetCurServerDateTime().Date;
        }

        private ConsultationTimeSegments _ObjTimeSegments_Current;
        public ConsultationTimeSegments ObjTimeSegments_Current
        {
            get { return _ObjTimeSegments_Current; }
            set
            {
                _ObjTimeSegments_Current = value;
                NotifyOfPropertyChange(() => ObjTimeSegments_Current);
            }
        }

        private ObservableCollection<Lookup> _TimeSegmentType;
        public ObservableCollection<Lookup> TimeSegmentType
        {
            get { return _TimeSegmentType; }
            set
            {
                _TimeSegmentType = value;
                NotifyOfPropertyChange(() => TimeSegmentType);
            }
        }

        private string _TitleForm;
        public string TitleForm
        {
            get { return _TitleForm; }
            set
            {
                _TitleForm = value;
                NotifyOfPropertyChange(() => TitleForm);
            }
        }
        private IMinHourControl _FromTimeHour;

        public IMinHourControl FromTimeHour
        {
            get
            {
                return _FromTimeHour;
            }
            set
            {
                if (_FromTimeHour == value)
                    return;
                _FromTimeHour = value;
                NotifyOfPropertyChange(() => FromTimeHour);
            }
        }
        private IMinHourControl _ToTimeHour;

        public IMinHourControl ToTimeHour
        {
            get
            {
                return _ToTimeHour;
            }
            set
            {
                if (_ToTimeHour == value)
                    return;
                _ToTimeHour = value;
                NotifyOfPropertyChange(() => ToTimeHour);
            }
        }
        public void InitializeNewItem()
        {
            if(ObjTimeSegments_Current == null || ObjTimeSegments_Current.ConsultationTimeSegmentID == 0)
            {
                ObjTimeSegments_Current = new ConsultationTimeSegments();
            }
            FromTimeHour.DateTime = ObjTimeSegments_Current.StartTime;
            ToTimeHour.DateTime = ObjTimeSegments_Current.EndTime;
        }

        public void btSave()
        {
            if (string.IsNullOrWhiteSpace(ObjTimeSegments_Current.SegmentName))
            {
                MessageBox.Show("Nhập đầy đủ thông tin tên khung giờ!");
                return;
            }
            if (ObjTimeSegments_Current.V_TimeSegmentType <= 0)
            {
                MessageBox.Show("Nhập đầy đủ thông tin loại khung giờ!");
                return;
            }
            if (FromTimeHour == null || ToTimeHour == null)
            {
                MessageBox.Show("Nhập đầy đủ thông tin từ giờ, đến giờ!");
                return;
            }
            if (FromTimeHour.DateTime >= ToTimeHour.DateTime)
            {
                MessageBox.Show("Giờ đến không được nhỏ hơn hoặc bằng giờ bắt đầu");
                return;
            }
            ObjTimeSegments_Current.StartTime = (DateTime)FromTimeHour.DateTime;
            ObjTimeSegments_Current.EndTime = (DateTime)ToTimeHour.DateTime;
            TimeSegments_InsertUpdate(ObjTimeSegments_Current, true);
        }

        public bool CheckValid(object temp)
        {
            ConsultationTimeSegments p = temp as ConsultationTimeSegments;
            if (p == null)
            {
                return false;
            }
            return p.Validate();
        }

        public void btClose()
        {
            TryClose();
        }

        private void TimeSegments_InsertUpdate(ConsultationTimeSegments Obj, bool SaveToDB)
        {
            this.ShowBusyIndicator();
            var t = new Thread(() =>
            {
                using (var serviceFactory = new ConfigurationManagerServiceClient())
                {
                    var contract = serviceFactory.ServiceInstance;
                    Obj.StaffID = (long)Globals.LoggedUserAccount.StaffID;
                    contract.BeginTimeSegment_Save(Obj, Globals.DispatchCallback((asyncResult) =>
                    {
                        try
                        {
                            string Result = "";
                            contract.EndTimeSegment_Save(out Result, asyncResult);
                            switch (Result)
                            {
                                case "Duplex-Name":
                                    {
                                        MessageBox.Show(string.Format("{0} {1}!", "Tên khung giờ đã tồn tại!", "Vui lòng chọn tên khác"), eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Duplex-Time":
                                    {
                                        MessageBox.Show(string.Format("{0} {1}!", "Thời gian khung giờ đã tồn tại!", "Vui lòng chọn khung giờ khác"), eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Update-0":
                                    {
                                        MessageBox.Show(eHCMSResources.A0608_G1_Msg_InfoHChinhFail, eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Update-1":
                                    {
                                        Globals.EventAggregator.Publish(new CRUDEvent());
                                        MessageBox.Show(eHCMSResources.A0468_G1_Msg_InfoLuuOK, eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                        TryClose();
                                        break;
                                    }
                                case "Insert-0":
                                    {
                                        MessageBox.Show(eHCMSResources.A1026_G1_Msg_InfoThemFail, eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                        break;
                                    }
                                case "Insert-1":
                                    {
                                        Globals.EventAggregator.Publish(new CRUDEvent());
                                        MessageBox.Show(eHCMSResources.A0468_G1_Msg_InfoLuuOK, eHCMSResources.T2937_G1_Luu, MessageBoxButton.OK);
                                        TryClose();
                                        break;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                            this.HideBusyIndicator();
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
    }
}
