using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {
	#region Configuration
	
	public float CameraMovementSpeed = 1.0f;
	public bool DragTerrainMode = true;
	public BoxCollider GrabPanel = null;
	public bool BlankCameraAtStart = false;

	#endregion
	
	#region Construction
	
	void Start () {
		// Used for constraining camera movement
		this.dragPanelRect = new Rect();
		Vector2 tmptranformPos = new Vector2(GrabPanel.gameObject.transform.position.x, GrabPanel.gameObject.transform.position.z);
		this.dragPanelRect.size = new Vector2(GrabPanel.size.x, GrabPanel.size.z);
		this.dragPanelRect.center = tmptranformPos + new Vector2(GrabPanel.center.x, GrabPanel.center.z);
		//this.dragPanelRect.size = new Vector2(1000, 1000);

		if(BlankCameraAtStart) 
		{
			this.defaultCameraLayers = Camera.main.cullingMask;
			Camera.main.cullingMask = 0;
		}
		var gm = Manager.GetInstance<GameManager>() as IGameManager;
		gm.OnStartGame += delegate(object sender, System.EventArgs e) { this.cameraMovementState = CameraMovementState.WaitingForInput; if(BlankCameraAtStart) Camera.main.cullingMask = this.defaultCameraLayers; };
		gm.OnGameOver += delegate(object sender, GameManager.GameOverEventArgs e) { this.cameraMovementState = CameraMovementState.Disabled; };

		this.gestureManager = Manager.GetInstance<GestureManager>();
	}
	
	#endregion
	
	#region Update
	
	void Update () {
		switch(this.cameraMovementState)
		{
		case CameraMovementState.WaitingForInput:
			if(this.CatchStartPosition())
				this.cameraMovementState = CameraMovementState.TrackingMovement;
			break;
		case CameraMovementState.TrackingMovement:
			if(this.CatchMovementFinished())
			{
				this.cameraMovementState = CameraMovementState.WaitingForInput;
				break;
			}

			this.MoveCamera();
			break;
		default:
			break;
		}
	}
	
	#endregion
	
	#region Implementation
	
	private bool CatchStartPosition()
	{
		if(this.gestureManager.CurrentTouchState == GestureManager.TouchState.DoubleTouch)
		{
			this.cameraStartPosition = Camera.main.transform.position; 
			if(!DragTerrainMode)
			{
				this.inputStartPosition = this.gestureManager.GetDoubleTouchPosition().Value;
				return true;
			}

			this.terrainInputStartPosition = this.FindGrabPoint(); 

			return true;
		}
		
		return false;
	}
	
	private bool CatchMovementFinished()
	{
		if(this.gestureManager.ReleasedDoubleTouch())
		{
			this.inputStartPosition = null;
			this.terrainInputStartPosition = null;
			return true;
		}
		
		return false;
	}

	private void MoveCamera()
	{
		Vector3 inputOffset;
		if(!this.DragTerrainMode)
		{
			inputOffset = Vector3.Scale(new Vector3(1,1,0) * CameraMovementSpeed, (this.gestureManager.GetDoubleTouchPosition().Value - this.inputStartPosition.Value));
			Camera.main.transform.position = this.cameraStartPosition + new Vector3(inputOffset.x, 0f, inputOffset.y);
		}
		else
		{
			var currentPos = this.FindGrabPoint();
			inputOffset = this.terrainInputStartPosition.Value - currentPos;
			var tempNewPosition = Camera.main.transform.position + new Vector3(inputOffset.x, 0f, inputOffset.z);

			if(this.IsWithinDragPanelLimits(tempNewPosition))
				Camera.main.transform.position = tempNewPosition;

			this.terrainInputStartPosition = this.FindGrabPoint();
		}
	}

	private Vector3 FindGrabPoint()
	{
		var ray = Camera.main.ScreenPointToRay(this.gestureManager.GetDoubleTouchPosition().Value);
		RaycastHit hit = new RaycastHit();
		Physics.Raycast(ray, out hit, 1000f, ScriptAssistant.CreateLayerMask(new int[]{ GrabPanel.gameObject.layer }, false));

		return hit.point;
	}

	private bool IsWithinDragPanelLimits(Vector3 cameraWorldPosition)
	{
		/*
		var ray = new Ray(cameraWorldPosition, Vector3.down);
		RaycastHit hit = new RaycastHit();
		Physics.Raycast(ray, out hit, 1000f, ScriptAssistant.CreateLayerMask(new int[]{ GrabPanel.gameObject.layer }, false));
		*/
		Vector2 camWorld2DPos = new Vector2(cameraWorldPosition.x, cameraWorldPosition.z);

		return this.dragPanelRect.Contains(camWorld2DPos);
		// return hit.collider == GrabPanel;
	}

	#endregion
	
	#region State
	
	private enum CameraMovementState
	{
		WaitingForInput,
		TrackingMovement,
		Disabled
	}
	
	#endregion
	
	#region Data

	private GestureManager gestureManager = null;
	private CameraMovementState cameraMovementState = CameraMovementState.Disabled;
	private Vector2? inputStartPosition = null;
	private Vector3? terrainInputStartPosition = null;
	private Vector3 cameraStartPosition = Vector3.zero;
	private Rect dragPanelRect;
	private int defaultCameraLayers = 1;
	
	#endregion
}
