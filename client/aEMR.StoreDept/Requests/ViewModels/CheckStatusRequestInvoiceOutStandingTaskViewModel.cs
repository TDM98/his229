using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ServiceModel;
using System.Threading;
using aEMR.DataContracts;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using aEMR.ServiceClient;
using aEMR.ViewContracts;
using aEMR.Common.Collections;
using aEMR.Infrastructure.CachingUtils;
using Castle.Windsor;
using Castle.Core.Logging;
using Caliburn.Micro;
using DataEntities;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using aEMR.Controls;
using eHCMSLanguage;
using System.Windows.Data;
using aEMR.CommonTasks;
/*
* 20230815 #001 BLQ: Thêm điều kiện load phiếu từ màn hình thủ thuật
*/
namespace aEMR.StoreDept.Requests.ViewModels
{
    [Export(typeof(ICheckStatusRequestInvoiceOutStandingTask)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class CheckStatusRequestInvoiceOutStandingTaskViewModel : Conductor<object>, ICheckStatusRequestInvoiceOutStandingTask
    {
        private readonly INavigationService _navigationService;
        private readonly ISalePosCaching _salePosCaching;
        private readonly ILogger _logger;
        [ImportingConstructor]
        public CheckStatusRequestInvoiceOutStandingTaskViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventAgr, ISalePosCaching salePosCaching)
        {
            _navigationService = navigationService;
            _logger = container.Resolve<ILogger>();
            _salePosCaching = salePosCaching;
            
            RequestDruglist = new ObservableCollection<RequestDrugInwardClinicDept>();
            RequestDruglist_ForSmallProcedure = new ObservableCollection<RequestDrugForTechnicalService>();
        }
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            Globals.EventAggregator.Unsubscribe(this);
        }
        AxComboBox CbxStoreRequest;
        public void cbxStoreRequest_Loaded(object sender, RoutedEventArgs e)
        {
            CbxStoreRequest = sender as AxComboBox;
        }
        private ObservableCollection<RefStorageWarehouseLocation> _StoreCbxStaff;
        public ObservableCollection<RefStorageWarehouseLocation> StoreCbxStaff
        {
            get
            {
                return _StoreCbxStaff;
            }
            set
            {
                if (_StoreCbxStaff != value)
                {
                    _StoreCbxStaff = value;
                    NotifyOfPropertyChange(() => StoreCbxStaff);
                }
            }
        }
        private RefStorageWarehouseLocation _SelectedStoreCbxStaff;
        public RefStorageWarehouseLocation SelectedStoreCbxStaff
        {
            get
            {
                return _SelectedStoreCbxStaff;
            }
            set
            {
                if (_SelectedStoreCbxStaff != value)
                {
                    _SelectedStoreCbxStaff = value;
                    NotifyOfPropertyChange(() => SelectedStoreCbxStaff);
                }
            }
        }

        private ObservableCollection<RequestDrugInwardClinicDept> _RequestDrugList;
        public ObservableCollection<RequestDrugInwardClinicDept> RequestDruglist
        {
            get
            {
                return _RequestDrugList;
            }
            set
            {
                if (_RequestDrugList != value)
                {
                    _RequestDrugList = value;
                    NotifyOfPropertyChange(() => RequestDruglist);
                }
            }
        }
        public void dataGrid1_DblClick(object sender, Common.EventArgs<object> e)
        {
            TryClose();
            Globals.EventAggregator.Publish(new DrugDeptCloseSearchRequestEvent { SelectedRequest = e.Value, IsCreateNewFromExisting = false });
        }
        public void btnFindRequest(object sender, RoutedEventArgs e)
        {
            if (CbxStoreRequest == null || (long)CbxStoreRequest.SelectedValue <= 0)
            {
                MessageBox.Show(eHCMSResources.K0310_G1_ChonKhoCanXem);
                return;
            }
            //▼====: #001
            if (!IsLoadFromSmallProcedure)
            {
                SearchRequestDrugInwardClinicDept_NotPaging(V_MedProductType, (long)CbxStoreRequest.SelectedValue);
            }
            else
            {
                if(PtRegDetailID == 0)
                {
                    MessageBox.Show("Chọn thủ thuật cần xem phiếu yêu cầu");
                    return;
                }
                SearchRequestDrugForTechnicalService_NotPaging(V_MedProductType, (long)CbxStoreRequest.SelectedValue, PtRegDetailID);
            }
            //▲====: #001
        }
        private long _V_MedProductType = (long)AllLookupValues.MedProductType.THUOC; 
        public long V_MedProductType
        {
            get
            {
                return _V_MedProductType;
            }
            set
            {
                if (_V_MedProductType != value)
                {
                    _V_MedProductType = value;
                    NotifyOfPropertyChange(() => V_MedProductType);
                }

            }
        }

        public void SearchRequestDrugInwardClinicDept_NotPaging(long V_MedProductType, long StoreID)
        {
            try
            {
                var t = new Thread(() =>
                {
                    using (var serviceFactory = new PharmacyClinicDeptServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginSearchRequestDrugInwardClinicDept_NotPaging(V_MedProductType, StoreID, Globals.DispatchCallback((asyncResult) =>
                        {

                            try
                            {
                                var results = contract.EndSearchRequestDrugInwardClinicDept_NotPaging( asyncResult);
                                RequestDruglist.Clear();
                                if (results != null)
                                {
                                    foreach (RequestDrugInwardClinicDept p in results)
                                    {
                                        RequestDruglist.Add(p);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                                _logger.Error(ex.Message);
                            }
                            finally
                            {
                            }

                        }), null);

                    }

                });

                t.Start();
            }
            catch (Exception ex)
            {
                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                _logger.Error(ex.Message);
                this.DlgHideBusyIndicator();
            }
        }
        public void LoadStore()
        {
            if (Globals.allRefStorageWarehouseLocation == null || Globals.allRefStorageWarehouseLocation.Count == 0)
            {
                Coroutine.BeginExecute(DoGetStoreDeptAll());
            }
            else
            {
                StoreCbxStaff = Globals.checkStoreWareHouse(V_MedProductType, false, true);
                SelectedStoreCbxStaff = StoreCbxStaff.FirstOrDefault();
                if (StoreCbxStaff == null || StoreCbxStaff.Count < 1)
                {
                    MessageBox.Show(eHCMSResources.A0110_G1_Msg_InfoChuaCauHinhTNKho);
                    return;
                }
            }
        }
        private IEnumerator<IResult> DoGetStoreDeptAll()
        {
            var paymentTypeTask = new LoadStoreListTask(null, false, null, false, true);
            yield return paymentTypeTask;
            Globals.allRefStorageWarehouseLocation = paymentTypeTask.LookupList;
            StoreCbxStaff = Globals.checkStoreWareHouse(V_MedProductType, false, true);
            SelectedStoreCbxStaff = StoreCbxStaff.FirstOrDefault();
            if (StoreCbxStaff == null || StoreCbxStaff.Count < 1)
            {
                MessageBox.Show(eHCMSResources.A0110_G1_Msg_InfoChuaCauHinhTNKho);
                yield break;
            }
            yield break;
        }
        //▼====: #001
        private bool _IsLoadFromSmallProcedure = false;
        public bool IsLoadFromSmallProcedure
        {
            get
            {
                return _IsLoadFromSmallProcedure;
            }
            set
            {
                if (_IsLoadFromSmallProcedure != value)
                {
                    _IsLoadFromSmallProcedure = value;
                    NotifyOfPropertyChange(() => IsLoadFromSmallProcedure);
                }
            }
        }
        private long _PtRegDetailID = 0;
        public long PtRegDetailID
        {
            get
            {
                return _PtRegDetailID;
            }
            set
            {
                if (_PtRegDetailID != value)
                {
                    _PtRegDetailID = value;
                    NotifyOfPropertyChange(() => PtRegDetailID);
                }
            }
        }
        private ObservableCollection<RequestDrugForTechnicalService> _RequestDrugList_ForSmallProcedure;
        public ObservableCollection<RequestDrugForTechnicalService> RequestDruglist_ForSmallProcedure
        {
            get
            {
                return _RequestDrugList_ForSmallProcedure;
            }
            set
            {
                if (_RequestDrugList_ForSmallProcedure != value)
                {
                    _RequestDrugList_ForSmallProcedure = value;
                    NotifyOfPropertyChange(() => RequestDruglist_ForSmallProcedure);
                }
            }
        }
        private RequestDrugForTechnicalService _SelectedRequestDrugList_ForSmallProcedure;
        public RequestDrugForTechnicalService SelectedRequestDruglist_ForSmallProcedure
        {
            get
            {
                return _SelectedRequestDrugList_ForSmallProcedure;
            }
            set
            {
                if (_SelectedRequestDrugList_ForSmallProcedure != value)
                {
                    _SelectedRequestDrugList_ForSmallProcedure = value;
                    NotifyOfPropertyChange(() => SelectedRequestDruglist_ForSmallProcedure);
                }
            }
        }
        public void dataGrid2_DblClick(object sender, Common.EventArgs<object> e)
        {
            //TryClose();
            Globals.EventAggregator.Publish(new SelectRequestDrugForTechnicalServiceForSmallProcedure { SelectedRequest = SelectedRequestDruglist_ForSmallProcedure });
        }
        public void SearchRequestDrugForTechnicalService_NotPaging(long V_MedProductType, long StoreID, long PtRegDetailID)
        {
            try
            {
                var t = new Thread(() =>
                {
                    using (var serviceFactory = new PharmacyClinicDeptServiceClient())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginSearchRequestDrugForTechnicalService_NotPaging(V_MedProductType, StoreID, PtRegDetailID, Globals.DispatchCallback((asyncResult) =>
                        {
                            try
                            {
                                var results = contract.EndSearchRequestDrugForTechnicalService_NotPaging(asyncResult);
                                RequestDruglist_ForSmallProcedure.Clear();
                                if (results != null)
                                {
                                    foreach (RequestDrugForTechnicalService p in results)
                                    {
                                        RequestDruglist_ForSmallProcedure.Add(p);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                                _logger.Error(ex.Message);
                            }
                            finally
                            {
                            }

                        }), null);

                    }

                });

                t.Start();
            }
            catch (Exception ex)
            {
                Globals.ShowMessage(ex.Message, eHCMSResources.T0432_G1_Error);
                _logger.Error(ex.Message);
                this.DlgHideBusyIndicator();
            }
        }
        //▲====: #001
    }
}