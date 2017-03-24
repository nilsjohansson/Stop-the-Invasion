using UnityEngine;
using System.Collections;

public class AncientCubeActor : Actor {
	
	#region Cofiguration
	
	public Color[] ColorShades;
	
	#endregion
	
	#region Construction
	
	void Start()
	{
		if(ColorShades.Length == 0) 
			return;
		
		string rndString = transform.position.x.ToString();
		double lastDigit = char.GetNumericValue(rndString[rndString.Length -1]);
		this.gameObject.GetComponent<Renderer>().material.color = ColorShades[(int)(lastDigit / 9f * (ColorShades.Length -1))];
	}
	
	#endregion
	
	#region Actor members
	
	public override string PresentationName {
		get 
		{
			return "Ancient cube";
		}
	}
	
	public override Vector3 NavigationPosition
	{
		get
		{
			var projectedPosition = ScriptAssistant.GetGroundPosition(this.transform.position, Manager.GetInstance<TouchInteractionManager>().TheGround);
			return (projectedPosition.HasValue ? projectedPosition.Value : this.transform.position);
		}
	}
	
	#endregion
	
}
