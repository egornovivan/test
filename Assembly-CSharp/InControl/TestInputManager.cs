using System.Collections.Generic;
using UnityEngine;

namespace InControl;

public class TestInputManager : MonoBehaviour
{
	public Font font;

	private GUIStyle style = new GUIStyle();

	private List<LogMessage> logMessages = new List<LogMessage>();

	private bool isPaused;

	private void OnEnable()
	{
		isPaused = false;
		Time.timeScale = 1f;
		Logger.OnLogMessage += delegate(LogMessage logMessage)
		{
			logMessages.Add(logMessage);
		};
		InputManager.OnDeviceAttached += delegate(InputDevice inputDevice)
		{
			Debug.Log("Attached: " + inputDevice.Name);
		};
		InputManager.OnDeviceDetached += delegate(InputDevice inputDevice)
		{
			Debug.Log("Detached: " + inputDevice.Name);
		};
		InputManager.OnActiveDeviceChanged += delegate(InputDevice inputDevice)
		{
			Debug.Log("Active device changed to: " + inputDevice.Name);
		};
		TestInputMappings();
	}

	private void FixedUpdate()
	{
		CheckForPauseButton();
	}

	private void Update()
	{
		if (isPaused)
		{
			CheckForPauseButton();
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			Application.LoadLevel("TestInputManager");
		}
	}

	private void CheckForPauseButton()
	{
		if (Input.GetKeyDown(KeyCode.P) || InputManager.MenuWasPressed)
		{
			Time.timeScale = ((!isPaused) ? 0f : 1f);
			isPaused = !isPaused;
		}
	}

	private void SetColor(Color color)
	{
		style.normal.textColor = color;
	}

