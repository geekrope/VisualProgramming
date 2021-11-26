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

    }

    public class CodeBlock
    {
        public Document Document
        {
            get; protected set;
        }

        public virtual void Compile()
        {

        }

        public CodeBlock(Document doc = null)
        {
            if (doc != null)
            {
                this.Document = doc;
            }
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
        public List<CodeBlock> InnerCode
        {
            get; private set;
        }
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

        public Document() : base(null)
        {
            Variables = new Dictionary<string, Parameter>();
            OriginalVariables = new Dictionary<string, Parameter>();
            InnerCode = new List<CodeBlock>();
        }
    }

    public class Cycle : CodeBlock
    {
        public string Condition
        {
            get; set;
        }
        public List<CodeBlock> InnerCode
        {
            get; private set;
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
        public Cycle(Document doc) : base(doc)
        {
            InnerCode = new List<CodeBlock>();
        }
    }

    public class If : CodeBlock
    {
        public string Condition
        {
            get; set;
        }
        public List<CodeBlock> InnerCode
        {
            get; private set;
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
        public If(Document doc) : base(doc)
        {
            InnerCode = new List<CodeBlock>();
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
    }

    public partial class MainWindow : Window
    {
        private List<VisualCode> VisualCodes = new List<VisualCode>();

        public const double Tab = 150;
        public const double BetweenMargin = 10;

        public const double DefaultHeight = 39;

        public static Document Document = new Document();

        public void AddCondition(VisualCode visualCode = null)
        {
            if (visualCode != null)
            {
                switch (visualCode)
                {
                    case Condition condition:
                        var conditionControl = new Condition();

                        Grid.SetRow(conditionControl.InnerContent, condition.If.InnerCode.Count + 1);
                        condition.InnerContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });

                        conditionControl.InnerContent.Margin = new Thickness(Tab, BetweenMargin, 0, 0);

                        VisualCodes.Add(conditionControl);
                        condition.If.InnerCode.Add(conditionControl.If);

                        condition.InnerContent.Children.Add(conditionControl.InnerContent);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                var conditionControl = new Condition();

                Grid.SetRow(conditionControl.InnerContent, VisualCodes.Count);

                Document.InnerCode.Add(conditionControl.If);

                conditionControl.InnerContent.Margin = new Thickness(0, BetweenMargin, 0, 0);

                VisualCodes.Add(conditionControl);

                Playground.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto), });
                Playground.Children.Add(conditionControl.InnerContent);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            AddCondition();
            AddCondition();
            AddCondition(VisualCodes[0]);

            Document.Compile();
        }
    }
}