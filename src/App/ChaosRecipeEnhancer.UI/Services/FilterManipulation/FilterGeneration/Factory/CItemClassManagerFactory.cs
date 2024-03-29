﻿using System;
using ChaosRecipeEnhancer.UI.Models.Enums;
using ChaosRecipeEnhancer.UI.Services.FilterManipulation.FilterGeneration.Factory.Managers;
using ChaosRecipeEnhancer.UI.Services.FilterManipulation.FilterGeneration.Factory.Managers.Implementation;

namespace ChaosRecipeEnhancer.UI.Services.FilterManipulation.FilterGeneration.Factory;

public class CItemClassManagerFactory
{
	public ABaseItemClassManager GetItemClassManager(ItemClass itemClass)
	{
		switch (itemClass)
		{
			case ItemClass.Helmets:
				return new CHelmetManager();
			case ItemClass.BodyArmours:
				return new CBodyArmoursManager();
			case ItemClass.Gloves:
				return new CGlovesManager();
			case ItemClass.Boots:
				return new CBootsManager();
			case ItemClass.Rings:
				return new CRingsManager();
			case ItemClass.Amulets:
				return new CAmuletsManager();
			case ItemClass.Belts:
				return new CBeltsManager();
			case ItemClass.OneHandWeapons:
				return new COneHandWeaponsManager();
			case ItemClass.TwoHandWeapons:
				return new CTwoHandWeaponsManager();
			default:
				throw new Exception("Wrong item class.");
		}
	}
}