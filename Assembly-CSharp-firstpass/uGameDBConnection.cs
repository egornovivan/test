using System;
using uGameDB;
using UnityEngine;

public class uGameDBConnection : MonoBehaviour
{
	[Serializable]
	public class RiakNode
	{
		public string HostName = "localhost";

		public int RiakPbcPort = 8087;

		public string UniqueId = "riak";

		public int SocketPoolSize = 10;
	}

	public RiakNode[] RiakNodes = new RiakNode[1];

	public void Awake()
	{
		ReconnectDB();
	}

	private void ReconnectDB()
	{
		Database.Disconnect();
		Database.RemoveAllNodes();
		RiakNode[] riakNodes = RiakNodes;
		foreach (RiakNode riakNode in riakNodes)
		{
			Database.AddNode(riakNode.UniqueId, riakNode.HostName, riakNode.RiakPbcPort, riakNode.SocketPoolSize, 5000, 5000);
		}
		Database.Connect(OnDBConnect, OnDBError);
	}

	private void OnDBConnect()
	{
		Debug.Log("OnDBConnect successful");
	}

	private void OnDBError(string error)
	{
		Debug.LogError("Connect error: " + error);
	}
}
