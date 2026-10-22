using Microsoft.Extensions.Logging;
using RiggingPlanner.Infrastructure;

namespace RiggingPlanner.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddInfrastructure(GetDatabasePath());

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
		app.Services.ApplyMigrations();

		return app;
	}

	private static string GetDatabasePath()
	{
		var directory = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"RiggingPlanner");

		Directory.CreateDirectory(directory);

		return Path.Combine(directory, "rigging-planner.db");
	}
}
