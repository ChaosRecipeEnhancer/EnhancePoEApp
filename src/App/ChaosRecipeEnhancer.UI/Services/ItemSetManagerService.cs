﻿using System.Collections.Generic;
using System.Linq;
using ChaosRecipeEnhancer.UI.Constants;
using ChaosRecipeEnhancer.UI.Models;
using ChaosRecipeEnhancer.UI.Models.Enums;

namespace ChaosRecipeEnhancer.UI.Services;

public interface IItemSetManagerService
{
    public bool UpdateData(
        int setThreshold,
        List<int> selectedTabIndices,
        List<EnhancedItem> filteredStashContents,
        bool includeIdentified = false,
        bool chaosRecipe = true
    );

    public void GenerateItemSets(bool chaosRecipe);
    public void CalculateItemAmounts();
    public void ResetCompletedSets();
    public void ResetItemAmounts();

    public bool RetrieveNeedsFetching();
    public int RetrieveCompletedSets();

    public int RetrieveRingsAmount();
    public int RetrieveAmuletsAmount();
    public int RetrieveBeltsAmount();
    public int RetrieveChestsAmount();
    public int RetrieveWeaponsSmallAmount();
    public int RetrieveWeaponsBigAmount();
    public int RetrieveGlovesAmount();
    public int RetrieveHelmetsAmount();
    public int RetrieveBootsAmount();
}

public class ItemSetManagerService : IItemSetManagerService
{
    private int _setThreshold;
    private List<EnhancedItemSet> _setsInProgress = new();
    private List<EnhancedItem> _currentItemsFilteredForRecipe = new(); // filtered for chaos recipe

    // item amounts by kind that will be exposed
    // while ItemSetManagerService doesn't know,
    // others need to render this out to users
    public int RingsAmount { get; set; }
    public int AmuletsAmount { get; set; }
    public int BeltsAmount { get; set; }
    public int ChestsAmount { get; set; }
    public int WeaponsSmallAmount { get; set; }
    public int WeaponsBigAmount { get; set; }
    public int GlovesAmount { get; set; }
    public int HelmetsAmount { get; set; }
    public int BootsAmount { get; set; }

    // full set amounts will also need to be rendered
    public int CompletedSets { get; set; }

    public bool NeedsFetching { get; set; } = true;

    // this is the primary method being called by external entities
    // return true if successful update, false if some error (likely caller error missing important data)
    public bool UpdateData(
        int setThreshold,
        List<int> selectedTabIndices,
        List<EnhancedItem> filteredStashContents, // i feel like implicitly this comes pre-filtered
        bool includeIdentified = false,
        bool chaosRecipe = true
    )
    {
        // if no tabs are selected (this isn't a realistic case)
        if (selectedTabIndices.Count == 0) return false;

        // setting some new properties
        _setThreshold = setThreshold;
        _currentItemsFilteredForRecipe = filteredStashContents;

        NeedsFetching = false;
        return true;
    }

    public void CalculateItemAmounts()
    {
        foreach (var item in _currentItemsFilteredForRecipe)
        {
            switch (item.DerivedItemClass)
            {
                case GameTerminology.Rings:
                    RingsAmount++; // 0: rings
                    break;
                case GameTerminology.Amulets:
                    AmuletsAmount++; // 1: amulets
                    break;
                case GameTerminology.Belts:
                    BeltsAmount++; // 2: belts
                    break;
                case GameTerminology.BodyArmor:
                    ChestsAmount++; // 3: chests
                    break;
                case GameTerminology.OneHandWeapons:
                    WeaponsSmallAmount++; // 4: weaponsSmall
                    break;
                case GameTerminology.TwoHandWeapons:
                    WeaponsBigAmount++; // 5: weaponsBig
                    break;
                case GameTerminology.Gloves:
                    GlovesAmount++; // 6: gloves
                    break;
                case GameTerminology.Helmets:
                    HelmetsAmount++; // 7: helmets
                    break;
                case GameTerminology.Boots:
                    BootsAmount++; // 8: boots
                    break;
            }
        }

        NeedsFetching = false;
    }

    public void ResetCompletedSets()
    {
        CompletedSets = 0;
    }

