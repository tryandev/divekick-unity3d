//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

//#define SHOW_HIDDEN_OBJECTS

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Draw Call")]
public class UIDrawCall : MonoBehaviour
{
	static BetterList<UIDrawCall> mActiveList = new BetterList<UIDrawCall>();
	static BetterList<UIDrawCall> mInactiveList = new BetterList<UIDrawCall>();

	[System.Obsolete("Use UIDrawCall.activeList")]
	static public BetterList<UIDrawCall> list { get { return mActiveList; } }

	/// <summary>
	/// List of active draw calls.
	/// </summary>

	static public BetterList<UIDrawCall> activeList { get { return mActiveList; } }

	/// <summary>
	/// List of inactive draw calls. Only used at run-time in order to avoid object creation/destruction.
	/// </summary>

	static public BetterList<UIDrawCall> inactiveList { get { return mInactiveList; } }

	public enum Clipping : int
	{
		None = 0,
		AlphaClip = 2,				// Adjust the alpha, compatible with all devices
		SoftClip = 3,				// Alpha-based clipping with a softened edge
		ConstrainButDontClip = 4,	// No actual clipping, but does have an area
	}

	[HideInInspector]
	[System.NonSerialized]
	public int depthStart = int.MaxValue;

	[HideInInspector]
	[System.NonSerialized]
	public int depthEnd = int.MinValue;

	[HideInInspector]
	[System.NonSerialized]
	public UIPanel manager;

	[HideInInspector]
	[System.NonSerialized]
	public UIPanel panel;

	[HideInInspector]
	[System.NonSerialized]
	public bool alwaysOnScreen = false;

	Material		mMaterial;		// Material used by this screen
	Texture			mTexture;		// Main texture used by the material
	Shader			mShader;		// Shader used by the dynamically created material
	Clipping		mClipping;		// Clipping mode
	Vector4			mClipRange;		// Clipping, if used
	Vector2			mClipSoft;		// Clipping softness

	Transform		mTrans;			// Cached transform
	Mesh			mMesh;			// First generated mesh
	MeshFilter		mFilter;		// Mesh filter for this draw call
	MeshRenderer	mRenderer;		// Mesh renderer for this screen
	Material		mDynamicMat;	// Instantiated material
	int[]			mIndices;		// Cached indices

	bool mRebuildMat = true;
	bool mReset = true;
	int mRenderQueue = 3000;
	Clipping mLastClip = Clipping.None;
	int mTriangles = 0;

	/// <summary>
	/// Whether the draw call has changed recently.
	/// </summary>

	[System.NonSerialized]
	public bool isDirty = false;

	/// <summary>
	/// Render queue used by the draw call.
	/// </summary>

	public int renderQueue
	{
		get
		{
			return mRenderQueue;
		}
		set
		{
			if (mRenderQueue != value)
			{
				mRenderQueue = value;

				if (mDynamicMat != null)
				{
					mDynamicMat.renderQueue = value;
#if UNITY_EDITOR
					if (mRenderer != null) mRenderer.enabled = isActive;
#endif
				}
			}
		}
	}

	/// <summary>
	/// Final render queue used to draw the draw call's geometry.
	/// </summary>

	public int finalRenderQueue
	{
		get
		{
			return (mDynamicMat != null) ? mDynamicMat.renderQueue : mRenderQueue;
		}
	}

#if UNITY_EDITOR

	/// <summary>
	/// Whether the draw call is currently active.
	/// </summary>

	public bool isActive
	{
		get
		{
			return mActive;
		}
		set
		{
			if (mActive != value)
			{
				mActive = value;

				if (mRenderer != null)
				{
					mRenderer.enabled = value;
					UnityEditor.EditorUtility.SetDirty(gameObject);
				}
			}
		}
	}
	bool mActive = true;
#endif

	/// <summary>
	/// Transform is cached for speed and efficiency.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Material used by this screen.
	/// </summary>

	public Material baseMaterial
	{
		get
		{
			return mMaterial;
		}
		set
		{
			if (mMaterial != value)
			{
				mMaterial = value;
				mRebuildMat = true;
			}
		}
	}

	/// <summary>
	/// Dynamically created material used by the draw call to actually draw the geometry.
	/// </summary>

	public Material dynamicMaterial { get { return mDynamicMat; } }

	/// <summary>
	/// Texture used by the material.
	/// </summary>

	public Texture mainTexture
	{
		get
		{
			return mTexture;
		}
		set
		{
			mTexture = value;
			if (mDynamicMat != null) mDynamicMat.mainTexture = value;
		}
	}

	/// <summary>
	/// Shader used by the material.
	/// </summary>

	public Shader shader
	{
		get
		{
			return mShader;
		}
		set
		{
			if (mShader != value)
			{
				mShader = value;
				mRebuildMat = true;
			}
		}
	}

