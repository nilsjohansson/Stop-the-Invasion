using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A Manager is a singleton implementation of a monobehaviour. It provides a GetInstance operation that allows any script to get a reference to the unique instance in this scene. 
/// Used for modules that has public operations.
/// </summary>
public class ManagerOld : MonoBehaviour 
{
	#region Static construction
	
	static List<Manager> theInstances = null;
	public static int GetInstanceCalls = 0;
	public static float GetInstanceCallsTotaltime = 0f;
	/// <summary>
	/// Gets THE unique instance of the <see cref="Manager"/> of type T in the scene. If none exists it will be created and added to the scene.
	/// </summary>
	/// <returns>
	/// The unique instance of the <see cref="Manager"/> of type T.
	/// </returns>
	/// <typeparam name='T'>
	/// The type parameter.
	/// </typeparam>
	public static T GetInstance<T>()
		where T : Manager
	{
		GetInstanceCalls ++;
		float time = Time.time;
		T result = null;
		if(TryGetInstance<T>(out result))
		{
			// Debug.Log("Using aready added instance of "+typeof(T).ToString() + "!");
			GetInstanceCallsTotaltime += Time.time - time;
			return result;
		} 
		else 
		{
			// Create a new monobehaviour instance here
			GameObject go = GameObject.Find ("Managers");
			if (go == null) 
			{
				go = new GameObject ("Managers");
			}
			var components = go.GetComponents(typeof(T));
			foreach(var manager in components)
			{
				if(manager is T)
				{
					Debug.Log("Found existing "+typeof(T).ToString() + "!");
					result = (T)manager;
					break;
				}
			}
			
			if(result == null)
			{
				Debug.Log("Added new "+typeof(T).ToString() + " to "+go.name+" !");
				result = (T)go.AddComponent(typeof(T));
			}
			
			if(result == null)
			{
				Debug.LogError("No instance of " + typeof(T).GetType().ToString () + " could be created!");
				GetInstanceCallsTotaltime += Time.time - time;
				return result;
			}
			
			Init(result);
		}
		GetInstanceCallsTotaltime += Time.time - time;
		return result;
	}

	public static void ResetAll()
	{
		theInstances.Clear();
	}

	protected static void Init<T>(T managerInstance)
		where T : Manager
	{
		// Check if it already exists, if it does, use the old instance
		T existing = null;
		if (!TryGetInstance<T>(out existing)) {
			// Debug.Log("Adding new instance of type " + managerInstance.GetType ().ToString () +" to static library.");
			theInstances.Add(managerInstance);
			return;
		} 
		if(existing != null && existing != managerInstance){
			Debug.Log ("Multiple instances of " + managerInstance.GetType ().ToString () + " Hash1: "+((object)managerInstance).GetHashCode()+" Hash2: "+((object)existing).GetHashCode()+". Destroying instance residing on gameobject '" + managerInstance.gameObject.name + "'. Original resides on '" + existing.transform.name + "'");
			Destroy (managerInstance);
		}
	}
	
	private static bool TryGetInstance<T>(out T manager)
		where T : Manager
	{
		if(theInstances == null)
		{
			theInstances = new List<Manager>();
			manager = null;
			return false;
		}
		
		foreach(var instance in theInstances)
		{
			if(instance is T)
			{
				manager = (T)instance;
				return true;
			}
		}
		
		manager = null;
		return false;
	}

	#endregion
	
	#region Construction

	#endregion

    #region Coroutines

    protected void InvokeNextFrame(System.Action command)
    {
        StartCoroutine(this.InvokeNextFrameCore(command));
    }

    private System.Collections.IEnumerator InvokeNextFrameCore(System.Action command)
    {
        yield return null;
        
        command.Invoke();
    }

    #endregion
}
