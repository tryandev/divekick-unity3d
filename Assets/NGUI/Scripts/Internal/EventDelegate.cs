//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if UNITY_EDITOR || (!UNITY_FLASH && !NETFX_CORE)
#define REFLECTION_SUPPORT
#endif

#if REFLECTION_SUPPORT
using System.Reflection;
#endif

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Delegate callback that Unity can serialize and set via Inspector.
/// </summary>

[System.Serializable]
public class EventDelegate
{
	[SerializeField] MonoBehaviour mTarget;
	[SerializeField] string mMethodName;

	/// <summary>
	/// Whether the event delegate will be removed after execution.
	/// </summary>

	public bool oneShot = false;

	public delegate void Callback();
	Callback mCachedCallback;
	bool mRawDelegate = false;

	/// <summary>
	/// Event delegate's target object.
	/// </summary>

	public MonoBehaviour target { get { return mTarget; } set { mTarget = value; mCachedCallback = null; mRawDelegate = false; } }

	/// <summary>
	/// Event delegate's method name.
	/// </summary>

	public string methodName { get { return mMethodName; } set { mMethodName = value; mCachedCallback = null; mRawDelegate = false; } }

	/// <summary>
	/// Whether this delegate's values have been set.
	/// </summary>

	public bool isValid { get { return (mRawDelegate && mCachedCallback != null) || (mTarget != null && !string.IsNullOrEmpty(mMethodName)); } }

	/// <summary>
	/// Whether the target script is actually enabled.
	/// </summary>

	public bool isEnabled { get { return (mRawDelegate && mCachedCallback != null) || (mTarget != null && mTarget.enabled); } }

	public EventDelegate () { }
	public EventDelegate (Callback call) { Set(call); }
	public EventDelegate (MonoBehaviour target, string methodName) { Set(target, methodName); }

	/// <summary>
	/// GetMethodName is not supported on some platforms.
	/// </summary>

#if REFLECTION_SUPPORT
	#if !UNITY_EDITOR && UNITY_WP8
		static string GetMethodName (Callback callback)
		{
			System.Delegate d = callback as System.Delegate;
			return d.Method.Name;
		}

		static bool IsValid (Callback callback)
		{
			System.Delegate d = callback as System.Delegate;
			return d != null && d.Method != null;
		}
	#elif !UNITY_EDITOR && UNITY_METRO
		static string GetMethodName (Callback callback)
		{
			System.Delegate d = callback as System.Delegate;
			return d.GetMethodInfo().Name;
		}

		static bool IsValid (Callback callback)
		{
			System.Delegate d = callback as System.Delegate;
			return d != null && d.GetMethodInfo() != null;
		}
	#else
		static string GetMethodName (Callback callback) { return callback.Method.Name; }
		static bool IsValid (Callback callback) { return callback != null && callback.Method != null; }
	#endif
#else
	static bool IsValid (Callback callback) { return callback != null; }
#endif

	/// <summary>
	/// Equality operator.
	/// </summary>

	public override bool Equals (object obj)
	{
		if (obj == null)
		{
			return !isValid;
		}

		if (obj is Callback)
		{
			Callback callback = obj as Callback;
#if REFLECTION_SUPPORT
			if (callback.Equals(mCachedCallback)) return true;
			MonoBehaviour mb = callback.Target as MonoBehaviour;
			return (mTarget == mb && string.Equals(mMethodName, GetMethodName(callback)));
#elif UNITY_FLASH
			return (callback == mCachedCallback);
#else
			return callback.Equals(mCachedCallback);
#endif
		}
		
		if (obj is EventDelegate)
		{
			EventDelegate del = obj as EventDelegate;
			return (mTarget == del.mTarget && string.Equals(mMethodName, del.mMethodName));
		}
		return false;
	}

	static int s_Hash = "EventDelegate".GetHashCode();

	/// <summary>
	/// Used in equality operators.
	/// </summary>

	public override int GetHashCode () { return s_Hash; }

	/// <summary>
	/// Convert the saved target and method name into an actual delegate.
	/// </summary>

	Callback Get ()
	{
#if REFLECTION_SUPPORT
		if (!mRawDelegate && (mCachedCallback == null || (mCachedCallback.Target as MonoBehaviour) != mTarget || GetMethodName(mCachedCallback) != mMethodName))
		{
			if (mTarget != null && !string.IsNullOrEmpty(mMethodName))
			{
				mCachedCallback = (Callback)System.Delegate.CreateDelegate(typeof(Callback), mTarget, mMethodName);
			}
			else return null;
		}
#endif
		return mCachedCallback;
	}

	/// <summary>
	/// Set the delegate callback directly.
	/// </summary>

	void Set (Callback call)
	{
		if (call == null || !IsValid(call))
		{
			mTarget = null;
			mMethodName = null;
			mCachedCallback = null;
			mRawDelegate = false;
		}
		else
		{
#if REFLECTION_SUPPORT
			mTarget = call.Target as MonoBehaviour;

			if (mTarget == null)
			{
				mRawDelegate = true;
				mCachedCallback = call;
				mMethodName = null;
			}
			else
			{
				mMethodName = GetMethodName(call);
				mRawDelegate = false;
			}
#else
			mRawDelegate = true;
			mCachedCallback = call;
			mMethodName = null;
			mTarget = null;
#endif
		}
	}

