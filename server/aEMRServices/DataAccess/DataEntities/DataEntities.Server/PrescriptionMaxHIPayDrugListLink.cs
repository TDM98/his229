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
    public partial class PrescriptionMaxHIPayDrugListLink : NotifyChangedBase
    {
        public PrescriptionMaxHIPayDrugListLink()
         : base()
        {

        }
        private PrescriptionMaxHIPayDrugListLink _tempPrescriptionMaxHIPayDrugListLink;
        public override bool Equals(object obj)
        {
            PrescriptionMaxHIPayDrugListLink info = obj as PrescriptionMaxHIPayDrugListLink;
            if (info == null)
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            return this.PrescriptionMaxHIPayDrugListLinkID > 0 && this.PrescriptionMaxHIPayDrugListLinkID == info.PrescriptionMaxHIPayDrugListLinkID;
        }

        public void BeginEdit()
        {
            _tempPrescriptionMaxHIPayDrugListLink = (PrescriptionMaxHIPayDrugListLink)this.MemberwiseClone();
        }

        public void CancelEdit()
        {
            if (null != _tempPrescriptionMaxHIPayDrugListLink)
                CopyFrom(_tempPrescriptionMaxHIPayDrugListLink);
            //_tempPatient = null;
        }

        public void EndEdit()
        {
        }

        public void CopyFrom(PrescriptionMaxHIPayDrugListLink p)
        {
            PropertyCopierHelper.CopyPropertiesTo(p, this);
        }

        [DataMemberAttribute]
        public long PrescriptionMaxHIPayDrugListLinkID
        {
            get
            {
                return _PrescriptionMaxHIPayDrugListLinkID;
            }
            set
            {
                if (_PrescriptionMaxHIPayDrugListLinkID != value)
                {
                    _PrescriptionMaxHIPayDrugListLinkID = value;
                    RaisePropertyChanged("PrescriptionMaxHIPayDrugListLinkID");
                }
            }
        }
        private long _PrescriptionMaxHIPayDrugListLinkID;

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
        public long GenMedProductID
        {
            get
            {
                return _GenMedProductID;
            }
            set
            {
                if (_GenMedProductID != value)
                {
                    _GenMedProductID = value;
                    RaisePropertyChanged("GenMedProductID");
                }
            }
        }
        private long _GenMedProductID;

        [DataMemberAttribute]
        public string Code
        {
            get
            {
                return _Code;
            }
            set
            {
                if (_Code != value)
                {
                    _Code = value;
                    RaisePropertyChanged("Code");
                }
            }
        }
        private string _Code;

        [DataMemberAttribute]
        public string BrandName
        {
            get
            {
                return _BrandName;
            }
            set
            {
                if (_BrandName != value)
                {
                    _BrandName = value;
                    RaisePropertyChanged("BrandName");
                }
            }
        }
        private string _BrandName;

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
