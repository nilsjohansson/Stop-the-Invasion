using UnityEngine;
using System.Collections;

public class GestureManager : Manager {
	#region Construction

	void Start () {
		Init (this);
		Input.multiTouchEnabled = true;
	}

	#endregion

	#region Properties

	public TouchState CurrentTouchState
	{
		get { return this.currentFrameState; }
	}

	#endregion

	#region Public methods

	public Vector2? GetSingleTouchPosition()
	{
		return this.currentSingleTouchPosition;
	}

	public Vector2? GetDoubleTouchPosition()
	{
		return this.currentDoubleTouchPosition;
	}

	/// <summary>
	/// A single touch was released this frame.
	/// </summary>
	/// <returns><c>true</c>, if single touch was released, <c>false</c> otherwise.</returns>
	/// <param name="position">Last known position of touch.</param>
	public bool ReleasedSingleTouch()
	{
		// #if UNITY_STANDALONE || UNITY_EDITOR
		// return Input.GetMouseButtonUp(0);

		// #else
		return this.previousFrameState == TouchState.SingleTouch && this.currentFrameState != TouchState.SingleTouch;
		// #endif
	}

	/// <summary>
	/// A double touch was released this frame.
	/// </summary>
	/// <returns><c>true</c>, if double touch was released, <c>false</c> otherwise.</returns>
	/// <param name="position">Last known position of touch.</param>
	public bool ReleasedDoubleTouch()
	{
		// #if UNITY_STANDALONE || UNITY_EDITOR

		// return Input.GetMouseButtonUp();
		// #else
		return this.previousFrameState == TouchState.DoubleTouch && this.currentFrameState != TouchState.DoubleTouch;
		// #endif
	}

	#endregion

	#region Update

	void Update()
	{
		this.previousFrameState = this.currentFrameState;

#if UNITY_STANDALONE || UNITY_EDITOR
		this.DetectMouse();
#else
		this.DetectTouches();
#endif

	}

	private void DetectTouches()
	{
		switch(Input.touchCount)
		{
		case 0:
			this.currentFrameState = TouchState.NoTouches;
			break;
		case 1:
			this.currentFrameState = TouchState.SingleTouch;
			this.currentSingleTouchPosition = Input.touches[0].position;
			this.currentDoubleTouchPosition = null;
			break;
		case 2:
			if(Vector3.Magnitude(Input.touches[1].position - Input.touches[0].position) < Screen.width * 0.25f)
			{
				this.currentFrameState = TouchState.DoubleTouch;
				this.currentSingleTouchPosition = null;
				this.currentDoubleTouchPosition = Input.touches[0].position + .5f * (Input.touches[1].position - Input.touches[0].position);
			}
			else
			{
				this.currentFrameState = TouchState.NoTouches;
				this.currentSingleTouchPosition = null;
				this.currentDoubleTouchPosition = null;
			}
			break;
		default:
			this.currentFrameState = TouchState.NoTouches;
			this.currentSingleTouchPosition = null;
			this.currentDoubleTouchPosition = null;
			break;
		}
	}

	private void DetectMouse()
	{
		if(Input.GetMouseButton(0))
		{
			this.currentFrameState = TouchState.SingleTouch;
			this.currentSingleTouchPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y); // Screen.height - 
			this.currentDoubleTouchPosition = null;
			return;
		} 

		if(Input.GetMouseButton(1))
		{
			this.currentFrameState = TouchState.DoubleTouch;
			this.currentSingleTouchPosition = null;
			this.currentDoubleTouchPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y); 
			return;
		}
		
		this.currentFrameState = TouchState.NoTouches;
		this.currentSingleTouchPosition = null;
		this.currentDoubleTouchPosition = null;
	}

	#endregion

	#region State

	public enum TouchState
	{
		NoTouches,
		SingleTouch,
		DoubleTouch
	}

	#endregion

	#region Data

	private TouchState currentFrameState = TouchState.NoTouches;
	private TouchState previousFrameState = TouchState.NoTouches;
	
	private Vector2? currentSingleTouchPosition = null;
	private Vector2? currentDoubleTouchPosition = null;

	// private Vector2? previousSingleTouchPosition = null;
	// private Vector2? previousDoubleTouchPosition = null;

	#endregion
}
