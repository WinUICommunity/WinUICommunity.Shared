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
    public ControlInfoDataItem(string uniqueId, string title, string secondaryTitle, string apiNamespace, string subtitle, 
        string imagePath, string imageIconPath, string badgeString, string description, string content, 
        bool isNew, bool isUpdated, bool isPreview, bool hideItem, bool hideNavigationViewItem,
        bool hideSourceCodeAndRelatedControls, ControlInfoBadge infoBadge)
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
        HideItem = hideItem;
        HideNavigationViewItem = hideNavigationViewItem;
        InfoBadge = infoBadge;
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
    public bool HideItem { get; private set; }
    public bool HideNavigationViewItem { get; private set; }
    public bool HideSourceCodeAndRelatedControls { get; private set; }
    public ObservableCollection<ControlInfoDocLink> Docs { get; private set; }
    public ObservableCollection<string> RelatedControls { get; private set; }
    public ControlInfoBadge InfoBadge { get; private set; }
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

public class ControlInfoBadge
{
    public ControlInfoBadge(string badgeValue, string badgeStyle, string badgeSymbolIcon, string badgeBitmapIcon, string badgeFontIconGlyph, string badgeFontIconFontName, bool hideBadge, bool hideNavigationViewItemBadge, int badgeHeight, int badgeWidth)
    {
        BadgeValue = badgeValue;
        BadgeStyle = badgeStyle;
        BadgeSymbolIcon = badgeSymbolIcon;
        BadgeBitmapIcon = badgeBitmapIcon;
        BadgeFontIconGlyph = badgeFontIconGlyph;
        BadgeFontIconFontName = badgeFontIconFontName;
        HideBadge = hideBadge;
        HideNavigationViewItemBadge = hideNavigationViewItemBadge;
        BadgeHeight = badgeHeight;
        BadgeWidth = badgeWidth;
    }
    public string BadgeValue { get; private set; }
    public string BadgeStyle { get; private set; }
    public string BadgeSymbolIcon { get; private set; }
    public string BadgeBitmapIcon { get; private set; }
    public string BadgeFontIconGlyph { get; private set; }
    public string BadgeFontIconFontName { get; private set; }
    public int BadgeWidth { get; private set; }
    public int BadgeHeight { get; private set; }
    public bool HideBadge { get; private set; }
    public bool HideNavigationViewItemBadge { get; private set; }
}


/// <summary>
/// Generic group data model.
/// </summary>
public class ControlInfoDataGroup
{
    public ControlInfoDataGroup(string uniqueId, string title, string secondaryTitle, string subtitle, string imagePath, 
        string imageIconPath, string description, string apiNamespace, bool isSpecialSection, bool hideGroup,
        bool isSingleGroup, bool isExpanded, ControlInfoBadge infoBadge)
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
        HideGroup = hideGroup;
        IsSingleGroup = isSingleGroup;
        IsExpanded = isExpanded;
        InfoBadge = infoBadge;
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
    public bool HideGroup { get; private set; }
    public bool IsSingleGroup { get; private set; }
    public bool IsExpanded { get; private set; }
    public ObservableCollection<ControlInfoDataItem> Items { get; private set; }
    public ControlInfoBadge InfoBadge { get; private set; }
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
                var hideGroup = groupObject.ContainsKey("HideGroup") ? groupObject["HideGroup"].GetBoolean() : false;
                var isSingleGroup = groupObject.ContainsKey("IsSingleGroup") ? groupObject["IsSingleGroup"].GetBoolean() : false;
                var isExpanded = groupObject.ContainsKey("IsExpanded") ? groupObject["IsExpanded"].GetBoolean() : false;

                var infoBadgeGroup = GetInfoBadge(groupObject);

                ControlInfoDataGroup group = new ControlInfoDataGroup(groupObject["UniqueId"].GetString(),
                                                                      groupObject["Title"].GetString(),
                                                                      groupObject["SecondaryTitle"].GetString(),
                                                                      groupObject["Subtitle"].GetString(),
                                                                      groupObject["ImagePath"].GetString(),
                                                                      groupObject["ImageIconPath"].GetString(),
                                                                      groupObject["Description"].GetString(),
                                                                      groupObject["ApiNamespace"].GetString(),
                                                                      usesCustomNavigationItems,
                                                                      hideGroup,
                                                                      isSingleGroup,
                                                                      isExpanded,
                                                                      infoBadgeGroup);

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();

