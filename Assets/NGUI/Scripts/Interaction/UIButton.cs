//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Similar to UIButtonColor, but adds a 'disabled' state based on whether the collider is enabled or not.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button")]
public class UIButton : UIButtonColor
{
	/// <summary>
	/// Current button that sent out the onClick event.
	/// </summary>

	static public UIButton current;

	/// <summary>
	/// Color that will be applied when the button is disabled.
	/// </summary>

	public Color disabledColor = Color.grey;

	/// <summary>
	/// Whether the button will highlight when you drag something over it.
	/// </summary>

	public bool dragHighlight = false;

	/// <summary>
	/// Click event listener.
	/// </summary>

	public List<EventDelegate> onClick = new List<EventDelegate>();

	/// <summary>
	/// Whether the button should be enabled.
	/// </summary>

	public virtual bool isEnabled
	{
		get
		{
			if (!enabled) return false;
			Collider col = collider;
			return col && col.enabled;
		}
		set
		{
			Collider col = collider;
			if (col != null) col.enabled = value;
			else enabled = value;
			UpdateColor(value, false);
		}
	}

	protected override void OnEnable ()
	{
		if (isEnabled)
		{
			if (mStarted)
			{
				if (UICamera.currentScheme == UICamera.ControlScheme.Controller)
				{
					OnHover(UICamera.selectedObject == gameObject);
				}
				else if (UICamera.currentScheme == UICamera.ControlScheme.Mouse)
				{
					OnHover(UICamera.hoveredObject == gameObject);
				}
				else UpdateColor(true, false);
			}
		}
		else UpdateColor(false, true);
	}

	protected override void OnHover (bool isOver)
	{
		if (isEnabled)
			base.OnHover(isOver);
	}
	
	protected override void OnPress (bool isPressed)
	{
		if (isEnabled)
			base.OnPress(isPressed);
	}
	
	protected override void OnDragOver ()
	{
		if (isEnabled && (dragHighlight || UICamera.currentTouch.pressed == gameObject))
			base.OnDragOver();
	}
	
	protected override void OnDragOut ()
	{
		if (isEnabled && (dragHighlight || UICamera.currentTouch.pressed == gameObject))
			base.OnDragOut();
	}

	protected override void OnSelect (bool isSelected)
	{
		if (isEnabled)
			base.OnSelect(isSelected);
	}

	/// <summary>
	/// Call the listener function.
	/// </summary>

	protected virtual void OnClick ()
	{
		if (isEnabled)
		{
			current = this;
			EventDelegate.Execute(onClick);
			current = null;
		}
	}

	/// <summary>
	/// Update the button's color to either enabled or disabled state.
	/// </summary>

	public void UpdateColor (bool shouldBeEnabled, bool immediate)
	{
		if (tweenTarget)
		{
			if (!mStarted)
			{
				mStarted = true;
				Init();
			}

			Color c = shouldBeEnabled ? defaultColor : disabledColor;
			TweenColor tc = TweenColor.Begin(tweenTarget, 0.15f, c);

			if (tc != null && immediate)
			{
				tc.value = c;
				tc.enabled = false;
			}
		}
	}
}
