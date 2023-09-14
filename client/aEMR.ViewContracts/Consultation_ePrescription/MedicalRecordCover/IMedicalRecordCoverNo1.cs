

namespace aEMR.ViewContracts
{
    public interface IMedicalRecordCoverNo1
    {
        void InitPatientInfo();
        IRegistration_DataStorage Registration_DataStorage { get; set; }
    
    }
}