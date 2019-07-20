//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple example script of how a button can be colored when the mouse hovers over it or it gets pressed.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Button Color")]
public class UIButtonColor : UIWidgetContainer
{
	/// <summary>
	/// Target with a widget, renderer, or light that will have its color tweened.
	/// </summary>

	public GameObject tweenTarget;

	/// <summary>
	/// Color to apply on hover event (mouse only).
	/// </summary>

	public Color hover = new Color(225f / 255f, 200f / 255f, 150f / 255f, 1f);

	/// <summary>
	/// Color to apply on the pressed event.
	/// </summary>

	public Color pressed = new Color(183f / 255f, 163f / 255f, 123f / 255f, 1f);

	/// <summary>
	/// Duration of the tween process.
	/// </summary>

	public float duration = 0.2f;

	protected Color mColor;
	protected bool mStarted = false;
	protected UIWidget mWidget;

	/// <summary>
	/// UIButtonColor's default (starting) color. It's useful to be able to change it, just in case.
	/// </summary>

	public Color defaultColor
	{
		get
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return Color.white;
#endif
			Start();
			return mColor;
		}
		set
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif
			Start();
			mColor = value;
		}
	}

	void Start ()
	{
		if (!mStarted)
		{
			mStarted = true;
			Init();
		}
	}

	protected virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (mStarted) OnHover(UICamera.IsHighlighted(gameObject));
		
		if (UICamera.currentTouch != null)
		{
			if (UICamera.currentTouch.pressed == gameObject) OnPress(true);
			else if (UICamera.currentTouch.current == gameObject) OnHover(true);
		}
	}

	protected virtual void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (mStarted && tweenTarget != null)
		{
			TweenColor tc = tweenTarget.GetComponent<TweenColor>();

			if (tc != null)
			{
				tc.value = mColor;
				tc.enabled = false;
			}
		}
	}

	protected void Init ()
	{
		if (tweenTarget == null) tweenTarget = gameObject;
		mWidget = tweenTarget.GetComponent<UIWidget>();

		if (mWidget != null)
		{
			mColor = mWidget.color;
		}
		else
		{
			Renderer ren = tweenTarget.renderer;

			if (ren != null)
			{
				mColor = Application.isPlaying ? ren.material.color : ren.sharedMaterial.color;
			}
			else
			{
				Light lt = tweenTarget.light;

				if (lt != null)
				{
					mColor = lt.color;
				}
				else
				{
					tweenTarget = null;

					if (Application.isPlaying)
					{
						Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " has nothing for UIButtonColor to color", this);
						enabled = false;
					}
				}
			}
		}
		OnEnable();
	}

	protected virtual void OnPress (bool isPressed)
	{
		if (enabled && UICamera.currentTouch != null)
		{
			if (!mStarted) Start();
			
			if (isPressed)
			{
				TweenColor.Begin(tweenTarget, duration, pressed);
			}
			else if (UICamera.currentTouch.current == gameObject && UICamera.currentScheme == UICamera.ControlScheme.Controller)
			{
				TweenColor.Begin(tweenTarget, duration, hover);
			}
			else TweenColor.Begin(tweenTarget, duration, mColor);
		}
	}

	protected virtual void OnHover (bool isOver)
	{
		if (enabled)
		{
			if (!mStarted) Start();
			TweenColor.Begin(tweenTarget, duration, isOver ? hover : mColor);
		}
	}

	protected virtual void OnDragOver ()
	{
		if (enabled)
		{
			if (!mStarted) Start();
			TweenColor.Begin(tweenTarget, duration, pressed);
		}
	}

	protected virtual void OnDragOut ()
	{
		if (enabled)
		{
			if (!mStarted) Start();
			TweenColor.Begin(tweenTarget, duration, mColor);
		}
	}

	protected virtual void OnSelect (bool isSelected)
	{
		if (enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
			OnHover(isSelected);
	}
}