	private void OnGUI()
	{
		int num = 300;
		int num2 = 10;
		int num3 = 10;
		int num4 = 15;
		GUI.skin.font = font;
		SetColor(Color.white);
		string text = "Devices:";
		text = text + " (Platform: " + InputManager.Platform + ")";
		text = text + " " + InputManager.ActiveDevice.Direction.Vector;
		if (isPaused)
		{
			SetColor(Color.red);
			text = "+++ PAUSED +++";
		}
		GUI.Label(new Rect(num2, num3, num2 + num, num3 + 10), text, style);
		SetColor(Color.white);
		foreach (InputDevice device in InputManager.Devices)
		{
			Color color = ((InputManager.ActiveDevice != device) ? Color.white : Color.yellow);
			num3 = 35;
			SetColor(color);
			GUI.Label(new Rect(num2, num3, num2 + num, num3 + 10), device.Name, style);
			num3 += num4;
			GUI.Label(new Rect(num2, num3, num2 + num, num3 + 10), "SortOrder: " + device.SortOrder, style);
			num3 += num4;
			GUI.Label(new Rect(num2, num3, num2 + num, num3 + 10), "LastChangeTick: " + device.LastChangeTick, style);
			num3 += num4;
			InputControl[] controls = device.Controls;
			foreach (InputControl inputControl in controls)
			{
				if (inputControl != null)
				{
					string arg = ((!device.IsKnown) ? inputControl.Handle : $"{inputControl.Target} ({inputControl.Handle})");
					SetColor((!inputControl.State) ? color : Color.green);
					string text2 = string.Format("{0} {1}", arg, (!inputControl.State) ? string.Empty : ("= " + inputControl.Value));
					GUI.Label(new Rect(num2, num3, num2 + num, num3 + 10), text2, style);
					num3 += num4;
				}
			}
			SetColor(Color.cyan);
			InputControl anyButton = device.AnyButton;
			if ((bool)anyButton)
			{
				GUI.Label(new Rect(num2, num3, num2 + num, num3 + 10), "AnyButton = " + anyButton.Handle, style);
			}
			num2 += 200;
		}
		Color[] array = new Color[3]
		{
			Color.gray,
			Color.yellow,
			Color.white
		};
		SetColor(Color.white);
		num2 = 10;
		num3 = Screen.height - (10 + num4);
		for (int num5 = logMessages.Count - 1; num5 >= 0; num5--)
		{
			LogMessage logMessage = logMessages[num5];
			SetColor(array[(int)logMessage.type]);
			string[] array2 = logMessage.text.Split('\n');
			foreach (string text3 in array2)
			{
				GUI.Label(new Rect(num2, num3, Screen.width, num3 + 10), text3, style);
				num3 -= num4;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 center = InputManager.ActiveDevice.Direction.Vector * 4f;
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(center, 1f);
	}

	private void TestInputMappings()
	{
		InputControlMapping.Range complete = InputControlMapping.Range.Complete;
		InputControlMapping.Range positive = InputControlMapping.Range.Positive;
		InputControlMapping.Range negative = InputControlMapping.Range.Negative;
		bool invert = false;
		bool invert2 = true;
		TestInputMapping(complete, complete, invert, -1f, 0f, 1f);
		TestInputMapping(complete, negative, invert, -1f, -0.5f, 0f);
		TestInputMapping(complete, positive, invert, 0f, 0.5f, 1f);
		TestInputMapping(negative, complete, invert, -1f, 1f, 0f);
		TestInputMapping(negative, negative, invert, -1f, 0f, 0f);
		TestInputMapping(negative, positive, invert, 0f, 1f, 0f);
		TestInputMapping(positive, complete, invert, 0f, -1f, 1f);
		TestInputMapping(positive, negative, invert, 0f, -1f, 0f);
		TestInputMapping(positive, positive, invert, 0f, 0f, 1f);
		TestInputMapping(complete, complete, invert2, 1f, 0f, -1f);
		TestInputMapping(complete, negative, invert2, 1f, 0.5f, 0f);
		TestInputMapping(complete, positive, invert2, 0f, -0.5f, -1f);
		TestInputMapping(negative, complete, invert2, 1f, -1f, 0f);
		TestInputMapping(negative, negative, invert2, 1f, 0f, 0f);
		TestInputMapping(negative, positive, invert2, 0f, -1f, 0f);
		TestInputMapping(positive, complete, invert2, 0f, 1f, -1f);
		TestInputMapping(positive, negative, invert2, 0f, 1f, 0f);
		TestInputMapping(positive, positive, invert2, 0f, 0f, -1f);
	}

	private void TestInputMapping(InputControlMapping.Range sourceRange, InputControlMapping.Range targetRange, bool invert, float expectA, float expectB, float expectC)
	{
		InputControlMapping inputControlMapping = new InputControlMapping();
		inputControlMapping.SourceRange = sourceRange;
		inputControlMapping.TargetRange = targetRange;
		inputControlMapping.Invert = invert;
		InputControlMapping inputControlMapping2 = inputControlMapping;
		string text = "Complete";
		if (sourceRange == InputControlMapping.Range.Negative)
		{
			text = "Negative";
		}
		else if (sourceRange == InputControlMapping.Range.Positive)
		{
			text = "Positive";
		}
		string text2 = "Complete";
		if (targetRange == InputControlMapping.Range.Negative)
		{
			text2 = "Negative";
		}
		else if (targetRange == InputControlMapping.Range.Positive)
		{
			text2 = "Positive";
		}
		float num = -1f;
		float num2 = inputControlMapping2.MapValue(num);
		if (Mathf.Abs(num2 - expectA) > float.Epsilon)
		{
			Debug.LogError("Input of " + num + " got value of " + num2 + " instead of " + expectA + " (SR = " + text + ", TR = " + text2 + ")");
		}
		num = 0f;
		num2 = inputControlMapping2.MapValue(num);
		if (Mathf.Abs(num2 - expectB) > float.Epsilon)
		{
			Debug.LogError("Input of " + num + " got value of " + num2 + " instead of " + expectB + " (SR = " + text + ", TR = " + text2 + ")");
		}
		num = 1f;
		num2 = inputControlMapping2.MapValue(num);
		if (Mathf.Abs(num2 - expectC) > float.Epsilon)
		{
			Debug.LogError("Input of " + num + " got value of " + num2 + " instead of " + expectC + " (SR = " + text + ", TR = " + text2 + ")");
		}
	}
}
