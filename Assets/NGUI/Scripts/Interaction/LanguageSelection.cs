//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Turns the popup list it's attached to into a language selection list.
/// </summary>

[RequireComponent(typeof(UIPopupList))]
[AddComponentMenu("NGUI/Interaction/Language Selection")]
public class LanguageSelection : MonoBehaviour
{
	UIPopupList mList;

	void Start ()
	{
		mList = GetComponent<UIPopupList>();

		if (Localization.instance != null && Localization.instance.languages != null && Localization.instance.languages.Length > 0)
		{
			mList.items.Clear();

			for (int i = 0, imax = Localization.instance.languages.Length; i < imax; ++i)
			{
				TextAsset asset = Localization.instance.languages[i];
				if (asset != null) mList.items.Add(asset.name);
			}
			mList.value = Localization.instance.currentLanguage;
		}
		EventDelegate.Add(mList.onChange, OnChange);
	}

	void OnChange ()
	{
		if (Localization.instance != null)
		{
			Localization.instance.currentLanguage = UIPopupList.current.value;
		}
	}
}
