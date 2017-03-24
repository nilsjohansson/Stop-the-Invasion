using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IConditionalCallback 
{
	/// <summary>
	/// Registers a callback action that will be executed when the <see cref="CallbackCondition"/> returns true. When the condition is checked is determined by the <see cref="ConditionLoopType"/>.
	/// The action will be performed once by default but can be set to execute every time the condiion returns true.
	/// </summary>
	/// <returns>An identifier for the callback. Used for unregistering if the callback is set to repeat.</returns>
	/// <param name="condition">The trigger condition. When it returns true, the <see cref="CallbackAction"/> will execute.</param>
	/// <param name="action">The action that will be executed.</param>
	/// <param name="conditionLoop">Defines what method to use for checking the condition. Default is in the Update loop.</param>
	/// <param name="repeat">If set to <c>true</c> do not remove the <see cref="CallbackAction"/> after it has executed once. Default is false.</param>
	int RegisterCallback(CallbackCondition condition, CallbackAction action, ConditionalCallbackManager.ConditionLoopType conditionLoop = ConditionalCallbackManager.ConditionLoopType.Update, bool repeat = false);

	/// <summary>
	/// Registers a callback action that will executed after a certain delay. 
	/// </summary>
	/// <returns>The callback action.</returns>
	/// <param name="delay">Time to wait before executing the callback action.</param>
	/// <param name="action">The action that will be executed.</param>
	int RegisterDelayedCallback(float delay, CallbackAction action);

	/// <summary>
	/// Unregisters an existing callback.
	/// </summary>
	/// <param name="callbackId">Callback identifier.</param>
	void UnRegisterCallback(int callbackId);

}

public class ConditionalCallbackManager : Manager, IConditionalCallback {
	#region Configuration
	
	#endregion
	
	#region Construction

	void Awake () 
	{
		Init(this);
	}

	#endregion
	
	#region Operations

	public int RegisterCallback (CallbackCondition condition, CallbackAction action, ConditionLoopType conditionLoop = ConditionLoopType.Update, bool repeat = false)
	{
		ConditionalCallback newCallback = new ConditionalCallback(this.GenerateUniqueId(), condition, action, conditionLoop, repeat);
		this.registeredCallbacks.Add(newCallback.Id, newCallback);

		return newCallback.Id;
	}

	public int RegisterDelayedCallback (float delay, CallbackAction action)
	{
		ConditionalCallback newCallback = new ConditionalCallback(this.GenerateUniqueId(), action, Time.time, delay);
		this.registeredCallbacks.Add(newCallback.Id, newCallback);

		return newCallback.Id;
	}

	public void UnRegisterCallback (int callbackId)
	{
		this.finishedCallbacks.Add(callbackId);
	}

	#endregion

	#region Implementation

	private int GenerateUniqueId()
	{
		int result = 0;
		foreach(var existingId in this.registeredCallbacks.Keys)
		{
			if(existingId > result)
				result = existingId;
		}

		return result +1;
	}

	private void ClearCallbacks()
	{
		// Remove all actions that are in this list
		foreach(int id in this.finishedCallbacks)
			if(this.registeredCallbacks.ContainsKey(id))
				this.registeredCallbacks.Remove(id);
		
		this.finishedCallbacks.Clear ();
	}

	#endregion

	#region Update
	void Update () {
		if(this.registeredCallbacks.Count == 0)
			return;
		this.ClearCallbacks();

		// Loop through all registered actions
		foreach(var registeredCallback in this.registeredCallbacks.Values)
		{
			// Make sure it should be executed in Update
			if(registeredCallback.LoopType == ConditionLoopType.Update)
			{
				// Is the condition time controlled?
				if(registeredCallback.StartTime.HasValue)
				{
					if(Time.time > registeredCallback.StartTime + registeredCallback.Delay)
					{
						registeredCallback.ExecuteAction();
						this.finishedCallbacks.Add(registeredCallback.Id);
					}
					continue;
				}

				// If the condition is fulfilled, execute it
				if(registeredCallback.Condition())
					registeredCallback.ExecuteAction();

				// If the action should be executed only once, add it to removal list
				if(!registeredCallback.IsSetToRepeat && registeredCallback.HasRepeated)
					this.finishedCallbacks.Add(registeredCallback.Id);
			}
		}

		this.ClearCallbacks();
	}

	void PhysicsUpdate()
	{
		if(this.registeredCallbacks.Count == 0)
			return;

		this.ClearCallbacks();

		// Loop through all registered actions
		foreach(var registeredCallback in this.registeredCallbacks.Values)
		{
			// Make sure it should be executed in Physics
			if(registeredCallback.LoopType == ConditionLoopType.Physics)
			{
				// Is the condition time controlled?
				if(registeredCallback.StartTime.HasValue)
				{
					if(Time.time > registeredCallback.StartTime + registeredCallback.Delay)
					{
						registeredCallback.ExecuteAction();
						this.finishedCallbacks.Add(registeredCallback.Id);
					}
					continue;
				}

				// If the condition is fulfilled, execute it
				if(registeredCallback.Condition())
					registeredCallback.ExecuteAction();
				
				// If the action should be executed only once, add it to removal list
				if(!registeredCallback.IsSetToRepeat && registeredCallback.HasRepeated)
					this.finishedCallbacks.Add(registeredCallback.Id);
			}
		}

		this.ClearCallbacks();
	}

	#endregion
	
	#region Data

	private Dictionary<int, ConditionalCallback> registeredCallbacks = new Dictionary<int, ConditionalCallback>();
	private List<int> finishedCallbacks = new List<int>();
	#endregion

	#region Private classes
	
	public enum ConditionLoopType
	{
		Update,
		Physics
	}

	public class ConditionalCallback
	{
		#region Construction

		public ConditionalCallback (int id, CallbackCondition condition, CallbackAction action, ConditionLoopType conditionLoop, bool repeat)
		{
			this.Condition = condition;
			this.action = action;
			this.isRepeating = repeat;
			this.LoopType = conditionLoop; 
			this.Id = id;
		}

		public ConditionalCallback (int id, CallbackAction action, float startTime, float delay)
		{
			this.Id = id;
			this.action = action;
			this.StartTime = startTime;
			this.Delay = delay;
			this.LoopType = ConditionLoopType.Update;
		}

		#endregion

		#region Properties

		public int Id { get; private set; }

		public CallbackCondition Condition { get; private set; }

		public ConditionLoopType LoopType { get; private set; }

		public bool IsSetToRepeat { get {return this.isRepeating; } }

		public bool HasRepeated { get { return this.invokes > 0; } }

		public float? StartTime { get; private set; }

		public float Delay { get; private set; }

		#endregion

		#region Operations

		public void ExecuteAction()
		{
			this.action();
			this.invokes++;
		}

		#endregion

		#region Data

		private ConditionLoopType conditionLoop;
		private CallbackAction action = null;
		private long invokes = 0;
		private bool isRepeating = false;

		#endregion
	}

	#endregion
}

public delegate bool CallbackCondition();

public delegate void CallbackAction();

