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
    public partial class PrescriptionMaxHIPayDrugList : NotifyChangedBase
    {
        public PrescriptionMaxHIPayDrugList()
         : base()
        {

        }
        private PrescriptionMaxHIPayDrugList _tempPrescriptionMaxHIPayDrugList;
        public override bool Equals(object obj)
        {
            PrescriptionMaxHIPayDrugList info = obj as PrescriptionMaxHIPayDrugList;
            if (info == null)
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            return this.PrescriptionMaxHIPayDrugListID > 0 && this.PrescriptionMaxHIPayDrugListID == info.PrescriptionMaxHIPayDrugListID;
        }

        public void BeginEdit()
        {
            _tempPrescriptionMaxHIPayDrugList = (PrescriptionMaxHIPayDrugList)this.MemberwiseClone();
        }

        public void CancelEdit()
        {
            if (null != _tempPrescriptionMaxHIPayDrugList)
                CopyFrom(_tempPrescriptionMaxHIPayDrugList);
            //_tempPatient = null;
        }

        public void EndEdit()
        {
        }

        public void CopyFrom(PrescriptionMaxHIPayDrugList p)
        {
            PropertyCopierHelper.CopyPropertiesTo(p, this);
        }

        [DataMemberAttribute]
        public long PrescriptionMaxHIPayDrugListID
        {
            get
            {
                return _PrescriptionMaxHIPayDrugListID;
            }
            set
            {
                if (_PrescriptionMaxHIPayDrugListID != value)
                {
                    _PrescriptionMaxHIPayDrugListID = value;
                    RaisePropertyChanged("PrescriptionMaxHIPayDrugListID");
                }
            }
        }
        private long _PrescriptionMaxHIPayDrugListID;

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
        public PrescriptionMaxHIPayGroup PrescriptionMaxHIPayGroup
        {
            get
            {
                return _PrescriptionMaxHIPayGroup;
            }
            set
            {
                if (_PrescriptionMaxHIPayGroup != value)
                {
                    _PrescriptionMaxHIPayGroup = value;
                    RaisePropertyChanged("PrescriptionMaxHIPayGroup");
                }
            }
        }
        private PrescriptionMaxHIPayGroup _PrescriptionMaxHIPayGroup;

        [DataMemberAttribute]
        public decimal? MaxHIPay
        {
            get
            {
                return _MaxHIPay;
            }
            set
            {
                if (_MaxHIPay != value)
                {
                    _MaxHIPay = value;
                    RaisePropertyChanged("MaxHIPay");
                }
            }
        }
        private decimal? _MaxHIPay;

        [DataMemberAttribute]
        public DateTime ValidDate
        {
            get
            {
                return _ValidDate;
            }
            set
            {
                if (_ValidDate != value)
                {
                    _ValidDate = value;
                    RaisePropertyChanged("ValidDate");
                }
            }
        }
        private DateTime _ValidDate;

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
        public ObservableCollection<PrescriptionMaxHIPayDrugListLink> DrugLists
        {
            get { return _DrugLists; }
            set
            {
                _DrugLists = value;
                RaisePropertyChanged("DrugLists");
            }
        }
        private ObservableCollection<PrescriptionMaxHIPayDrugListLink> _DrugLists;
    }
}