	/// <summary>
	/// The number of triangles in this draw call.
	/// </summary>

	public int triangles { get { return (mMesh != null) ? mTriangles : 0; } }

	/// <summary>
	/// Whether the draw call is currently using a clipped shader.
	/// </summary>

	public bool isClipped { get { return mClipping != Clipping.None; } }

	/// <summary>
	/// Clipping used by the draw call
	/// </summary>

	public Clipping clipping { get { return mClipping; } set { if (mClipping != value) { mClipping = value; mReset = true; } } }

	/// <summary>
	/// Clip range set by the panel -- used with a shader that has the "_ClipRange" property.
	/// </summary>

	public Vector4 clipRange { get { return mClipRange; } set { mClipRange = value; } }

	/// <summary>
	/// Clipping softness factor, if soft clipping is used.
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoft; } set { mClipSoft = value; } }

	/// <summary>
	/// Create an appropriate material for the draw call.
	/// </summary>

	void CreateMaterial ()
	{
		const string alpha = " (AlphaClip)";
		const string soft = " (SoftClip)";
		string shaderName = (mShader != null) ? mShader.name :
			((mMaterial != null) ? mMaterial.shader.name : "Unlit/Transparent Colored");

		// Figure out the normal shader's name
		shaderName = shaderName.Replace("GUI/Text Shader", "Unlit/Text");
		shaderName = shaderName.Replace(alpha, "");
		shaderName = shaderName.Replace(soft, "");

		// Try to find the new shader
		Shader shader;

		if (mClipping == Clipping.SoftClip)
		{
			shader = Shader.Find(shaderName + soft);
		}
		else if (mClipping == Clipping.AlphaClip)
		{
			shader = Shader.Find(shaderName + alpha);
		}
		else // No clipping
		{
			shader = Shader.Find(shaderName);
		}

		if (mMaterial != null)
		{
			mDynamicMat = new Material(mMaterial);
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			mDynamicMat.CopyPropertiesFromMaterial(mMaterial);
		}
		else
		{
			mDynamicMat = new Material(shader);
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		}

		// If there is a valid shader, assign it to the custom material
		if (shader != null)
		{
			mDynamicMat.shader = shader;
		}
		else if (mClipping != Clipping.None)
		{
			Debug.LogError(shaderName + " doesn't have a clipped shader version for " + mClipping);
			mClipping = Clipping.None;
		}
	}

	/// <summary>
	/// Rebuild the draw call's material.
	/// </summary>

	Material RebuildMaterial ()
	{
		// Destroy the old material
		NGUITools.DestroyImmediate(mDynamicMat);

		// Create a new material
		CreateMaterial();

		mDynamicMat.renderQueue = mRenderQueue;
		mLastClip = mClipping;

		// Assign the main texture
		if (mTexture != null) mDynamicMat.mainTexture = mTexture;

		// Update the renderer
		if (mRenderer != null) mRenderer.sharedMaterials = new Material[] { mDynamicMat };
		return mDynamicMat;
	}

	/// <summary>
	/// Update the renderer's materials.
	/// </summary>

	void UpdateMaterials ()
	{
		// If clipping should be used, we need to find a replacement shader
		if (mRebuildMat || mDynamicMat == null || mClipping != mLastClip)
		{
			RebuildMaterial();
			mRebuildMat = false;
		}
		else if (mRenderer.sharedMaterial != mDynamicMat)
		{
#if UNITY_EDITOR
			Debug.LogError("Hmm... This point got hit!");
#endif
			mRenderer.sharedMaterials = new Material[] { mDynamicMat };
		}
	}

	/// <summary>
	/// Set the draw call's geometry.
	/// </summary>

	public void Set (
		BetterList<Vector3> verts,
		BetterList<Vector3> norms,
		BetterList<Vector4> tans,
		BetterList<Vector2> uvs,
		BetterList<Color32> cols)
	{
		int count = verts.size;

		// Safety check to ensure we get valid values
		if (count > 0 && (count == uvs.size && count == cols.size) && (count % 4) == 0)
		{
			// Cache all components
			if (mFilter == null) mFilter = gameObject.GetComponent<MeshFilter>();
			if (mFilter == null) mFilter = gameObject.AddComponent<MeshFilter>();

			if (verts.size < 65000)
			{
				// Populate the index buffer
				int indexCount = (count >> 1) * 3;
				bool setIndices = (mIndices == null || mIndices.Length != indexCount);

				// Create the mesh
				if (mMesh == null)
				{
					mMesh = new Mesh();
					mMesh.hideFlags = HideFlags.DontSave;
					mMesh.name = (mMaterial != null) ? mMaterial.name : "Mesh";
#if !UNITY_3_5
					mMesh.MarkDynamic();
#endif
					setIndices = true;
				}
#if !UNITY_FLASH
				// If the buffer length doesn't match, we need to trim all buffers
				bool trim = (uvs.buffer.Length != verts.buffer.Length) ||
					(cols.buffer.Length != verts.buffer.Length) ||
					(norms != null && norms.buffer.Length != verts.buffer.Length) ||
					(tans != null && tans.buffer.Length != verts.buffer.Length);

				// Non-automatic render queues rely on Z position, so it's a good idea to trim everything
				if (!trim && panel.renderQueue != UIPanel.RenderQueue.Automatic)
					trim = (mMesh == null || mMesh.vertexCount != verts.buffer.Length);

				mTriangles = (verts.size >> 1);

				if (trim || verts.buffer.Length > 65000)
				{
					if (trim || mMesh.vertexCount != verts.size)
					{
						mMesh.Clear();
						setIndices = true;
					}

					mMesh.vertices = verts.ToArray();
					mMesh.uv = uvs.ToArray();
					mMesh.colors32 = cols.ToArray();

					if (norms != null) mMesh.normals = norms.ToArray();
					if (tans != null) mMesh.tangents = tans.ToArray();
				}
				else
				{
					if (mMesh.vertexCount != verts.buffer.Length)
					{
						mMesh.Clear();
						setIndices = true;
					}

					mMesh.vertices = verts.buffer;
					mMesh.uv = uvs.buffer;
					mMesh.colors32 = cols.buffer;

					if (norms != null) mMesh.normals = norms.buffer;
					if (tans != null) mMesh.tangents = tans.buffer;
				}
#else
				mTriangles = (verts.size >> 1);

				if (mMesh.vertexCount != verts.size)
				{
					mMesh.Clear();
					setIndices = true;
				}

				mMesh.vertices = verts.ToArray();
				mMesh.uv = uvs.ToArray();
				mMesh.colors32 = cols.ToArray();

				if (norms != null) mMesh.normals = norms.ToArray();
				if (tans != null) mMesh.tangents = tans.ToArray();
#endif
				if (setIndices)
				{
					mIndices = GenerateCachedIndexBuffer(count, indexCount);
					mMesh.triangles = mIndices;
				}

#if !UNITY_FLASH
				if (trim || !alwaysOnScreen)
#endif
					mMesh.RecalculateBounds();

				mFilter.mesh = mMesh;
			}
			else
			{
				mTriangles = 0;
				if (mFilter.mesh != null) mFilter.mesh.Clear();
				Debug.LogError("Too many vertices on one panel: " + verts.size);
			}

			if (mRenderer == null) mRenderer = gameObject.GetComponent<MeshRenderer>();

			if (mRenderer == null)
			{
				mRenderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_EDITOR
				mRenderer.enabled = isActive;
#endif
			}
			UpdateMaterials();
		}
		else
		{
			if (mFilter.mesh != null) mFilter.mesh.Clear();
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + count);
		}
	}

	const int maxIndexBufferCache = 10;

