using eHCMS.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public partial class MedicalRecordCoverSample3 : EntityBase
    {
        #region Primitive Properties
        [DataMemberAttribute()]
        public long MedicalRecordCoverSample3ID
        {
            get
            {
                return _MedicalRecordCoverSample3ID;
            }
            set
            {
                if (_MedicalRecordCoverSample3ID == value)
                {
                    return;
                }
                _MedicalRecordCoverSample3ID = value;
                RaisePropertyChanged("MedicalRecordCoverSample3ID");
            }
        }
        private long _MedicalRecordCoverSample3ID;

        [DataMemberAttribute()]
        public long InPatientAdmDisDetailID
        {
            get
            {
                return _InPatientAdmDisDetailID;
            }
            set
            {
                if (_InPatientAdmDisDetailID == value)
                {
                    return;
                }
                _InPatientAdmDisDetailID = value;
                RaisePropertyChanged("InPatientAdmDisDetailID");
            }
        }
        private long _InPatientAdmDisDetailID;

        [DataMemberAttribute()]
        public long PtRegistrationID
        {
            get
            {
                return _PtRegistrationID;
            }
            set
            {
                if (_PtRegistrationID == value)
                {
                    return;
                }
                _PtRegistrationID = value;
                RaisePropertyChanged("PtRegistrationID");
            }
        }
        private long _PtRegistrationID;

        [DataMemberAttribute()]
        public long ServiceRecID
        {
            get
            {
                return _ServiceRecID;
            }
            set
            {
                if (_ServiceRecID == value)
                {
                    return;
                }
                _ServiceRecID = value;
                RaisePropertyChanged("ServiceRecID");
            }
        }
        private long _ServiceRecID;

        [DataMemberAttribute()]
        public string RespiratoryTestResult
        {
            get
            {
                return _RespiratoryTestResult;
            }
            set
            {
                if (_RespiratoryTestResult == value)
                {
                    return;
                }
                _RespiratoryTestResult = value;
                RaisePropertyChanged("RespiratoryTestResult");
            }
        }
        private string _RespiratoryTestResult;

        [DataMemberAttribute()]
        public string DigestionTestResult
        {
            get
            {
                return _DigestionTestResult;
            }
            set
            {
                if (_DigestionTestResult == value)
                {
                    return;
                }
                _DigestionTestResult = value;
                RaisePropertyChanged("DigestionTestResult");
            }
        }
        private string _DigestionTestResult;

        [DataMemberAttribute()]
        public string UrologyTestResult
        {
            get
            {
                return _UrologyTestResult;
            }
            set
            {
                if (_UrologyTestResult == value)
                {
                    return;
                }
                _UrologyTestResult = value;
                RaisePropertyChanged("UrologyTestResult");
            }
        }
        private string _UrologyTestResult;

        [DataMemberAttribute()]
        public string NeurologyTestResult
        {
            get
            {
                return _NeurologyTestResult;
            }
            set
            {
                if (_NeurologyTestResult == value)
                {
                    return;
                }
                _NeurologyTestResult = value;
                RaisePropertyChanged("NeurologyTestResult");
            }
        }
        private string _NeurologyTestResult;

        [DataMemberAttribute()]
        public string OrthopaedicsTestResult
        {
            get
            {
                return _OrthopaedicsTestResult;
            }
            set
            {
                if (_OrthopaedicsTestResult == value)
                {
                    return;
                }
                _OrthopaedicsTestResult = value;
                RaisePropertyChanged("OrthopaedicsTestResult");
            }
        }
        private string _OrthopaedicsTestResult;

        [DataMemberAttribute()]
        public string OtherDiseases
        {
            get
            {
                return _OtherDiseases;
            }
            set
            {
                if (_OtherDiseases == value)
                {
                    return;
                }
                _OtherDiseases = value;
                RaisePropertyChanged("OtherDiseases");
            }
        }
        private string _OtherDiseases;

        [DataMemberAttribute()]
        public string SummaryOfMedicalRecords
        {
            get
            {
                return _SummaryOfMedicalRecords;
            }
            set
            {
                if (_SummaryOfMedicalRecords == value)
                {
                    return;
                }
                _SummaryOfMedicalRecords = value;
                RaisePropertyChanged("SummaryOfMedicalRecords");
            }
        }
        private string _SummaryOfMedicalRecords;

        [DataMemberAttribute()]
        public string Distinguish
        {
            get
            {
                return _Distinguish;
            }
            set
            {
                if (_Distinguish == value)
                {
                    return;
                }
                _Distinguish = value;
                RaisePropertyChanged("Distinguish");
            }
        }
        private string _Distinguish;

        [DataMemberAttribute()]
        public string Prognosis
        {
            get
            {
                return _Prognosis;
            }
            set
            {
                if (_Prognosis == value)
                {
                    return;
                }
                _Prognosis = value;
                RaisePropertyChanged("Prognosis");
            }
        }
        private string _Prognosis;

        [DataMemberAttribute()]
        public string TreatmentDirection
        {
            get
            {
                return _TreatmentDirection;
            }
            set
            {
                if (_TreatmentDirection == value)
                {
                    return;
                }
                _TreatmentDirection = value;
                RaisePropertyChanged("TreatmentDirection");
            }
        }
        private string _TreatmentDirection;

        [DataMemberAttribute()]
        public Staff DoctorStaff
        {
            get
            {
                return _DoctorStaff;
            }
            set
            {
                if (_DoctorStaff == value)
                {
                    return;
                }
                _DoctorStaff = value;
                RaisePropertyChanged("DoctorStaff");
            }
        }
        private Staff _DoctorStaff;

        [DataMemberAttribute]
        public bool IsDeleted
        {
            get
            {
                return _IsDeleted;
            }
            set
            {
                if (_IsDeleted == value)
                {
                    return;
                }
                _IsDeleted = value;
                RaisePropertyChanged("IsDeleted");
            }
        }
        private bool _IsDeleted;

        [DataMemberAttribute]
        public Staff CreatedStaff
        {
            get
            {
                return _CreatedStaff;
            }
            set
            {
                if (_CreatedStaff == value)
                {
                    return;
                }
                _CreatedStaff = value;
                RaisePropertyChanged("CreatedStaff");
            }
        }
        private Staff _CreatedStaff;

        [DataMemberAttribute]
        public DateTime CreatedDate
        {
            get
            {
                return _CreatedDate;
            }
            set
            {
                if (_CreatedDate == value)
                {
                    return;
                }
                _CreatedDate = value;
                RaisePropertyChanged("CreatedDate");
            }
        }
        private DateTime _CreatedDate;

        [DataMemberAttribute]
        public Staff LastUpdateStaff
        {
            get
            {
                return _LastUpdateStaff;
            }
            set
            {
                if (_LastUpdateStaff == value)
                {
                    return;
                }
                _LastUpdateStaff = value;
                RaisePropertyChanged("LastUpdateStaff");
            }
        }
        private Staff _LastUpdateStaff;

        [DataMemberAttribute]
        public DateTime LastUpdateDate
        {
            get
            {
                return _LastUpdateDate;
            }
            set
            {
                if (_LastUpdateDate == value)
                {
                    return;
                }
                _LastUpdateDate = value;
                RaisePropertyChanged("LastUpdateDate");
            }
        }
        private DateTime _LastUpdateDate;
        #endregion
    }
}
