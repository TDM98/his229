using eHCMS.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public partial class MedicalRecordCoverSample4 : EntityBase
    {
        #region Primitive Properties
        [DataMemberAttribute()]
        public long MedicalRecordCoverSample4ID
        {
            get
            {
                return _MedicalRecordCoverSample4ID;
            }
            set
            {
                if (_MedicalRecordCoverSample4ID == value)
                {
                    return;
                }
                _MedicalRecordCoverSample4ID = value;
                RaisePropertyChanged("MedicalRecordCoverSample4ID");
            }
        }
        private long _MedicalRecordCoverSample4ID;

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
        public string XQuangFilmNum
        {
            get
            {
                return _XQuangFilmNum;
            }
            set
            {
                if (_XQuangFilmNum == value)
                {
                    return;
                }
                _XQuangFilmNum = value;
                RaisePropertyChanged("XQuangFilmNum");
            }
        }
        private string _XQuangFilmNum;

        [DataMemberAttribute()]
        public string CTFilmNum
        {
            get
            {
                return _CTFilmNum;
            }
            set
            {
                if (_CTFilmNum == value)
                {
                    return;
                }
                _CTFilmNum = value;
                RaisePropertyChanged("CTFilmNum");
            }
        }
        private string _CTFilmNum;

        [DataMemberAttribute()]
        public string UltrasoundFilmNum
        {
            get
            {
                return _UltrasoundFilmNum;
            }
            set
            {
                if (_UltrasoundFilmNum == value)
                {
                    return;
                }
                _UltrasoundFilmNum = value;
                RaisePropertyChanged("UltrasoundFilmNum");
            }
        }
        private string _UltrasoundFilmNum;

        [DataMemberAttribute()]
        public string LaboratoryFilmNum
        {
            get
            {
                return _LaboratoryFilmNum;
            }
            set
            {
                if (_LaboratoryFilmNum == value)
                {
                    return;
                }
                _LaboratoryFilmNum = value;
                RaisePropertyChanged("LaboratoryFilmNum");
            }
        }
        private string _LaboratoryFilmNum;

        [DataMemberAttribute()]
        public string OrderFilmName
        {
            get
            {
                return _OrderFilmName;
            }
            set
            {
                if (_OrderFilmName == value)
                {
                    return;
                }
                _OrderFilmName = value;
                RaisePropertyChanged("OrderFilmName");
            }
        }
        private string _OrderFilmName;

        [DataMemberAttribute()]
        public string OrderFilmNum
        {
            get
            {
                return _OrderFilmNum;
            }
            set
            {
                if (_OrderFilmNum == value)
                {
                    return;
                }
                _OrderFilmNum = value;
                RaisePropertyChanged("OrderFilmNum");
            }
        }
        private string _OrderFilmNum;
        
        [DataMemberAttribute()]
        public byte TotalFilmNum
        {
            get
            {
                return _TotalFilmNum;
            }
            set
            {
                if (_TotalFilmNum == value)
                {
                    return;
                }
                _TotalFilmNum = value;
                RaisePropertyChanged("TotalFilmNum");
            }
        }
        private byte _TotalFilmNum;

        [DataMemberAttribute()]
        public Staff DeliverStaff
        {
            get
            {
                return _DeliverStaff;
            }
            set
            {
                if (_DeliverStaff == value)
                {
                    return;
                }
                _DeliverStaff = value;
                RaisePropertyChanged("DeliverStaff");
            }
        }
        private Staff _DeliverStaff;

        [DataMemberAttribute()]
        public Staff ReceiverStaff
        {
            get
            {
                return _ReceiverStaff;
            }
            set
            {
                if (_ReceiverStaff == value)
                {
                    return;
                }
                _ReceiverStaff = value;
                RaisePropertyChanged("ReceiverStaff");
            }
        }
        private Staff _ReceiverStaff;
        
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
