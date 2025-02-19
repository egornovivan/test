using System.Collections.Generic;
using PETools;
using UnityEngine;

public class PEChat
{
	public class Chat
	{
		private int m_Initiator;

		private float m_ChatRadius;

		private float m_JoinRadius;

		private Vector3 m_Position;

		private List<int> m_ChatIDs;

		public int Initiator => m_Initiator;

		public Vector3 Position => m_Position;

		public float ChatRadius => m_ChatRadius;

		public float JoinRadius => m_JoinRadius;

		public Chat(int initiator, Vector3 pos, float minRadius, float maxRadius)
		{
			m_Initiator = initiator;
			m_Position = pos;
			m_ChatRadius = minRadius;
			m_JoinRadius = maxRadius;
			m_ChatIDs = new List<int>();
			m_ChatIDs.Add(m_Initiator);
		}

		public void Join(int id)
		{
			if (!m_ChatIDs.Contains(id))
			{
				m_ChatIDs.Add(id);
			}
		}

		public void Exit(int id)
		{
			if (m_ChatIDs.Contains(id))
			{
				m_ChatIDs.Remove(id);
			}
		}
	}

	private static List<Chat> s_Chats = new List<Chat>();

	public static Chat CreatChat(int initiator, Vector3 pos, float minRadius, float maxRadius)
	{
		Chat chat = new Chat(initiator, pos, minRadius, maxRadius);
		s_Chats.Add(chat);
		return chat;
	}

	public static Chat GetChat(Vector3 pos)
	{
		return s_Chats.Find((Chat ret) => PEUtil.SqrMagnitudeH(ret.Position, pos) < ret.JoinRadius * ret.JoinRadius);
	}
}
