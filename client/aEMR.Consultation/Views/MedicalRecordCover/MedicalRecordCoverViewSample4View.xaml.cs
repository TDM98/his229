using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace aEMR.Consultation.Views.MedicalRecordCover
{
    public partial class MedicalRecordCoverSample4View : UserControl
    {
        public MedicalRecordCoverSample4View()
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