                    string badgeString = null;

                    bool isNew = itemObject.ContainsKey("IsNew") ? itemObject["IsNew"].GetBoolean() : false;
                    bool isUpdated = itemObject.ContainsKey("IsUpdated") ? itemObject["IsUpdated"].GetBoolean() : false;
                    bool isPreview = itemObject.ContainsKey("IsPreview") ? itemObject["IsPreview"].GetBoolean() : false;
                    bool isIncludedInBuild = itemObject.ContainsKey("IncludedInBuild") ? itemObject["IncludedInBuild"].GetBoolean() : false;
                    bool hideItem = itemObject.ContainsKey("HideItem") ? itemObject["HideItem"].GetBoolean() : false;
                    bool hideNavigationViewItem = itemObject.ContainsKey("HideNavigationViewItem") ? itemObject["HideNavigationViewItem"].GetBoolean() : false;

                    var infoBadge = GetInfoBadge(itemObject);

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
                                                            hideItem,
                                                            hideNavigationViewItem,
                                                            hideSourceCodeAndRelatedControls,
                                                            infoBadge);

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

    private ControlInfoBadge GetInfoBadge(JsonObject jsonObject)
    {
        var infoBadgeObject = jsonObject.ContainsKey("InfoBadge") ? jsonObject["InfoBadge"].GetObject() : null;

        string badgeValue = null;
        string badgeStyle = null;
        string badgeSymbolcon = null;
        string badgeBitmapIcon = null;
        string badgeFontIconGlyph = null;
        string badgeFontIconFontName = null;
        int badgeWidth = 0;
        int badgeHeight = 0;
        bool hideBadge = false;
        bool hideNavigationViewItemBadge = false;

        if (infoBadgeObject is not null)
        {
            badgeValue = infoBadgeObject.ContainsKey("BadgeValue") ? infoBadgeObject["BadgeValue"].GetString() : null;
            badgeStyle = infoBadgeObject.ContainsKey("BadgeStyle") ? infoBadgeObject["BadgeStyle"].GetString() : "AttentionValueInfoBadgeStyle";
            badgeSymbolcon = infoBadgeObject.ContainsKey("BadgeSymbolIcon") ? infoBadgeObject["BadgeSymbolIcon"].GetString() : null;
            badgeBitmapIcon = infoBadgeObject.ContainsKey("BadgeBitmapIcon") ? infoBadgeObject["BadgeBitmapIcon"].GetString() : null;
            badgeFontIconGlyph = infoBadgeObject.ContainsKey("BadgeFontIconGlyph") ? infoBadgeObject["BadgeFontIconGlyph"].GetString() : null;
            badgeFontIconFontName = infoBadgeObject.ContainsKey("BadgeFontIconFontName") ? infoBadgeObject["BadgeFontIconFontName"].GetString() : null;
            badgeHeight = infoBadgeObject.ContainsKey("BadgeHeight") ? Convert.ToInt32(infoBadgeObject["BadgeHeight"].GetNumber()) : 0;
            badgeWidth = infoBadgeObject.ContainsKey("BadgeWidth") ? Convert.ToInt32(infoBadgeObject["BadgeWidth"].GetNumber()) : 0;
            hideBadge = infoBadgeObject.ContainsKey("HideBadge") ? infoBadgeObject["HideBadge"].GetBoolean() : false;
            hideNavigationViewItemBadge = infoBadgeObject.ContainsKey("HideNavigationViewItemBadge") ? infoBadgeObject["HideNavigationViewItemBadge"].GetBoolean() : false;
            
            return new ControlInfoBadge(badgeValue, badgeStyle, badgeSymbolcon, badgeBitmapIcon, badgeFontIconGlyph, badgeFontIconFontName, hideBadge, hideNavigationViewItemBadge, badgeHeight, badgeWidth);
        }
        return null;
    }
}
