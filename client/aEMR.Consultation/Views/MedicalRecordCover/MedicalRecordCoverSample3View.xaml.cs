using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
namespace aEMR.Consultation.Views.MedicalRecordCover
{
    public partial class MedicalRecordCoverSample3View : UserControl
    {
        public MedicalRecordCoverSample3View()
        {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
