using System.Configuration;
using System.Data;
using System.Windows;

namespace TestGpu1;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void OnExit(object sender, ExitEventArgs e)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