    public void ResetItemAmounts()
    {
        // reset all item amounts
        RingsAmount = 0;
        AmuletsAmount = 0;
        BeltsAmount = 0;
        ChestsAmount = 0;
        WeaponsSmallAmount = 0;
        WeaponsBigAmount = 0;
        GlovesAmount = 0;
        HelmetsAmount = 0;
        BootsAmount = 0;
    }

    public void GenerateItemSets(bool chaosRecipe)
    {
        _setsInProgress.Clear();

        var listOfSets = new List<EnhancedItemSet>();

        // iterate once to create as many sets as possible that can produce recipe
        if (chaosRecipe)
        {
            var noChaosItemsLeft = false;
            for (var i = 0; i < _setThreshold; i++)
            {
                // create new 'empty' enhanced item set
                var enhancedItemSet = new EnhancedItemSet();

                if (!noChaosItemsLeft)
                {
                    var eligibleChaosItems = _currentItemsFilteredForRecipe.Where(x => x.IsChaosRecipeEligible).ToList();

                    if (eligibleChaosItems.Count != 0)
                    {
                        foreach (var attempt in eligibleChaosItems)
                        {
                            var addSuccessful = enhancedItemSet.TryAddItem(attempt);

                            if (addSuccessful)
                            {
                                _currentItemsFilteredForRecipe.Remove(attempt);
                                break;
                            }
                        }
                    }
                    else
                    {
                        noChaosItemsLeft = true;
                    }
                }

                listOfSets.Add(enhancedItemSet);
            }
        }
        else
        {
            for (var i = 0; i < _setThreshold; i++)
            {
                listOfSets.Add(new EnhancedItemSet());
            }
        }

        for (var i = 0; i < _setThreshold; i++)
        {
            // iterate until the end of time (jk)
            while (true)
            {
                // the closest item is assumed to be in another galaxy
                EnhancedItem closestMissingItem = null;
                var minDistance = double.PositiveInfinity;

                // find the for real closes item
                // this is a nested for over each other item in our current item filtered for recipe
                // probably could be optimized? maybe?
                foreach (var item in _currentItemsFilteredForRecipe
                             .Where(item => listOfSets[i].IsItemClassNeeded(item) && // item is of a class we need
                                            listOfSets[i].GetItemDistance(item) < minDistance)) // item is closer than the current closest
                {
                    minDistance = listOfSets[i].GetItemDistance(item);
                    closestMissingItem = item;
                }

                if (closestMissingItem is not null)
                {
                    // if we found a new closer we're good to add it to our enhanced set
                    _ = listOfSets[i].TryAddItem(closestMissingItem);
                    // promptly remove it from our pool of 'available' items
                    // do i actually want to do this? lol
                    _ = _currentItemsFilteredForRecipe.Remove(closestMissingItem);
                }
                // you didn't find a closer item, gg break out of infinite loop
                else
                {
                    break;
                }

                if (chaosRecipe)
                {
                    var canProduce = listOfSets[i].Items.FirstOrDefault(x => x.IsChaosRecipeEligible, null);

                    // my reason for separating out this logic is that it's a bit more readable and debuggable
                    if (canProduce is not null)
                    {
                        listOfSets[i].HasRecipeQualifier = true;

                        if (listOfSets[i].EmptyItemSlots.Count == 0)
                        {
                            CompletedSets++;
                        }
                    }
                }
            }

            // add new enhanced item set to our list of sets in progress
            _setsInProgress = listOfSets;
        }
    }

    // workaround to expose properties as functions on our interface
    public bool RetrieveNeedsFetching() => NeedsFetching;
    public int RetrieveCompletedSets() => CompletedSets;

    // item amount public properties via functions
    public int RetrieveRingsAmount() => RingsAmount;
    public int RetrieveAmuletsAmount() => AmuletsAmount;
    public int RetrieveBeltsAmount() => BeltsAmount;
    public int RetrieveChestsAmount() => ChestsAmount;
    public int RetrieveWeaponsSmallAmount() => WeaponsSmallAmount;
    public int RetrieveWeaponsBigAmount() => WeaponsBigAmount;
    public int RetrieveGlovesAmount() => GlovesAmount;
    public int RetrieveHelmetsAmount() => HelmetsAmount;
    public int RetrieveBootsAmount() => BootsAmount;

}