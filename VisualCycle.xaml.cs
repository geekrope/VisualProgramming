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
    /// Interaction logic for VisualCycle.xaml
    /// </summary>
    public partial class VisualCycle : UserControl, VisualCode
    {
        public Cycle Cycle
        {
            get; private set;
        }

        public double Tabulation = 0;

        public Grid InnerContent
        {
            get; private set;
        }

        public CodeBlock GetInnerCodeBlock()
        {
            return Cycle;
        }

        public Grid GetInnerGrid()
        {
            return InnerContent;
        }

        private void FitContent()
        {
            if (Function != null && ConditionInput != null)
            {
                Function.Measure(new Size(double.MaxValue, double.MaxValue));
                ConditionInput.Measure(new Size(double.MaxValue, double.MaxValue));

                var width = Function.DesiredSize.Width + ConditionInput.DesiredSize.Width;

                this.Width = width;
                this.Height = MainWindow.DefaultHeight;
            }
        }
        public VisualCycle()
        {
            Cycle = new Cycle(this, MainWindow.SelectedCodeBlock, MainWindow.Document);

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

        private void ConditionInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            FitContent();
            Cycle.Condition = ConditionInput.Text;
            MainWindow.OnUpdate();
        }

        private void Select_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (MainWindow.SelectedItem != this)
                {
                    MainWindow.SelectedItem = this;
                    Selection.Background = new SolidColorBrush(Color.FromArgb(30, Colors.DodgerBlue.R, Colors.DodgerBlue.G, Colors.DodgerBlue.B));
                }
                else
                {
                    MainWindow.SelectedItem = null;
                    Selection.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                MainWindow.RemoveItem(this);
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainWindow.Swap(this, e.Delta);
        }
    }
}
