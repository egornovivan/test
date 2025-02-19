using System.Collections.Generic;
using UnityEngine;

public class RPCController : MonoBehaviour
{
	public static Dictionary<string, bool> RPCEnable = new Dictionary<string, bool>();

	private void Awake()
	{
		RPCEnable.Clear();
		RPCEnable.Add("RPC_C2S_GetShop", value: true);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public static void Reset(string rpcName)
	{
		RPCEnable[rpcName] = true;
	}
}
