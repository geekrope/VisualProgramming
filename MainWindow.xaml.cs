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
using MathParser2;

namespace VisualProgramming
{
    public interface VisualCode
    {
        public CodeBlock GetInnerCodeBlock();
        public Grid GetInnerGrid();
    }

    public class CodeBlock
    {
        public Document Document
        {
            get; protected set;
        }

        public CodeBlock Parent
        {
            get; private set;
        }

        public List<CodeBlock> InnerCode
        {
            get; protected set;
        }

        public VisualCode Container
        {
            get; private set;
        }

        public virtual void Compile()
        {

        }

        public CodeBlock(VisualCode container = null, CodeBlock parent = null, Document doc = null)
        {
            Parent = parent;
            Document = doc;
            Container = container;
        }
    }

    public class VisualBlock
    {
        public Action OnUpdate
        {
            get; set;
        }
    }

    public class Document : CodeBlock
    {
        private Dictionary<string, Parameter> OriginalVariables
        {
            get; set;
        }

        public Dictionary<string, Parameter> Variables
        {
            get; private set;
        }

        public void SetValue(string varName, Literal value)
        {
            if (Variables.ContainsKey(varName))
            {
                Variables[varName].Value = value;
            }
            else
            {
                OriginalVariables.Add(varName, new Parameter(varName, value));
                Variables.Add(varName, new Parameter(varName, value));
            }
        }
        public override void Compile()
        {
            Variables = OriginalVariables.ToDictionary(entry => entry.Key, entry => new Parameter(entry.Value.Name, entry.Value.Value));
            foreach (var block in InnerCode)
            {
                block.Compile();
            }
        }

        public Document() : base(null, null)
        {
            Variables = new Dictionary<string, Parameter>();
            OriginalVariables = new Dictionary<string, Parameter>();
            InnerCode = new List<CodeBlock>();
        }

        public override string ToString()
        {
            var innerCode = "";

            for (int index = 0; index < InnerCode.Count; index++)
            {
                innerCode += InnerCode[index].ToString();
                if (index < InnerCode.Count - 1)
                {
                    innerCode += "\n";
                }
            }

            return innerCode;
        }
    }

    public class Cycle : CodeBlock
    {
        public string Condition
        {
            get; set;
        }

        public override void Compile()
        {
            var parsedCondition = MathParser.Parse(Condition, this.Document.Variables.Values.ToList());

            while ((BooleanLiteral)MathParser.EvaluateOperand(parsedCondition))
            {
                foreach (var block in InnerCode)
                {
                    block.Compile();
                }
            }
        }
        public Cycle(VisualCode container, CodeBlock parent, Document doc) : base(container, parent, doc)
        {
            InnerCode = new List<CodeBlock>();
        }
        public override string ToString()
        {
            var innerCode = "";
            foreach (var code in InnerCode)
            {
                innerCode += "\n" + code.ToString();
            }
            return "while(" + Condition + ")\n{" + innerCode + "\n}";
        }
    }

    public class If : CodeBlock
    {
        public string Condition
        {
            get; set;
        }

        public override void Compile()
        {
            var parsedCondition = MathParser.Parse(Condition, this.Document.Variables.Values.ToList());

            if ((BooleanLiteral)MathParser.EvaluateOperand(parsedCondition))
            {
                foreach (var block in InnerCode)
                {
                    block.Compile();
                }
            }
        }
        public If(VisualCode container, CodeBlock parent, Document doc) : base(container, parent, doc)
        {
            InnerCode = new List<CodeBlock>();
        }
        public override string ToString()
        {
            var innerCode = "";
            foreach (var code in InnerCode)
            {
                innerCode += "\n" + code.ToString();
            }
            return "if(" + Condition + ")\n{" + innerCode + "\n}";
        }
    }

    public class ParameterAssignment : CodeBlock
    {
        public string ParameterName
        {
            get; set;
        }
        public string Value
        {
            get; set;
        }

        public override void Compile()
        {
            var parsedValue = MathParser.Parse(Value, this.Document.Variables.Values.ToList());
            this.Document.SetValue(ParameterName, MathParser.EvaluateOperand(parsedValue));
        }
        public ParameterAssignment(VisualCode container, CodeBlock parent, Document doc) : base(container, parent, doc)
        {

        }
        public override string ToString()
        {
            return ParameterName + "=" + Value;
        }
    }

    public class ConsoleLog : CodeBlock
    {
        public string Value
        {
            get; set;
        }

        public override void Compile()
        {
            var parsedValue = MathParser.Parse(Value, this.Document.Variables.Values.ToList());
            MainWindow.ConsoleOutput += "\n" + MathParser.EvaluateOperand(parsedValue);
        }
        public ConsoleLog(VisualCode container, CodeBlock parent, Document doc) : base(container, parent, doc)
        {

        }
        public override string ToString()
        {
            return "console.log(" + Value + ")";
        }
    }

    public partial class MainWindow : Window
    {
        private static VisualCode selectedItem = null;
        private static List<VisualCode> VisualCodes
        {
            get; set;
        }
        private static string consoleOutput = "";
        private static Action OnConsoleChanged;

