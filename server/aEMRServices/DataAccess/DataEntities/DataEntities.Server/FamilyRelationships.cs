using eHCMS.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public partial class FamilyRelationships : EntityBase
    {
        #region Primitive Properties
        [DataMemberAttribute()]
        public long FamilyRelationshipID
        {
            get
            {
                return _FamilyRelationshipID;
            }
            set
            {
                if (_FamilyRelationshipID == value)
                {
                    return;
                }
                _FamilyRelationshipID = value;
                RaisePropertyChanged("FamilyRelationshipID");
            }
        }
        private long _FamilyRelationshipID;

        [DataMemberAttribute()]
        public long PatientID
        {
            get
            {
                return _PatientID;
            }
            set
            {
                if (_PatientID == value)
                {
                    return;
                }
                _PatientID = value;
                RaisePropertyChanged("PatientID");
            }
        }
        private long _PatientID;

        [DataMemberAttribute()]
        public string FFullName
        {
            get
            {
                return _FFullName;
            }
            set
            {
                if (_FFullName == value)
                {
                    return;
                }
                _FFullName = value;
                RaisePropertyChanged("FFullName");
            }
        }
        private string _FFullName;

        [DataMemberAttribute()]
        public string FCulturalLevel
        {
            get
            {
                return _FCulturalLevel;
            }
            set
            {
                if (_FCulturalLevel == value)
                {
                    return;
                }
                _FCulturalLevel = value;
                RaisePropertyChanged("FCulturalLevel");
            }
        }
        private string _FCulturalLevel;

        [DataMemberAttribute()]
        public string FOccupation
        {
            get
            {
                return _FOccupation;
            }
            set
            {
                if (_FOccupation == value)
                {
                    return;
                }
                _FOccupation = value;
                RaisePropertyChanged("FOccupation");
            }
        }
        private string _FOccupation;

        [DataMemberAttribute()]
        public Lookup V_FamilyRelationship
        {
            get
            {
                return _V_FamilyRelationship;
            }
            set
            {
                if (_V_FamilyRelationship == value)
                {
                    return;
                }
                _V_FamilyRelationship = value;
                RaisePropertyChanged("V_FamilyRelationship");
            }
        }
        private Lookup _V_FamilyRelationship;

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
