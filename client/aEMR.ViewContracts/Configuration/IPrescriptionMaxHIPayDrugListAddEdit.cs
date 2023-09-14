using DataEntities;
using System.Collections.ObjectModel;

namespace aEMR.ViewContracts.Configuration
{
    public interface IPrescriptionMaxHIPayDrugListAddEdit
    {
        string TitleForm { get; set; }
        bool IsEdit { get; set; }
        PrescriptionMaxHIPayDrugList ObjPrescriptionMaxHIPayDrugList_Current { get; set; }
        void InitializeNewItem();
    }
}
