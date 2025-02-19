using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using UnityEngine;

[AddComponentMenu("uLink Utilities/Console GUI")]
internal class uLinkConsoleGUI : MonoBehaviour
{
	[Serializable]
	public class LogMask
	{
		public bool message;

		public bool warning;

		public bool error;

		public LogMask()
		{
		}

		public LogMask(bool message, bool warning, bool error)
		{
			this.message = message;
			this.warning = warning;
			this.error = error;
		}

		public bool Filter(LogType type)
		{
			switch (type)
			{
			case LogType.Log:
				return message;
			case LogType.Warning:
				return warning;
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				return error;
			default:
				return true;
			}
		}
	}

	private class CollapsedEntries : KeyedCollection<string, Entry>
	{
		protected override string GetKeyForItem(Entry entry)
		{
			return entry.key;
		}
	}

	private class Entry
	{
		public string key;

		public string log;

		public string logSpaced;

		public string stacktrace;

		public string stacktraceTabed;

		public LogType type;

		public bool expanded;
	}

	public enum Position
	{
		Bottom,
		Top
	}

	private const float WINDOW_MARGIN_X = 10f;

	private const float WINDOW_MARGIN_Y = 10f;

	public Position position;

	public LogMask captureLogMask = new LogMask
	{
		message = true,
		warning = true,
		error = true
	};

	public LogMask filterLogMask = new LogMask
	{
		message = true,
		warning = true,
		error = true
	};

	public LogMask autoShowOnLogMask = new LogMask
	{
		message = false,
		warning = true,
		error = true
	};

	public int maxEntries = 1000;

	public int windowheight = 150;

	public KeyCode showByKey = KeyCode.Tab;

	[SerializeField]
	private bool _isVisible = true;

	public bool dontDestroyOnLoad;

	public GUISkin guiSkin;

	public int guiDepth;

	public string saveFilename = "console_{0:yyyy-MM-dd_HH.mm.ss}{1}.log";

	public bool collapse;

	public bool autoScroll = true;

	public bool unlockCursorWhenVisible = true;

	private List<Entry> entries;

	private int messageCount;

	private int warningCount;

	private int errorCount;

	private Vector2 scrollPosition;

	private bool oldLockCursor;

	private static readonly Color[] typeColors = new Color[5]
	{
		Color.red,
		Color.magenta,
		Color.yellow,
		Color.white,
		Color.red
	};

