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
    /// Interaction logic for VisualParameterAssignment.xaml
    /// </summary>
    public partial class VisualParameterAssignment : UserControl, VisualCode
    {
        public ParameterAssignment ParameterAssignment
        {
            get; private set;
        }

        public double Tabulation = 0;

        public Grid InnerContent
        {
            get; private set;
        }

        private void FitContent()
        {
            if (Function != null && ParameterValue != null && ParameterName != null)
            {
                Function.Measure(new Size(double.MaxValue, double.MaxValue));
                ParameterValue.Measure(new Size(double.MaxValue, double.MaxValue));
                ParameterName.Measure(new Size(double.MaxValue, double.MaxValue));

                var width = Function.DesiredSize.Width + ParameterValue.DesiredSize.Width + ParameterName.DesiredSize.Width;

                this.Width = width;
                this.Height = MainWindow.DefaultHeight;
            }
        }

        public CodeBlock GetInnerCodeBlock()
        {
            return ParameterAssignment;
        }

        public Grid GetInnerGrid()
        {
            return InnerContent;
        }

        public VisualParameterAssignment()
        {
            ParameterAssignment = new ParameterAssignment(this, MainWindow.SelectedCodeBlock, MainWindow.Document);

            InnerContent = new Grid();

            InitializeComponent();

            InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
            InnerContent.Children.Add(this);

            FitContent();
        }

        private void ParameterName_TextChanged(object sender, TextChangedEventArgs e)
        {
            FitContent();
            ParameterAssignment.ParameterName = ParameterName.Text;
            MainWindow.OnUpdate();
        }

        private void ParameterValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            FitContent();
            ParameterAssignment.Value = ParameterValue.Text;
            MainWindow.OnUpdate();
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainWindow.Swap(this, e.Delta);
        }
    }
}
