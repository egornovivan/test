using System.Collections.Generic;
using System.IO;
using Jboy;
using UnityEngine;

public class ServerAdministrator : MonoBehaviour
{
	private static List<UserAdmin> UserAdminList = new List<UserAdmin>();

	private static List<int> LockedArea = new List<int>();

	private static bool _allowJoin = true;

	private static bool _allowModify = true;

	public static bool AllowJoin
	{
		get
		{
			return _allowJoin;
		}
		set
		{
			_allowJoin = value;
		}
	}

	public static bool AllowModify
	{
		get
		{
			return _allowModify;
		}
		set
		{
			_allowModify = value;
		}
	}

	public static void ReadConfig()
	{
		string path = Path.Combine(ServerConfig.ServerDir, "Admin.conf");
		if (!File.Exists(path))
		{
			return;
		}
		string jsonText = File.ReadAllText(path);
		JsonReader jsonReader = new JsonReader(jsonText);
		JsonToken jsonToken = JsonToken.None;
		if ((jsonToken = jsonReader.Read(out var value)) != JsonToken.ObjectStart)
		{
			return;
		}
		while ((jsonToken = jsonReader.Read(out value)) != JsonToken.ObjectEnd)
		{
			if (jsonToken != JsonToken.PropertyName)
			{
				continue;
			}
			if (value.Equals("AllowJoin"))
			{
				_allowJoin = Json.ReadObject<bool>(jsonReader);
			}
			else if (value.Equals("AllowModify"))
			{
				_allowModify = Json.ReadObject<bool>(jsonReader);
			}
			else if (value.Equals("Roles"))
			{
				if ((jsonToken = jsonReader.Read(out value)) != JsonToken.ArrayStart)
				{
					continue;
				}
				while ((jsonToken = jsonReader.Read(out value)) != JsonToken.ArrayEnd)
				{
					if (jsonToken != JsonToken.ObjectStart)
					{
						continue;
					}
					int roleId = -1;
					string roleName = string.Empty;
					ulong privileges = 0uL;
					while ((jsonToken = jsonReader.Read(out value)) != JsonToken.ObjectEnd)
					{
						if (jsonToken == JsonToken.PropertyName)
						{
							if (value.Equals("ID"))
							{
								roleId = Json.ReadObject<int>(jsonReader);
							}
							else if (value.Equals("Name"))
							{
								roleName = Json.ReadObject<string>(jsonReader);
							}
							else if (value.Equals("TypeMask"))
							{
								privileges = Json.ReadObject<ulong>(jsonReader);
							}
						}
					}
					if (roleId != -1 && !UserAdminList.Exists((UserAdmin iter) => iter.Id == roleId))
					{
						UserAdmin item = new UserAdmin(roleId, roleName, privileges);
						UserAdminList.Add(item);
					}
				}
			}
			else
			{
				if (!value.Equals("BlockedAreas") || (jsonToken = jsonReader.Read(out value)) != JsonToken.ArrayStart)
				{
					continue;
				}
				while ((jsonToken = jsonReader.Read(out value)) != JsonToken.ArrayEnd)
				{
					int item2 = Json.ReadObject<int>(jsonReader);
					if (!LockedArea.Contains(item2))
					{
						LockedArea.Add(item2);
					}
				}
			}
		}
	}

