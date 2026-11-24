namespace RiggingPlanner.App.Components.Shared;

public static class AppUrl
{
    public static string Normalize(string url) => url.StartsWith('/') ? url : $"/{url}";
}