	public bool isVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			SetVisible(value);
		}
	}

	private void Awake()
	{
		entries = new List<Entry>(maxEntries);
		if (_isVisible)
		{
			_isVisible = false;
			SetVisible(visibility: true);
		}
		Application.logMessageReceived += CaptureLog;
	}

	private void Update()
	{
		if (showByKey != 0 && Input.GetKeyDown(showByKey))
		{
			SetVisible(!_isVisible);
		}
	}

	private void OnGUI()
	{
		if (_isVisible)
		{
			GUISkin skin = GUI.skin;
			int depth = GUI.depth;
			Color color = GUI.color;
			GUI.skin = guiSkin;
			GUI.depth = guiDepth;
			GUI.color = Color.white;
			float y = ((position != 0) ? 10f : ((float)(Screen.height - windowheight) - 10f));
			GUILayout.BeginArea(new Rect(10f, y, (float)Screen.width - 20f, windowheight), GUI.skin.box);
			DrawGUI();
			GUILayout.EndArea();
			GUI.skin = skin;
			GUI.depth = depth;
			GUI.color = color;
		}
	}

	private void DrawGUI()
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.box);
		gUIStyle.alignment = TextAnchor.UpperLeft;
		gUIStyle.fontSize = 10;
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		if (entries.Count == 0)
		{
			GUI.enabled = false;
		}
		if (GUILayout.Button("Clear", GUILayout.Width(50f)))
		{
			Clear();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Save", GUILayout.Width(50f)))
		{
			Save();
		}
		GUILayout.Space(5f);
		if (entries.Count == 0)
		{
			GUI.enabled = true;
		}
		collapse = GUILayout.Toggle(collapse, " Collapse", GUILayout.ExpandWidth(expand: false));
		GUILayout.Space(5f);
		autoScroll = GUILayout.Toggle(autoScroll, " Auto Scroll", GUILayout.ExpandWidth(expand: false));
		GUILayout.FlexibleSpace();
		int num;
		int num2;
		int num3;
		IEnumerable<Entry> enumerable;
		if (collapse)
		{
			CollapsedEntries collapsedEntries = new CollapsedEntries();
			num = 0;
			num2 = 0;
			num3 = 0;
			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (!collapsedEntries.Contains(entry.key))
				{
					collapsedEntries.Add(entry);
					switch (entry.type)
					{
					case LogType.Log:
						num++;
						break;
					case LogType.Warning:
						num2++;
						break;
					case LogType.Error:
					case LogType.Assert:
					case LogType.Exception:
						num3++;
						break;
					}
				}
			}
			enumerable = collapsedEntries;
		}
		else
		{
			enumerable = entries;
			num = messageCount;
			num2 = warningCount;
			num3 = errorCount;
		}
		GUI.color = typeColors[3];
		filterLogMask.message = GUILayout.Toggle(filterLogMask.message, " " + num + " Message(s)", GUILayout.ExpandWidth(expand: false));
		GUI.color = Color.white;
		GUILayout.Space(5f);
		GUI.color = typeColors[2];
		filterLogMask.warning = GUILayout.Toggle(filterLogMask.warning, " " + num2 + " Warning(s)", GUILayout.ExpandWidth(expand: false));
		GUI.color = Color.white;
		GUILayout.Space(5f);
		GUI.color = typeColors[0];
		filterLogMask.error = GUILayout.Toggle(filterLogMask.error, " " + num3 + " Error(s)", GUILayout.ExpandWidth(expand: false));
		GUI.color = Color.white;
		GUILayout.EndHorizontal();
		GUI.changed = false;
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUI.skin.box);
		if (GUI.changed)
		{
			autoScroll = false;
		}
		foreach (Entry item in enumerable)
		{
			if (filterLogMask.Filter(item.type))
			{
				int type = (int)item.type;
				GUI.color = ((type >= typeColors.Length) ? Color.white : typeColors[type]);
				GUI.changed = false;
				item.expanded = GUILayout.Toggle(item.expanded, item.logSpaced, GUILayout.ExpandWidth(expand: false));
				if (GUI.changed)
				{
					autoScroll = false;
				}
				if (item.expanded)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(20f);
					GUILayout.Label(item.stacktrace, gUIStyle);
					GUILayout.EndHorizontal();
				}
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	public void SetVisible(bool visibility)
	{
		if (_isVisible == visibility)
		{
			return;
		}
		_isVisible = visibility;
		if (unlockCursorWhenVisible)
		{
			if (visibility)
			{
				oldLockCursor = Cursor.lockState == CursorLockMode.None || CursorLockMode.Confined == Cursor.lockState;
			}
			else if (oldLockCursor)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.lockState = (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None);
				Cursor.visible = true;
			}
		}
	}

	public void Clear()
	{
		entries.Clear();
		messageCount = 0;
		warningCount = 0;
		errorCount = 0;
	}

	public void Save()
	{
		int num = 0;
		DateTime now = DateTime.Now;
		string empty = string.Empty;
		do
		{
			empty = string.Format(saveFilename, now, (num <= 0) ? string.Empty : ("_" + num));
			num++;
			if (num == 1000)
			{
				Debug.LogError("Failed to save console log, because too many log files with the same filename");
				return;
			}
		}
		while (File.Exists(empty));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < entries.Count; i++)
		{
			Entry entry = entries[i];
			stringBuilder.AppendLine(entry.log);
			stringBuilder.AppendLine(entry.stacktraceTabed);
			stringBuilder.AppendLine();
		}
		Debug.Log("Saving console log to " + empty);
		File.WriteAllText(empty, stringBuilder.ToString());
	}

	private void CaptureLog(string log, string stacktrace, LogType type)
	{
		if (!captureLogMask.Filter(type))
		{
			return;
		}
		if (autoShowOnLogMask.Filter(type))
		{
			isVisible = true;
		}
		if (entries.Count == maxEntries)
		{
			LogType type2 = entries[0].type;
			entries.RemoveAt(0);
			switch (type2)
			{
			case LogType.Log:
				messageCount--;
				break;
			case LogType.Warning:
				warningCount--;
				break;
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				errorCount--;
				break;
			}
		}
		stacktrace = stacktrace.Trim('\n');
		string key = $"{(int)type}:{log}\n{stacktrace}";
		string logSpaced = ' ' + log.Replace("\n", "\n ");
		string stacktraceTabed = '\t' + stacktrace.Replace("\n", "\n\t");
		Entry entry = new Entry();
		entry.key = key;
		entry.log = log;
		entry.logSpaced = logSpaced;
		entry.stacktrace = stacktrace;
		entry.stacktraceTabed = stacktraceTabed;
		entry.type = type;
		entry.expanded = false;
		Entry item = entry;
		entries.Add(item);
		switch (type)
		{
		case LogType.Log:
			messageCount++;
			break;
		case LogType.Warning:
			warningCount++;
			break;
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			errorCount++;
			break;
		}
		if (autoScroll)
		{
			scrollPosition.y = float.MaxValue;
		}
	}
}
