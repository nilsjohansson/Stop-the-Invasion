using System.Collections.Generic;

using UnityEngine;

public interface IHealthDisplayManager
{
	#region Operations

	#endregion

	#region Properties

	#endregion

	#region Events

	#endregion
}

public class HealthDisplayManager : Manager, IHealthDisplayManager
{
	#region Configuration

	public Texture2D HealthBarBackground = null;
	public Texture2D HealthBarForeground = null;
	public Texture2D HealthBarGreen = null;
	public Texture2D HealthBarYellow = null;
	public Texture2D HealthBarRed = null;
	public Texture2D HealthBarBarExtension = null;

	#endregion

	#region Construction

	void Awake ()
	{
		Init (this);
	}

	void Start()
	{
		this.gameManager = Manager.GetInstance<GameManager>();
		this.gameManager.OnActorCreated += delegate(object sender, GameManager.ActorUpdatedEventArgs e) 
		{
			if(this.allHealthActors.Contains(e.AffectedActor))
				return;

			if((e.AffectedActor is MineralSpotActor))
				return;

			Debug.Log ("Registered actor: " + e.AffectedActor.PresentationName);
			HealthProperty hp = null;
			if(e.AffectedActor.TryGetProperty<HealthProperty>(out hp))
				this.allHealthActors.Add(e.AffectedActor);
		};

		foreach(var faction in this.gameManager.Factions.Keys)
		{
			foreach(var actor in this.gameManager.Factions[faction])
			{
				if(actor is MineralSpotActor)
					continue;

				HealthProperty hp = null;
				if(actor.TryGetProperty<HealthProperty>(out hp))
					this.allHealthActors.Add(actor);
			}
		}

		this.foregroundRect.height = 0.008f * Screen.height;
		this.foregroundRect.width = (float)this.HealthBarForeground.width / (float)this.HealthBarForeground.height * this.foregroundRect.height;
		this.backgroundRect.height = (float)this.HealthBarBackground.height / (float)this.HealthBarForeground.height * this.foregroundRect.height;
		this.backgroundRect.width = (float)this.HealthBarBackground.width / (float)this.HealthBarBackground.height * this.backgroundRect.height;
		this.barRect.height = (float)this.HealthBarGreen.height / (float)this.HealthBarForeground.height * this.backgroundRect.height;
		this.barRect.width = (float)this.HealthBarGreen.width / (float)this.HealthBarForeground.width * this.foregroundRect.width;
	}

	#endregion

	#region Properties

	#endregion

	#region Operations

	#endregion

	#region Implementation

	private void DrawHealthBar(Actor actor)
	{
		var worldPosition = actor.NavigationPosition + new Vector3(0f, actor.Height + 0.2f, 0f);
		var guiPosition = Camera.main.WorldToScreenPoint(worldPosition);
		guiPosition.y = Screen.height - guiPosition.y;

		//GUI.Label(rect, "asdff");
		HealthProperty hp = null;
		actor.TryGetProperty<HealthProperty>(out hp);
		if(hp.PropertyValue.CurrentHealth > 0)
		{
			Debug.Log("pos" + guiPosition.ToString());
			this.backgroundRect.center = guiPosition;
			GUI.DrawTexture(this.backgroundRect, this.HealthBarBackground);

			Rect mainRect = new Rect(this.barRect);
			mainRect.width = hp.PropertyValue.GetNormalizedHealthLevel() * this.barRect.width;
			mainRect.y = guiPosition.y - this.barRect.height / 2f;
			mainRect.x = guiPosition.x - this.barRect.width / 2f;

			// Select correct health bar color
			var healthBar = this.HealthBarGreen;
			var normalizedHealth = hp.PropertyValue.GetNormalizedHealthLevel();
			if(actor.Faction == FactionType.Neutral)
			{
				healthBar = this.HealthBarYellow;
				if(normalizedHealth < 0.333f)
					healthBar = this.HealthBarRed;
			} else if(actor.Faction != this.gameManager.PlayerFaction)
			{
				healthBar = this.HealthBarRed;
			} else
			{
				if(normalizedHealth < 0.666f && normalizedHealth > 0.333f)
				{
					healthBar = this.HealthBarYellow;
				}
				if(normalizedHealth <= 0.333f)
					healthBar = this.HealthBarRed;
			}

			GUI.DrawTexture(mainRect, healthBar);

			this.foregroundRect.center = guiPosition;
			GUI.DrawTexture(this.foregroundRect, this.HealthBarForeground);
			// this.textGUIManager.GUIDisplayText(guiRect, string.Format("{0}", Mathf.CeilToInt(hp.PropertyValue.CurrentHealth)));
		}
	}

	#endregion

	#region Messages

	void OnGUI()
	{
		foreach(var actor in this.allHealthActors)
		{
			DrawHealthBar(actor);
		}
	}

	#endregion

	#region Data

	private IGameManager gameManager = null;

	private List<Actor> allHealthActors = new List<Actor>();

	// Health bars
	private Rect backgroundRect;
	private Rect barRect;
	private Rect foregroundRect;

	#endregion
}