using DataEntities;
using aEMR.Common.Collections;
using System.Collections.ObjectModel;

namespace aEMR.ViewContracts
{
    public interface ICheckStatusRequestInvoiceOutStandingTask
    {
        ObservableCollection<RequestDrugInwardClinicDept> RequestDruglist { get; set; }
        long V_MedProductType { get; set; }
        bool IsLoadFromSmallProcedure { get; set; }
        long PtRegDetailID { get; set; }
        void LoadStore();
    }
}