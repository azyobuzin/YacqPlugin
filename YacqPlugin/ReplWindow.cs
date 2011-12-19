using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Mystique;
using XSpect.Yacq;
using XSpect.Yacq.Expressions;

namespace YacqPlugin
{
    partial class ReplWindow : Window
    {
        public ReplWindow()
        {
            this.Width = 450;
            this.Height = 350;
            this.Title = "YACQ Console";
            this.Content = this.textBox;
            this.textBox.AcceptsReturn = true;
            this.textBox.BorderThickness = new Thickness(0);
            this.textBox.FontFamily = new FontFamily("Consolas");
            this.textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            this.textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.textBox.TextWrapping = TextWrapping.Wrap;
            this.textBox.Text = string.Format("YACQ {0} on Krile {1}\r\n", YacqServices.Version, typeof(App).Assembly.GetName().Version);
            this.textBox.Select(this.textBox.Text.Length, 0);
            this.textBox.PreviewKeyDown += this.textBox_PreviewKeyDown;
            this.symbolTable = new SymbolTable(typeof(Symbols))
            {
                {"*textbox*", YacqExpression.Constant(textBox)},
            };
            var rcPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "yacq_lib\\rc.yacq"
            );
            if(File.Exists(rcPath))
            {
                YacqServices.ParseAll(this.symbolTable, File.ReadAllText(rcPath))
                    .ForEach(e => YacqExpression.Lambda(e).Compile().DynamicInvoke());
                this.textBox.AppendText("rc.yacq was loaded.\r\n");
            }
            this.textBox.AppendText(">>> ");
        }

        private readonly TextBox textBox = new TextBox();

        private readonly LinkedList<string> history = new LinkedList<string>();
        private LinkedListNode<string> historyPos = null;

        private readonly SymbolTable symbolTable;

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    var previous = this.historyPos != null
                        ? this.historyPos.Previous
                        : this.history.Last;
                    if (previous != null)
                    {
                        var lines = this.textBox.Text.Split('\n');
                        lines[lines.Length - 1] = ">>> " + previous.Value;
                        this.textBox.Text = string.Join("\n", lines);
                        this.textBox.Select(this.textBox.Text.Length, 0);
                        this.historyPos = previous;
                    }
                    e.Handled = true;
                    break;
                case Key.Down:
                    if (this.historyPos != null && this.historyPos.Next != null)
                    {
                        this.historyPos = this.historyPos.Next;
                        var lines = this.textBox.Text.Split('\n');
                        lines[lines.Length - 1] = ">>> " + this.historyPos.Value;
                        this.textBox.Text = string.Join("\n", lines);
                        this.textBox.Select(this.textBox.Text.Length, 0);
                    }
                    e.Handled = true;
                    break;
                case Key.Left:
                case Key.Back:
                    e.Handled = this.textBox.SelectionStart <=
                        (this.textBox.Text
                        .Split('\n')
                        .Select(s => s.Length + 1)
                        .Reverse()
                        .Skip(1)
                        .Sum()
                        + ">>> ".Length);
                    break;
                case Key.Enter:
                    var code = this.textBox.Text.Split('\n').Last().Substring(">>> ".Length);
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        this.history.AddLast(code);
                        if (this.historyPos != null && this.historyPos.Value != code)
                            this.historyPos = null;
                        this.textBox.AppendText("\r\n");
                        try
                        {
                            var exp = YacqExpression.Lambda(
                                YacqServices.Parse(this.symbolTable, code)
                            );
                            var ret = exp.Compile().DynamicInvoke();
                            if (ret == null)
                            {
                                this.textBox.AppendText("null");
                            }
                            else if (ret is IEnumerable && !(ret is String))
                            {
                                var sb = new StringBuilder("[ ");
                                var data = ((IEnumerable) ret)
                                    .Cast<Object>()
                                    .Select(_ => (_ ?? "(null)").ToString())
                                    .Take(101)
                                    .ToArray();
                                if (data.Any(s => s.Length > 40))
                                {
                                    sb.Append("\n");
                                    sb.Append(String.Join(
                                        "\n",
                                        data.Take(100)
                                            .Select(s => "    " + s)
                                    ));
                                    sb.Append(data.Length > 100
                                        ? "    (more...)\n]"
                                        : "\n]"
                                    );
                                }
                                else
                                {
                                    sb.Append(String.Join(" ", data.Take(100)));
                                    sb.Append(data.Length > 100
                                        ? " (more...) ]"
                                        : " ]"
                                    );
                                }
                                this.textBox.AppendText(sb.ToString());
                            }
                            else
                            {
                                this.textBox.AppendText(ret.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            this.textBox.AppendText(ex.GetType().Name + ":" + ex);
                        }
                    }
                    this.textBox.AppendText("\r\n>>> ");
                    this.textBox.Select(this.textBox.Text.Length, 0);
                    e.Handled = true;
                    break;
                default:
                    if (
                        e.KeyboardDevice.Modifiers == ModifierKeys.None ||
                        e.KeyboardDevice.Modifiers == ModifierKeys.Shift ||
                        e.KeyboardDevice.Modifiers == ModifierKeys.Control &&
                            (e.Key == Key.V || e.Key == Key.X || e.Key == Key.Back || e.Key == Key.Delete)
                    )
                    {
                        var inputStartPos = this.textBox.Text
                            .Split('\n')
                            .Select(s => s.Length + 1)
                            .Reverse()
                            .Skip(1)
                            .Sum()
                            + ">>> ".Length;
                        if (this.textBox.SelectionStart < inputStartPos)
                            this.textBox.Select(this.textBox.Text.Length, 0);
                    }
                    break;
            }
        }
    }
}
