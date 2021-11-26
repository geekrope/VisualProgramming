using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualProgramming
{
    /// <summary>
    /// Interaction logic for Condition.xaml
    /// </summary>
    public partial class Condition : UserControl
    {
        public If If
        {
            get; private set;
        }

        const double MarginX = 10;
        private void FitContent()
        {
            Function.Measure(new Size(double.MaxValue, double.MaxValue));
            ConditionInput.Measure(new Size(double.MaxValue, double.MaxValue));

            var width = Function.DesiredSize.Width + ConditionInput.DesiredSize.Width;

            this.Width = width;
            this.Height = 39;
        }

        public Action AddedInnerCode;

        public Condition()
        {
            If = new If(MainWindow.Document);

            InitializeComponent();

            FitContent();            
        }

        private void Condition_TextChanged(object sender, TextChangedEventArgs e)
        {
            FitContent();

            If.Condition = ConditionInput.Text;
        }
    }
}
