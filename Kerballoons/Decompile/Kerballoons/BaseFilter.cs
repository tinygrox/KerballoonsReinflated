using KSP.UI.Screens;
using RUI.Icons.Selectable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Kerballoons
{
	public abstract class BaseFilter : MonoBehaviour
	{
		private readonly List<AvailablePart> parts = new List<AvailablePart>();

		internal string category = "Filter by function";

		internal bool filter = true;

		protected abstract string Manufacturer
		{
			get;
			set;
		}

		protected abstract string categoryTitle
		{
			get;
			set;
		}

		private void Awake()
		{
			parts.Clear();
			int count = PartLoader.LoadedPartsList.Count;
			for (int i = 0; i < count; i++)
			{
				AvailablePart availablePart = PartLoader.LoadedPartsList[i];
				if ((bool)availablePart.partPrefab && availablePart.manufacturer == Manufacturer)
				{
					parts.Add(availablePart);
				}
			}
			if (parts.Count > 0)
			{
				GameEvents.onGUIEditorToolbarReady.Add(SubCategories);
			}
		}

		private bool EditorItemsFilter(AvailablePart avPart)
		{
			return parts.Contains(avPart);
		}

		private void SubCategories()
		{
			Icon icon = GetIcon(categoryTitle);
			PartCategorizer.Category category = PartCategorizer.Instance.filters.Find((PartCategorizer.Category f) => f.button.categorydisplayName == "#autoLOC_453547");
			PartCategorizer.AddCustomSubcategoryFilter(category, categoryTitle, categoryTitle, icon, (Func<AvailablePart, bool>)EditorItemsFilter);
		}

		private Icon GetIcon(string iconName)
		{
			Texture2D texture2D = new Texture2D(32, 32, TextureFormat.RGBA32, false);
			string url = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), iconName + "_off.png");
			WWW wWW = new WWW(url);
			wWW.LoadImageIntoTexture(texture2D);
			texture2D.Apply();
			Texture2D texture2D2 = new Texture2D(32, 32, TextureFormat.RGBA32, false);
			string url2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), iconName + "_on.png");
			wWW = new WWW(url2);
			wWW.LoadImageIntoTexture(texture2D2);
			texture2D2.Apply();
			wWW.Dispose();
			return new Icon(iconName + "Icon", texture2D, texture2D2, false);
		}
	}
}