	public static void CreateConfig()
	{
		string path = Path.Combine(ServerConfig.ServerDir, "Admin.conf");
		JsonWriter jsonWriter = new JsonWriter(validate: true, prettyPrint: true, 2u);
		jsonWriter.WriteObjectStart();
		jsonWriter.WritePropertyName("AllowJoin");
		jsonWriter.WriteBoolean(AllowJoin);
		jsonWriter.WritePropertyName("AllowModify");
		jsonWriter.WriteBoolean(AllowModify);
		jsonWriter.WritePropertyName("Roles");
		jsonWriter.WriteArrayStart();
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			jsonWriter.WriteObjectStart();
			jsonWriter.WritePropertyName("ID");
			jsonWriter.WriteNumber(userAdmin.Id);
			jsonWriter.WritePropertyName("Name");
			jsonWriter.WriteString(userAdmin.RoleName);
			jsonWriter.WritePropertyName("TypeMask");
			jsonWriter.WriteNumber(userAdmin.Privileges);
			jsonWriter.WriteObjectEnd();
		}
		jsonWriter.WriteArrayEnd();
		jsonWriter.WritePropertyName("BlockedAreas");
		jsonWriter.WriteArrayStart();
		foreach (int item in LockedArea)
		{
			jsonWriter.WriteNumber(item);
		}
		jsonWriter.WriteArrayEnd();
		jsonWriter.WriteObjectEnd();
		File.WriteAllText(path, jsonWriter.ToString());
	}

	public static void OnPlayerInitializedEvent(NetInterface netlayer)
	{
		Player player = netlayer as Player;
		if (!(null == player) && !UserAdminList.Exists((UserAdmin iter) => iter.Id == player.Id))
		{
			UserAdmin userAdmin = new UserAdmin(player.Id, player.roleName, 0uL);
			if (player.roleName.Equals(ServerConfig.MasterRoleName))
			{
				userAdmin.AddPrivileges(AdminMask.AdminRole);
				UserAdminList.Insert(0, userAdmin);
			}
			else
			{
				UserAdminList.Add(userAdmin);
			}
		}
	}

	public static bool IsAdmin(int id)
	{
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.AdminRole));
	}

	public static bool IsAssistant(int id)
	{
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.AssistRole));
	}

	public static bool IsBlack(int id)
	{
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.BlackRole));
	}

	public static bool IsBuildLock(int id)
	{
		if (!AllowModify && !IsAdmin(id))
		{
			return true;
		}
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.BuildLock));
	}

	public static void AddBlacklist(int playerId)
	{
		UserAdminList.Find((UserAdmin iter) => iter.Id == playerId)?.AddPrivileges(AdminMask.BlackRole);
	}

	public static void DeleteBlacklist(int id)
	{
		UserAdminList.Find((UserAdmin iter) => iter.Id == id)?.RemovePrivileges(AdminMask.BlackRole);
	}

	public static void ClearBlacklist()
	{
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			userAdmin.RemovePrivileges(AdminMask.BlackRole);
		}
	}

	public static bool IsLockedArea(int areaIndex)
	{
		return LockedArea.Contains(areaIndex);
	}

	public static void LockArea(int areaIndex)
	{
		if (!LockedArea.Contains(areaIndex))
		{
			LockedArea.Add(areaIndex);
		}
	}

	public static void UnLockArea(int areaIndex)
	{
		LockedArea.Remove(areaIndex);
	}

	public static void AddAssistant(int playerId)
	{
		UserAdminList.Find((UserAdmin iter) => iter.Id == playerId)?.AddPrivileges(AdminMask.AssistRole);
	}

	public static void DeleteAssistant(int id)
	{
		UserAdminList.Find((UserAdmin iter) => iter.Id == id)?.RemovePrivileges(AdminMask.AssistRole);
	}

	public static void ClearAssistant()
	{
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			userAdmin.RemovePrivileges(AdminMask.AssistRole);
		}
	}

	public static void LockBuild(int playerId)
	{
		UserAdminList.Find((UserAdmin iter) => iter.Id == playerId)?.AddPrivileges(AdminMask.BuildLock);
	}

	public static void BuildUnLock(int id)
	{
		UserAdminList.Find((UserAdmin iter) => iter.Id == id)?.RemovePrivileges(AdminMask.BuildLock);
	}

	public static void ClearBuildLock()
	{
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			userAdmin.RemovePrivileges(AdminMask.BuildLock);
		}
	}

	public static byte[] SerializeAdminData()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		BufferHelper.Serialize(writer, AllowJoin);
		BufferHelper.Serialize(writer, AllowModify);
		BufferHelper.Serialize(writer, UserAdminList.Count);
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			BufferHelper.Serialize(writer, userAdmin.Id);
			BufferHelper.Serialize(writer, userAdmin.RoleName);
			BufferHelper.Serialize(writer, userAdmin.Privileges);
		}
		BufferHelper.Serialize(writer, LockedArea.Count);
		foreach (int item in LockedArea)
		{
			BufferHelper.Serialize(writer, item);
		}
		return memoryStream.ToArray();
	}
}
