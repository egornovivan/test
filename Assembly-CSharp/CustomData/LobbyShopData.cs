using uLink;

namespace CustomData;

public class LobbyShopData
{
	public int id;

	public int itemtype;

	public int price;

	public int rebate;

	public int tab;

	public bool bind;

	public bool bshow;

	public int forbid;

	public static void Serialize(BitStream stream, object obj, params object[] codecOptions)
	{
		LobbyShopData lobbyShopData = (LobbyShopData)obj;
		stream.Write(lobbyShopData.id);
		stream.Write(lobbyShopData.itemtype);
		stream.Write(lobbyShopData.price);
		stream.Write(lobbyShopData.rebate);
		stream.Write(lobbyShopData.tab);
		stream.Write(lobbyShopData.bind);
		stream.Write(lobbyShopData.bshow);
		stream.Write(lobbyShopData.forbid);
	}

	public static object Deserialize(BitStream stream, params object[] codecOptions)
	{
		LobbyShopData lobbyShopData = new LobbyShopData();
		lobbyShopData.id = stream.Read<int>(new object[0]);
		lobbyShopData.itemtype = stream.Read<int>(new object[0]);
		lobbyShopData.price = stream.Read<int>(new object[0]);
		lobbyShopData.rebate = stream.Read<int>(new object[0]);
		lobbyShopData.tab = stream.Read<int>(new object[0]);
		lobbyShopData.bind = stream.Read<bool>(new object[0]);
		lobbyShopData.bshow = stream.Read<bool>(new object[0]);
		lobbyShopData.forbid = stream.Read<int>(new object[0]);
		return lobbyShopData;
	}
}
