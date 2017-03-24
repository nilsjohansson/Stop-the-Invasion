using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ScriptAssistant {

	#region Public methods

	/// <summary>
	/// Shoots a ray first straight down, then straight up to detect the ground. Uses the Vector3.Up & Vector3.Down constants.
	/// </summary>
	/// <returns>The ground position.</returns>
	/// <param name="currentPosition">Current position.</param>
	/// <param name="ground">Ground.</param>
	public static Vector3? GetGroundPosition(Vector3 currentPosition, GameObject ground)
	{
		// Shoot a ray downwards to detect the ground
		Ray ray = new Ray(currentPosition, Vector3.down);
		RaycastHit hit = new RaycastHit();
		int layerMask = ScriptAssistant.CreateLayerMask(new int[1] { 0 }, false);
		if(Physics.Raycast(ray, out hit, 1000f, layerMask))
		{
			if(hit.collider.gameObject == ground)
			{
				// Set this world position to the point where the ray intersects the ground
				return hit.point;
			}
		}

		ray = new Ray(currentPosition, Vector3.up);
		hit = new RaycastHit();
		if(Physics.Raycast(ray, out hit, 1000f, layerMask))
		{
			if(hit.collider.gameObject == ground)
			{
				// Set this world position to the point where the ray intersects the ground
				return hit.point;
			}
		}

		return null;
	}
	
	public static List<T> GetAllInstancesWithType<T>()
	{
		List<T> result = null;
		var allMonobehavioursInScene = GameObject.FindObjectsOfType(typeof(MonoBehaviour));
		foreach(var obj in allMonobehavioursInScene)
		{	
			if(obj is T)
			{
				if(result == null) result = new List<T>();
				//T objAsInterface = obj;
				result.Add((T)(object)obj);
			}
		}
		return result;
	}
	
	/// <summary>
	/// Creates a layer mask by bit shifting an integer.
	/// </summary>
	/// <returns>
	/// A layer mask.
	/// </returns>
	/// <param name='layers'>
	/// Layers included in the mask
	/// </param>
	/// <param name='excludeLayers'>
	/// Indicate whether the layermask should include or exclude the specified layers.
	/// </param>
	public static int CreateLayerMask(int[] layers, bool excludeLayers)
	{
		int layerMask = 0;
		
		foreach(var layer in layers)
		{
			layerMask = layerMask | (1 << layer);
		}
		
		layerMask = excludeLayers ? ~layerMask : layerMask;
		
		 return layerMask;
	}
	
	public static TurnDirection AngleDirection(Vector3 forward, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(forward, targetDir);
        float dir = Vector3.Dot(perp, up);
        
		if (dir > 0.0f) {
            return TurnDirection.Right;
        } else if (dir < 0.0f) {
            return TurnDirection.Left;
        } else {
            return TurnDirection.Forward;
        }
    }
	
	public static float CircularAngleBetween(Vector2 fromAngle, Vector2 toAngle)
	{
		float angle = Vector2.Angle(fromAngle, toAngle);
		if(AngleDirection(fromAngle, toAngle, Vector3.forward) == TurnDirection.Right)
			angle = 360f - angle;
		return angle;
	}
	
	#endregion
	
	#region Public classes
	
	public enum TurnDirection
	{
		Right,
		Left,
		Forward
	}
	
	#endregion
}
/// <summary>
/// Priority queue. Stores items in float based priority order, higher value means higher priority.
/// </summary>
public class PriorityQueue<T> 
{
	#region Properties
	
	/// <summary>
	/// Gets all items stored in order of priority. Lowest to highest.
	/// </summary>
	/// <value>
	/// All items.
	/// </value>
	public List<T> allItems {
		get 
		{
			List<T> items = new List<T>();
			foreach(var entry in this.theQueue)
			{
				items.Add(entry.Item);
			}
			return items;
		}
	}
	
	public LinkedList<PriorityQueueItem> PriorityItems {
		get { return this.theQueue; }
	}
	
	#endregion
	
	#region Construction
	
	public PriorityQueue ()
	{
		this.theQueue = new LinkedList<PriorityQueueItem>();
	}
	
