using System;
using System.Globalization;
using uLink;
using UnityEngine;

[AddComponentMenu("uLink Utilities/Statistics GUI")]
public class uLinkStatisticsGUI : uLink.MonoBehaviour
{
	public enum Position
	{
		BottomLeft,
		BottomRight,
		TopLeft,
		TopRight
	}

	private const float WINDOW_MARGIN_X = 10f;

	private const float WINDOW_MARGIN_Y = 10f;

	private const float WINDOW_WDITH = 300f;

	private const float COLUMN_WIDTH = 150f;

	public Position position = Position.TopLeft;

	public bool showOnlyInEditor;

	public KeyCode enabledByKey = KeyCode.Tab;

	public bool isEnabled;

	public bool dontDestroyOnLoad;

	public GUISkin guiSkin;

	public int guiDepth;

	public bool showDetails;

	private void Awake()
	{
		if (dontDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		isEnabled = false;
	}

	private void Update()
	{
		if (enabledByKey != 0 && Input.GetKeyDown(enabledByKey))
		{
			isEnabled = !isEnabled;
		}
	}

	private void OnGUI()
	{
		if (isEnabled && (!showOnlyInEditor || Application.isEditor))
		{
			GUISkin skin = GUI.skin;
			int depth = GUI.depth;
			GUI.skin = guiSkin;
			GUI.depth = guiDepth;
			DrawGUI(position, showDetails);
			GUI.skin = skin;
			GUI.depth = depth;
		}
	}

	public static void DrawGUI()
	{
		DrawGUI(Position.TopLeft, showDetails: false);
	}

	public static void DrawGUI(Position position)
	{
		DrawGUI(position, showDetails: false);
	}

	public static void DrawGUI(bool showDetails)
	{
		DrawGUI(Position.TopLeft, showDetails);
	}

	public static void DrawGUI(Position position, bool showDetails)
	{
		uLink.NetworkPlayer[] connections = uLink.Network.connections;
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
		GUILayout.BeginHorizontal();
		if (position == Position.TopLeft || position == Position.BottomLeft)
		{
			GUILayout.Space(10f);
		}
		else
		{
			GUILayout.FlexibleSpace();
		}
		GUILayout.BeginVertical();
		if (position == Position.TopLeft || position == Position.TopRight)
		{
			GUILayout.Space(10f);
		}
		else
		{
			GUILayout.FlexibleSpace();
		}
		GUILayout.BeginVertical(GUILayout.Width(300f));
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Frame Rate:", GUILayout.Width(150f));
		GUILayout.Label(Mathf.RoundToInt(1f / Time.smoothDeltaTime).ToString(CultureInfo.InvariantCulture) + " FPS");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Status:", GUILayout.Width(150f));
		GUILayout.Label(NetworkUtility.GetStatusString(uLink.Network.peerType, uLink.Network.status));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Last Error:", GUILayout.Width(150f));
		GUILayout.Label(NetworkUtility.GetErrorString(uLink.Network.lastError));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Network Time:", GUILayout.Width(150f));
		GUILayout.Label(uLink.Network.time.ToString(CultureInfo.InvariantCulture) + " s");
		GUILayout.EndHorizontal();
		if (showDetails)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Server Time Offset:", GUILayout.Width(150f));
			GUILayout.Label(((double)uLink.Network.config.serverTimeOffsetInMillis * 0.001).ToString(CultureInfo.InvariantCulture) + " s");
			GUILayout.EndHorizontal();
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label("Network Objects:", GUILayout.Width(150f));
		GUILayout.Label(uLink.Network.networkViewCount.ToString(CultureInfo.InvariantCulture));
		GUILayout.EndHorizontal();
		if (uLink.Network.isServer)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Connections:", GUILayout.Width(150f));
			GUILayout.Label(connections.Length.ToString(CultureInfo.InvariantCulture));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Name in Master Server:", GUILayout.Width(150f));
			GUILayout.Label((!uLink.MasterServer.isRegistered) ? "Not Registered" : uLink.MasterServer.gameName);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		uLink.NetworkPlayer[] array = connections;
		for (int i = 0; i < array.Length; i++)
		{
			uLink.NetworkPlayer networkPlayer = array[i];
			NetworkStatistics statistics = networkPlayer.statistics;
			if (statistics != null)
			{
				GUILayout.BeginVertical("Box");
				GUILayout.BeginHorizontal();
				GUILayout.Label("Connection:", GUILayout.Width(150f));
				GUILayout.Label(networkPlayer.ToString());
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Ping (average):", GUILayout.Width(150f));
				GUILayout.Label(networkPlayer.lastPing + " (" + networkPlayer.averagePing + ") ms");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Sent:", GUILayout.Width(150f));
				GUILayout.Label((int)Math.Round(statistics.bytesSentPerSecond) + " B/s");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Receive:", GUILayout.Width(150f));
				GUILayout.Label((int)Math.Round(statistics.bytesReceivedPerSecond) + " B/s");
				GUILayout.EndHorizontal();
				if (showDetails)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Packets sent:", GUILayout.Width(150f));
					GUILayout.Label(statistics.packetsSent.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Packets received:", GUILayout.Width(150f));
					GUILayout.Label(statistics.packetsReceived.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Messages sent:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messagesSent.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Messages received:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messagesReceived.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Messages resent:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messagesResent.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Messages unsent:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messagesUnsent.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Messages stored:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messagesStored.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Messages withheld:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messagesWithheld.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Msg duplicates rejected:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messageDuplicatesRejected.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Msg sequences rejected:", GUILayout.Width(150f));
					GUILayout.Label(statistics.messageSequencesRejected.ToString(CultureInfo.InvariantCulture));
					GUILayout.EndHorizontal();
				}
				GUILayout.BeginHorizontal();
				GUILayout.Label("Encryption:", GUILayout.Width(150f));
				GUILayout.Label((!networkPlayer.hasSecurity) ? "Off" : "On");
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			}
		}
		GUILayout.EndVertical();
		if (position != Position.TopLeft && position != Position.TopRight)
		{
			GUILayout.Space(10f);
		}
		else
		{
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndVertical();
		if (position != Position.TopLeft && position != 0)
		{
			GUILayout.Space(10f);
		}
		else
		{
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
