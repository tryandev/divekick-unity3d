//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Attaching this script to an element of a scroll view will make it possible to center on it by clicking on it.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Center Scroll View on Click")]
public class UICenterOnClick : MonoBehaviour
{
	UIPanel mPanel;
	UICenterOnChild mCenter;

	void Start ()
	{
		mCenter = NGUITools.FindInParents<UICenterOnChild>(gameObject);
		mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
	}

	void OnClick ()
	{
		if (mCenter != null)
		{
			if (mCenter.enabled)
				mCenter.CenterOn(transform);
		}
		else if (mPanel != null && mPanel.clipping != UIDrawCall.Clipping.None)
		{
			SpringPanel.Begin(mPanel.cachedGameObject, mPanel.cachedTransform.InverseTransformPoint(transform.position), 6f);
		}
	}
}
