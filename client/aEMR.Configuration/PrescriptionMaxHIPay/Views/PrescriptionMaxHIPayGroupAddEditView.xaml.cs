using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace aEMR.Configuration.PrescriptionMaxHIPay.Views
{
    [Export(typeof(PrescriptionMaxHIPayGroupAddEditView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PrescriptionMaxHIPayGroupAddEditView : UserControl
    {
        public PrescriptionMaxHIPayGroupAddEditView()
        {
            InitializeComponent();
        }
    }
}
