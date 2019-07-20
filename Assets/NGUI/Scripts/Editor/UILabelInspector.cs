//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_3_5 && !UNITY_FLASH
#define DYNAMIC_FONT
#endif

using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(UILabel))]
#else
[CustomEditor(typeof(UILabel), true)]
#endif
public class UILabelInspector : UIWidgetInspector
{
	public enum FontType
	{
		Bitmap,
		Dynamic,
	}

	UILabel mLabel;
	FontType mFontType;

	protected override void OnEnable ()
	{
		base.OnEnable();
		SerializedProperty bit = serializedObject.FindProperty("mFont");
		mFontType = (bit != null && bit.objectReferenceValue != null) ? FontType.Bitmap : FontType.Dynamic;
	}

	void OnBitmapFont (Object obj)
	{
		serializedObject.Update();
		SerializedProperty sp = serializedObject.FindProperty("mFont");
		sp.objectReferenceValue = obj;
		serializedObject.ApplyModifiedProperties();
		NGUISettings.ambigiousFont = obj;
	}

	void OnDynamicFont (Object obj)
	{
		serializedObject.Update();
		SerializedProperty sp = serializedObject.FindProperty("mTrueTypeFont");
		sp.objectReferenceValue = obj;
		serializedObject.ApplyModifiedProperties();
		NGUISettings.ambigiousFont = obj;
	}

	/// <summary>
	/// Draw the label's properties.
	/// </summary>