#if UNITY_FLASH
	List<int[]> mCache = new List<int[]>(maxIndexBufferCache);
#else
	static List<int[]> mCache = new List<int[]>(maxIndexBufferCache);
#endif

	/// <summary>
	/// Generates a new index buffer for the specified number of vertices (or reuses an existing one).
	/// </summary>

	int[] GenerateCachedIndexBuffer (int vertexCount, int indexCount)
	{
		for (int i = 0, imax = mCache.Count; i < imax; ++i)
		{
			int[] ids = mCache[i];
			if (ids != null && ids.Length == indexCount)
				return ids;
		}

		int[] rv = new int[indexCount];
		int index = 0;

		for (int i = 0; i < vertexCount; i += 4)
		{
			rv[index++] = i;
			rv[index++] = i + 1;
			rv[index++] = i + 2;

			rv[index++] = i + 2;
			rv[index++] = i + 3;
			rv[index++] = i;
		}

		if (mCache.Count > maxIndexBufferCache) mCache.RemoveAt(0);
		mCache.Add(rv);
		return rv;
	}

	/// <summary>
	/// This function is called when it's clear that the object will be rendered.
	/// We want to set the shader used by the material, creating a copy of the material in the process.
	/// We also want to update the material's properties before it's actually used.
	/// </summary>

	void OnWillRenderObject ()
	{
		if (mReset)
		{
			mReset = false;
			UpdateMaterials();
		}

		if (mDynamicMat != null && isClipped && mClipping != Clipping.ConstrainButDontClip)
		{
			mDynamicMat.mainTextureOffset = new Vector2(-mClipRange.x / mClipRange.z, -mClipRange.y / mClipRange.w);
			mDynamicMat.mainTextureScale = new Vector2(1f / mClipRange.z, 1f / mClipRange.w);

			Vector2 sharpness = new Vector2(1000.0f, 1000.0f);
			if (mClipSoft.x > 0f) sharpness.x = mClipRange.z / mClipSoft.x;
			if (mClipSoft.y > 0f) sharpness.y = mClipRange.w / mClipSoft.y;
			mDynamicMat.SetVector("_ClipSharpness", sharpness);
		}
	}

	void OnEnable () { mRebuildMat = true; }

	/// <summary>
	/// Clear all references.
	/// </summary>

	void OnDisable ()
	{
		depthStart = int.MaxValue;
		depthEnd = int.MinValue;
		panel = null;
		manager = null;
		mMaterial = null;
		mTexture = null;

		NGUITools.DestroyImmediate(mDynamicMat);
		mDynamicMat = null;
	}

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy ()
	{
		NGUITools.DestroyImmediate(mMesh);
	}

	/// <summary>
	/// Return an existing draw call.
	/// </summary>

	static public UIDrawCall Create (UIPanel panel, Material mat, Texture tex, Shader shader)
	{
#if UNITY_EDITOR
		string name = null;
		if (tex != null) name = tex.name;
		else if (shader != null) name = shader.name;
		else if (mat != null) name = mat.name;
		return Create(name, panel, mat, tex, shader);
#else
		return Create(null, panel, mat, tex, shader);
#endif
	}

	/// <summary>
	/// Create a new draw call, reusing an old one if possible.
	/// </summary>

	static UIDrawCall Create (string name, UIPanel pan, Material mat, Texture tex, Shader shader)
	{
		UIDrawCall dc = Create(name);
		dc.gameObject.layer = pan.cachedGameObject.layer;
		dc.clipping = pan.clipping;
		dc.baseMaterial = mat;
		dc.mainTexture = tex;
		dc.shader = shader;
		dc.renderQueue = pan.startingRenderQueue;
		dc.manager = pan;
		return dc;
	}

	/// <summary>
	/// Create a new draw call, reusing an old one if possible.
	/// </summary>

	static UIDrawCall Create (string name)
	{
#if SHOW_HIDDEN_OBJECTS && UNITY_EDITOR
		name = (name != null) ? "_UIDrawCall [" + name + "]" : "DrawCall";
#endif
		if (mInactiveList.size > 0)
		{
			UIDrawCall dc = mInactiveList.Pop();
			mActiveList.Add(dc);
			if (name != null) dc.name = name;
			NGUITools.SetActive(dc.gameObject, true);
			return dc;
		}

#if UNITY_EDITOR
		// If we're in the editor, create the game object with hide flags set right away
		GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags(name,
 #if SHOW_HIDDEN_OBJECTS
			HideFlags.DontSave | HideFlags.NotEditable);
 #else
			HideFlags.HideAndDontSave);
 #endif
