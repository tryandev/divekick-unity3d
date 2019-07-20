//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

#if UNITY_3_5
[CustomEditor(typeof(UIButtonColor))]
#else
[CustomEditor(typeof(UIButtonColor), true)]
#endif
public class UIButtonColorEditor : UIWidgetContainerEditor
{
	public override void OnInspectorGUI ()
	{
		NGUIEditorTools.SetLabelWidth(80f);
		UIButtonColor button = target as UIButtonColor;

		GUILayout.Space(6f);

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

		GUI.changed = false;
		Color hover = EditorGUILayout.ColorField("Hover", button.hover);
		Color pressed = EditorGUILayout.ColorField("Pressed", button.pressed);

		GUILayout.BeginHorizontal();
		float duration = EditorGUILayout.FloatField("Duration", button.duration, GUILayout.Width(120f));
		GUILayout.Label("seconds");
		GUILayout.EndHorizontal();

		GUILayout.Space(3f);

		if (GUI.changed)
		{
			NGUIEditorTools.RegisterUndo("Button Change", button);
			button.hover = hover;
			button.pressed = pressed;
			button.duration = duration;
			UnityEditor.EditorUtility.SetDirty(button);
		}

		if (GUILayout.Button("Upgrade to a Button"))
		{
			NGUIEditorTools.ReplaceClass(serializedObject, typeof(UIButton));
			Selection.activeGameObject = null;
		}
	}
}
