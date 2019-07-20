//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(UIButton))]
#else
[CustomEditor(typeof(UIButton), true)]
#endif
public class UIButtonEditor : UIWidgetContainerEditor
{
	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

		NGUIEditorTools.SetLabelWidth(80f);
		UIButton button = target as UIButton;

		GUILayout.Space(6f);

		if (!serializedObject.isEditingMultipleObjects)
		{
			GUI.changed = false;
			GameObject tt = (GameObject)EditorGUILayout.ObjectField("Target", button.tweenTarget, typeof(GameObject), true);

			if (GUI.changed)
			{
				NGUIEditorTools.RegisterUndo("Button Change", button);
				button.tweenTarget = tt;
				UnityEditor.EditorUtility.SetDirty(button);
			}

			if (tt != null)
			{
				UIWidget w = tt.GetComponent<UIWidget>();

				if (w != null)
				{
					GUI.changed = false;
					Color c = EditorGUILayout.ColorField("Normal", w.color);

					if (GUI.changed)
					{
						NGUIEditorTools.RegisterUndo("Button Change", w);
						w.color = c;
						UnityEditor.EditorUtility.SetDirty(w);
					}
				}
			}
		}

		NGUIEditorTools.DrawProperty("Hover", serializedObject, "hover");
		NGUIEditorTools.DrawProperty("Pressed", serializedObject, "pressed");
		NGUIEditorTools.DrawProperty("Disabled", serializedObject, "disabledColor");

		SerializedProperty sp = serializedObject.FindProperty("dragHighlight");
		Highlight ht = sp.boolValue ? Highlight.Press : Highlight.DoNothing;
		GUILayout.BeginHorizontal();
		bool highlight = (Highlight)EditorGUILayout.EnumPopup("Drag Over", ht) == Highlight.Press;
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();
		if (sp.boolValue != highlight) sp.boolValue = highlight;

		GUILayout.BeginHorizontal();
		NGUIEditorTools.DrawProperty("Transition", serializedObject, "duration", GUILayout.Width(120f));
		GUILayout.Label("seconds");
		GUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties();

		GUILayout.Space(3f);

		NGUIEditorTools.DrawEvents("On Click", button, button.onClick);
	}

	enum Highlight
	{
		DoNothing,
		Press,
	}
}
