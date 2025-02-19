using UnityEngine;

namespace PeCustom;

public class CustomGUIMgr
{
	public delegate void DNotify();

	public delegate void DStringNotify(string str);

	private string _title = string.Empty;

	private string _subtitle = string.Empty;

	private float _title_starttime;

	private float _title_showtime;

	private float _title_lifetime;

	private Color _title_color = Color.clear;

	public event DNotify OnGUIDrawing;

	public event DStringNotify OnGUIResponse;

	public void DrawGUI()
	{
		GUI.color = Color.white;
		if (this.OnGUIDrawing != null)
		{
			this.OnGUIDrawing();
		}
		TitleGUI();
	}

	public void GUIResponse(string eventname)
	{
		if (this.OnGUIResponse != null)
		{
			this.OnGUIResponse(eventname);
		}
	}

	public void ShowTitle(string title, string subtitle, float time, Color color)
	{
		_title = title;
		_subtitle = subtitle;
		_title_starttime = Time.time;
		_title_showtime = time;
		_title_lifetime = time;
		_title_color = color;
	}

	private void TitleGUI()
	{
		_title_lifetime = _title_showtime - (Time.time - _title_starttime);
		float a = Mathf.Clamp01((_title_showtime - _title_lifetime) * 2f);
		float b = Mathf.Clamp01(_title_lifetime * 2f);
		Color title_color = _title_color;
		title_color.a = Mathf.Min(a, b);
		if (_title_lifetime > 0.001f)
		{
			Color color = GUI.color;
			GUI.color = title_color;
			GUI.skin = Resources.Load<GUISkin>("GUISkin/CustomGames");
			GUI.Label(new Rect(0f, 160f, Screen.width, 100f), _title, "TitleText");
			GUI.Label(new Rect(0f, 260f, Screen.width, 50f), _subtitle, "SubtitleText");
			GUI.color = color;
		}
	}
}
