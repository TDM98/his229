﻿using eHCMSLanguage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using aEMR.Infrastructure;
using aEMR.Infrastructure.Events;
using aEMR.ServiceClient;
using aEMR.ViewContracts;
using Caliburn.Micro;
using DataEntities;
using PCLsProxy;
using aEMR.Controls;
using Castle.Windsor;

namespace aEMR.PCLDepartment.ViewModels
{
    [Export(typeof (ISATGSDipyKetQua)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class SATGSDipyKetQuaViewModel : Conductor<object>, ISATGSDipyKetQua
        , IHandle<ReaderInfoPatientFromPatientPCLReqEvent<PatientPCLRequest>>
    {
        [ImportingConstructor]
        public SATGSDipyKetQuaViewModel(IWindsorContainer container, INavigationService navigationService, IEventAggregator eventArg)
        {
            Globals.EventAggregator.Subscribe(this);
            GetV_CardialStatus();
            GetV_CardialResult();

            if (Globals.PatientPCLReqID_Imaging > 0)
            {
                PatientPCLReqID = Globals.PatientPCLReqID_Imaging;
                LoadInfo();
            }
        }

        private long _PatientPCLReqID;
        public long PatientPCLReqID
        {
            get { return _PatientPCLReqID; }
            set
            {
                if (_PatientPCLReqID != value)
                {
                    _PatientPCLReqID = value;
                    NotifyOfPropertyChange(() => PatientPCLReqID);
                }
            }
        }

        #region properties

        private ObservableCollection<Lookup> _allCardialResult;
        private ObservableCollection<Lookup> _allCardialStatus;
        private URP_FE_StressDipyridamoleResult _curURP_FE_StressDipyridamoleResult;
        private bool _isSave;
        private bool _isUpdate;

        private URP_FE_StressDipyridamoleResult _tempURP_FE_StressDipyridamoleResult;


        private bool isHasPatient;

        public URP_FE_StressDipyridamoleResult curURP_FE_StressDipyridamoleResult
        {
            get { return _curURP_FE_StressDipyridamoleResult; }
            set
            {
                if (_curURP_FE_StressDipyridamoleResult == value)
                    return;
                _curURP_FE_StressDipyridamoleResult = value;
                NotifyOfPropertyChange(() => curURP_FE_StressDipyridamoleResult);
            }
        }

        public URP_FE_StressDipyridamoleResult tempURP_FE_StressDipyridamoleResult
        {
            get { return _tempURP_FE_StressDipyridamoleResult; }
            set
            {
                if (_tempURP_FE_StressDipyridamoleResult == value)
                    return;
                _tempURP_FE_StressDipyridamoleResult = value;
                NotifyOfPropertyChange(() => tempURP_FE_StressDipyridamoleResult);
            }
        }

        public bool IsHasPatient
        {
            get { return isHasPatient; }
            set
            {
                if (isHasPatient == value)
                    return;
                isHasPatient = value;
                NotifyOfPropertyChange(() => isHasPatient);
            }
        }

        public bool isSave
        {
            get { return _isSave; }
            set
            {
                if (_isSave == value)
                    return;
                _isSave = value;
                NotifyOfPropertyChange(() => isSave);
            }
        }

        public bool isUpdate
        {
            get { return _isUpdate; }
            set
            {
                if (_isUpdate == value)
                    return;
                _isUpdate = value;
                NotifyOfPropertyChange(() => isUpdate);
            }
        }

        public ObservableCollection<Lookup> allCardialResult
        {
            get { return _allCardialResult; }
            set
            {
                if (_allCardialResult == value)
                    return;
                _allCardialResult = value;
                NotifyOfPropertyChange(() => allCardialResult);
            }
        }

        public ObservableCollection<Lookup> allCardialStatus
        {
            get { return _allCardialStatus; }
            set
            {
                if (_allCardialStatus == value)
                    return;
                _allCardialStatus = value;
                NotifyOfPropertyChange(() => allCardialStatus);
            }
        }

        #endregion

        #region fuction

        private ValidationSummary validationSummary { get; set; }

        public void CheckHasPCLImageID()
        {
            isHasPatient = true;
            //if()
            //{
            //    isHasPatient = true;
            //}else
            //{
            //    isHasPatient = false;
            //}
            //curURP_FE_StressDipyridamoleResult.PCLImgResultID
            //curURP_FE_StressDipyridamoleResult.DoctorStaffID
        }

        public void CheckSave()
        {
            isSave = true;
            isUpdate = false;
        }

        public void CheckUpdate()
        {
            isSave = false;
            isUpdate = true;
        }

        public void butReset()
        {
            curURP_FE_StressDipyridamoleResult = new URP_FE_StressDipyridamoleResult();
            curURP_FE_StressDipyridamoleResult = tempURP_FE_StressDipyridamoleResult;
            NotifyOfPropertyChange(() => curURP_FE_StressDipyridamoleResult);
        }

        public void butSave()
        {
            if (PatientPCLReqID <= 0)
            {
                MessageBox.Show(eHCMSResources.A0301_G1_Msg_InfoChonBN);
                return;
            }

            if (validationSummary.DisplayedErrors.Count > 0)
            {
                MessageBox.Show(eHCMSResources.A0559_G1_Msg_InfoGTriKhHopLe);
                return;
            }

            if (curURP_FE_StressDipyridamoleResult.URP_FE_StressDipyridamoleResultID > 0)
            {
                UpdateURP_FE_StressDipyridamoleResult(curURP_FE_StressDipyridamoleResult);
            }
            else
            {
                curURP_FE_StressDipyridamoleResult.PCLImgResultID = PatientPCLReqID;
                curURP_FE_StressDipyridamoleResult.DoctorStaffID = Globals.LoggedUserAccount.Staff.StaffID;
                AddURP_FE_StressDipyridamoleResult(curURP_FE_StressDipyridamoleResult);
            }
            
        }

        //public void butUpdate()
        //{
        //    if (validationSummary.DisplayedErrors.Count > 0)
        //    {
        //        MessageBox.Show(eHCMSResources.A0559_G1_Msg_InfoGTriKhHopLe);
        //        return;
        //    }
        //    UpdateURP_FE_StressDipyridamoleResult(curURP_FE_StressDipyridamoleResult);
        //}

        public void ValidationSummary_Loaded(object sender, RoutedEventArgs e)
        {
            validationSummary = sender as ValidationSummary;
        }

        #endregion

        #region method

        private void AddURP_FE_StressDipyridamoleResult(URP_FE_StressDipyridamoleResult entity)
        {
            Globals.EventAggregator.Publish(new BusyEvent {IsBusy = true, Message = eHCMSResources.Z0125_G1_DangXuLi});
            var t = new Thread(() =>
                                   {
                                       using (var serviceFactory = new PCLsClient())
                                       {
                                           IPCLs contract = serviceFactory.ServiceInstance;
                                           contract.BeginAddURP_FE_StressDipyridamoleResult(entity,
                                                                                            Globals.DispatchCallback(
                                                                                                (asyncResult) =>
                                                                                                    {
                                                                                                        try
                                                                                                        {
                                                                                                            bool res =contract.EndAddURP_FE_StressDipyridamoleResult(asyncResult);
                                                                                                            if (res)
                                                                                                            {
                                                                                                                MessageBox.Show(eHCMSResources.A0079_G1_Msg_InfoThemMoiOK);
                                                                                                            }
                                                                                                            else
                                                                                                            {
                                                                                                                MessageBox.Show("Thêm mới thất bại!");
                                                                                                            }
                                                                                                        }
                                                                                                        catch (Exception ex)
                                                                                                        {
                                                                                                            MessageBox.Show(ex.Message);
                                                                                                        }
                                                                                                        finally
                                                                                                        {
                                                                                                            Globals.IsBusy =false;
                                                                                                        }
                                                                                                    }), null);
                                       }
                                   });

            t.Start();
        }

        private void UpdateURP_FE_StressDipyridamoleResult(URP_FE_StressDipyridamoleResult entity)
        {
            Globals.EventAggregator.Publish(new BusyEvent {IsBusy = true, Message = eHCMSResources.Z0125_G1_DangXuLi});
            var t = new Thread(() =>
                                   {
                                       using (var serviceFactory = new PCLsClient())
                                       {
                                           IPCLs contract = serviceFactory.ServiceInstance;
                                           contract.BeginUpdateURP_FE_StressDipyridamoleResult(entity,
                                                                                               Globals.DispatchCallback(
                                                                                                   (asyncResult) =>
                                                                                                       {
                                                                                                           try
                                                                                                           {
                                                                                                               bool res=contract.EndUpdateURP_FE_StressDipyridamoleResult(asyncResult);
                                                                                                               if (res)
                                                                                                               {
                                                                                                                   MessageBox.Show(eHCMSResources.A0279_G1_Msg_InfoCNhatOK);
                                                                                                               }
                                                                                                               else
                                                                                                               {
                                                                                                                   MessageBox.Show(eHCMSResources.A0272_G1_Msg_InfoCNhatFail);
                                                                                                               }
                                                                                                           }
                                                                                                           catch (Exception ex)
                                                                                                           {
                                                                                                               MessageBox.Show(ex.Message);
                                                                                                           }
                                                                                                           finally
                                                                                                           {
                                                                                                               Globals.IsBusy=false;
                                                                                                           }
                                                                                                       }), null);
                                       }
                                   });

            t.Start();
        }

        private void GetURP_FE_StressDipyridamoleResultByID(long URP_FE_StressDipyridamoleResultID, long PCLImgResultID)
        {
            Globals.EventAggregator.Publish(new BusyEvent {IsBusy = true, Message = eHCMSResources.Z0125_G1_DangXuLi});
            var t = new Thread(() =>
                                   {
                                       using (var serviceFactory = new PCLsClient())
                                       {
                                           IPCLs contract = serviceFactory.ServiceInstance;
                                           contract.BeginGetURP_FE_StressDipyridamoleResult(URP_FE_StressDipyridamoleResultID, PCLImgResultID, Globals.DispatchCallback((asyncResult) =>
                                                                                              {
                                                                                                  try
                                                                                                  {
                                                                                                      var item = contract.EndGetURP_FE_StressDipyridamoleResult(asyncResult);
                                                                                                      if (item != null)
                                                                                                      {
                                                                                                          curURP_FE_StressDipyridamoleResult = item;
                                                                                                      }
                                                                                                  }
                                                                                                  catch (Exception ex)
                                                                                                  {
                                                                                                      MessageBox.Show(ex.Message);
                                                                                                  }
                                                                                                  finally
                                                                                                  {
                                                                                                      Globals.IsBusy =false;
                                                                                                  }
                                                                                              }), null);
                                       }
                                   });

            t.Start();
        }

        private void GetURP_FE_StressDobutamineResultByIDResult(long URP_FE_StressDobutamineResultID
                                                                , long PCLImgResultID)
        {
            Globals.EventAggregator.Publish(new BusyEvent {IsBusy = true, Message = eHCMSResources.Z0125_G1_DangXuLi});
            var t = new Thread(() =>
                                   {
                                       using (var serviceFactory = new PCLsClient())
                                       {
                                           IPCLs contract = serviceFactory.ServiceInstance;
                                           contract.BeginGetUltraResParams_EchoCardiographyResult(
                                               URP_FE_StressDobutamineResultID
                                               , PCLImgResultID, Globals.DispatchCallback((asyncResult) =>
                                                                                              {
                                                                                                  try
                                                                                                  {
                                                                                                      string st =
                                                                                                          contract.
                                                                                                              EndGetUltraResParams_EchoCardiographyResult
                                                                                                              (asyncResult);
                                                                                                      curURP_FE_StressDipyridamoleResult
                                                                                                          .
                                                                                                          KetQuaSieuAmTim
                                                                                                          = st;

                                                                                                      //else
                                                                                                      //{
                                                                                                      //    curURP_FE_StressDobutamineResult = new URP_FE_StressDobutamineResult();
                                                                                                      //    CheckSave();
                                                                                                      //}
                                                                                                  }
                                                                                                  catch (Exception ex)
                                                                                                  {
                                                                                                      MessageBox.Show(ex.Message);
                                                                                                  }
                                                                                                  finally
                                                                                                  {
                                                                                                      Globals.IsBusy =false;
                                                                                                  }
                                                                                              }), null);
                                       }
                                   });

            t.Start();
        }

        private void GetV_CardialStatus()
        {
            Globals.EventAggregator.Publish(new BusyEvent {IsBusy = true, Message = eHCMSResources.Z0125_G1_DangXuLi});
            var t = new Thread(() =>
                {
                    using (var serviceFactory = new CommonService_V2Client())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginGetAllLookupValuesByType(LookupValues.V_CardialStatus,Globals.DispatchCallback(
                    (asyncResult) =>
                        {
                            try
                            {
                                IList<Lookup> results=contract.EndGetAllLookupValuesByType(asyncResult);
                                if (results != null)
                                {
                                    if (allCardialStatus ==null)
                                    {
                                        allCardialStatus=
                                            new ObservableCollection<Lookup>();
                                    }
                                    else
                                    {
                                        allCardialStatus.Clear();
                                    }
                                    foreach (Lookup p in results)
                                    {
                                        allCardialStatus.Add(p);
                                    }
                                    NotifyOfPropertyChange(() =>allCardialStatus);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                Globals.IsBusy = false;
                            }
                        }), null);
                    }
                });

            t.Start();
        }

        private void GetV_CardialResult()
        {
            Globals.EventAggregator.Publish(new BusyEvent {IsBusy = true, Message = eHCMSResources.Z0125_G1_DangXuLi});
            var t = new Thread(() =>
                {
                    using (var serviceFactory = new CommonService_V2Client())
                    {
                        var contract = serviceFactory.ServiceInstance;
                        contract.BeginGetAllLookupValuesByType(LookupValues.V_CardialResult,Globals.DispatchCallback(
                    (asyncResult) =>
                        {
                            try
                            {
                                IList<Lookup> results=contract.EndGetAllLookupValuesByType(asyncResult);
                                if (results != null)
                                {
                                    if (allCardialResult ==null)
                                    {
                                        allCardialResult=new ObservableCollection<Lookup>();
                                    }
                                    else
                                    {
                                        allCardialResult.Clear();
                                    }
                                    foreach (Lookup p in results)
                                    {
                                        allCardialResult.Add(p);
                                    }
                                    NotifyOfPropertyChange(() =>allCardialResult);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                Globals.IsBusy = false;
                            }
                        }), null);
                    }
                });

            t.Start();
        }

        #endregion

        public void Handle(DbClickSelectedObjectEvent<PatientPCLRequest> message)
        {
            if (message != null)
            {
                PatientPCLReqID = message.Result.PatientPCLReqID;
                LoadInfo();
            }
        }

        public void Handle(ReaderInfoPatientFromPatientPCLReqEvent<PatientPCLRequest> message)
        {
            if (message != null)
            {
                PatientPCLReqID = message.PCLRequest.PatientPCLReqID;
                LoadInfo();
            }
        }
        private void LoadInfo()
        {
          
            //CheckHasPCLImageID();
            //CheckSave();
            //_tempURP_FE_StressDipyridamoleResult = new URP_FE_StressDipyridamoleResult();
            //curURP_FE_StressDipyridamoleResult = new URP_FE_StressDipyridamoleResult();
            //GetURP_FE_StressDipyridamoleResultByID(0, PatientPCLReqID);
        }
        //==== 20161201 CMN Begin: Add button save for all pages
        public void SetResultDetails(object resDetails)
        {
            curURP_FE_StressDipyridamoleResult = (URP_FE_StressDipyridamoleResult)resDetails;
        }
        public bool CheckValidate()
        {
            return (validationSummary != null && validationSummary.DisplayedErrors.Count == 0) || validationSummary == null;
        }
        //==== 20161201 CMN End.
    }
}