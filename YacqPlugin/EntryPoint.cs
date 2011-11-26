﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Acuerdo.Plugin;
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
                    .Select(file => File.ReadAllText(file))
                    .ForEach(code =>
                    {
                        try
                        {
                            YacqServices.ParseAll(new SymbolTable(), code)
                                .Select(exp => Linq.Expression.Lambda(exp).Compile())
                                .ForEach(dlg => dlg.DynamicInvoke());
                        }
                        catch (Exception ex)
                        {
                            ExceptionStorage.Register(ex, ExceptionCategory.PluginError);
                        }
                    });
                }
                catch { }
                NotifyStorage.Notify("YACQスクリプトの読み込みが完了しました");
            });

            Task.Factory.StartNew(() =>
            {
                while (true)
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
                        DispatcherHelper.BeginInvoke(() =>
                        {
                            var menu = new MenuItem();
                            menu.Header = "YACQ コンソール...";
                            menu.Click += (sender, e) => new ReplWindow() { Owner = w }.Show();

                            w.Content.Conv<Grid>()
                                .Children
                                .OfType<Grid>()
                                .SelectMany(grid => grid.Children.OfType<Mystique.Views.PartBlocks.InputBlock.InputBlock>())
                                .First()
                                .Content.Conv<DockPanel>()
                                .Children
                                .OfType<Grid>()
                                .SelectMany(grid => grid.Children.OfType<Mystique.Views.Common.DropDownButton>())
                                .First()
                                .DropDownMenu
                                .Items
                                .Insert(6, menu);
                        });
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