#else
		GameObject go = new GameObject(name);
		DontDestroyOnLoad(go);
#endif
		// Create the draw call
		UIDrawCall newDC = go.AddComponent<UIDrawCall>();
		mActiveList.Add(newDC);
		return newDC;
	}

	/// <summary>
	/// Clear all draw calls.
	/// </summary>

	static public void ClearAll ()
	{
		bool playing = Application.isPlaying;

		for (int i = mActiveList.size; i > 0; )
		{
			UIDrawCall dc = mActiveList[--i];

			if (dc)
			{
				if (playing) NGUITools.SetActive(dc.gameObject, false);
				else NGUITools.DestroyImmediate(dc.gameObject);
			}
		}
		mActiveList.Clear();
	}

	/// <summary>
	/// Immediately destroy all draw calls.
	/// </summary>

	static public void ReleaseAll ()
	{
		ClearAll();
		ReleaseInactive();
	}

	/// <summary>
	/// Immediately destroy all inactive draw calls (draw calls that have been recycled and are waiting to be re-used).
	/// </summary>

	static public void ReleaseInactive()
	{
		for (int i = mInactiveList.size; i > 0; )
		{
			UIDrawCall dc = mInactiveList[--i];
			if (dc) NGUITools.DestroyImmediate(dc.gameObject);
		}
		mInactiveList.Clear();
	}

	/// <summary>
	/// Count all draw calls managed by the specified panel.
	/// </summary>

	static public int Count (UIPanel panel)
	{
		int count = 0;
		for (int i = 0; i < mActiveList.size; ++i)
			if (mActiveList[i].manager == panel) ++count;
		return count;
	}

	/// <summary>
	/// Destroy the specified draw call.
	/// </summary>

	static public void Destroy (UIDrawCall dc)
	{
		if (dc)
		{
			if (Application.isPlaying)
			{
				if (mActiveList.Remove(dc))
				{
					NGUITools.SetActive(dc.gameObject, false);
					mInactiveList.Add(dc);
				}
			}
			else
			{
				mActiveList.Remove(dc);
				NGUITools.DestroyImmediate(dc.gameObject);
			}
		}
	}
}
