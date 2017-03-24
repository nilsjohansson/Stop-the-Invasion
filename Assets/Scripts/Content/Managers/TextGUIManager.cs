using UnityEngine;
using System.Collections;

public interface ITextGUIManager
{
	void GUIDisplayText(Rect rect, string text);
	void GUIDisplayText(Rect rect, string text, Color textColor, Color outlineColor);
	void GUIDisplayText(Rect rect, string text, Color textColor, Color outlineColor, int fontSize);
	void GUIDisplayText(Rect rect, string text, Color textColor, Color outlineColor, int fontSize, TextAnchor anchor);
	bool GUIButton(Rect rect, string caption);
	bool GUIButton(Rect rect, string caption, Color textColor, Color outlineColor, int fontSize);
}

public class TextGUIManager : Manager, ITextGUIManager {
	#region Configuration
	
	public Font DisplayFont = null;
	public int fontSize = 16;
	public float outlineOffset = 1f;
	
	#endregion
	
	#region Construction
	void Awake()
	{
		Init (this);
	}

	void Start()
	{
		this.screenResolutionFactor = (int)((float)Screen.height / 580f);
		this.outlineOffset *= Screen.height / 580f;

		ResetGuiStyles();
	}
	
	#endregion
	
	#region Public methods
	
	public void GUIDisplayText(Rect rect, string text)
	{
		var originalRect = rect;
		float outlineSize = outlineOffset + outlineOffset * this.myGuiStyle.fontSize / 180f;

		rect.center = originalRect.center + new Vector2(0,-1) * outlineSize;
		GUI.Label(rect, text, this.myGuiStyleOutline);
		rect.center = originalRect.center + new Vector2(0,1) * outlineSize;
		GUI.Label(rect, text, this.myGuiStyleOutline);
		rect.center = originalRect.center + new Vector2(-1,0) * outlineSize;
		GUI.Label(rect, text, this.myGuiStyleOutline);
		rect.center = originalRect.center + new Vector2(1,0) * outlineSize;
		GUI.Label(rect, text, this.myGuiStyleOutline);
		
		rect.center = originalRect.center + new Vector2(-1,1) * outlineSize; // up left
		GUI.Label(rect, text, this.myGuiStyleOutline);
		rect.center = originalRect.center + new Vector2(1,1) * outlineSize; // up right
		GUI.Label(rect, text, this.myGuiStyleOutline);
		rect.center = originalRect.center + new Vector2(-1,-1) * outlineSize; // down left
		GUI.Label(rect, text, this.myGuiStyleOutline);
		rect.center = originalRect.center + new Vector2(1,-1) * outlineSize; // down right
		GUI.Label(rect, text, this.myGuiStyleOutline);
		GUI.Label(originalRect, text, this.myGuiStyle);
	}
	
	public void GUIDisplayText(Rect rect, string text, Color textColor, Color outlineColor)
	{
		this.myGuiStyleOutline.normal.textColor = outlineColor;
		this.myGuiStyle.normal.textColor = textColor;
		
		this.GUIDisplayText(rect, text);
		
		ResetGuiStyles();
	}
	
	public void GUIDisplayText(Rect rect, string text, Color textColor, Color outlineColor, int fontSize)
	{
		this.myGuiStyleOutline.normal.textColor = outlineColor;
		this.myGuiStyle.normal.textColor = textColor;
		this.myGuiStyleOutline.fontSize = fontSize * this.screenResolutionFactor;
		this.myGuiStyle.fontSize = fontSize * this.screenResolutionFactor;
		
		this.GUIDisplayText(rect, text);
		
		ResetGuiStyles();
	}

	public void GUIDisplayText(Rect rect, string text, Color textColor, Color outlineColor, int fontSize, TextAnchor anchor)
	{
		this.myGuiStyleOutline.alignment = anchor;
		this.myGuiStyle.alignment = anchor;
		
		this.GUIDisplayText(rect, text, textColor, outlineColor, fontSize);
	}

	public Vector2 CalculateSize(string text, int fontSize)
	{
		this.myGuiStyleOutline.fontSize = fontSize * this.screenResolutionFactor;
		return this.myGuiStyleOutline.CalcSize(new GUIContent(text)) + Vector2.one * this.outlineOffset;
	}

	public bool GUIButton(Rect rect, string caption)
	{
		var btnRect = rect;
		btnRect.x -= rect.width * 0.5f;
		btnRect.y -= rect.height * 0.5f;
		bool result = GUI.Button(btnRect, "", this.myButtonStyle);

		this.GUIDisplayText(rect, caption);
		return result;
	}

	public bool GUIButton(Rect rect, string caption, Color textColor, Color outlineColor, int fontSize)
	{
		var btnRect = rect;
		btnRect.x -= rect.width * 0.5f;
		btnRect.y -= rect.height * 0.5f;
		bool result = GUI.Button(btnRect, "", this.myButtonStyle);
		this.GUIDisplayText(btnRect, caption, textColor, outlineColor, fontSize);

		return result;
	}

	#endregion
	
	#region Implementation
	
	private void ResetGuiStyles()
	{
		this.myGuiStyle.alignment = TextAnchor.MiddleCenter;
		this.myGuiStyleOutline.alignment = TextAnchor.MiddleCenter;
		if(this.DisplayFont != null)
		{
			this.myGuiStyle.font = DisplayFont;
			this.myGuiStyleOutline.font = DisplayFont;
		}
		this.myGuiStyle.fontSize = fontSize * this.screenResolutionFactor;
		this.myGuiStyleOutline.fontSize = fontSize * this.screenResolutionFactor;
		this.myGuiStyleOutline.normal.textColor = Color.black;
		this.myGuiStyle.normal.textColor = Color.white;

		//this.myButtonStyle.normal.background = null;
		this.myButtonStyle.hover.background = null;
		this.myButtonStyle.active.background = null;
	}
	
	#endregion
	
	#region Data
	
	private GUIStyle myGuiStyle = new GUIStyle();
	private GUIStyle myGuiStyleOutline = new GUIStyle();
	private GUIStyle myButtonStyle = new GUIStyle();
	private int screenResolutionFactor = 0;

	#endregion
}
