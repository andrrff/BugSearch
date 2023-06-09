namespace BugSearch.Shared.Interfaces;

public interface IWebSiteInfo
{
    string? Favicon { get; set; }

    string? Title { get; set; }

    string? Description { get; set; }
}