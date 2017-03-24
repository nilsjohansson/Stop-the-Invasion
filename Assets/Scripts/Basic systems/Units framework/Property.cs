using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IProperty
{
	/// <summary>
	/// Registers a callback method that will be invoked just before the property value is set.
	/// </summary>
	/// <param name='callback'>
	/// Callback method.
	/// </param>
	/// <param name='self'>
	/// The object that registers the callback.
	/// </param>
	void RegisterPropertyChangeIntervention(PropertyChangeIntervention callback, Actor self);

	/// <summary>
	/// Gets the property value.
	/// </summary>
	/// <value>
	/// The property value.
	/// </value>
	object PropertyValue { get; }

	/// <summary>
	/// Name of this <see cref="Property"/>. Used for presentation.
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	string PresentationName { get; }

	/// <summary>
	/// Gets the <see cref="Actor"/> that this Property is attached to.
	/// </summary>
	/// <value>My actor.</value>
	Actor AttachedActor { get; }

	/// <summary>
	/// Occurs when the value of a <see cref="Property"/> has changed.
	/// </summary>
	event System.EventHandler<Property.PropertyEventArgs> OnValueChanged;
}

/// <summary>
/// Used to change a value that is about to be set on a <see cref="Property"/>
/// </summary>
public delegate object PropertyChangeIntervention(object desiredValue, object currentValue, params object[] arguments);

[RequireComponent(typeof(Actor))]
public abstract class Property : MonoBehaviour, IProperty
{
	#region Delegates

	
	#endregion
	
	#region Events
	
	/// <summary>
	/// Occurs when the value of a <see cref="Property"/> has changed.
	/// </summary>
	public event System.EventHandler<PropertyEventArgs> OnValueChanged;
	
	#endregion
	
	#region Properties

	/// <summary>
	/// Property boiled down to the public members of <see cref="Property"/>.
	/// </summary>
	/// <value>As <see cref="IProperty"/> interface.</value>
	public virtual IProperty AsIProperty
	{
		get { return this as IProperty; }
	}

	/// <summary>
	/// Name of this <see cref="Property"/>. Used for presentation.
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	public virtual string PresentationName {
		get { return this.nameOfProperty; }
	}
	
	/// <summary>
	/// Gets or sets the property value.
	/// </summary>
	/// <value>
	/// The property value.
	/// </value>
	public virtual object PropertyValue 
	{
		get { return this.propertyValue; }
	}

	/// <summary>
	/// Gets the <see cref="Actor"/> that this Property is attached to.
	/// </summary>
	/// <value>My actor.</value>
	public virtual Actor AttachedActor 
	{
		get
		{
			if(this.myActor == null)
				this.myActor = this.GetComponent<Actor>();
			return this.myActor;
		}
	}

	#endregion
	
	#region Public methods
	
	/// <summary>
	/// Registers a callback method that will be invoked just before the property value is set.
	/// </summary>
	/// <param name='callback'>
	/// Callback method.
	/// </param>
	/// <param name='self'>
	/// The object that registers the callback.
	/// </param>
	public void RegisterPropertyChangeIntervention(PropertyChangeIntervention callback, Actor self)
	{
		if(self == null || callback == null) 
			return;
		
		if(interveners != null && interveners.Count > 0 && !interveners.Contains(self))
		{	
			this.propertyInterventions.Add(callback);
			this.interveners.Add(self);
		}
	}
	
	#endregion
	
	#region Operations

	/// <summary>
	/// Tries to set a new value for the property. Before the value is set, every actor that has registered an intervention gets a chance to alter the new value.
	/// </summary>
	/// <returns><c>true</c>, if property was set to exactly the new value, <c>false</c> otherwise.</returns>
	/// <param name="newValue">New value.</param>
	/// <param name="setter">Actor responsible for the new value.</param>
	/// <param name="arguments">Optional arguments passed along with update event and intervention.</param>
	protected bool SetProperty(object newValue, Actor setter, params object[] arguments)
	{
		object tempValue = newValue;
		object oldValue = this.propertyValue;
		foreach(var intervention in this.propertyInterventions)
		{
			tempValue = intervention(tempValue, this.propertyValue);
		}

		bool changed = false;
		this.propertyValue = ValidateProperty(tempValue, out changed);

		if(OnValueChanged != null) OnValueChanged(this, new PropertyEventArgs(newValue, oldValue, setter, arguments));

		return this.propertyValue == newValue;
	}
	
	/// <summary>
	/// Validates the property. Return value will be final value set to this property.
	/// </summary>
	/// <returns>
	/// The property value that will be set to this property.
	/// </returns>
	/// <param name='setValue'>
	/// Input value that is to be validated.
	/// </param>
	/// <param name='wasChanged'>
	/// Tells if the value was changed during validation
	/// </param>
	protected virtual object ValidateProperty(object setValue, out bool wasChanged)
	{
		wasChanged = false;
		return setValue;
	}
	
	#endregion
	
	#region Data

	private Actor myActor = null;
	private string nameOfProperty = "";
	protected object propertyValue = null;
	private List<PropertyChangeIntervention> propertyInterventions = new List<PropertyChangeIntervention>();
	private List<Actor> interveners = new List<Actor>();
	
	#endregion

	#region Private classes
	
	public class PropertyEventArgs : System.EventArgs
	{
		public PropertyEventArgs (object newValue, object oldValue, Actor responsible, params object[] arguments)
		{
			this.NewValue = newValue;
			this.OldValue = oldValue;
			this.Responsible = responsible;
			this.Arguments = arguments;
		}
		public object NewValue;
		public Actor Responsible;
		public object OldValue;
		public object[] Arguments = null;
	}
	
	#endregion
}


