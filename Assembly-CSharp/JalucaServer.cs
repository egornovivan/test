using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class JalucaServer : MonoBehaviour
{
	private const int portNo = 8000;

	private void Start()
	{
		IPAddress listen_ip = IPAddress.Parse("127.0.0.1");
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		TcpListener tcpListener = new TcpListener(listen_ip, 8000);
		tcpListener.Start();
		Console.WriteLine("Server is starting...\n");
		byte[] array = new byte[1024];
		int num = 0;
		while (true)
		{
			num = socket.Receive(array, array.Length, SocketFlags.None);
			if (num < 0)
			{
				num = 0;
			}
		}
	}

	private void Update()
	{
	}
}
