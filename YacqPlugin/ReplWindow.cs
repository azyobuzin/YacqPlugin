using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XSpect.Yacq;
using Linq = System.Linq.Expressions;

namespace YacqPlugin
{
    class ReplWindow : Window
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
            this.textBox.Text = "YACQ Console\r\n>>> ";
            this.textBox.Select(this.textBox.Text.Length, 0);
            this.textBox.PreviewKeyDown += this.textBox_PreviewKeyDown;
        }

        private TextBox textBox = new TextBox();

        private LinkedList<string> history = new LinkedList<string>();
        private LinkedListNode<string> historyPos = null;

        private SymbolTable symbolTable = new SymbolTable();

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
                            var exp = Linq.Expression.Lambda(
                                YacqServices.Parse(this.symbolTable, code)
                            );
                            var ret = exp.Compile().DynamicInvoke();
                            if (ret == null)
                            {
                                this.textBox.AppendText("null");
                            }
                            else
                            {
                                this.textBox.AppendText(ret.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            this.textBox.AppendText(ex.GetType().Name + ":" + ex.ToString());
                        }
                    }
                    this.textBox.AppendText("\r\n>>> ");
                    this.textBox.Select(this.textBox.Text.Length, 0);
                    e.Handled = true;
                    break;
                default:
                    var inputStartPos = this.textBox.Text
                        .Split('\n')
                        .Select(s => s.Length + 1)
                        .Reverse()
                        .Skip(1)
                        .Sum()
                        + ">>> ".Length;
                    if (this.textBox.SelectionStart < inputStartPos)
                        this.textBox.Select(this.textBox.Text.Length, 0);
                    break;
            }
        }
    }
}
