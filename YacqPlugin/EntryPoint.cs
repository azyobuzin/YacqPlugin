using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Acuerdo.Plugin;
using Inscribe.Core;
using Inscribe.Storage;
using Livet;
using XSpect.Yacq;
using Linq = System.Linq.Expressions;

namespace YacqPlugin
{
    [Export(typeof(IPlugin))]
    public class EntryPoint : IPlugin
    {
        public string Name
        {
            get
            {
                return Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(false)
                    .OfType<AssemblyTitleAttribute>()
                    .Select(a => a.Title)
                    .FirstOrDefault();
            }
        }

        public double Version
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return double.Parse(version.Major + "." + version.Minor);
            }
        }

        private void RunCode(string file)
        {
            try
            {
                YacqServices.ParseAll(new SymbolTable(), File.ReadAllText(file))
                    .Select(exp => Linq.Expression.Lambda(exp).Compile())
                    .ToArray() //全部コンパイルしてから
                    .ForEach(dlg => dlg.DynamicInvoke());
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(
                    ex,
                    ExceptionCategory.PluginError,
                    string.Format("{0} の実行に失敗しました: {1}", Path.GetFileName(file), ex.Message),
                    () => this.RunCode(file)
                );
            }
        }

        public void Loaded()
        {
            Task.Factory.StartNew(() =>
            {
                NotifyStorage.Notify("YACQスクリプトの読み込みを開始しました");
                try
                {
                    Directory.EnumerateFiles(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "yacq_lib"
                        ),
                        "*.yacq",
                        SearchOption.TopDirectoryOnly
                    )
                    .Where(file => !file.EndsWith("\\rc.yacq"))
                    .ForEach(this.RunCode);
                }
                catch { }
                NotifyStorage.Notify("YACQスクリプトの読み込みが完了しました");
            });

            Task.Factory.StartNew(() =>
            {
                while (true) //Windowが作成されるまで待たないとAddMenuでNullReferenceException吐かれる
                {
                    Window w = null;
                    DispatcherHelper.UIDispatcher.Invoke(
                        new Action(() =>
                            w = Application.Current.Windows
                                .OfType<Mystique.Views.MainWindow>()
                                .FirstOrDefault()
                        ));

                    if (w != null)
                    {
                        KernelService.AddMenu("YACQ コンソール", () => new ReplWindow() { Owner = w }.Show());
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }

    static class Extension
    {
        public static T Conv<T>(this object source)
        {
            return (T)source;
        }
    }
}
