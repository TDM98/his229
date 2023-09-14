using eHCMS.Services.Core;
using eHCMS.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    [DataContract]
    public partial class PrescriptionMaxHIPayGroup : NotifyChangedBase
    {
        public PrescriptionMaxHIPayGroup()
         : base()
        {

        }
        private PrescriptionMaxHIPayGroup _tempPrescriptionMaxHIPayGroup;
        public override bool Equals(object obj)
        {
            PrescriptionMaxHIPayGroup info = obj as PrescriptionMaxHIPayGroup;
            if (info == null)
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            return this.PrescriptionMaxHIPayGroupID > 0 && this.PrescriptionMaxHIPayGroupID == info.PrescriptionMaxHIPayGroupID;
        }

        public void BeginEdit()
        {
            _tempPrescriptionMaxHIPayGroup = (PrescriptionMaxHIPayGroup)this.MemberwiseClone();
        }

        public void CancelEdit()
        {
            if (null != _tempPrescriptionMaxHIPayGroup)
                CopyFrom(_tempPrescriptionMaxHIPayGroup);
            //_tempPatient = null;
        }

        public void EndEdit()
        {
        }

        public void CopyFrom(PrescriptionMaxHIPayGroup p)
        {
            PropertyCopierHelper.CopyPropertiesTo(p, this);
        }

        [DataMemberAttribute]
        public long PrescriptionMaxHIPayGroupID
        {
            get
            {
                return _PrescriptionMaxHIPayGroupID;
            }
            set
            {
                if (_PrescriptionMaxHIPayGroupID != value)
                {
                    _PrescriptionMaxHIPayGroupID = value;
                    RaisePropertyChanged("PrescriptionMaxHIPayGroupID");
                }
            }
        }
        private long _PrescriptionMaxHIPayGroupID;

        [DataMemberAttribute]
        public Lookup V_RegistrationType
        {
            get
            {
                return _V_RegistrationType;
            }
            set
            {
                if (_V_RegistrationType != value)
                {
                    _V_RegistrationType = value;
                    RaisePropertyChanged("V_RegistrationType");
                }
            }
        }
        private Lookup _V_RegistrationType;
        
        [DataMemberAttribute]
        public string GroupCode
        {
            get
            {
                return _GroupCode;
            }
            set
            {
                if (_GroupCode != value)
                {
                    _GroupCode = value;
                    RaisePropertyChanged("GroupCode");
                }
            }
        }
        private string _GroupCode;

        [DataMemberAttribute]
        public string GroupName
        {
            get
            {
                return _GroupName;
            }
            set
            {
                if (_GroupName != value)
                {
                    _GroupName = value;
                    RaisePropertyChanged("GroupName");
                }
            }
        }
        private string _GroupName;

        [DataMemberAttribute]
        public string Note
        {
            get
            {
                return _Note;
            }
            set
            {
                if (_Note != value)
                {
                    _Note = value;
                    RaisePropertyChanged("Note");
                }
            }
        }
        private string _Note;

        [DataMemberAttribute]
        public string Log
        {
            get
            {
                return _Log;
            }
            set
            {
                if (_Log != value)
                {
                    _Log = value;
                    RaisePropertyChanged("Log");
                }
            }
        }
        private string _Log;

        [DataMemberAttribute]
        public bool IsDeleted
        {
            get
            {
                return _IsDeleted;
            }
            set
            {
                if (_IsDeleted != value)
                {
                    _IsDeleted = value;
                    RaisePropertyChanged("IsDeleted");
                }
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
                if (_CreatedStaff != value)
                {
                    _CreatedStaff = value;
                    RaisePropertyChanged("CreatedStaff");
                }
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
                if (_CreatedDate != value)
                {
                    _CreatedDate = value;
                    RaisePropertyChanged("CreatedDate");
                }
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
                if (_LastUpdateStaff != value)
                {
                    _LastUpdateStaff = value;
                    RaisePropertyChanged("LastUpdateStaff");
                }
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
                if (_LastUpdateDate != value)
                {
                    _LastUpdateDate = value;
                    RaisePropertyChanged("LastUpdateDate");
                }
            }
        }
        private DateTime _LastUpdateDate;

        [DataMemberAttribute()]
        public ObservableCollection<PrescriptionMaxHIPayLinkICD> ListICD10Code
        {
            get { return _ListICD10Code; }
            set
            {
                _ListICD10Code = value;
                RaisePropertyChanged("ListICD10Code");
            }
        }
        private ObservableCollection<PrescriptionMaxHIPayLinkICD> _ListICD10Code;

        [DataMemberAttribute()]
        public ObservableCollection<PrescriptionMaxHIPayDrugList> PrescriptionMaxHIPayDrugLists
        {
            get { return _PrescriptionMaxHIPayDrugLists; }
            set
            {
                _PrescriptionMaxHIPayDrugLists = value;
                RaisePropertyChanged("PrescriptionMaxHIPayDrugLists");
            }
        }
        private ObservableCollection<PrescriptionMaxHIPayDrugList> _PrescriptionMaxHIPayDrugLists;
    }
}
