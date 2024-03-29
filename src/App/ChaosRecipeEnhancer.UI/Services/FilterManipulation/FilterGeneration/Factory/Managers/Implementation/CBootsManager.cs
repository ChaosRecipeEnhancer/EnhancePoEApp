﻿using ChaosRecipeEnhancer.UI.Properties;

namespace ChaosRecipeEnhancer.UI.Services.FilterManipulation.FilterGeneration.Factory.Managers.Implementation;

public class CBootsManager : ABaseItemClassManager
{
    public CBootsManager()
    {
        ClassName = "Boots";
        ClassFilterName = "\"Boots\"";
        ClassColor = Settings.Default.LootFilterBootsColor;
        AlwaysActive = Settings.Default.LootFilterBootsAlwaysActive;
    }

    public override ActiveItemTypes SetActiveTypes(ActiveItemTypes activeItems, bool newValue)
    {
        activeItems.BootsActive = newValue;
        return activeItems;
    }
}