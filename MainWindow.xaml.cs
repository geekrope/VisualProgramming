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
        public static Document Document = new Document();
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}