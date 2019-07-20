//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UIImageButton))]
public class UIImageButtonInspector : Editor
{
	UIImageButton mButton;
	UISprite mSprite;

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (Object obj)
	{
		if (mButton.target != null)
		{
			NGUIEditorTools.RegisterUndo("Atlas Selection", mButton.target);
			mButton.target.atlas = obj as UIAtlas;
			mButton.target.MakePixelPerfect();
		}
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();
		NGUIEditorTools.SetLabelWidth(80f);
		mButton = target as UIImageButton;
		mSprite = EditorGUILayout.ObjectField("Sprite", mButton.target, typeof(UISprite), true) as UISprite;

		if (mButton.target != mSprite)
		{
			NGUIEditorTools.RegisterUndo("Image Button Change", mButton);
			mButton.target = mSprite;
			if (mSprite != null) mSprite.spriteName = mButton.normalSprite;
		}

		if (mSprite != null)
		{
			ComponentSelector.Draw<UIAtlas>(mSprite.atlas, OnSelectAtlas, true);

			if (mSprite.atlas != null)
			{
				NGUIEditorTools.DrawSpriteField("Normal", mSprite.atlas, mButton.normalSprite, OnNormal);
				NGUIEditorTools.DrawSpriteField("Hover", mSprite.atlas, mButton.hoverSprite, OnHover);
				NGUIEditorTools.DrawSpriteField("Pressed", mSprite.atlas, mButton.pressedSprite, OnPressed);
				NGUIEditorTools.DrawSpriteField("Disabled", mSprite.atlas, mButton.disabledSprite, OnDisabled);
				NGUIEditorTools.DrawProperty("Pixel Snap", serializedObject, "pixelSnap");
			}
		}
		serializedObject.ApplyModifiedProperties();
	}

	void OnNormal (string spriteName)
	{
		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
		mButton.normalSprite = spriteName;
		mSprite.spriteName = spriteName;
		mSprite.MakePixelPerfect();
		if (mButton.collider == null || (mButton.collider is BoxCollider)) NGUITools.AddWidgetCollider(mButton.gameObject);
		Repaint();
	}

	void OnHover (string spriteName)
	{
		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
		mButton.hoverSprite = spriteName;
		Repaint();
	}

	void OnPressed (string spriteName)
	{
		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
		mButton.pressedSprite = spriteName;
		Repaint();
	}
	
	void OnDisabled(string spriteName)
	{
		NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, mSprite);
		mButton.disabledSprite = spriteName;
		Repaint();
	}
}
