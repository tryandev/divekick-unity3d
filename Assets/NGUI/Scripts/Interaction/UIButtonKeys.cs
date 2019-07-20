//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Attaching this script to a widget makes it react to key events such as tab, up, down, etc.
/// </summary>

[RequireComponent(typeof(Collider))]
[AddComponentMenu("NGUI/Interaction/Button Keys")]
public class UIButtonKeys : MonoBehaviour
{
	public bool startsSelected = false;
	public UIButtonKeys selectOnClick;
	public UIButtonKeys selectOnUp;
	public UIButtonKeys selectOnDown;
	public UIButtonKeys selectOnLeft;
	public UIButtonKeys selectOnRight;

	void OnEnable ()
	{
		if (startsSelected)
		{
			if (UICamera.selectedObject == null || !NGUITools.GetActive(UICamera.selectedObject))
			{
				UICamera.currentScheme = UICamera.ControlScheme.Controller;
				UICamera.selectedObject = gameObject;
			}
		}
	}

	void OnKey (KeyCode key)
	{
		if (NGUITools.GetActive(this))
		{
			switch (key)
			{
			case KeyCode.LeftArrow:
				if (NGUITools.GetActive(selectOnLeft)) UICamera.selectedObject = selectOnLeft.gameObject;
				break;
			case KeyCode.RightArrow:
				if (NGUITools.GetActive(selectOnRight)) UICamera.selectedObject = selectOnRight.gameObject;
				break;
			case KeyCode.UpArrow:
				if (NGUITools.GetActive(selectOnUp)) UICamera.selectedObject = selectOnUp.gameObject;
				break;
			case KeyCode.DownArrow:
				if (NGUITools.GetActive(selectOnDown)) UICamera.selectedObject = selectOnDown.gameObject;
				break;
			case KeyCode.Tab:
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					if (NGUITools.GetActive(selectOnLeft)) UICamera.selectedObject = selectOnLeft.gameObject;
					else if (NGUITools.GetActive(selectOnUp)) UICamera.selectedObject = selectOnUp.gameObject;
					else if (NGUITools.GetActive(selectOnDown)) UICamera.selectedObject = selectOnDown.gameObject;
					else if (NGUITools.GetActive(selectOnRight)) UICamera.selectedObject = selectOnRight.gameObject;
				}
				else
				{
					if (NGUITools.GetActive(selectOnRight)) UICamera.selectedObject = selectOnRight.gameObject;
					else if (NGUITools.GetActive(selectOnDown)) UICamera.selectedObject = selectOnDown.gameObject;
					else if (NGUITools.GetActive(selectOnUp)) UICamera.selectedObject = selectOnUp.gameObject;
					else if (NGUITools.GetActive(selectOnLeft)) UICamera.selectedObject = selectOnLeft.gameObject;
				}
				break;
			}
		}
	}

	void OnClick ()
	{
		if (NGUITools.GetActive(this) && NGUITools.GetActive(selectOnClick))
			UICamera.selectedObject = selectOnClick.gameObject;
	}
}
