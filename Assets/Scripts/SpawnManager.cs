using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public interface ISpawnManager
{
	#region Operations
	
	#endregion
	
	#region Properties
	
	#endregion
	
	#region Events
	
	#endregion
}

public class SpawnManager : Manager, ISpawnManager
{
	#region Configuration

	public GameObject EnemyPrefab;
	public List<Transform> SpawnLocations;
	public float TimeBetweenSpawns = 10f;

	#endregion

	#region Construction

	void Awake()
	{
		Init(this);
	}

	void Start()
	{
		this.mainBase = ScriptAssistant.GetAllInstancesWithType<MainBaseActor>().First();
		lastSpawnTime = -(TimeBetweenSpawns - 1f);
	}

	#endregion

	#region Properties

	#endregion

	#region Operations

	#endregion

	#region Implementation

	#endregion

	#region Messages

	void Update()
	{
		if(Time.time - lastSpawnTime < TimeBetweenSpawns)
			return;
		
		// Decide spawn location
		var spawnLocation = SpawnLocations[Random.Range(0, SpawnLocations.Count)];

		var newEnemy = Instantiate(EnemyPrefab, spawnLocation.position, Quaternion.identity);
		newEnemy.GetComponent<AttackAbility>().Activate(Enumerable.Repeat<Actor>(mainBase, 1).ToArray());

		lastSpawnTime = Time.time;
	}

	#endregion

	#region Data

	private MainBaseActor mainBase;
	private float lastSpawnTime = 0f;

	#endregion
}