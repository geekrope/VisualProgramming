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
    /// Interaction logic for ConsoleOut.xaml
    /// </summary>
    public partial class ConsoleOut : UserControl, VisualCode
    {
        public ConsoleLog ConsoleLog;

        public double Tabulation = 0;

        public Grid InnerContent
        {
            get; private set;
        }

        public CodeBlock GetInnerCodeBlock()
        {
            return ConsoleLog;
        }

        public Grid GetInnerGrid()
        {
            return InnerContent;
        }

        private void FitContent()
        {
            if (Function != null && ConsoleOutput != null)
            {
                Function.Measure(new Size(double.MaxValue, double.MaxValue));
                ConsoleOutput.Measure(new Size(double.MaxValue, double.MaxValue));

                var width = Function.DesiredSize.Width + ConsoleOutput.DesiredSize.Width;

                this.Width = width;
                this.Height = MainWindow.DefaultHeight;
            }
        }
        public ConsoleOut()
        {
            ConsoleLog = new ConsoleLog(this, MainWindow.SelectedCodeBlock, MainWindow.Document);

            InnerContent = new Grid();

            InitializeComponent();

            InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
            InnerContent.Children.Add(this);

            MainWindow.ChangedSelection += (VisualCode code) =>
            {
                if (this != code)
                {
                    Selection.Background = new SolidColorBrush(Colors.White);
                }
            };

            FitContent();
        }

        private void Output_TextChanged(object sender, TextChangedEventArgs e)
        {
            FitContent();

            ConsoleLog.Value = ConsoleOutput.Text;

            MainWindow.OnUpdate();
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainWindow.Swap(this, e.Delta);
        }
    }
}
