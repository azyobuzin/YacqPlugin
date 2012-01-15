using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Acuerdo.Plugin;
using Inscribe.Core;
using Inscribe.Storage;
using XSpect.Yacq;

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

        public Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        private void RunCode(string file)
        {
            try
            {
                YacqServices.ParseAll(new SymbolTable(), File.ReadAllText(file))
                    .Select(exp => Expression.Lambda(exp).Compile())
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

            KernelService.AddMenu("YACQ コンソール", () => new ReplWindow().Show());
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }
}