	/// <summary>
	/// Set the delegate callback using the target and method names.
	/// </summary>

	public void Set (MonoBehaviour target, string methodName)
	{
		this.mTarget = target;
		this.mMethodName = methodName;
		mCachedCallback = null;
		mRawDelegate = false;
	}

	/// <summary>
	/// Execute the delegate, if possible.
	/// This will only be used when the application is playing in order to prevent unintentional state changes.
	/// </summary>

	public bool Execute ()
	{
		Callback call = Get();

		if (call != null)
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				call();
			}
			else if (call.Target != null)
			{
				System.Type type = call.Target.GetType();
				object[] objs = type.GetCustomAttributes(typeof(ExecuteInEditMode), true);
				if (objs != null && objs.Length > 0) call();
			}
#else
			call();
#endif
			return true;
		}
#if !REFLECTION_SUPPORT
		if (isValid)
		{
			mTarget.SendMessage(mMethodName, SendMessageOptions.DontRequireReceiver);
			return true;
		}
#endif
		return false;
	}

	/// <summary>
	/// Clear the event delegate.
	/// </summary>

	public void Clear ()
	{
		mTarget = null;
		mMethodName = null;
		mRawDelegate = false;
		mCachedCallback = null;
	}

	/// <summary>
	/// Convert the delegate to its string representation.
	/// </summary>

	public override string ToString ()
	{
		if (mTarget != null)
		{
			string typeName = mTarget.GetType().ToString();
			int period = typeName.LastIndexOf('.');
			if (period > 0) typeName = typeName.Substring(period + 1);

			if (!string.IsNullOrEmpty(methodName)) return typeName + "." + methodName;
			else return typeName + ".[delegate]";
		}
		return mRawDelegate ? "[delegate]" : null;
	}

	/// <summary>
	/// Execute an entire list of delegates.
	/// </summary>

	static public void Execute (List<EventDelegate> list)
	{
		if (list != null)
		{
			for (int i = 0; i < list.Count; )
			{
				EventDelegate del = list[i];

				if (del != null)
				{
					del.Execute();

					if (i >= list.Count) break;
					if (list[i] != del) continue;

					if (del.oneShot)
					{
						list.RemoveAt(i);
						continue;
					}
				}
				++i;
			}
		}
	}

	/// <summary>
	/// Convenience function to check if the specified list of delegates can be executed.
	/// </summary>

	static public bool IsValid (List<EventDelegate> list)
	{
		if (list != null)
		{
			for (int i = 0, imax = list.Count; i < imax; ++i)
			{
				EventDelegate del = list[i];
				if (del != null && del.isValid)
					return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Assign a new event delegate.
	/// </summary>

	static public void Set (List<EventDelegate> list, Callback callback)
	{
		if (list != null)
		{
			list.Clear();
			list.Add(new EventDelegate(callback));
		}
	}

	/// <summary>
	/// Append a new event delegate to the list.
	/// </summary>

	static public void Add (List<EventDelegate> list, Callback callback) { Add(list, callback, false); }

	/// <summary>
	/// Append a new event delegate to the list.
	/// </summary>

	static public void Add (List<EventDelegate> list, Callback callback, bool oneShot)
	{
		if (list != null)
		{
			for (int i = 0, imax = list.Count; i < imax; ++i)
			{
				EventDelegate del = list[i];
				if (del != null && del.Equals(callback))
					return;
			}

			EventDelegate ed = new EventDelegate(callback);
			ed.oneShot = oneShot;
			list.Add(ed);
		}
		else
		{
			Debug.LogWarning("Attempting to add a callback to a list that's null");
		}
	}

	/// <summary>
	/// Append a new event delegate to the list.
	/// </summary>

	static public void Add (List<EventDelegate> list, EventDelegate ev) { Add(list, ev, false); }

	/// <summary>
	/// Append a new event delegate to the list.
	/// </summary>

	static public void Add (List<EventDelegate> list, EventDelegate ev, bool oneShot)
	{
		if (list != null)
		{
			for (int i = 0, imax = list.Count; i < imax; ++i)
			{
				EventDelegate del = list[i];
				if (del != null && del.Equals(ev))
					return;
			}
			
			EventDelegate ed = new EventDelegate(ev.target, ev.methodName);
			ed.oneShot = oneShot;
			list.Add(ed);
		}
		else
		{
			Debug.LogWarning("Attempting to add a callback to a list that's null");
		}
	}

	/// <summary>
	/// Remove an existing event delegate from the list.
	/// </summary>

	static public bool Remove (List<EventDelegate> list, Callback callback)
	{
		if (list != null)
		{
			for (int i = 0, imax = list.Count; i < imax; ++i)
			{
				EventDelegate del = list[i];
				
				if (del != null && del.Equals(callback))
				{
					list.RemoveAt(i);
					return true;
				}
			}
		}
		return false;
	}
}
