namespace RiggingPlanner.App;

// Fully qualified: the namespace "RiggingPlanner.Application" (PDF contracts in
// RiggingPlanner.Application.Pdf) would otherwise shadow Microsoft.Maui.Controls.Application.
public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "Rigging Planner" };
	}
}
