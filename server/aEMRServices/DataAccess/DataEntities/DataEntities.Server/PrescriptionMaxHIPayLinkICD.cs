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
    public partial class PrescriptionMaxHIPayLinkICD : EntityBase
    {
        public PrescriptionMaxHIPayLinkICD()
         : base()
        {

        }
        private PrescriptionMaxHIPayLinkICD _tempPrescriptionMaxHIPayLinkICD;
        public override bool Equals(object obj)
        {
            PrescriptionMaxHIPayLinkICD info = obj as PrescriptionMaxHIPayLinkICD;
            if (info == null)
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            return this.PrescriptionMaxHIPayLinkICDID > 0 && this.PrescriptionMaxHIPayLinkICDID == info.PrescriptionMaxHIPayLinkICDID;
        }

        public void BeginEdit()
        {
            _tempPrescriptionMaxHIPayLinkICD = (PrescriptionMaxHIPayLinkICD)this.MemberwiseClone();
        }

        public void CancelEdit()
        {
            if (null != _tempPrescriptionMaxHIPayLinkICD)
                CopyFrom(_tempPrescriptionMaxHIPayLinkICD);
            //_tempPatient = null;
        }

        public void EndEdit()
        {
        }

        public void CopyFrom(PrescriptionMaxHIPayLinkICD p)
        {
            PropertyCopierHelper.CopyPropertiesTo(p, this);
        }

        [DataMemberAttribute]
        public long PrescriptionMaxHIPayLinkICDID
        {
            get
            {
                return _PrescriptionMaxHIPayLinkICDID;
            }
            set
            {
                if (_PrescriptionMaxHIPayLinkICDID != value)
                {
                    _PrescriptionMaxHIPayLinkICDID = value;
                    RaisePropertyChanged("PrescriptionMaxHIPayLinkICDID");
                }
            }
        }
        private long _PrescriptionMaxHIPayLinkICDID;

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
        public long IDCode
        {
            get
            {
                return _IDCode;
            }
            set
            {
                if (_IDCode != value)
                {
                    _IDCode = value;
                    RaisePropertyChanged("IDCode");
                }
            }
        }
        private long _IDCode;

        [DataMemberAttribute]
        public string ICD10
        {
            get
            {
                return _ICD10;
            }
            set
            {
                if (_ICD10 != value)
                {
                    _ICD10 = value;
                    RaisePropertyChanged("ICD10");
                }
            }
        }
        private string _ICD10;

        [DataMemberAttribute]
        public string DiseaseNameVN
        {
            get
            {
                return _DiseaseNameVN;
            }
            set
            {
                if (_DiseaseNameVN != value)
                {
                    _DiseaseNameVN = value;
                    RaisePropertyChanged("DiseaseNameVN");
                }
            }
        }
        private string _DiseaseNameVN;

        [DataMemberAttribute]
        public string DiseaseDescription
        {
            get
            {
                return _DiseaseDescription;
            }
            set
            {
                if (_DiseaseDescription != value)
                {
                    _DiseaseDescription = value;
                    RaisePropertyChanged("DiseaseDescription");
                }
            }
        }
        private string _DiseaseDescription;

        [DataMemberAttribute]
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }
        private bool _IsActive;
        
        [DataMemberAttribute]
        public bool IsNewInYear
        {
            get
            {
                return _IsNewInYear;
            }
            set
            {
                if (_IsNewInYear != value)
                {
                    _IsNewInYear = value;
                    RaisePropertyChanged("IsNewInYear");
                }
            }
        }
        private bool _IsNewInYear;

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
        public Staff DeletedStaff
        {
            get
            {
                return _DeletedStaff;
            }
            set
            {
                if (_DeletedStaff != value)
                {
                    _DeletedStaff = value;
                    RaisePropertyChanged("DeletedStaff");
                }
            }
        }
        private Staff _DeletedStaff;

        [DataMemberAttribute]
        public DateTime DeletedDate
        {
            get
            {
                return _DeletedDate;
            }
            set
            {
                if (_DeletedDate != value)
                {
                    _DeletedDate = value;
                    RaisePropertyChanged("DeletedDate");
                }
            }
        }
        private DateTime _DeletedDate;
    }
}
