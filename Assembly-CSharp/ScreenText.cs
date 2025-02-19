using System.Collections.Generic;
using UnityEngine;

public class ScreenText : MonoBehaviour
{
	private sealed class ScreenTextData
	{
		public string Text;

		public float Timer;
	}

	public GUIStyle Style = new GUIStyle();

	public float MessageFadeTime = 5f;

	private readonly List<ScreenTextData> messages = new List<ScreenTextData>();

	public void AddMessage(string message)
	{
		messages.Add(new ScreenTextData
		{
			Text = message,
			Timer = MessageFadeTime
		});
	}

	private void Update()
	{
		for (int num = messages.Count - 1; num >= 0; num--)
		{
			ScreenTextData screenTextData = messages[num];
			if (screenTextData.Timer > 0f)
			{
				screenTextData.Timer -= Time.deltaTime;
				if (screenTextData.Timer <= 0f)
				{
					messages.RemoveAt(num);
				}
			}
		}
	}

	private void OnGUI()
	{
		Vector2 vector = GUIUtility.ScreenToGUIPoint(new Vector2(Screen.width, Screen.height));
		float num = 5f;
		vector -= new Vector2(num, num);
		for (int num2 = messages.Count - 1; num2 >= 0; num2--)
		{
			ScreenTextData screenTextData = messages[num2];
			GUIContent content = new GUIContent(screenTextData.Text);
			Vector2 vector2 = GUI.skin.label.CalcSize(content);
			GUI.Label(new Rect(vector.x - vector2.x, vector.y - vector2.y, vector2.x, vector2.y), content, Style);
			vector -= new Vector2(0f, vector2.y + num);
		}
	}

	public static void Log(object obj)
	{
		Log(obj.ToString());
	}

	public static void Log(string format, params object[] args)
	{
		ScreenText screenText = Object.FindObjectOfType<ScreenText>();
		if (!(screenText == null))
		{
			screenText.AddMessage(string.Format(format, args));
		}
	}
}
