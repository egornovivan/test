using System.IO;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

namespace SkillSystem;

public class SkEntity : MonoBehaviour
{
	internal SkEntity _parent;

	internal SkAttribs _attribs = new SkAttribs();

	internal SkAttribs _baseAttribs;

	internal ObjNetInterface _net;

	private bool _hasRecord;

	private int _id;

	public int ID => _id;

	private void OnDestroy()
	{
		Player.PlayerDisconnected -= OnPlayerDisconnect;
	}

	public void Init(ObjNetInterface net)
	{
		if (!(net == null))
		{
			_net = net;
			_id = net.Id;
			_hasRecord = false;
			LoadData();
			if (!_hasRecord)
			{
				SaveData();
			}
			Player.PlayerDisconnected += OnPlayerDisconnect;
		}
	}

	protected virtual void OnPlayerDisconnect(Player player)
	{
		if (_net.Equals(player))
		{
			SaveData();
		}
	}

	public void Import(byte[] data)
	{
		if (data == null || data.Length <= 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(data);
		using (BinaryReader binaryReader = new BinaryReader(memoryStream))
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int type = binaryReader.ReadInt32();
				float value = binaryReader.ReadSingle();
				SetAttribute((AttribType)type, value, isBase: true);
				type = binaryReader.ReadInt32();
				value = binaryReader.ReadSingle();
				SetAttribute((AttribType)type, value);
			}
			binaryReader.Close();
		}
		memoryStream.Close();
	}

	public byte[] Export()
	{
		byte[] array = null;
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		if (_baseAttribs == null)
		{
			binaryWriter.Write(0);
			binaryWriter.Close();
			array = memoryStream.ToArray();
			memoryStream.Close();
			return array;
		}
		int num = 0;
		for (int i = 0; i < SkAttribs.nAttribs; i++)
		{
			if (_attribs.NumAttribs.GetAttribute((AttribType)i, isBase: true) != _baseAttribs.NumAttribs.GetAttribute((AttribType)i, isBase: true) || _attribs.NumAttribs.GetAttribute((AttribType)i) != _baseAttribs.NumAttribs.GetAttribute((AttribType)i))
			{
				num++;
			}
		}
		binaryWriter.Write(num);
		for (int j = 0; j < SkAttribs.nAttribs; j++)
		{
			if (_attribs.NumAttribs.GetAttribute((AttribType)j, isBase: true) != _baseAttribs.NumAttribs.GetAttribute((AttribType)j, isBase: true) || _attribs.NumAttribs.GetAttribute((AttribType)j) != _baseAttribs.NumAttribs.GetAttribute((AttribType)j))
			{
				binaryWriter.Write(j);
				binaryWriter.Write(_attribs.NumAttribs.GetAttribute((AttribType)j, isBase: true));
				binaryWriter.Write(j);
				binaryWriter.Write(_attribs.NumAttribs.GetAttribute((AttribType)j));
			}
		}
		binaryWriter.Close();
		array = memoryStream.ToArray();
		memoryStream.Close();
		return array;
	}

	public void SaveData()
	{
		byte[] data = Export();
		EntityData entityData = new EntityData();
		entityData.ExportData(_id, data);
		AsyncSqlite.AddRecord(entityData);
	}

	public void LoadComplete(SqliteDataReader dataReader)
	{
		if (dataReader.Read())
		{
			int @int = dataReader.GetInt32(dataReader.GetOrdinal("ver"));
			byte[] data = (byte[])dataReader.GetValue(dataReader.GetOrdinal("data"));
			Import(data);
			_hasRecord = true;
		}
	}

	public void LoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM skentitydata WHERE id = @id;");
			pEDbOp.BindParam("@id", _id);
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public float GetAttribute(AttribType type, bool isBase = false)
	{
		return _attribs.NumAttribs.GetAttribute(type, isBase);
	}

	public void SetAttribute(AttribType type, float value, bool isBase = false)
	{
		_attribs.NumAttribs.SetAttribute(type, value, isBase);
	}

	public void SetAllAttribute(AttribType type, float value)
	{
		_attribs.NumAttribs.SetAllAttribute(type, value);
	}

	public float GetTemplateAttribute(AttribType type, bool isBase = false)
	{
		if (_baseAttribs != null)
		{
			return _baseAttribs.NumAttribs.GetAttribute(type, isBase);
		}
		return -1f;
	}

	public void CreateBaseAttr(byte[] data)
	{
		if (_baseAttribs == null)
		{
			_baseAttribs = new SkAttribs();
		}
		if (data == null || data.Length <= 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(data);
		using (BinaryReader binaryReader = new BinaryReader(memoryStream))
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int type = binaryReader.ReadInt32();
				float value = binaryReader.ReadSingle();
				_baseAttribs.NumAttribs.SetAllAttribute((AttribType)type, value);
			}
			binaryReader.Close();
		}
		memoryStream.Close();
	}

	public float GetDValue(AttribType type, float value)
	{
		return value - _attribs.NumAttribs.GetAttribute(type);
	}

	public void CheckAttrEvent(AttribType attType, float oldVal, float dVal, bool bRaw, bool bDValue)
	{
		float num = dVal;
		if (bDValue)
		{
			num = GetAttribute(attType, bRaw) + dVal;
		}
		switch (attType)
		{
		case AttribType.Hp:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.HpMax))
			{
				SetAttribute(AttribType.Hp, Mathf.Clamp(num, 0f, GetAttribute(AttribType.HpMax)), bRaw);
			}
			break;
		case AttribType.Stamina:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.StaminaMax))
			{
				SetAttribute(AttribType.Stamina, Mathf.Clamp(num, 0f, GetAttribute(AttribType.StaminaMax)), bRaw);
			}
			break;
		case AttribType.Comfort:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.ComfortMax))
			{
				SetAttribute(AttribType.Comfort, Mathf.Clamp(num, 0f, GetAttribute(AttribType.ComfortMax)), bRaw);
			}
			break;
		case AttribType.Oxygen:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.OxygenMax))
			{
				SetAttribute(AttribType.Oxygen, Mathf.Clamp(num, 0f, GetAttribute(AttribType.OxygenMax)), bRaw);
			}
			break;
		case AttribType.Hunger:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.HungerMax))
			{
				SetAttribute(AttribType.Hunger, Mathf.Clamp(num, 0f, GetAttribute(AttribType.HungerMax)), bRaw);
			}
			break;
		case AttribType.Energy:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.EnergyMax))
			{
				SetAttribute(AttribType.Energy, Mathf.Clamp(num, 0f, GetAttribute(AttribType.EnergyMax)), bRaw);
			}
			break;
		case AttribType.Rigid:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.RigidMax))
			{
				SetAttribute(AttribType.Rigid, Mathf.Clamp(num, 0f, GetAttribute(AttribType.RigidMax)), bRaw);
			}
			break;
		case AttribType.Hitfly:
			if (oldVal != num || num < 0f || num > GetAttribute(AttribType.HitflyMax))
			{
				SetAttribute(AttribType.Hitfly, Mathf.Clamp(num, 0f, GetAttribute(AttribType.HitflyMax)), bRaw);
			}
			break;
		default:
			SetAttribute(attType, num, bRaw);
			break;
		}
	}
}
