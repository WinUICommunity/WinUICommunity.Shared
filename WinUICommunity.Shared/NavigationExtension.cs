using Microsoft.UI.Xaml.Controls;

namespace WinUICommunity.Shared.Internal;
public static class NavigationExtension
{
    public static void EnsureNavigationSelection(this NavigationView navigationView, string id)
    {
        foreach (object rawGroup in navigationView.MenuItems)
        {
            if (rawGroup is NavigationViewItem group)
            {
                foreach (object rawItem in group.MenuItems)
                {
                    if (rawItem is NavigationViewItem item)
                    {
                        if ((string)item.Tag == id)
                        {
                            group.IsExpanded = true;
                            navigationView.SelectedItem = item;
                            item.IsSelected = true;
                            return;
                        }
                        else if (item.MenuItems.Count > 0)
                        {
                            foreach (var rawInnerItem in item.MenuItems)
                            {
                                if (rawInnerItem is NavigationViewItem innerItem)
                                {
                                    if ((string)innerItem.Tag == id)
                                    {
                                        group.IsExpanded = true;
                                        item.IsExpanded = true;
                                        navigationView.SelectedItem = innerItem;
                                        innerItem.IsSelected = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static bool EnsureItemIsVisibleInNavigation(this NavigationView navigationView, string name)
    {
        bool changedSelection = false;
        foreach (object rawItem in navigationView.MenuItems)
        {
            // Check if we encountered the separator
            if (!(rawItem is NavigationViewItem))
            {
                // Skipping this item
                continue;
            }

            var item = rawItem as NavigationViewItem;

            // Check if we are this category
            if ((string)item.Content == name)
            {
                navigationView.SelectedItem = item;
                changedSelection = true;
            }
            // We are not :/
            else
            {
                // Maybe one of our items is?
                if (item.MenuItems.Count != 0)
                {
                    foreach (NavigationViewItem child in item.MenuItems)
                    {
                        if ((string)child.Content == name)
                        {
                            // We are the item corresponding to the selected one, update selection!

                            // Deal with differences in displaymodes
                            if (navigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
                            {
                                // In Topmode, the child is not visible, so set parent as selected
                                // Everything else does not work unfortunately
                                navigationView.SelectedItem = item;
                                item.StartBringIntoView();
                            }
                            else
                            {
                                // Expand so we animate
                                item.IsExpanded = true;
                                // Ensure parent is expanded so we actually show the selection indicator
                                navigationView.UpdateLayout();
                                // Set selected item
                                navigationView.SelectedItem = child;
                                child.StartBringIntoView();
                            }
                            // Set to true to also skip out of outer for loop
                            changedSelection = true;
                            // Break out of child iteration for loop
                            break;
                        }
                    }
                }
            }
            // We updated selection, break here!
            if (changedSelection)
            {
                break;
            }
        }
        return changedSelection;
    }
}
