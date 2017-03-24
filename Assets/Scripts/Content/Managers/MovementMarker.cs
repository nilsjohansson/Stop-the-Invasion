using UnityEngine;
using System.Collections;

public class MovementMarker : MonoBehaviour {
	
	#region Properties
	
	public GameObject leftHandCircle;
	public GameObject rightHandCircle;
	public float rotationAnglesPerSecond = 45f;
	public float separationDistance = 0.5f;
	public float pulseFrequency = 1f;
	public float pulseDistance = 1f;
	public bool animate = true;
	
	#endregion
	
	#region Construction
	
	void Start () {
		if(leftHandCircle != null && rightHandCircle != null)
		{
			totalSeparationDistance = separationDistance * 0.5f 
				+ (leftHandCircle.transform.localScale.z 
					* leftHandCircle.GetComponent<MeshFilter>().sharedMesh.bounds.size.z) * 0.5f;
			
		}
	}
	
	#endregion
	
	#region Update
	void Update () {
		if(leftHandCircle != null && rightHandCircle != null && animate)
		{
			pulseAngle += Time.deltaTime * 360f * pulseFrequency;
			//if(pulseAngle > 360f) pulseAngle -= 360f;
			this.transform.Rotate(Vector3.up, Time.deltaTime * this.rotationAnglesPerSecond);
			float pulse = Mathf.Cos(pulseAngle*Mathf.Deg2Rad) * 0.5f + 0.5f;
			
			leftHandCircle.transform.localPosition = new Vector3(0f, 0f, -pulse * pulseDistance + totalSeparationDistance);
			rightHandCircle.transform.localPosition = new Vector3(0f, 0f, pulse * pulseDistance - totalSeparationDistance);
		}
	}
	
	#endregion
	
	#region Data
	
	private float pulseAngle = 0f;
	private float totalSeparationDistance = 0f;
	
	#endregion
}