	protected override bool ShouldDrawProperties ()
	{
		mLabel = mWidget as UILabel;

		GUILayout.BeginHorizontal();
		
		if (NGUIEditorTools.DrawPrefixButton("Font"))
		{
			if (mFontType == FontType.Bitmap)
			{
				ComponentSelector.Show<UIFont>(OnBitmapFont);
			}
			else
			{
				ComponentSelector.Show<Font>(OnDynamicFont, new string[] { ".ttf", ".otf" });
			}
		}

#if DYNAMIC_FONT
		mFontType = (FontType)EditorGUILayout.EnumPopup(mFontType, GUILayout.Width(62f));
#else
		mFontType = FontType.Bitmap;
#endif
		bool isValid = false;
		SerializedProperty fnt = null;
		SerializedProperty ttf = null;

		if (mFontType == FontType.Bitmap)
		{
			fnt = NGUIEditorTools.DrawProperty("", serializedObject, "mFont", GUILayout.MinWidth(40f));
			
			if (fnt.objectReferenceValue != null)
			{
				NGUISettings.ambigiousFont = fnt.objectReferenceValue;
				isValid = true;
			}
		}
		else
		{
			ttf = NGUIEditorTools.DrawProperty("", serializedObject, "mTrueTypeFont", GUILayout.MinWidth(40f));

			if (ttf.objectReferenceValue != null)
			{
				NGUISettings.ambigiousFont = ttf.objectReferenceValue;
				isValid = true;
			}
		}

		GUILayout.EndHorizontal();

		EditorGUI.BeginDisabledGroup(!isValid);
		{
			if (ttf != null && ttf.objectReferenceValue != null)
			{
				GUILayout.BeginHorizontal();
				{
					EditorGUI.BeginDisabledGroup(ttf.hasMultipleDifferentValues);
					
					SerializedProperty prop = NGUIEditorTools.DrawProperty("Font Size", serializedObject, "mFontSize", GUILayout.Width(142f));
					NGUISettings.fontSize = prop.intValue;
					
					prop = NGUIEditorTools.DrawProperty("", serializedObject, "mFontStyle", GUILayout.MinWidth(40f));
					NGUISettings.fontStyle = (FontStyle)prop.intValue;
					
					GUILayout.Space(18f);
					EditorGUI.EndDisabledGroup();
				}
				GUILayout.EndHorizontal();

				NGUIEditorTools.DrawProperty("Material", serializedObject, "mMaterial");
			}
			else if (fnt != null)
			{
				GUILayout.BeginHorizontal();
				SerializedProperty prop = NGUIEditorTools.DrawProperty("Font Size", serializedObject, "mFontSize", GUILayout.Width(142f));

				EditorGUI.BeginDisabledGroup(true);
				if (!serializedObject.isEditingMultipleObjects)
					GUILayout.Label(" Default: " + mLabel.defaultFontSize);
				EditorGUI.EndDisabledGroup();

				NGUISettings.fontSize = prop.intValue;
				GUILayout.EndHorizontal();
			}

			bool ww = GUI.skin.textField.wordWrap;
			GUI.skin.textField.wordWrap = true;
#if UNITY_3_5
			GUI.changed = false;
			SerializedProperty textField = serializedObject.FindProperty("mText");
			string text = EditorGUILayout.TextArea(textField.stringValue, GUI.skin.textArea, GUILayout.Height(100f));
			if (GUI.changed) textField.stringValue = text;
#else
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			GUILayout.Space(-16f);
#endif
			GUILayout.BeginHorizontal();
			GUILayout.Space(4f);
			NGUIEditorTools.DrawProperty("", serializedObject, "mText", GUILayout.Height(80f));
			GUILayout.Space(4f);
			GUILayout.EndHorizontal();
#endif
			GUI.skin.textField.wordWrap = ww;
			SerializedProperty ov = NGUIEditorTools.DrawProperty("Overflow", serializedObject, "mOverflow");
			NGUISettings.overflowStyle = (UILabel.Overflow)ov.intValue;

			if (ttf != null && ttf.objectReferenceValue != null)
				NGUIEditorTools.DrawProperty("Keep crisp", serializedObject, "keepCrispWhenShrunk");

			GUILayout.BeginHorizontal();
			GUILayout.Label("Spacing", GUILayout.Width(56f));
			NGUIEditorTools.SetLabelWidth(20f);
			NGUIEditorTools.DrawProperty("X", serializedObject, "mSpacingX", GUILayout.MinWidth(40f));
			NGUIEditorTools.DrawProperty("Y", serializedObject, "mSpacingY", GUILayout.MinWidth(40f));
			GUILayout.Space(18f);
			NGUIEditorTools.SetLabelWidth(80f);
			GUILayout.EndHorizontal();

			NGUIEditorTools.DrawProperty("Max Lines", serializedObject, "mMaxLineCount", GUILayout.Width(110f));

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Encoding", serializedObject, "mEncoding", GUILayout.Width(100f));
			GUILayout.Label("use emoticons and colors");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			SerializedProperty gr = NGUIEditorTools.DrawProperty("Gradient", serializedObject, "mApplyGradient", GUILayout.Width(100f));
			if (gr.hasMultipleDifferentValues || gr.boolValue)
			{
				NGUIEditorTools.DrawProperty("", serializedObject, "mGradientBottom", GUILayout.MinWidth(40f));
				NGUIEditorTools.DrawProperty("", serializedObject, "mGradientTop", GUILayout.MinWidth(40f));
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(4f);

			if (mLabel.supportEncoding && mLabel.bitmapFont != null && mLabel.bitmapFont.hasSymbols)
				NGUIEditorTools.DrawProperty("Symbols", serializedObject, "mSymbols");

			GUILayout.BeginHorizontal();
			SerializedProperty sp = NGUIEditorTools.DrawProperty("Effect", serializedObject, "mEffectStyle", GUILayout.MinWidth(170f));
			GUILayout.Space(18f);
			GUILayout.EndHorizontal();

			if (sp.hasMultipleDifferentValues || sp.boolValue)
				NGUIEditorTools.DrawProperty("Effect Color", serializedObject, "mEffectColor", GUILayout.MinWidth(30f));

			if (sp.hasMultipleDifferentValues || sp.boolValue)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Distance", GUILayout.Width(56f));
				NGUIEditorTools.SetLabelWidth(20f);
				NGUIEditorTools.DrawProperty("X", serializedObject, "mEffectDistance.x", GUILayout.MinWidth(40f));
				NGUIEditorTools.DrawProperty("Y", serializedObject, "mEffectDistance.y", GUILayout.MinWidth(40f));
				GUILayout.Space(18f);
				NGUIEditorTools.SetLabelWidth(80f);
				GUILayout.EndHorizontal();
			}
		}
		EditorGUI.EndDisabledGroup();
		return isValid;
	}
}
