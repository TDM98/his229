using eHCMS.Services.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataEntities
{
    public partial class MedicalRecordCoverSample2 : EntityBase
    {
        #region Primitive Properties
        [DataMemberAttribute()]
        public long MedicalRecordCoverSample2ID
        {
            get
            {
                return _MedicalRecordCoverSample2ID;
            }
            set
            {
                if (_MedicalRecordCoverSample2ID == value)
                {
                    return;
                }
                _MedicalRecordCoverSample2ID = value;
                RaisePropertyChanged("MedicalRecordCoverSample2ID");
            }
        }
        private long _MedicalRecordCoverSample2ID;

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
        public string ReasonHospitalStay
        {
            get
            {
                return _ReasonHospitalStay;
            }
            set
            {
                if (_ReasonHospitalStay == value)
                {
                    return;
                }
                _ReasonHospitalStay = value;
                RaisePropertyChanged("ReasonHospitalStay");
            }
        }
        private string _ReasonHospitalStay;

        [DataMemberAttribute()]
        public int DayOfIllness
        {
            get
            {
                return _DayOfIllness;
            }
            set
            {
                if (_DayOfIllness == value)
                {
                    return;
                }
                _DayOfIllness = value;
                RaisePropertyChanged("DayOfIllness");
            }
        }
        private int _DayOfIllness;

        [DataMemberAttribute()]
        public int NumberOfChild
        {
            get
            {
                return _NumberOfChild;
            }
            set
            {
                if (_NumberOfChild == value)
                {
                    return;
                }
                _NumberOfChild = value;
                RaisePropertyChanged("NumberOfChild");
            }
        }
        private int _NumberOfChild;

        [DataMemberAttribute()]
        public int Para
        {
            get
            {
                return _Para;
            }
            set
            {
                if (_Para == value)
                {
                    return;
                }
                _Para = value;
                RaisePropertyChanged("Para");
            }
        }
        private int _Para;

        [DataMemberAttribute()]
        public Lookup V_ConditionAtBirth
        {
            get
            {
                return _V_ConditionAtBirth;
            }
            set
            {
                if (_V_ConditionAtBirth == value)
                {
                    return;
                }
                _V_ConditionAtBirth = value;
                RaisePropertyChanged("V_ConditionAtBirth");
            }
        }
        private Lookup _V_ConditionAtBirth;

        [DataMemberAttribute]
        public float Weight
        {
            get
            {
                return _Weight;
            }
            set
            {
                if (_Weight == value)
                {
                    return;
                }
                _Weight = value;
                RaisePropertyChanged("Weight");
            }
        }
        private float _Weight;
         
        [DataMemberAttribute]
        public bool IsBirthDefects
        {
            get
            {
                return _IsBirthDefects;
            }
            set
            {
                if (_IsBirthDefects == value)
                {
                    return;
                }
                _IsBirthDefects = value;
                RaisePropertyChanged("IsBirthDefects");
            }
        }
        private bool _IsBirthDefects;

        [DataMemberAttribute()]
        public string NoteBirthDefects
        {
            get
            {
                return _NoteBirthDefects;
            }
            set
            {
                if (_NoteBirthDefects == value)
                {
                    return;
                }
                _NoteBirthDefects = value;
                RaisePropertyChanged("NoteBirthDefects");
            }
        }
        private string _NoteBirthDefects;

        [DataMemberAttribute()]
        public string MentalDevelopment
        {
            get
            {
                return _MentalDevelopment;
            }
            set
            {
                if (_MentalDevelopment == value)
                {
                    return;
                }
                _MentalDevelopment = value;
                RaisePropertyChanged("MentalDevelopment");
            }
        }
        private string _MentalDevelopment;

        [DataMemberAttribute()]
        public string MovementDevelopment
        {
            get
            {
                return _MovementDevelopment;
            }
            set
            {
                if (_MovementDevelopment == value)
                {
                    return;
                }
                _MovementDevelopment = value;
                RaisePropertyChanged("MovementDevelopment");
            }
        }
        private string _MovementDevelopment;

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
        public Lookup V_Alimentation
        {
            get
            {
                return _V_Alimentation;
            }
            set
            {
                if (_V_Alimentation == value)
                {
                    return;
                }
                _V_Alimentation = value;
                RaisePropertyChanged("V_Alimentation");
            }
        }
        private Lookup _V_Alimentation;

        [DataMemberAttribute()]
        public int WeaningMonth
        {
            get
            {
                return _WeaningMonth;
            }
            set
            {
                if (_WeaningMonth == value)
                {
                    return;
                }
                _WeaningMonth = value;
                RaisePropertyChanged("WeaningMonth");
            }
        }
        private int _WeaningMonth;
        
        [DataMemberAttribute()]
        public Lookup V_TakeCare
        {
            get
            {
                return _V_TakeCare;
            }
            set
            {
                if (_V_TakeCare == value)
                {
                    return;
                }
                _V_TakeCare = value;
                RaisePropertyChanged("V_TakeCare");
            }
        }
        private Lookup _V_TakeCare;

        [DataMemberAttribute]
        public bool IsVaccinated_Tuberculosis
        {
            get
            {
                return _IsVaccinated_Tuberculosis;
            }
            set
            {
                if (_IsVaccinated_Tuberculosis == value)
                {
                    return;
                }
                _IsVaccinated_Tuberculosis = value;
                RaisePropertyChanged("IsVaccinated_Tuberculosis");
            }
        }
        private bool _IsVaccinated_Tuberculosis;

        [DataMemberAttribute]
        public bool IsVaccinated_Polio
        {
            get
            {
                return _IsVaccinated_Polio;
            }
            set
            {
                if (_IsVaccinated_Polio == value)
                {
                    return;
                }
                _IsVaccinated_Polio = value;
                RaisePropertyChanged("IsVaccinated_Polio");
            }
        }
        private bool _IsVaccinated_Polio;

        [DataMemberAttribute]
        public bool IsVaccinated_Measles
        {
            get
            {
                return _IsVaccinated_Measles;
            }
            set
            {
                if (_IsVaccinated_Measles == value)
                {
                    return;
                }
                _IsVaccinated_Measles = value;
                RaisePropertyChanged("IsVaccinated_Measles");
            }
        }
        private bool _IsVaccinated_Measles;

        [DataMemberAttribute]
        public bool IsVaccinated_WhoopingCough
        {
            get
            {
                return _IsVaccinated_WhoopingCough;
            }
            set
            {
                if (_IsVaccinated_WhoopingCough == value)
                {
                    return;
                }
                _IsVaccinated_WhoopingCough = value;
                RaisePropertyChanged("IsVaccinated_WhoopingCough");
            }
        }
        private bool _IsVaccinated_WhoopingCough;

        [DataMemberAttribute]
        public bool IsVaccinated_Tetanus
        {
            get
            {
                return _IsVaccinated_Tetanus;
            }
            set
            {
                if (_IsVaccinated_Tetanus == value)
                {
                    return;
                }
                _IsVaccinated_Tetanus = value;
                RaisePropertyChanged("IsVaccinated_Tetanus");
            }
        }
        private bool _IsVaccinated_Tetanus;

        [DataMemberAttribute]
        public bool IsVaccinated_Diphtheria
        {
            get
            {
                return _IsVaccinated_Diphtheria;
            }
            set
            {
                if (_IsVaccinated_Diphtheria == value)
                {
                    return;
                }
                _IsVaccinated_Diphtheria = value;
                RaisePropertyChanged("IsVaccinated_Diphtheria");
            }
        }
        private bool _IsVaccinated_Diphtheria;

        [DataMemberAttribute]
        public bool IsVaccinated_Other
        {
            get
            {
                return _IsVaccinated_Other;
            }
            set
            {
                if (_IsVaccinated_Other == value)
                {
                    return;
                }
                _IsVaccinated_Other = value;
                RaisePropertyChanged("IsVaccinated_Other");
            }
        }
        private bool _IsVaccinated_Other;

        [DataMemberAttribute()]
        public string Vaccinated_Other
        {
            get
            {
                return _Vaccinated_Other;
            }
            set
            {
                if (_Vaccinated_Other == value)
                {
                    return;
                }
                _Vaccinated_Other = value;
                RaisePropertyChanged("Vaccinated_Other");
            }
        }
        private string _Vaccinated_Other;

        [DataMemberAttribute()]
        public string FullBodyExamination
        {
            get
            {
                return _FullBodyExamination;
            }
            set
            {
                if (_FullBodyExamination == value)
                {
                    return;
                }
                _FullBodyExamination = value;
                RaisePropertyChanged("FullBodyExamination");
            }
        }
        private string _FullBodyExamination;

        [DataMemberAttribute()]
        public string CirculatoryExamination
        {
            get
            {
                return _CirculatoryExamination;
            }
            set
            {
                if (_CirculatoryExamination == value)
                {
                    return;
                }
                _CirculatoryExamination = value;
                RaisePropertyChanged("CirculatoryExamination");
            }
        }
        private string _CirculatoryExamination;

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
