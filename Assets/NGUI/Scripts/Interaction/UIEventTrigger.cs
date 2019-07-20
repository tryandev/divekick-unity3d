//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attaching this script to an object will let you trigger remote functions using NGUI events.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Event Trigger")]
public class UIEventTrigger : MonoBehaviour
{
	static public UIEventTrigger current;

	public List<EventDelegate> onHoverOver = new List<EventDelegate>();
	public List<EventDelegate> onHoverOut = new List<EventDelegate>();
	public List<EventDelegate> onPress = new List<EventDelegate>();
	public List<EventDelegate> onRelease = new List<EventDelegate>();
	public List<EventDelegate> onSelect = new List<EventDelegate>();
	public List<EventDelegate> onDeselect = new List<EventDelegate>();
	public List<EventDelegate> onClick = new List<EventDelegate>();
	public List<EventDelegate> onDoubleClick = new List<EventDelegate>();

	void OnHover (bool isOver)
	{
		current = this;
		if (isOver) EventDelegate.Execute(onHoverOver);
		else EventDelegate.Execute(onHoverOut);
		current = null;
	}

	void OnPress (bool pressed)
	{
		current = this;
		if (pressed) EventDelegate.Execute(onPress);
		else EventDelegate.Execute(onRelease);
		current = null;
	}

	void OnSelect (bool selected)
	{
		current = this;
		if (selected) EventDelegate.Execute(onSelect);
		else EventDelegate.Execute(onDeselect);
		current = null;
	}

	void OnClick ()
	{
		current = this;
		EventDelegate.Execute(onClick);
		current = null;
	}

	void OnDoubleClick ()
	{
		current = this;
		EventDelegate.Execute(onDoubleClick);
		current = null;
	}
}