        private const double Tab = 150;
        private const double BetweenMargin = 10;

        public const double DefaultHeight = 55;

        public static string ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnConsoleChanged();
            }
        }

        public static Document Document = new Document();

        public static VisualCode SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                ChangedSelection?.Invoke(value);
            }
        }

        public static CodeBlock SelectedCodeBlock
        {
            get
            {
                switch (SelectedItem)
                {
                    case Condition condition:
                        return condition.If;
                    case VisualCycle cycle:
                        return cycle.Cycle;
                    case VisualParameterAssignment parameter:
                        return parameter.ParameterAssignment;
                    default:
                        return Document;
                }
            }
        }

        public static Action<VisualCode> ChangedSelection;

        public static void Swap(VisualCode element, int delta)
        {
            var row = Grid.GetRow(element.GetInnerGrid());

            var index = VisualCodes.IndexOf(element);

            var codeBlock = element.GetInnerCodeBlock();
            var codeBlockIndex = codeBlock.Parent.InnerCode.IndexOf(codeBlock);

            if (delta < 0)
            {
                if (codeBlockIndex - 1 >= 0)
                {
                    var elementToSwap = codeBlock.Parent.InnerCode[codeBlockIndex - 1].Container;

                    Grid.SetRow(VisualCodes[index].GetInnerGrid(), row - 1);
                    Grid.SetRow(elementToSwap.GetInnerGrid(), row);

                    var swapElement = VisualCodes[index];

                    VisualCodes[index] = elementToSwap;
                    VisualCodes[VisualCodes.IndexOf(elementToSwap)] = swapElement;

                    codeBlock.Parent.InnerCode[codeBlockIndex] = codeBlock.Parent.InnerCode[codeBlockIndex - 1];
                    codeBlock.Parent.InnerCode[codeBlockIndex - 1] = codeBlock;
                }
            }
            else
            {
                if (codeBlockIndex + 1 < codeBlock.Parent.InnerCode.Count)
                {
                    var elementToSwap = codeBlock.Parent.InnerCode[codeBlockIndex + 1].Container;

                    Grid.SetRow(VisualCodes[index].GetInnerGrid(), row + 1);
                    Grid.SetRow(elementToSwap.GetInnerGrid(), row);

                    var swapElement = VisualCodes[index];

                    VisualCodes[index] = elementToSwap;
                    VisualCodes[VisualCodes.IndexOf(elementToSwap)] = swapElement;

                    codeBlock.Parent.InnerCode[codeBlockIndex] = codeBlock.Parent.InnerCode[codeBlockIndex + 1];
                    codeBlock.Parent.InnerCode[codeBlockIndex + 1] = codeBlock;
                }
            }

            OnUpdate();
        }

        private void AddCondition(VisualCode visualCode = null)
        {
            if (visualCode != null)
            {
                Action<VisualCode> addCondition = (VisualCode code) =>
                   {
                       var conditionControl = new Condition();

                       conditionControl.InnerContent.Margin = new Thickness(Tab, BetweenMargin, 0, 0);

                       switch (visualCode)
                       {
                           case VisualCycle cycle:
                               Grid.SetRow(conditionControl.InnerContent, cycle.Cycle.InnerCode.Count + 1);
                               cycle.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                               cycle.Cycle.InnerCode.Add(conditionControl.If);

                               cycle.InnerContent.Children.Add(conditionControl.InnerContent);
                               break;
                           case Condition condition:

                               Grid.SetRow(conditionControl.InnerContent, condition.If.InnerCode.Count + 1);
                               condition.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                               condition.If.InnerCode.Add(conditionControl.If);

                               condition.InnerContent.Children.Add(conditionControl.InnerContent);
                               break;
                       }

                       VisualCodes.Add(conditionControl);
                   };

                addCondition(visualCode);
            }
            else
            {
                var conditionControl = new Condition();

                Grid.SetRow(conditionControl.InnerContent, Document.InnerCode.Count);

                Document.InnerCode.Add(conditionControl.If);

                conditionControl.InnerContent.Margin = new Thickness(0, BetweenMargin, 0, 0);

                VisualCodes.Add(conditionControl);

                Playground.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
                Playground.Children.Add(conditionControl.InnerContent);
            }
        }

        private void AddParameterAssignment(VisualCode visualCode = null)
        {
            if (visualCode != null)
            {
                Action<VisualCode> addCondition = (VisualCode code) =>
                {
                    var parameterControl = new VisualParameterAssignment();

                    parameterControl.InnerContent.Margin = new Thickness(Tab, BetweenMargin, 0, 0);

                    switch (visualCode)
                    {
                        case VisualCycle cycle:
                            Grid.SetRow(parameterControl.InnerContent, cycle.Cycle.InnerCode.Count + 1);
                            cycle.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                            cycle.Cycle.InnerCode.Add(parameterControl.ParameterAssignment);

                            cycle.InnerContent.Children.Add(parameterControl.InnerContent);
                            break;
                        case Condition condition:

                            Grid.SetRow(parameterControl.InnerContent, condition.If.InnerCode.Count + 1);
                            condition.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                            condition.If.InnerCode.Add(parameterControl.ParameterAssignment);

                            condition.InnerContent.Children.Add(parameterControl.InnerContent);
                            break;
                    }

                    VisualCodes.Add(parameterControl);
                };

                addCondition(visualCode);
            }
            else
            {
                var parameterControl = new VisualParameterAssignment();

                Grid.SetRow(parameterControl.InnerContent, Document.InnerCode.Count);

                Document.InnerCode.Add(parameterControl.ParameterAssignment);

                parameterControl.InnerContent.Margin = new Thickness(0, BetweenMargin, 0, 0);

                VisualCodes.Add(parameterControl);

                Playground.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
                Playground.Children.Add(parameterControl.InnerContent);
            }
        }

        private void AddCycle(VisualCode visualCode = null)
        {
            if (visualCode != null)
            {
                Action<VisualCode> addCondition = (VisualCode code) =>
                {
                    var cycleControl = new VisualCycle();

                    cycleControl.InnerContent.Margin = new Thickness(Tab, BetweenMargin, 0, 0);

                    switch (visualCode)
                    {
                        case VisualCycle cycle:
                            Grid.SetRow(cycleControl.InnerContent, cycle.Cycle.InnerCode.Count + 1);
                            cycle.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                            cycle.Cycle.InnerCode.Add(cycleControl.Cycle);

                            cycle.InnerContent.Children.Add(cycleControl.InnerContent);
                            break;
                        case Condition condition:

                            Grid.SetRow(cycleControl.InnerContent, condition.If.InnerCode.Count + 1);
                            condition.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                            condition.If.InnerCode.Add(cycleControl.Cycle);

                            condition.InnerContent.Children.Add(cycleControl.InnerContent);
                            break;
                    }

                    VisualCodes.Add(cycleControl);
                };

                addCondition(visualCode);
            }
            else
            {
                var cycleControl = new VisualCycle();

                Grid.SetRow(cycleControl.InnerContent, Document.InnerCode.Count);

                Document.InnerCode.Add(cycleControl.Cycle);

                cycleControl.InnerContent.Margin = new Thickness(0, BetweenMargin, 0, 0);

                VisualCodes.Add(cycleControl);

                Playground.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
                Playground.Children.Add(cycleControl.InnerContent);
            }
        }

        private void AddConsoleLog(VisualCode visualCode = null)
        {
            if (visualCode != null)
            {
                Action<VisualCode> addCondition = (VisualCode code) =>
                {
                    var consoleControl = new ConsoleOut();

                    consoleControl.InnerContent.Margin = new Thickness(Tab, BetweenMargin, 0, 0);

                    switch (visualCode)
                    {
                        case VisualCycle cycle:
                            Grid.SetRow(consoleControl.InnerContent, cycle.Cycle.InnerCode.Count + 1);
                            cycle.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                            cycle.Cycle.InnerCode.Add(consoleControl.ConsoleLog);

                            cycle.InnerContent.Children.Add(consoleControl.InnerContent);
                            break;
                        case Condition condition:

                            Grid.SetRow(consoleControl.InnerContent, condition.If.InnerCode.Count + 1);
                            condition.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                            condition.If.InnerCode.Add(consoleControl.ConsoleLog);

                            condition.InnerContent.Children.Add(consoleControl.InnerContent);
                            break;
                    }

                    VisualCodes.Add(consoleControl);
                };

                addCondition(visualCode);
            }
            else
            {
                var consoleControl = new ConsoleOut();

                Grid.SetRow(consoleControl.InnerContent, Document.InnerCode.Count);

                Document.InnerCode.Add(consoleControl.ConsoleLog);

                consoleControl.InnerContent.Margin = new Thickness(0, BetweenMargin, 0, 0);

                VisualCodes.Add(consoleControl);

                Playground.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
                Playground.Children.Add(consoleControl.InnerContent);
            }
        }

        public static Action OnUpdate;

        public void UpdateText()
        {
            PlainText.Content = Document.ToString();
        }

        public MainWindow()
        {
            InitializeComponent();

            VisualCodes = new List<VisualCode>();

            Document.Compile();

            OnUpdate += UpdateText;
            OnConsoleChanged += () =>
            {
                Output.Text = ConsoleOutput;
            };
        }

        private void _AddCondition_Click(object sender, RoutedEventArgs e)
        {
            AddCondition(SelectedItem);
            OnUpdate();
        }

        private void _AddCycle_Click(object sender, RoutedEventArgs e)
        {
            AddCycle(SelectedItem);
            OnUpdate();
        }

        private void _AddParameter_Click(object sender, RoutedEventArgs e)
        {
            AddParameterAssignment(SelectedItem);
            OnUpdate();
        }

        private void _AddConsoleLog_Click(object sender, RoutedEventArgs e)
        {
            AddConsoleLog(SelectedItem);
            OnUpdate();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Compile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ConsoleOutput = "";

            try
            {
                Document.Compile();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "error");
                return;
            }
        }
    }
}