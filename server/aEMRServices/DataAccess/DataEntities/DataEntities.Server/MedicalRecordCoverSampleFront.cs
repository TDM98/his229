using eHCMS.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public partial class MedicalRecordCoverSampleFront : EntityBase
    {
        #region Primitive Properties
        [DataMemberAttribute()]
        public long MedicalRecordCoverSampleFrontID
        {
            get
            {
                return _MedicalRecordCoverSampleFrontID;
            }
            set
            {
                if (_MedicalRecordCoverSampleFrontID == value)
                {
                    return;
                }
                _MedicalRecordCoverSampleFrontID = value;
                RaisePropertyChanged("MedicalRecordCoverSampleFrontID");
            }
        }
        private long _MedicalRecordCoverSampleFrontID;

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
        public Lookup V_ReferralType
        {
            get
            {
                return _V_ReferralType;
            }
            set
            {
                if (_V_ReferralType == value)
                {
                    return;
                }
                _V_ReferralType = value;
                RaisePropertyChanged("V_ReferralType");
            }
        }
        private Lookup _V_ReferralType;

        [DataMemberAttribute()]
        public int HospitalizedForThisDisease
        {
            get
            {
                return _HospitalizedForThisDisease;
            }
            set
            {
                if (_HospitalizedForThisDisease == value)
                {
                    return;
                }
                _HospitalizedForThisDisease = value;
                RaisePropertyChanged("HospitalizedForThisDisease");
            }
        }
        private int _HospitalizedForThisDisease;

        [DataMemberAttribute()]
        public Lookup V_HospitalTransfer
        {
            get
            {
                return _V_HospitalTransfer;
            }
            set
            {
                if (_V_HospitalTransfer == value)
                {
                    return;
                }
                _V_HospitalTransfer = value;
                RaisePropertyChanged("V_HospitalTransfer");
            }
        }
        private Lookup _V_HospitalTransfer;

        [DataMemberAttribute]
        public Lookup V_Surgery_Tips_Item
        {
            get
            {
                return _V_Surgery_Tips_Item;
            }
            set
            {
                if (_V_Surgery_Tips_Item == value)
                {
                    return;
                }
                _V_Surgery_Tips_Item = value;
                RaisePropertyChanged("V_Surgery_Tips_Item");
            }
        }
        private Lookup _V_Surgery_Tips_Item;

        [DataMemberAttribute]
        public Lookup V_Stroke_Complications
        {
            get
            {
                return _V_Stroke_Complications;
            }
            set
            {
                if (_V_Stroke_Complications == value)
                {
                    return;
                }
                _V_Stroke_Complications = value;
                RaisePropertyChanged("V_Stroke_Complications");
            }
        }
        private Lookup _V_Stroke_Complications;

        [DataMemberAttribute()]
        public Lookup V_Pathology
        {
            get
            {
                return _V_Pathology;
            }
            set
            {
                if (_V_Pathology == value)
                {
                    return;
                }
                _V_Pathology = value;
                RaisePropertyChanged("V_Pathology");
            }
        }
        private Lookup _V_Pathology;

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
