﻿using ChaosRecipeEnhancer.UI.Models.ApiResponses;
using ChaosRecipeEnhancer.UI.Models.Enums;
using ChaosRecipeEnhancer.UI.Properties;
using ChaosRecipeEnhancer.UI.Services;
using System.Collections.Generic;
using System.Linq;

namespace ChaosRecipeEnhancer.UI.UserControls.StashTab;

/// <summary>
///     CRE-specific model of a stash tab within our app (NOT the GGG ToggleStashTabOverlay object model).
///     Represents various UI elements and additional metadata used in our app that is derived from the original
///     stash tab JSON object requested from GGG's API.
/// </summary>
public static class StashTabControlManager
{
    public static List<StashTabControl> StashTabControls { get; set; } = [];
    public static List<int> StashTabIndices { get; private set; }

    public static void GetStashTabIndicesFromSettingsForQueryByIndex()
    {
        // update the stash tab metadata based on your target stash

        List<int> selectedTabIndices = [];

        if (Settings.Default.StashTabQueryMode == (int)StashTabQueryMode.SelectTabsByIndex)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.StashTabIndices))
            {
                GlobalErrorHandler.Spawn(
                    "It looks like you haven't selected any stash tab indices. Please navigate to the 'General > General > Select Stash Tabs' setting and select some tabs, and try again.",
                    "Error: Stash Tab Overlay"
                );
            }
            else
            {
                selectedTabIndices = Settings.Default.StashTabIndices.Split(',').ToList().Select(int.Parse).ToList();
            }
        }
        else if (Settings.Default.StashTabQueryMode == (int)StashTabQueryMode.TabNamePrefix)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.StashTabPrefix))
            {
                GlobalErrorHandler.Spawn(
                    "It looks like you haven't entered a stash tab prefix or no tabs match that naming prefix. Please navigate to the 'General > General > Stash Tab Prefix' setting and enter a valid value, and try again.",
                    "Error: Stash Tab Overlay"
                );
            }
            else
            {
                selectedTabIndices = Settings.Default.StashTabPrefixIndices.Split(',').ToList().Select(int.Parse).ToList();
            }
        }

        StashTabIndices = selectedTabIndices;

        foreach (var s in selectedTabIndices)
        {
            if (!selectedTabIndices.Contains(s)) selectedTabIndices.Add(s);
        }

        if (selectedTabIndices.Count == 0)
        {
            GlobalErrorHandler.Spawn("Stash Tab indices empty", "Error: Stash Tab Overlay");
        }
    }

    public static void GetStashTabIndicesFromSettingsForQueryById(List<BaseStashTabMetadata> stashData)
    {
        // update the stash tab metadata based on your target stash

        List<int> selectedTabIndices = [];

        var selectedStashIds = Settings.Default.StashTabIdentifiers.Split(',').ToList();

        foreach (var id in selectedStashIds)
        {
            var tab = stashData.FirstOrDefault(x => x.Id == id);
            if (tab != null)
            {
                selectedTabIndices.Add(tab.Index);
            }
        }

        StashTabIndices = selectedTabIndices;

        foreach (var s in selectedTabIndices)
        {
            if (!selectedTabIndices.Contains(s)) selectedTabIndices.Add(s);
        }

        if (selectedTabIndices.Count == 0)
        {
            GlobalErrorHandler.Spawn("Stash Tab indices empty", "Error: Stash Tab Overlay");
        }
    }
}