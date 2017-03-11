using System.Collections;
using System.Linq;

using UnityEngine;

public class Enemy : MonoBehaviour 
{
	#region Configuration

	public float MovementSpeed = 0.2f;
	public float Health = 100f;

	#endregion

	#region Construction

	void Start () 
	{
		_attackTarget = Manager.GetInstance<LevelManager>().PlayerDefendedTargets.First();
	}

	#endregion

	#region Operations

	public void Impact(float damage)
	{
		Health -= damage;

		if (Health < 0)
			Die ();
	}

	#endregion

	#region Messages

	void Update()
	{
		if(_attackTarget == null)
			return;

		var direction = _attackTarget.CurrentPosition - this.transform.position;
		direction.Normalize();

		var nextPosition = this.transform.position + direction * MovementSpeed * Time.deltaTime;

		var nextDistance = Vector3.Distance(nextPosition, _attackTarget.CurrentPosition);
		var currentDistance = Vector3.Distance(this.transform.position, _attackTarget.CurrentPosition);

		if(nextDistance < currentDistance && nextDistance >= 1f)
		{
			this.transform.position = nextPosition; 
		}
		else
		{
			// Attack mainbase.
		}
	}

	void LateUpdate()
	{
		if(_attackTarget == null)
			return;

		transform.LookAt(_attackTarget.CurrentPosition);
	}

	#endregion

	private void Die()
	{
		_attackTarget = null;
		GameObject.Destroy (this.gameObject);
	}

	#region Data

	private ITarget _attackTarget;

	#endregion
}
