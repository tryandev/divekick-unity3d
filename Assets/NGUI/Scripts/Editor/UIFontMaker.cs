//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Font maker lets you create font prefabs with a single click of a button.
/// </summary>

public class UIFontMaker : EditorWindow
{
	enum FontType
	{
		Bitmap,
		Dynamic,
	}

	FontType mType = FontType.Bitmap;

	/// <summary>
	/// Update all labels associated with this font.
	/// </summary>

	void MarkAsChanged ()
	{
		if (NGUISettings.ambigiousFont != null)
		{
			List<UILabel> labels = NGUIEditorTools.FindAll<UILabel>();

			foreach (UILabel lbl in labels)
			{
				if (lbl.ambigiousFont == NGUISettings.ambigiousFont)
				{
					lbl.ambigiousFont = null;
					lbl.ambigiousFont = NGUISettings.ambigiousFont;
				}
			}
		}
	}

	/// <summary>
	/// Font selection callback.
	/// </summary>

	void OnSelectFont (Object obj)
	{
		NGUISettings.ambigiousFont = obj;
		Repaint();
	}

	/// <summary>
	/// Atlas selection callback.
	/// </summary>

	void OnSelectAtlas (Object obj)
	{
		NGUISettings.atlas = obj as UIAtlas;
		Repaint();
	}

	/// <summary>
	/// Refresh the window on selection.
	/// </summary>

	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Convenience function.
	/// </summary>

