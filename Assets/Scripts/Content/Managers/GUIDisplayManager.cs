using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIDisplayManager : MonoBehaviour {

	#region Configuration

	public Texture2D HealthBarBackground = null;
	public Texture2D HealthBarForeground = null;
	public Texture2D HealthBarGreen = null;
	public Texture2D HealthBarYellow = null;
	public Texture2D HealthBarRed = null;
	public Texture2D HealthBarBarExtension = null;

	#endregion

	#region Construction
	
	void Start () {
		this.textGUIManager = Manager.GetInstance<TextGUIManager>();
		var mbatemp = ScriptAssistant.GetAllInstancesWithType<MainBaseActor>();
		if(mbatemp != null)
		{
			var mba = mbatemp[0];
			if(mba != null)
			{
				this.mainBaseActor = mba;
				this.mainBaseActor.TryGetProperty<MineralsProperty>(out mainBaseMinerals);
			}
		}

		this.gameManager = Manager.GetInstance<GameManager>();
		this.gameManager.OnGameOver += delegate(object sender, GameManager.GameOverEventArgs e) 
		{
			this.gameOverMsg = "Victory";
			if((sender as IGameManager).PlayerFaction != e.WinningFaction)
				this.gameOverMsg = "Defeat";
			this.gameOverDetails = e.WinningFaction.ToString() + " wins";
			this.gameOvertime = Time.time;
			this.gameOverState = GameOverState.Outcome;
		};

		this.gameManager.OnStartGame += delegate(object sender, System.EventArgs e) { this.gameOverState = GameOverState.Begun; };
		this.gameManager.OnActorCreated += delegate(object sender, GameManager.ActorUpdatedEventArgs e) 
			{
				if(this.healthActors.Contains(e.AffectedActor))
					return;

				if((e.AffectedActor is MineralSpotActor))
					return;

				Debug.Log ("Registered actor: " + e.AffectedActor.PresentationName);
				HealthProperty hp = null;
				if(e.AffectedActor.TryGetProperty<HealthProperty>(out hp))
					this.healthActors.Add(e.AffectedActor);
			};
		this.gameManager.OnActorDestroyed += delegate(object sender, GameManager.ActorUpdatedEventArgs e) 
			{
				this.healthActors.Remove(e.AffectedActor);
			};

		// Fill list with actors already registered
		foreach(var faction in this.gameManager.Factions.Keys)
		{
			foreach(var actor in this.gameManager.Factions[faction])
			{
				if(actor is MineralSpotActor)
					continue;

				HealthProperty hp = null;
				if(actor.TryGetProperty<HealthProperty>(out hp))
					this.healthActors.Add(actor);
			}
		}

		this.statisticsManager = Manager.GetInstance<GameStatisticsManager>();

		this.foregroundRect.height = 0.008f * Screen.height;
		this.foregroundRect.width = (float)this.HealthBarForeground.width / (float)this.HealthBarForeground.height * this.foregroundRect.height;
		this.backgroundRect.height = (float)this.HealthBarBackground.height / (float)this.HealthBarForeground.height * this.foregroundRect.height;
		this.backgroundRect.width = (float)this.HealthBarBackground.width / (float)this.HealthBarBackground.height * this.backgroundRect.height;
		this.barRect.height = (float)this.HealthBarGreen.height / (float)this.HealthBarForeground.height * this.backgroundRect.height;
		this.barRect.width = (float)this.HealthBarGreen.width / (float)this.HealthBarForeground.width * this.foregroundRect.width;
	}
	
	#endregion
	
	#region GUI
	
	void OnGUI()
	{
		if(this.gameOverState == GameOverState.NotStarted)
			return;

		//this.textGUIManager.GUIDisplayText(new Rect(80, 10, 1, 50), "W: "+ Screen.width + " H: " + Screen.height);

		if(this.gameOverState == GameOverState.Outcome)
		{
			this.GameOverDisplay();
		}
		else if(this.gameOverState == GameOverState.DamageDealt)
		{
			this.DamageDealtDisplay();
		}
		else
	 	{	
			if(mainBaseMinerals != null)
				this.textGUIManager.GUIDisplayText(new Rect(Screen.width * 0.1f, Screen.height * 0.1f, 1, 50), "Minerals: "+ (int)mainBaseMinerals.PropertyValue);
			foreach(var actor in this.healthActors)
			{
				this.DrawHealthBar(actor);
			}
		}
	}

	private void DrawHealthBar(Actor actor)
	{
		var worldPosition = actor.NavigationPosition + new Vector3(0f, actor.Height + 0.2f, 0f);
		var guiPosition = Camera.main.WorldToScreenPoint(worldPosition);
		guiPosition.y = Screen.height - guiPosition.y;
		
		HealthProperty hp = null;
		actor.TryGetProperty<HealthProperty>(out hp);
		if(hp.PropertyValue.CurrentHealth > 0)
		{
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

	private void GameOverDisplay()
	{
		if(Time.time > this.gameOvertime + this.gameOverMessageDelay)
		{
			this.textGUIManager.GUIDisplayText(new Rect(Screen.width / 2f, this.gameOverMsgYCoords, 0,0), this.gameOverMsg, Color.white, Color.black, 150);
			this.textGUIManager.GUIDisplayText(new Rect(Screen.width / 2f, this.gameOverMsgDetailsYCoords, 0,0), this.gameOverDetails, Color.white, Color.black, 60);
		}
		if(Manager.GetInstance<GestureManager>().ReleasedSingleTouch())
			this.gameOverState = GameOverState.DamageDealt;
	}

	private void DamageDealtDisplay()
	{
		
		var heightOffset = 0f;
		var dmgDealtSize = this.textGUIManager.CalculateSize("Damage dealt", 50);
		this.textGUIManager.GUIDisplayText(new Rect(Screen.width / 2f, this.gameOverMsgYCoords + Screen.height * 0.05f * heightOffset, 0,0), "Damage dealt", Color.white, Color.black, 50);
		heightOffset = 2.5f;
		foreach(var faction in this.gameManager.Factions.Keys)
		{
			// Faction name
			this.textGUIManager.GUIDisplayText(
				new Rect(Screen.width / 2f - dmgDealtSize.x * 0.5f, this.gameOverMsgYCoords + Screen.height * 0.05f * heightOffset, 0,0), 
				this.gameManager.PlayerFaction == faction ? faction.ToString() + " (player)" : faction.ToString(), 
				Color.white, 
				Color.black, 
				21, 
				TextAnchor.MiddleLeft);

			// Damage dealt
			this.textGUIManager.GUIDisplayText(
				new Rect(Screen.width / 2f + dmgDealtSize.x * 0.5f, this.gameOverMsgYCoords + Screen.height * 0.05f * heightOffset, 0,0), 
				this.statisticsManager.GetDamageDealt(faction).ToString(), 
				Color.white, 
				Color.black, 
				21, 
				TextAnchor.MiddleRight);

			heightOffset++;
		}

		if((this.textGUIManager as ITextGUIManager).GUIButton(new Rect(Screen.width / 3f, this.resetButtonYCoords, 400f, 100f), "Retry", Color.white, Color.black, 50))
		{
			this.gameOverState = GameOverState.NotStarted;
			Manager.GetInstance<ProgressManager>().ReloadLevel();
		}

		if((this.textGUIManager as ITextGUIManager).GUIButton(new Rect(Screen.width * 2f / 3f, this.resetButtonYCoords, 400f, 100f), "Next level", Color.white, Color.black, 50))
		{
			this.gameOverState = GameOverState.NotStarted;
			Manager.GetInstance<ProgressManager>().LoadNextLevel();
		}
	}

	#endregion

	#region Implementation

	#endregion

	#region State

	private enum GameOverState
	{
		NotStarted,
		Begun,
		Outcome,
		DamageDealt
	}

	#endregion

	#region Data

	// Floating text
	// private float gameOverMsgYCoordsInitial = Screen.height / 2f;
	private float gameOverMsgYCoords = Screen.height / 3f;
	private float gameOverMsgDetailsYCoords = Screen.height / 2f;
	private float resetButtonYCoords = Screen.height * 7f / 8f;
	// private float gameOverMsgYCoordsTarget = Screen.height / 3f;
	// private float transitiontime = 2f;
	private float gameOverMessageDelay = 2.5f;

	// Health bars
	private Rect backgroundRect;
	private Rect barRect;
	private Rect foregroundRect;

	// Operation data
	private GameOverState gameOverState = GameOverState.NotStarted;
	private MainBaseActor mainBaseActor = null;
	private MineralsProperty mainBaseMinerals = null; 
	private TextGUIManager textGUIManager = null;
	private IGameManager gameManager = null;
	private GameStatisticsManager statisticsManager = null;
	private List<Actor> healthActors = new List<Actor>();
	private string gameOverMsg = string.Empty;
	private string gameOverDetails = string.Empty;
	private float gameOvertime = 0f;
	
	#endregion
}
