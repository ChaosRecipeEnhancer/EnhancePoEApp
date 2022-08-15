﻿using ChaosRecipeEnhancer.UI.Model;
using ChaosRecipeEnhancer.UI.Properties;

namespace ChaosRecipeEnhancer.UI.Factory.Managers.Implementation
{
    internal class CGlovesManager : ABaseItemClassManager
    {
        #region Constructors

        public CGlovesManager()
        {
            ClassName = "Gloves";
            ClassFilterName = "\"Gloves\"";
            ClassColor = Settings.Default.LootFilterGlovesColor;
            AlwaysActive = Settings.Default.LootFilterGlovesAlwaysActive;
        }

        #endregion

        #region Methods

        public override ActiveItemTypes SetActiveTypes(ActiveItemTypes activeItems, bool newValue)
        {
            activeItems.GlovesActive = newValue;
            return activeItems;
        }

        #endregion
    }
}