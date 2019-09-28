using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro;

namespace StringToolset
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //ThemeManager.AddTheme(new Uri("pack://application:,,,/StringToolset;component/CustomThemes.xaml"));

            //var theme = ThemeManager.DetectTheme(Application.Current);
            ThemeManager.ChangeTheme(Application.Current, "Dark.Teal");
            //var r = ThemeManager.Themes;
            //ThemeManager.ChangeTheme(Application.Current, new Theme(new Uri("pack://application:,,,/StringToolset;component/CustomThemes.xaml")));

            base.OnStartup(e);
        }
    }
}
