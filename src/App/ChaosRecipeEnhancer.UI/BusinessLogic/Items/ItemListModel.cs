﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChaosRecipeEnhancer.UI.BusinessLogic.Items;

/// <summary>
/// Represents JSON response objects from `get-stash-items` and `get-guild-stash-items` endpoints.
///
/// The full response object is structured as follows:
///
///     {
///         "numTabs" : number,
///         "quadLayout": bool
///         "items": [ ... ]
///     }
///
/// The only thing we're interested in is the `items` array and the `quadLayout` field.
/// </summary>
public class ItemListModel
{
	// Keeping unused field to retain integrity of JSON response object mappings
	// In other words, we want this to remain the source of truth for GGG's item model composition
	// ReSharper disable once UnusedMember.Global
	[JsonPropertyName("items")]
	public List<ItemModel> Items
	{
		get; set;
	}
	[JsonPropertyName("quadLayout")]
	public bool IsQuadLayout
	{
		get; set;
	}
}