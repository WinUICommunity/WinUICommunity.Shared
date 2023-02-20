using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Data.Json;
using WinUICommunity.Shared.Internal;

namespace WinUICommunity.Shared.DataModel;

/// <summary>
/// Generic item data model.
/// </summary>
public class ControlInfoDataItem
{
    public ControlInfoDataItem(string uniqueId, string title, string secondaryTitle, string apiNamespace, string subtitle, string imagePath, string imageIconPath, string badgeString, string description, string content, bool isNew, bool isUpdated, bool isPreview, bool hideSourceCodeAndRelatedControls)
    {
        UniqueId = uniqueId;
        Title = title;
        SecondaryTitle = secondaryTitle;
        ApiNamespace = apiNamespace;
        Subtitle = subtitle;
        Description = description;
        ImagePath = imagePath;
        ImageIconPath = imageIconPath;
        BadgeString = badgeString;
        Content = content;
        IsNew = isNew;
        IsUpdated = isUpdated;
        IsPreview = isPreview;
        Docs = new ObservableCollection<ControlInfoDocLink>();
        RelatedControls = new ObservableCollection<string>();
        HideSourceCodeAndRelatedControls = hideSourceCodeAndRelatedControls;
    }

    public string UniqueId { get; private set; }
    public string Title { get; private set; }
    public string SecondaryTitle { get; private set; }
    public string ApiNamespace { get; private set; }
    public string Subtitle { get; private set; }
    public string Description { get; private set; }
    public string ImagePath { get; private set; }
    public string ImageIconPath { get; private set; }
    public string BadgeString { get; private set; }
    public string Content { get; private set; }
    public bool IsNew { get; private set; }
    public bool IsUpdated { get; private set; }
    public bool IsPreview { get; private set; }
    public bool HideSourceCodeAndRelatedControls { get; private set; }
    public ObservableCollection<ControlInfoDocLink> Docs { get; private set; }
    public ObservableCollection<string> RelatedControls { get; private set; }

    public bool IncludedInBuild { get; set; }

    public override string ToString()
    {
        return Title;
    }
}

public class ControlInfoDocLink
{
    public ControlInfoDocLink(string title, string uri)
    {
        Title = title;
        Uri = uri;
    }
    public string Title { get; private set; }
    public string Uri { get; private set; }
}


/// <summary>
/// Generic group data model.
/// </summary>
public class ControlInfoDataGroup
{
    public ControlInfoDataGroup(string uniqueId, string title, string secondaryTitle, string subtitle, string imagePath, string imageIconPath, string description, string apiNamespace, bool isSpecialSection)
    {
        UniqueId = uniqueId;
        Title = title;
        SecondaryTitle = secondaryTitle;
        ApiNamespace = apiNamespace;
        Subtitle = subtitle;
        Description = description;
        ImagePath = imagePath;
        ImageIconPath = imageIconPath;
        Items = new ObservableCollection<ControlInfoDataItem>();
        IsSpecialSection = isSpecialSection;
    }

    public string UniqueId { get; private set; }
    public string Title { get; private set; }
    public string SecondaryTitle { get; private set; }
    public string Subtitle { get; private set; }
    public string Description { get; private set; }
    public string ImagePath { get; private set; }
    public string ImageIconPath { get; private set; }
    public string ApiNamespace { get; private set; } = "";
    public bool IsSpecialSection { get; set; }
    public ObservableCollection<ControlInfoDataItem> Items { get; private set; }

    public override string ToString()
    {
        return Title;
    }
}

/// <summary>
/// Creates a collection of groups and items with content read from a static json file.
///
/// ControlInfoSource initializes with data read from a static json file included in the
/// project.  This provides sample data at both design-time and run-time.
/// </summary>
public sealed class ControlInfoDataSource
{
    private static readonly object _lock = new object();

    #region Singleton

    private static ControlInfoDataSource _instance;

    public static ControlInfoDataSource Instance
    {
        get
        {
            return _instance;
        }
    }

    static ControlInfoDataSource()
    {
        _instance = new ControlInfoDataSource();
    }

    private ControlInfoDataSource() { }

    #endregion

    private IList<ControlInfoDataGroup> _groups = new List<ControlInfoDataGroup>();
    public IList<ControlInfoDataGroup> Groups
    {
        get { return _groups; }
    }

    public async Task<IEnumerable<ControlInfoDataGroup>> GetGroupsAsync(string JsonRelativeFilePath, IncludedInBuildMode IncludedInBuildMode = IncludedInBuildMode.CheckBasedOnIncludedInBuildProperty)
    {
        await _instance.GetControlInfoDataAsync(JsonRelativeFilePath, IncludedInBuildMode);

        return _instance.Groups;
    }

    public async Task<ControlInfoDataGroup> GetGroupAsync(string uniqueId, string JsonRelativeFilePath, IncludedInBuildMode IncludedInBuildMode = IncludedInBuildMode.CheckBasedOnIncludedInBuildProperty)
    {
        await _instance.GetControlInfoDataAsync(JsonRelativeFilePath, IncludedInBuildMode);
        // Simple linear search is acceptable for small data sets
        var matches = _instance.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
        if (matches.Count() == 1) return matches.First();
        return null;
    }