	public static explicit operator PriorityQueue<T>(LinkedList<PriorityQueueItem> linkedList)
    {
        PriorityQueue<T> newQueue = new PriorityQueue<T>();
		typeof(PriorityQueue<T>).GetField("theQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			.SetValue(newQueue, linkedList);
        // code to convert from int to SampleClass... 

        return newQueue;
    }
	
	#endregion
	
	#region Public methods
	
	/// <summary>
	/// Look at the item with highest priority without removing it from the queue.
	/// </summary>
	public PriorityQueueItem PeekHighest()
	{
		if(this.theQueue.Count == 0) throw new System.InvalidOperationException("No items in priority queue.");
		var result = this.theQueue.Last.Value;
		
		return result;
	}
	/// <summary>
	/// Look at the item with highest priority without removing it from the queue.
	/// </summary>
	public PriorityQueueItem PeekLowest()
	{
		if(this.theQueue.Count == 0) throw new System.InvalidOperationException("No items in priority queue.");
		var result = this.theQueue.First.Value;
		
		return result;
	}

	/// <summary>
	/// Dequeues the item with the highest priority weight and returns it.
	/// </summary>
	public PriorityQueueItem DequeueHighest()
	{
		if(this.theQueue.Count == 0) throw new System.InvalidOperationException("No items in priority queue.");
		var result = this.theQueue.Last.Value;
		this.theQueue.RemoveLast();
		
		return result;
	}
	/// <summary>
	/// Dequeues the item with the lowest priority weight and returns it.
	/// </summary>
	public PriorityQueueItem DequeueLowest()
	{
		if(this.theQueue.Count == 0) throw new System.InvalidOperationException("No items in priority queue.");
		var result = this.theQueue.First.Value;
		this.theQueue.RemoveFirst();
		
		return result;
	}

	/// <summary>
	/// Attempts to dequeue the item with the lowest priority weight.
	/// </summary>
	/// <returns><c>true</c>, if the operation was successful, <c>false</c> otherwise.</returns>
	/// <param name="lowest">The item with the lowest priority weight.</param>
	public bool TryDequeueLowest(out PriorityQueueItem lowest)
	{
		lowest = default(PriorityQueueItem);

		if(this.theQueue.Count == 0)
			return false;

		lowest = this.DequeueLowest();
		return true;
	}

	/// <summary>
	/// Attempts to dequeue the item with the highest priority weight.
	/// </summary>
	/// <returns><c>true</c>, if the operation was successful, <c>false</c> otherwise.</returns>
	/// <param name="highest">The item with the highest priority weight.</param>
	public bool TryDequeueHighest(out PriorityQueueItem highest)
	{
		highest = default(PriorityQueueItem);
		
		if(this.theQueue.Count == 0)
			return false;
		
		highest = this.DequeueHighest();
		return true;
	}

	/// <summary>
	/// Enqueues the specified item and places it in the queue based on its priorityWeight. When a duplicate item is added, the item with lowest priority is kept.
	/// </summary>
	/// <param name='item'>
	/// Item.
	/// </param>
	/// <param name='priorityWeight'>
	/// Priority. Higher value means higher priority
	/// </param>
	public void Enqueue(T item, float priorityWeight)
	{
		// Check for duplicates
		PriorityQueueItem removeThis = this.TryGetItem(item);
		if(removeThis != null)
		{
			if(removeThis.PriorityWeight > priorityWeight)
			{
				// Remove the duplicate with higher priorityWeight
				this.theQueue.Remove(removeThis);
			}
			// If new item has higher priorityWeight, dont put it in queue
			else return;
		}
		
		// Calculate priority & store in queue
		PriorityQueueItem newItem = new PriorityQueueItem(priorityWeight, item);
		PriorityQueueItem indexItem = null;
		foreach(var existingItem in this.theQueue)
		{
			if(existingItem.PriorityWeight < priorityWeight) 
			{
				indexItem = existingItem;
			}
		}
		
		if(indexItem != null)
		{
			this.theQueue.AddAfter(this.theQueue.FindLast(indexItem), newItem);
		} else
		{
			this.theQueue.AddFirst(newItem);
		}
	}
	
	/// <summary>
	/// Returns a subset of the queue
	/// </summary>
	/// <returns>
	/// A subsection of the original PriorityQueue
	/// </returns>
	/// <param name='fromNode'>
	/// First item of subsection
	/// </param>
	/// <param name='toNode'>
	/// Last item of subsection
	/// </param>
	public PriorityQueue<T> SubSet(T fromNode, T toNode)
	{
		LinkedList<PriorityQueueItem> newQ = new LinkedList<PriorityQueueItem>();
		var indexItem = this.theQueue.First;
		bool foundFirst = false;
		for(int i = 0; i < this.theQueue.Count; i++)
		{
			if(indexItem.Value.Item.Equals(fromNode)) foundFirst = true;
			if(foundFirst) 
			{
				newQ.AddLast(indexItem.Value);
			}
			if(indexItem.Value.Item.Equals(toNode)) break;
			indexItem = indexItem.Next;
		}
		
		return (PriorityQueue<T>)newQ;
	}
	
	/// <summary>
	/// Tries to find an item within the Queue. 
	/// </summary>
	/// <returns>
	/// The item. If not item exists, returns default(T)
	/// </returns>
	public PriorityQueueItem TryGetItem(T findme)
	{
		PriorityQueueItem result = null;
		
		foreach(var item in this.theQueue)
		{
			if(item.Item.Equals(findme))
			{
				result = item;
				break;
			}
		}
		
		return result;
	}
	
	/// <summary>
	/// Returns true if the specified item is stored in the queue.
	/// </summary>
	/// <param name='item'>
	/// If set to <c>true</c> item.
	/// </param>
	public bool Contains(T item)
	{
		bool result = false;
		
		foreach(var existingItem in this.theQueue)
		{
			if(existingItem.Item.Equals(item))
			{
				result = true;
				break;
			}
		}
		
		return result;
	}
	
	#endregion
	
	#region Operations
	
	
	
	#endregion
	
	#region IEnumerable
	
	public IEnumerator<T> GetEnumerator()
	{
		return this.allItems.GetEnumerator();
	}
	
	#endregion
	
	#region Data
	
	private LinkedList<PriorityQueueItem> theQueue = null;
	
	#endregion
	
	#region PriorityQueueItem class
	
	public class PriorityQueueItem
	{
		public PriorityQueueItem (float priorityWeight, T item)
		{
			this.PriorityWeight = priorityWeight;
			this.Item = item;
		}
		
		public float PriorityWeight {
			get;
			set;
		}
		
		public T Item {
			get;
			set;
		}
	}
	
	#endregion
}