using Microsoft.Extensions.DependencyInjection;

namespace BiblioLinx;

public partial class App : Application
{
    public App()
    {
        // A sua licença
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCekxwWmFZfVhgcl9GYVZTQmY/P1ZhSXxVdkZgWX5WcHVXQGhdVUR9XEE=");

        InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}