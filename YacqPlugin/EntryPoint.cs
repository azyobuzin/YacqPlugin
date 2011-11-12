using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Acuerdo.Plugin;
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

        public double Version
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return double.Parse(version.Major + "." + version.Minor);
            }
        }

        public void Loaded()
        {
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
                .Select(file => File.ReadAllText(file))
                .ForEach(code =>
                {
                    try
                    {
                        YacqServices.ParseAll(code)
                            .Select(exp => Expression.Lambda(exp).Compile())
                            .ForEach(dlg => dlg.DynamicInvoke());
                    }
                    catch (Exception ex)
                    {
                        ExceptionStorage.Register(ex, ExceptionCategory.PluginError);
                    }
                });
            }
            catch { }
        }

        public IConfigurator ConfigurationInterface
        {
            get { return null; }
        }
    }
}