    public async Task<ControlInfoDataItem> GetItemAsync(string uniqueId, string JsonRelativeFilePath, IncludedInBuildMode IncludedInBuildMode = IncludedInBuildMode.CheckBasedOnIncludedInBuildProperty)
    {
        await _instance.GetControlInfoDataAsync(JsonRelativeFilePath, IncludedInBuildMode);
        // Simple linear search is acceptable for small data sets
        var matches = _instance.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
        if (matches.Count() > 0) return matches.First();
        return null;
    }

    public async Task<ControlInfoDataGroup> GetGroupFromItemAsync(string uniqueId, string JsonRelativeFilePath, IncludedInBuildMode IncludedInBuildMode = IncludedInBuildMode.CheckBasedOnIncludedInBuildProperty)
    {
        await _instance.GetControlInfoDataAsync(JsonRelativeFilePath, IncludedInBuildMode);
        var matches = _instance.Groups.Where((group) => group.Items.FirstOrDefault(item => item.UniqueId.Equals(uniqueId)) != null);
        if (matches.Count() == 1) return matches.First();
        return null;
    }

    private async Task GetControlInfoDataAsync(string JsonRelativeFilePath, IncludedInBuildMode IncludedInBuildMode = IncludedInBuildMode.CheckBasedOnIncludedInBuildProperty)
    {
        lock (_lock)
        {
            if (Groups.Count() != 0)
            {
                return;
            }
        }

        string jsonText = await FileLoader.LoadText(JsonRelativeFilePath);

        JsonObject jsonObject = JsonObject.Parse(jsonText);
        JsonArray jsonArray = jsonObject["Groups"].GetArray();

        lock (_lock)
        {
            foreach (JsonValue groupValue in jsonArray)
            {

                JsonObject groupObject = groupValue.GetObject();

                var usesCustomNavigationItems = groupObject.ContainsKey("IsSpecialSection") ? groupObject["IsSpecialSection"].GetBoolean() : false;
                ControlInfoDataGroup group = new ControlInfoDataGroup(groupObject["UniqueId"].GetString(),
                                                                      groupObject["Title"].GetString(),
                                                                      groupObject["SecondaryTitle"].GetString(),
                                                                      groupObject["ApiNamespace"].GetString(),
                                                                      groupObject["Subtitle"].GetString(),
                                                                      groupObject["ImagePath"].GetString(),
                                                                      groupObject["ImageIconPath"].GetString(),
                                                                      groupObject["Description"].GetString(),
                                                                      usesCustomNavigationItems);

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();

                    string badgeString = null;

                    bool isNew = itemObject.ContainsKey("IsNew") ? itemObject["IsNew"].GetBoolean() : false;
                    bool isUpdated = itemObject.ContainsKey("IsUpdated") ? itemObject["IsUpdated"].GetBoolean() : false;
                    bool isPreview = itemObject.ContainsKey("IsPreview") ? itemObject["IsPreview"].GetBoolean() : false;
                    bool isIncludedInBuild = itemObject.ContainsKey("IncludedInBuild") ? itemObject["IncludedInBuild"].GetBoolean() : false;

                    if (isNew)
                    {
                        badgeString = "New";
                    }
                    else if (isUpdated)
                    {
                        badgeString = "Updated";
                    }
                    else if (isPreview)
                    {
                        badgeString = "Preview";
                    }

                    var hideSourceCodeAndRelatedControls = itemObject.ContainsKey("HideSourceCodeAndRelatedControls") ? itemObject["HideSourceCodeAndRelatedControls"].GetBoolean() : false;
                    var item = new ControlInfoDataItem(itemObject["UniqueId"].GetString(),
                                                            itemObject["Title"].GetString(),
                                                            itemObject["SecondaryTitle"].GetString(),
                                                            itemObject["ApiNamespace"].GetString(),
                                                            itemObject["Subtitle"].GetString(),
                                                            itemObject["ImagePath"].GetString(),
                                                            itemObject["ImageIconPath"].GetString(),
                                                            badgeString,
                                                            itemObject["Description"].GetString(),
                                                            itemObject["Content"].GetString(),
                                                            isNew,
                                                            isUpdated,
                                                            isPreview,
                                                            hideSourceCodeAndRelatedControls);

                    {
                        string pageString = item.UniqueId;
                        if (IncludedInBuildMode == IncludedInBuildMode.CheckBasedOnIncludedInBuildProperty)
                        {
                            item.IncludedInBuild = isIncludedInBuild;
                        }
                        else
                        {
                            Type pageType = null;
                            Assembly assembly = Assembly.Load(item.ApiNamespace);
                            if (assembly is not null)
                            {
                                pageType = assembly.GetType(pageString);
                            }
                            item.IncludedInBuild = pageType != null;
                        }

                    }

                    if (itemObject.ContainsKey("Docs"))
                    {
                        foreach (JsonValue docValue in itemObject["Docs"].GetArray())
                        {
                            JsonObject docObject = docValue.GetObject();
                            item.Docs.Add(new ControlInfoDocLink(docObject["Title"].GetString(), docObject["Uri"].GetString()));
                        }
                    }
                    if (itemObject.ContainsKey("RelatedControls"))
                    {
                        foreach (JsonValue relatedControlValue in itemObject["RelatedControls"].GetArray())
                        {
                            item.RelatedControls.Add(relatedControlValue.GetString());
                        }
                    }

                    group.Items.Add(item);
                }
                if (!Groups.Any(g => g.Title == group.Title))
                {
                    Groups.Add(group);
                }
            }
        }
    }
}