	static string fontName
	{
		get { return NGUISettings.GetString("NGUI Font Name", null); }
		set { NGUISettings.SetString("NGUI Font Name", value); }
	}

	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	void OnGUI ()
	{
		string prefabPath = "";
		string matPath = "";

		Object fnt = NGUISettings.ambigiousFont;
		UIFont bf = (fnt as UIFont);

		if (bf != null && bf.name == fontName)
		{
			prefabPath = AssetDatabase.GetAssetPath(bf.gameObject.GetInstanceID());
			if (bf.material != null) matPath = AssetDatabase.GetAssetPath(bf.material.GetInstanceID());
		}

		// Assume default values if needed
		if (string.IsNullOrEmpty(fontName)) fontName = "New Font";
		if (string.IsNullOrEmpty(prefabPath)) prefabPath = NGUIEditorTools.GetSelectionFolder() + fontName + ".prefab";
		if (string.IsNullOrEmpty(matPath)) matPath = NGUIEditorTools.GetSelectionFolder() + fontName + ".mat";

		NGUIEditorTools.SetLabelWidth(80f);
		NGUIEditorTools.DrawHeader("Input", true);
		NGUIEditorTools.BeginContents();

		GUILayout.BeginHorizontal();
		mType = (FontType)EditorGUILayout.EnumPopup("Type", mType, GUILayout.MinWidth(200f));
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();
		int create = 0;

		if (mType == FontType.Dynamic)
		{
#if UNITY_3_5
			EditorGUILayout.HelpBox("Unity 3 doesn't support dynamic fonts.", MessageType.Error);
#else
			EditorGUILayout.HelpBox("You no longer need to create a UIFont for dynamic fonts. Just reference the True Type font directly on your labels.", MessageType.Info);
#endif
		}
		else
		{
			NGUISettings.fontData = EditorGUILayout.ObjectField("Font Data", NGUISettings.fontData, typeof(TextAsset), false) as TextAsset;
			NGUISettings.fontTexture = EditorGUILayout.ObjectField("Texture", NGUISettings.fontTexture, typeof(Texture2D), false) as Texture2D;
			NGUIEditorTools.EndContents();

			// Draw the atlas selection only if we have the font data and texture specified, just to make it easier
			if (NGUISettings.fontData != null && NGUISettings.fontTexture != null)
			{
				NGUIEditorTools.DrawHeader("Output", true);
				NGUIEditorTools.BeginContents();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Font Name", GUILayout.Width(76f));
				GUI.backgroundColor = Color.white;
				fontName = GUILayout.TextField(fontName);
				GUILayout.EndHorizontal();

				ComponentSelector.Draw<UIFont>("Select", bf, OnSelectFont, true);
				ComponentSelector.Draw<UIAtlas>(NGUISettings.atlas, OnSelectAtlas, true);
				NGUIEditorTools.EndContents();
			}

			// Helpful info
			if (NGUISettings.fontData == null)
			{
				EditorGUILayout.HelpBox("The bitmap font creation mostly takes place outside of Unity. You can use BMFont on " +
					"Windows or your choice of Glyph Designer or the less expensive bmGlyph on the Mac.\n\n" +
					"Either of these tools will create a FNT file for you that you will drag & drop into the field above.", MessageType.Info);
			}
			else if (NGUISettings.fontTexture == null)
			{
				EditorGUILayout.HelpBox("When exporting your font, you should get two files: the TXT, and the texture. Only one texture can be used per font.", MessageType.Info);
			}
			else if (NGUISettings.atlas == null)
			{
				EditorGUILayout.HelpBox("You can create a font that doesn't use a texture atlas. This will mean that the text " +
					"labels using this font will generate an extra draw call, and will need to be sorted by " +
					"adjusting the Z instead of the Depth.\n\nIf you do specify an atlas, the font's texture will be added to it automatically.", MessageType.Info);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUI.backgroundColor = Color.red;
				if (GUILayout.Button("Create a Font without an Atlas", GUILayout.Width(200f))) create = 2;
				GUI.backgroundColor = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

				if (go != null)
				{
					if (go.GetComponent<UIFont>() != null)
					{
						GUI.backgroundColor = Color.red;
						if (GUILayout.Button("Replace the Font", GUILayout.Width(140f))) create = 3;
					}
					else
					{
						GUI.backgroundColor = Color.grey;
						GUILayout.Button("Rename Your Font", GUILayout.Width(140f));
					}
				}
				else
				{
					GUI.backgroundColor = Color.green;
					if (GUILayout.Button("Create the Font", GUILayout.Width(140f))) create = 3;
				}
				GUI.backgroundColor = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}

		if (create != 0)
		{
			GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

			if (go == null || EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to replace the contents of the " +
				fontName + " font with the currently selected values? This action can't be undone.", "Yes", "No"))
			{
				// Try to load the material
				Material mat = null;
				
				// Non-atlased font
				if (create == 2)
				{
					mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

					// If the material doesn't exist, create it
					if (mat == null)
					{
						Shader shader = Shader.Find("Unlit/Transparent Colored");
						mat = new Material(shader);

						// Save the material
						AssetDatabase.CreateAsset(mat, matPath);
						AssetDatabase.Refresh();

						// Load the material so it's usable
						mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
					}
					mat.mainTexture = NGUISettings.fontTexture;
				}
				else if (create != 1)
				{
					UIAtlasMaker.AddOrUpdate(NGUISettings.atlas, NGUISettings.fontTexture);
				}

				// Font doesn't exist yet
				if (go == null || go.GetComponent<UIFont>() == null)
				{
					// Create a new prefab for the atlas
					Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);

					// Create a new game object for the font
					go = new GameObject(fontName);
					bf = go.AddComponent<UIFont>();
					CreateFont(bf, create, mat);

					// Update the prefab
					PrefabUtility.ReplacePrefab(go, prefab);
					DestroyImmediate(go);
					AssetDatabase.Refresh();

					// Select the atlas
					go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
					bf = go.GetComponent<UIFont>();
					NGUISettings.ambigiousFont = bf;
				}
				else
				{
					bf = go.GetComponent<UIFont>();
					CreateFont(bf, create, mat);
					NGUISettings.ambigiousFont = bf;
				}
				MarkAsChanged();
			}
		}
	}

	static void CreateFont (UIFont font, int create, Material mat)
	{
		if (create == 1)
		{
			// New dynamic font
			//font.atlas = null;
			//font.dynamicFont = NGUISettings.trueTypeFont;
			//font.dynamicFontStyle = NGUISettings.fontStyle;
			Debug.LogError("Creating UIFont for dynamic fonts is no longer needed. Reference the font directly on your label.");
		}
		else
		{
			// New bitmap font
			font.dynamicFont = null;
			BMFontReader.Load(font.bmFont, NGUITools.GetHierarchy(font.gameObject), NGUISettings.fontData.bytes);

			if (create == 2)
			{
				font.atlas = null;
				font.material = mat;
			}
			else if (create == 3)
			{
				font.spriteName = NGUISettings.fontTexture.name;
				font.atlas = NGUISettings.atlas;
			}
		}
	}
}
