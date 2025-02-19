using System.Collections.Generic;
using System.Text.RegularExpressions;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using UnityEngine;

public static class CSUtils
{
	public static string GetOccupaName(int occupation)
	{
		string result = string.Empty;
		switch (occupation)
		{
		case 0:
			result = PELocalization.GetString(82211001);
			break;
		case 1:
			result = PELocalization.GetString(82211002);
			break;
		case 2:
			result = PELocalization.GetString(82211003);
			break;
		case 3:
			result = PELocalization.GetString(82211004);
			break;
		case 4:
			result = PELocalization.GetString(82211005);
			break;
		case 5:
			result = PELocalization.GetString(82230015);
			break;
		case 6:
			result = PELocalization.GetString(82211006);
			break;
		case 7:
			result = PELocalization.GetString(82211007);
			break;
		}
		return result;
	}

	public static string GetWorkModeName(int mode)
	{
		string result = string.Empty;
		switch (mode)
		{
		case 1:
			result = PELocalization.GetString(82212001);
			break;
		case 2:
			result = PELocalization.GetString(82212002);
			break;
		case 3:
			result = PELocalization.GetString(82212003);
			break;
		case 4:
			result = PELocalization.GetString(82212101);
			break;
		case 6:
			result = PELocalization.GetString(82212102);
			break;
		case 5:
			result = PELocalization.GetString(82212103);
			break;
		case 7:
			result = PELocalization.GetString(82212201);
			break;
		case 8:
			result = PELocalization.GetString(82212202);
			break;
		}
		return result;
	}

	public static string GetEntityName(int type)
	{
		string result = string.Empty;
		switch (type)
		{
		case 1:
			result = PELocalization.GetString(82210001);
			break;
		case 2:
			result = PELocalization.GetString(82210002);
			break;
		case 3:
			result = PELocalization.GetString(82210009);
			break;
		case 4:
			result = PELocalization.GetString(82210003);
			break;
		case 5:
			result = PELocalization.GetString(82210004);
			break;
		case 6:
			result = PELocalization.GetString(82210005);
			break;
		case 21:
			result = PELocalization.GetString(82210006);
			break;
		case 33:
			result = PELocalization.GetString(82210007);
			break;
		case 7:
			result = PELocalization.GetString(82210008);
			break;
		case 8:
			result = PELocalization.GetString(82210010);
			break;
		case 9:
			result = PELocalization.GetString(82210012);
			break;
		case 10:
			result = PELocalization.GetString(82210011);
			break;
		case 11:
			result = PELocalization.GetString(82210013);
			break;
		case 12:
			result = PELocalization.GetString(82210014);
			break;
		case 13:
			result = PELocalization.GetString(82210015);
			break;
		case 14:
			result = PELocalization.GetString(82210016);
			break;
		case 35:
			result = PELocalization.GetString(82210017);
			break;
		}
		return result;
	}

	public static string GetEntityEnlishName(int type)
	{
		string result = string.Empty;
		int num = type;
		if ((type & 0x20) != 0)
		{
			num = 32;
		}
		switch (num)
		{
		case 1:
			result = "Assembly Core";
			break;
		case 2:
			result = "Storage";
			break;
		case 3:
			result = "Machinery";
			break;
		case 4:
			result = "Enhance machine";
			break;
		case 5:
			result = "Repair machine";
			break;
		case 6:
			result = "Recycle machine";
			break;
		case 21:
			result = "Dwellings";
			break;
		case 32:
			result = "Power Plant";
			break;
		case 7:
			result = "Farm";
			break;
		}
		return result;
	}

	public static int GetEntityNameID(int type)
	{
		int result = 82210001;
		switch (type)
		{
		case 1:
			result = 82210001;
			break;
		case 2:
			result = 82210002;
			break;
		case 3:
			result = 82210009;
			break;
		case 4:
			result = 82210003;
			break;
		case 5:
			result = 82210004;
			break;
		case 6:
			result = 82210005;
			break;
		case 21:
			result = 82210006;
			break;
		case 33:
			result = 82210007;
			break;
		case 7:
			result = 82210008;
			break;
		case 8:
			result = 82210010;
			break;
		case 9:
			result = 82210012;
			break;
		case 10:
			result = 82210011;
			break;
		case 11:
			result = 82210013;
			break;
		case 12:
			result = 82210014;
			break;
		case 13:
			result = 82210015;
			break;
		case 14:
			result = 82210016;
			break;
		case 35:
			result = 82210017;
			break;
		}
		return result;
	}

	public static string GetRealTimeMS(int time, string linkStr = " : ")
	{
		if (time <= 0)
		{
			return "00" + linkStr + "00";
		}
		string empty = string.Empty;
		int num = time / 60;
		int num2 = time % 60;
		empty = ((num >= 10) ? (empty + num) : (empty + "0" + num));
		empty += linkStr;
		if (num2 < 10)
		{
			return empty + "0" + num2;
		}
		return empty + num2;
	}

	public static float SplitDecimals(float f)
	{
		return (!(f > 0f)) ? (f - Mathf.Ceil(f)) : (f - Mathf.Floor(f));
	}

	public static void ResetLoacalTransform(Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one;
	}

	public static Vector3 FindAffixedPos(Vector3 source_pos, Vector3[] bounds, float downward_dis)
	{
		Vector3 origin = source_pos + new Vector3(0f, 3f, 0f);
		if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 4f, 4096))
		{
			source_pos = hitInfo.point;
		}
		Vector3 source_pos2 = source_pos;
		Vector3 vector = _FindAffixedPosForBlock(source_pos2, bounds, downward_dis);
		Vector3 vector2 = _FindAffixedPosForVoxel(source_pos2, bounds, downward_dis);
		return (!(vector.y > vector2.y)) ? vector2 : vector;
	}

	private static Vector3 _FindAffixedPosForBlock(Vector3 source_pos, Vector3[] bounds, float downward_dis)
	{
		Vector3 result = source_pos;
		if (Block45Man.self != null)
		{
			int num = 0;
			foreach (Vector3 vector in bounds)
			{
				Vector3 vector2 = (source_pos + vector) * 2f;
				B45Block b45Block = Block45Man.self.DataSource.SafeRead((int)vector2.x, (int)vector2.y, (int)vector2.z);
				if (b45Block.blockType != 0 || b45Block.materialType != 0)
				{
					num++;
				}
			}
			if (num > 0)
			{
				int num2 = 2;
				Vector3 vector3 = source_pos + bounds[bounds.Length - 1];
				while (true)
				{
					Vector3 vector4 = vector3;
					int num3 = 0;
					for (int j = 0; j < num2; j++)
					{
						Vector3 vector5 = vector4 * 2f;
						B45Block b45Block2 = Block45Man.self.DataSource.SafeRead((int)vector5.x, (int)vector5.y, (int)vector5.z);
						if (b45Block2.blockType != 0 || b45Block2.materialType != 0)
						{
							num3++;
						}
						vector4 += new Vector3(0f, 0.5f, 0f);
					}
					if (num3 == 0)
					{
						break;
					}
					vector3 += new Vector3(0f, (float)num2 * 0.5f, 0f);
				}
				result = vector3;
			}
			else
			{
				Vector3 vector6 = source_pos + bounds[0];
				int num4 = Mathf.CeilToInt(downward_dis) * 2 + 1;
				for (int num5 = num4; num5 > 0; num5--)
				{
					Vector3 vector7 = vector6 * 2f;
					B45Block b45Block3 = Block45Man.self.DataSource.SafeRead((int)vector7.x, (int)vector7.y, (int)vector7.z);
					if (b45Block3.blockType != 0 || b45Block3.materialType != 0)
					{
						vector6 += new Vector3(0f, 0.5f, 0f);
						break;
					}
					vector6 -= new Vector3(0f, 0.5f, 0f);
				}
				result = vector6;
			}
		}
		return result;
	}

	private static Vector3 _FindAffixedPosForVoxel(Vector3 source_pos, Vector3[] bounds, float downward_dis)
	{
		Vector3 result = source_pos;
		if (VFVoxelTerrain.self != null)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < bounds.Length; i++)
			{
				Vector3 vector = source_pos + bounds[i];
				if (VFVoxelTerrain.self.Voxels.SafeRead((int)vector.x, (int)vector.y, (int)vector.z).Volume >= 128)
				{
					num2 = Mathf.Max(i, num2);
					num++;
				}
			}
			if (num > 0)
			{
				Vector3 vector2 = source_pos + bounds[num2] + Vector3.up;
				while (true)
				{
					int num3 = 0;
					int num4 = 0;
					for (int j = 0; j < bounds.Length; j++)
					{
						Vector3 vector3 = vector2 + bounds[j];
						if (VFVoxelTerrain.self.Voxels.SafeRead((int)vector3.x, (int)vector3.y, (int)vector3.z).Volume >= 128)
						{
							num4 = Mathf.Max(j, num4);
							num3++;
						}
					}
					if (num3 <= 0)
					{
						break;
					}
					vector2 = vector2 + bounds[num4] + Vector3.up;
				}
				vector2 = vector2;
				result = vector2;
			}
			else
			{
				Vector3 vector4 = source_pos + bounds[0] + Vector3.down;
				int num5 = Mathf.CeilToInt(downward_dis) + 1;
				for (int num6 = num5; num6 > 0; num6--)
				{
					Vector3 vector5 = vector4;
					if (VFVoxelTerrain.self.Voxels.SafeRead((int)vector5.x, (int)vector5.y, (int)vector5.z).Volume > 128)
					{
						break;
					}
					vector4 -= Vector3.up;
				}
				result = vector4 + Vector3.up;
			}
		}
		return result;
	}

	public static bool SphereContainsAndIntersectBound(Vector3 sphere_center, float sphere_radius, Bounds bound)
	{
		Vector3[] array = new Vector3[8]
		{
			bound.min,
			new Vector3(bound.min.x, bound.min.y, bound.max.z),
			new Vector3(bound.max.x, bound.min.y, bound.max.z),
			new Vector3(bound.max.x, bound.min.y, bound.min.z),
			new Vector3(bound.min.x, bound.max.y, bound.min.z),
			new Vector3(bound.min.x, bound.max.y, bound.max.z),
			bound.max,
			new Vector3(bound.max.x, bound.max.y, bound.min.z)
		};
		float num = sphere_radius * sphere_radius;
		bool result = false;
		for (int i = 0; i < 8; i++)
		{
			float num2 = Vector3.SqrMagnitude(sphere_center - array[i]);
			if (num2 < num)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static string GetNoFormatString(string source, string replace_str1)
	{
		return source.Replace("$A$", replace_str1);
	}

	public static string GetNoFormatString(string source, string replace_str1, string replace_str2)
	{
		string empty = string.Empty;
		empty = source.Replace("$A$", replace_str1);
		return empty.Replace("$B$", replace_str2);
	}

	public static string GetNoFormatString(string source, object[] replace_objs, string link_str = ", ")
	{
		string text = string.Empty;
		for (int i = 0; i < replace_objs.Length; i++)
		{
			text = text + replace_objs[i].ToString() + link_str;
		}
		return source.Replace("$A$", text);
	}

	public static bool MatchString(string src, string match)
	{
		if (match.Trim().Length == 0)
		{
			return true;
		}
		Regex regex = new Regex(match);
		Match match2 = regex.Match(src);
		if (match2.Success)
		{
			return true;
		}
		return false;
	}

	public static bool IsNumber(string str)
	{
		Regex regex = new Regex("[^0-9.-]");
		Regex regex2 = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
		Regex regex3 = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
		string text = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
		string text2 = "^([-]|[0-9])[0-9]*$";
		Regex regex4 = new Regex("(" + text + ")|(" + text2 + ")");
		return !regex.IsMatch(str) && !regex2.IsMatch(str) && !regex3.IsMatch(str) && regex4.IsMatch(str);
	}

	public static Vector3 GetColonyPos(CSCreator creator)
	{
		CSAssembly assembly = creator.Assembly;
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		Vector3 vector = new Vector3(assembly.Position.x + 5f, 1000f, assembly.Position.z) + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * (assembly.Radius - 10f);
		if (Physics.Raycast(vector, Vector3.down, out var hitInfo, 1000f, 4096))
		{
			vector.y = hitInfo.point.y + 1f;
		}
		else
		{
			vector.y = assembly.Position.y + 5f;
		}
		return vector;
	}

	public static CSStorage GetTargetStorage(List<int> protoTypeIdList, PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle || PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
		{
			cSMgCreator = (CSMgCreator)CSMain.GetCreator(0);
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		if (cSMgCreator == null || cSMgCreator.Assembly == null)
		{
			return null;
		}
		List<CSCommon> storages = cSMgCreator.Assembly.Storages;
		if (storages == null)
		{
			return null;
		}
		CSStorage cSStorage = null;
		foreach (CSCommon item in storages)
		{
			if (!(item is CSStorage { IsRunning: not false } cSStorage2))
			{
				continue;
			}
			foreach (int protoTypeId in protoTypeIdList)
			{
				if (cSStorage2.m_Package.FindItemByProtoId(protoTypeId) != null)
				{
					cSStorage = cSStorage2;
					break;
				}
			}
			if (cSStorage == null)
			{
				continue;
			}
			break;
		}
		return cSStorage;
	}

	public static ItemObject GetItemNumInStorage(CSStorage storage, int protoTypeId)
	{
		return storage.m_Package.FindItemByProtoId(protoTypeId);
	}

	public static bool DeleteItemInStorage(CSStorage storage, int protoTypeId, int num = 1)
	{
		return storage.m_Package.Destroy(protoTypeId, num);
	}

	public static List<ItemObject> GetItemListInStorage(int protoId, CSAssembly assembly)
	{
		List<ItemObject> list = new List<ItemObject>();
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return list;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			list.AddRange(cSStorage.m_Package.GetAllItemByProtoId(protoId));
		}
		return list;
	}

	public static int GetItemCounFromFactoryAndAllStorage(int protoId, CSAssembly assembly)
	{
		if (assembly == null)
		{
			return 0;
		}
		if (assembly.Factory != null)
		{
			return assembly.Factory.GetCompoundEndItemCount(protoId) + GetItemCountFromAllStorage(protoId, assembly);
		}
		return GetItemCountFromAllStorage(protoId, assembly);
	}

	public static int GetItemCountFromAllStorage(int protoId, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			num += cSStorage.GetItemCount(protoId);
		}
		return num;
	}

	public static int GetMaterialListCount(List<ItemIdCount> materialList, CSAssembly assembly)
	{
		ItemIdCount itemIdCount = materialList[0];
		int num = GetItemCounFromFactoryAndAllStorage(itemIdCount.protoId, assembly) / itemIdCount.count;
		for (int i = 1; i < materialList.Count; i++)
		{
			int num2 = GetItemCounFromFactoryAndAllStorage(materialList[i].protoId, assembly) / materialList[i].count;
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static bool CountDownItemFromAllStorage(int protoId, int count, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return false;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			int itemCount = cSStorage.GetItemCount(protoId);
			if (itemCount >= count)
			{
				cSStorage.CountDownItem(protoId, count);
				return true;
			}
			cSStorage.CountDownItem(protoId, itemCount);
			count -= itemCount;
		}
		return false;
	}

	public static bool CountDownItemFromFactoryAndAllStorage(int protoId, int count, CSAssembly assembly)
	{
		if (assembly == null)
		{
			return false;
		}
		if (assembly.Factory == null && assembly.Storages == null)
		{
			return false;
		}
		if (assembly.Factory != null)
		{
			int compoundEndItemCount = assembly.Factory.GetCompoundEndItemCount(protoId);
			if (compoundEndItemCount > 0)
			{
				if (compoundEndItemCount >= count)
				{
					assembly.Factory.CountDownItem(protoId, count);
					return true;
				}
				assembly.Factory.CountDownItem(protoId, compoundEndItemCount);
				count -= compoundEndItemCount;
			}
		}
		if (assembly.Storages != null)
		{
			foreach (CSCommon storage in assembly.Storages)
			{
				CSStorage cSStorage = storage as CSStorage;
				int itemCount = cSStorage.GetItemCount(protoId);
				if (itemCount >= count)
				{
					cSStorage.CountDownItem(protoId, count);
					return true;
				}
				cSStorage.CountDownItem(protoId, itemCount);
				count -= itemCount;
			}
		}
		return false;
	}

	public static bool CanAddToStorage(int protoId, int count, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return false;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			if (cSStorage.CanAdd(protoId, count))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanAddListToStorage(List<ItemIdCount> iicList, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return false;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			if (cSStorage.CanAdd(iicList))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AddToStorage(int protoId, int count, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return false;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			if (cSStorage.CanAdd(protoId, count))
			{
				cSStorage.Add(protoId, count);
				return true;
			}
		}
		return false;
	}

	public static bool AddItemObjToStorage(int instanceId, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return false;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			if (cSStorage.AddItemObj(instanceId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool RemoveItemObjFromStorage(int instanceId, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return false;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			if (cSStorage.RemoveItemObj(instanceId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AddItemListToStorage(List<ItemIdCount> iicList, CSAssembly assembly)
	{
		if (assembly == null || assembly.Storages == null || assembly.Storages.Count == 0)
		{
			return false;
		}
		foreach (CSCommon storage in assembly.Storages)
		{
			CSStorage cSStorage = storage as CSStorage;
			if (cSStorage.CanAdd(iicList))
			{
				cSStorage.Add(iicList);
				return true;
			}
		}
		return false;
	}

	public static bool CheckFarmAvailable(PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (PeGameMgr.IsSingle)
		{
			if (CSMain.s_MgCreator == null)
			{
				return false;
			}
			if (CSMain.s_MgCreator.Assembly == null)
			{
				return false;
			}
			if (CSMain.s_MgCreator.Assembly.Farm == null)
			{
				return false;
			}
			if (!CSMain.s_MgCreator.Assembly.Farm.IsRunning)
			{
				return false;
			}
		}
		else
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface == null)
			{
				return false;
			}
			int teamId = networkInterface.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			if (creator == null)
			{
				return false;
			}
			if (creator.Assembly == null)
			{
				return false;
			}
			if (creator.Assembly.Farm == null)
			{
				return false;
			}
			if (!creator.Assembly.Farm.IsRunning)
			{
				return false;
			}
		}
		return true;
	}

	public static bool CheckPlantExist(FarmWorkInfo fwi)
	{
		if (fwi == null)
		{
			return false;
		}
		if (fwi.m_Plant == null || fwi.m_Plant.gameObject == null)
		{
			return false;
		}
		return true;
	}

	public static bool FarmPlantReady(PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		return cSMgCreator.Assembly.Farm.HasPlantSeed();
	}

	public static bool FarmWaterReady(PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		return cSMgCreator.Assembly.Farm.GetPlantTool(0) != null;
	}

	public static bool FarmCleanReady(PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		return cSMgCreator.Assembly.Farm.GetPlantTool(1) != null;
	}

	public static bool CheckFarmPlantAround(Vector3 pos, PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		if (cSMgCreator.Assembly == null || cSMgCreator.Assembly.Farm == null)
		{
			return false;
		}
		int plantSeedId = cSMgCreator.Assembly.Farm.GetPlantSeedId();
		if (plantSeedId < 0)
		{
			return false;
		}
		return cSMgCreator.Assembly.Farm.checkRroundCanPlant(plantSeedId, pos);
	}

	public static bool FarmWaterEnough(PeEntity npc = null, FarmPlantLogic plant = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (plant == null)
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		ItemObject plantTool = cSMgCreator.Assembly.Farm.GetPlantTool(0);
		if (plantTool == null)
		{
			return false;
		}
		int waterItemCount = plant.GetWaterItemCount();
		if (waterItemCount <= 0)
		{
			return false;
		}
		if (plantTool.GetCount() < waterItemCount)
		{
			return false;
		}
		return true;
	}

	public static bool FarmCleanEnough(PeEntity npc = null, FarmPlantLogic plant = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (plant == null)
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		ItemObject plantTool = cSMgCreator.Assembly.Farm.GetPlantTool(1);
		if (plantTool == null)
		{
			return false;
		}
		int cleaningItemCount = plant.GetCleaningItemCount();
		if (cleaningItemCount <= 0)
		{
			return false;
		}
		if (plantTool.GetCount() < cleaningItemCount)
		{
			return false;
		}
		return true;
	}

	public static FarmWorkInfo FindPlantPos(PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		if (cSMgCreator.m_Clod == null)
		{
			return null;
		}
		ClodChunk clodChunk = cSMgCreator.m_Clod.FindCleanChunk(cSMgCreator.Assembly.Position, cSMgCreator.Assembly.Radius);
		if (clodChunk == null)
		{
			return null;
		}
		if (clodChunk.FindCleanClod(out var pos))
		{
			FarmWorkInfo result = new FarmWorkInfo(clodChunk, pos);
			cSMgCreator.m_Clod.DirtyTheChunk(clodChunk.m_ChunkIndex, dirty: true);
			return result;
		}
		return null;
	}

	public static FarmWorkInfo FindPlantPos(FarmWorkInfo farmWorkInfo, PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		if (farmWorkInfo.m_ClodChunk.FindCleanClod(out var pos))
		{
			return new FarmWorkInfo(farmWorkInfo.m_ClodChunk, pos);
		}
		return null;
	}

	public static FarmWorkInfo FindPlantPosNewChunk(PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		if (cSMgCreator.m_Clod == null)
		{
			return null;
		}
		if (cSMgCreator.Assembly.Farm == null)
		{
			return null;
		}
		int plantSeedId = cSMgCreator.Assembly.Farm.GetPlantSeedId();
		if (plantSeedId < 0)
		{
			return null;
		}
		Vector3 pos;
		ClodChunk clodChunk = cSMgCreator.m_Clod.FindHasIdleClodsChunk(cSMgCreator.Assembly.Position, cSMgCreator.Assembly.Radius, cSMgCreator.Assembly.Farm, plantSeedId, out pos);
		if (clodChunk != null)
		{
			FarmWorkInfo result = new FarmWorkInfo(clodChunk, pos);
			cSMgCreator.m_Clod.DirtyTheChunk(clodChunk.m_ChunkIndex, dirty: true);
			return result;
		}
		return null;
	}

	public static FarmWorkInfo FindPlantPosSameChunk(FarmWorkInfo farmWorkInfo, PeEntity npc = null)
	{
		if (npc == null || farmWorkInfo == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		if (cSMgCreator.Assembly == null || cSMgCreator.Assembly.Farm == null)
		{
			return null;
		}
		int plantSeedId = cSMgCreator.Assembly.Farm.GetPlantSeedId();
		if (plantSeedId < 0)
		{
			return null;
		}
		if (farmWorkInfo.m_ClodChunk.FindBetterClod(cSMgCreator.Assembly.Position, cSMgCreator.Assembly.Radius, cSMgCreator.Assembly.Farm, plantSeedId, out var pos))
		{
			return new FarmWorkInfo(farmWorkInfo.m_ClodChunk, pos);
		}
		return null;
	}

	public static bool TryPlant(FarmWorkInfo farmworkInfo, PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (FarmPlantReady(npc))
		{
			if (PeGameMgr.IsSingle)
			{
				CSMgCreator s_MgCreator = CSMain.s_MgCreator;
				FarmPlantLogic farmPlantLogic = s_MgCreator.Assembly.Farm.PlantTo(farmworkInfo.m_Pos);
				if (farmPlantLogic != null)
				{
					farmworkInfo.m_ClodChunk.DirtyTheClodByPlantBounds(farmPlantLogic.protoTypeId, farmworkInfo.m_Pos, dirty: true);
				}
			}
			else
			{
				AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
				if (aiAdNpcNetwork == null)
				{
					return false;
				}
				int teamId = aiAdNpcNetwork.TeamId;
				CSMgCreator s_MgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
				int iD = s_MgCreator.Assembly.Farm.ID;
				IntVector3 intVector = new IntVector3(farmworkInfo.m_Pos + 0.1f * Vector3.down);
				byte type = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z).Type;
				aiAdNpcNetwork.PlantPutOut(farmworkInfo.m_Pos, iD, type);
				farmworkInfo.m_ClodChunk.DirtyTheClod(farmworkInfo.m_Pos, dirty: true);
			}
			return true;
		}
		return false;
	}

	public static void ReturnCleanChunk(FarmWorkInfo farmWorkInfo, PeEntity npc = null)
	{
		if (npc == null)
		{
			return;
		}
		CSMgCreator cSMgCreator;
		if (PeGameMgr.IsSingle)
		{
			cSMgCreator = CSMain.s_MgCreator;
		}
		else
		{
			if (NetworkInterface.Get(npc.Id) == null)
			{
				return;
			}
			int teamId = NetworkInterface.Get(npc.Id).TeamId;
			cSMgCreator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
		}
		if (!(cSMgCreator == null) && cSMgCreator.m_Clod != null)
		{
			cSMgCreator.m_Clod.AddClod(farmWorkInfo.m_Pos);
		}
	}

	public static FarmWorkInfo FindPlantToWater(PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return null;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		ItemObject plantTool = farm.GetPlantTool(0);
		FarmPlantLogic farmPlantLogic = ((plantTool != null) ? farm.AssignOutWateringPlant() : null);
		if (farmPlantLogic != null)
		{
			int waterItemCount = farmPlantLogic.GetWaterItemCount();
			if (waterItemCount <= 0)
			{
				return null;
			}
			if (plantTool.GetCount() < waterItemCount)
			{
				farm.RestoreWateringPlant(farmPlantLogic);
				return null;
			}
			return new FarmWorkInfo(farmPlantLogic);
		}
		return null;
	}

	public static bool TryWater(FarmWorkInfo fwi, PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (!CheckPlantExist(fwi))
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		if (!FarmWaterReady(npc))
		{
			return false;
		}
		if (PeGameMgr.IsSingle)
		{
			CSFarm farm = CSMain.s_MgCreator.Assembly.Farm;
			fwi.m_Plant.UpdateStatus();
			int waterItemCount = fwi.m_Plant.GetWaterItemCount();
			if (waterItemCount <= 0)
			{
				return false;
			}
			ItemObject plantTool = farm.GetPlantTool(0);
			int count = plantTool.GetCount();
			if (count >= waterItemCount)
			{
				fwi.m_Plant.Watering(waterItemCount);
				fwi.m_Plant.UpdateStatus();
				plantTool.DecreaseStackCount(waterItemCount);
				if (plantTool.GetCount() <= 0)
				{
					PeSingleton<ItemMgr>.Instance.DestroyItem(plantTool.instanceId);
					farm.SetPlantTool(0, null);
				}
				return true;
			}
			return false;
		}
		AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
		if (aiAdNpcNetwork == null)
		{
			return false;
		}
		int teamId = aiAdNpcNetwork.TeamId;
		CSFarm farm2 = MultiColonyManager.GetCreator(teamId, createNewIfNone: false).Assembly.Farm;
		int iD = farm2.ID;
		int id = fwi.m_Plant.id;
		aiAdNpcNetwork.PlantWater(id, iD);
		return true;
	}

	public static void ReturnWaterPlant(PeEntity npc, FarmWorkInfo fwi)
	{
		if (npc == null || !CheckPlantExist(fwi) || !CheckFarmAvailable(npc))
		{
			return;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		farm?.RestoreWateringPlant(fwi.m_Plant);
	}

	public static FarmWorkInfo FindPlantToClean(PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return null;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		ItemObject plantTool = farm.GetPlantTool(1);
		FarmPlantLogic farmPlantLogic = ((plantTool != null) ? farm.AssignOutCleaningPlant() : null);
		if (farmPlantLogic != null)
		{
			int cleaningItemCount = farmPlantLogic.GetCleaningItemCount();
			if (cleaningItemCount <= 0)
			{
				return null;
			}
			if (plantTool.GetCount() < cleaningItemCount)
			{
				farm.RestoreCleaningPlant(farmPlantLogic);
				return null;
			}
			return new FarmWorkInfo(farmPlantLogic);
		}
		return null;
	}

	public static bool TryClean(FarmWorkInfo fwi, PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (!CheckPlantExist(fwi))
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		if (!FarmCleanReady(npc))
		{
			return false;
		}
		if (PeGameMgr.IsSingle)
		{
			CSFarm farm = CSMain.s_MgCreator.Assembly.Farm;
			fwi.m_Plant.UpdateStatus();
			int cleaningItemCount = fwi.m_Plant.GetCleaningItemCount();
			if (cleaningItemCount <= 0)
			{
				return false;
			}
			ItemObject plantTool = farm.GetPlantTool(1);
			int count = plantTool.GetCount();
			if (count >= cleaningItemCount)
			{
				fwi.m_Plant.Cleaning(cleaningItemCount);
				fwi.m_Plant.UpdateStatus();
				plantTool.DecreaseStackCount(cleaningItemCount);
				if (plantTool.GetCount() <= 0)
				{
					PeSingleton<ItemMgr>.Instance.DestroyItem(plantTool.instanceId);
					farm.SetPlantTool(1, null);
				}
				return true;
			}
			return false;
		}
		AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
		if (aiAdNpcNetwork == null)
		{
			return false;
		}
		int teamId = aiAdNpcNetwork.TeamId;
		CSFarm farm2 = MultiColonyManager.GetCreator(teamId, createNewIfNone: false).Assembly.Farm;
		int iD = farm2.ID;
		int id = fwi.m_Plant.id;
		aiAdNpcNetwork.PlantClean(id, iD);
		return true;
	}

	public static void ReturnCleanPlant(PeEntity npc, FarmWorkInfo fwi)
	{
		if (npc == null || !CheckPlantExist(fwi) || !CheckFarmAvailable(npc))
		{
			return;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		farm?.RestoreCleaningPlant(fwi.m_Plant);
	}

	public static FarmWorkInfo FindPlantGet(PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return null;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		CSStorage cSStorage = null;
		if (CSMain.s_MgCreator.Assembly.Storages == null)
		{
			return null;
		}
		using (List<CSCommon>.Enumerator enumerator = CSMain.s_MgCreator.Assembly.Storages.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				CSStorage cSStorage2 = (CSStorage)enumerator.Current;
				cSStorage = cSStorage2;
			}
		}
		if (cSStorage != null)
		{
			FarmPlantLogic farmPlantLogic = farm.AssignOutRipePlant();
			if (farmPlantLogic != null)
			{
				int harvestItemNumMax = farmPlantLogic.GetHarvestItemNumMax();
				Dictionary<int, int> harvestItemIdsMax = farmPlantLogic.GetHarvestItemIdsMax(harvestItemNumMax);
				foreach (CSStorage storage in CSMain.s_MgCreator.Assembly.Storages)
				{
					if (CanAddToPackage(storage.m_Package, harvestItemIdsMax))
					{
						return new FarmWorkInfo(farmPlantLogic);
					}
				}
				farm.RestoreRipePlant(farmPlantLogic);
			}
		}
		return null;
	}

	public static bool TryHarvest(FarmWorkInfo fwi, PeEntity npc = null)
	{
		if (npc == null)
		{
			return false;
		}
		if (!CheckPlantExist(fwi))
		{
			return false;
		}
		if (!CheckFarmAvailable(npc))
		{
			return false;
		}
		FarmPlantLogic plant = fwi.m_Plant;
		if (PeGameMgr.IsSingle)
		{
			CSFarm farm = CSMain.s_MgCreator.Assembly.Farm;
			float harvestAbility = 1f + npc.GetCmpt<NpcCmpt>().Npcskillcmpt.GetTalentPercent(AblityType.Harvest);
			int harvestItemNum = plant.GetHarvestItemNum(harvestAbility);
			Dictionary<int, int> harvestItemIds = plant.GetHarvestItemIds(harvestItemNum);
			CSStorage cSStorage = FindStorageForHarvest(harvestItemIds, npc);
			if (cSStorage == null)
			{
				return false;
			}
			AddToPackage(cSStorage.m_Package, harvestItemIds);
			FarmManager.Instance.RemovePlant(plant.mPlantInstanceId);
			DragArticleAgent.Destory(plant.id);
			CSMain.s_MgCreator.m_Clod.AddClod(fwi.m_Pos);
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return false;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			CSFarm farm = creator.Assembly.Farm;
			float harvestAbility2 = 1f + npc.GetCmpt<NpcCmpt>().Npcskillcmpt.GetTalentPercent(AblityType.Harvest);
			int harvestItemNum2 = plant.GetHarvestItemNum(harvestAbility2);
			Dictionary<int, int> harvestItemIds2 = plant.GetHarvestItemIds(harvestItemNum2);
			CSStorage cSStorage2 = FindStorageForHarvest(harvestItemIds2, npc);
			if (cSStorage2 == null)
			{
				return false;
			}
			aiAdNpcNetwork.PlantGetBack(plant.id, farm.ID);
		}
		return true;
	}

	public static void ReturnHarvestPlant(PeEntity npc, FarmWorkInfo fwi)
	{
		if (npc == null || !CheckPlantExist(fwi) || !CheckFarmAvailable(npc))
		{
			return;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		farm?.RestoreRipePlant(fwi.m_Plant);
	}

	public static CSStorage FindStorageForHarvest(Dictionary<int, int> retItems, PeEntity npc)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return null;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		foreach (CSStorage storage in farm.Assembly.Storages)
		{
			if (CanAddToPackage(storage.m_Package, retItems))
			{
				return storage;
			}
		}
		return null;
	}

	public static FarmWorkInfo FindPlantRemove(PeEntity npc = null)
	{
		if (npc == null)
		{
			return null;
		}
		if (!CheckFarmAvailable(npc))
		{
			return null;
		}
		CSFarm farm;
		if (PeGameMgr.IsSingle)
		{
			farm = CSMain.s_MgCreator.Assembly.Farm;
		}
		else
		{
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return null;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			CSMgCreator creator = MultiColonyManager.GetCreator(teamId, createNewIfNone: false);
			farm = creator.Assembly.Farm;
		}
		FarmPlantLogic farmPlantLogic = farm.AssignOutDeadPlant();
		if (farmPlantLogic != null)
		{
			return new FarmWorkInfo(farmPlantLogic);
		}
		return null;
	}

	public static bool TryRemove(FarmWorkInfo fwi, PeEntity npc = null)
	{
		if (!CheckPlantExist(fwi))
		{
			return false;
		}
		FarmPlantLogic plant = fwi.m_Plant;
		if (PeGameMgr.IsSingle)
		{
			FarmManager.Instance.RemovePlant(plant.mPlantInstanceId);
			DragArticleAgent.Destory(plant.id);
			CSMain.s_MgCreator.m_Clod.AddClod(fwi.m_Pos);
		}
		else
		{
			if (npc == null)
			{
				return false;
			}
			AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)NetworkInterface.Get(npc.Id);
			if (aiAdNpcNetwork == null)
			{
				return false;
			}
			int teamId = aiAdNpcNetwork.TeamId;
			aiAdNpcNetwork.PlantClear(plant.id);
		}
		return true;
	}

	public static bool CanAddToPackage(ItemPackage package, Dictionary<int, int> retItems)
	{
		List<MaterialItem> list = new List<MaterialItem>();
		foreach (int key in retItems.Keys)
		{
			MaterialItem materialItem = new MaterialItem();
			materialItem.protoId = key;
			materialItem.count = retItems[key];
			list.Add(materialItem);
		}
		return package.CanAdd(list);
	}

	public static void AddToPackage(ItemPackage package, Dictionary<int, int> retItems)
	{
		List<MaterialItem> list = new List<MaterialItem>();
		foreach (int key in retItems.Keys)
		{
			MaterialItem materialItem = new MaterialItem();
			materialItem.protoId = key;
			materialItem.count = retItems[key];
			list.Add(materialItem);
		}
		package.Add(list);
	}

	public static List<MaterialItem> ItemIdCountToMaterialItem(List<ItemIdCount> itemList)
	{
		List<MaterialItem> list = new List<MaterialItem>();
		foreach (ItemIdCount item in itemList)
		{
			MaterialItem materialItem = new MaterialItem();
			materialItem.protoId = item.protoId;
			materialItem.count = item.count;
			list.Add(materialItem);
		}
		return list;
	}

	public static void AddItemIdCount(List<ItemIdCount> list, int protoId, int count)
	{
		ItemIdCount itemIdCount = list.Find((ItemIdCount it) => it.protoId == protoId);
		if (itemIdCount == null)
		{
			list.Add(new ItemIdCount(protoId, count));
		}
		else
		{
			itemIdCount.count += count;
		}
	}

	public static void RemoveItemIdCount(List<ItemIdCount> list, int protoId, int count)
	{
		ItemIdCount itemIdCount = list.Find((ItemIdCount it) => it.protoId == protoId);
		if (itemIdCount != null)
		{
			if (itemIdCount.count <= count)
			{
				list.Remove(itemIdCount);
			}
			else
			{
				itemIdCount.count -= count;
			}
		}
	}

	public static int[] ItemIdCountListToIntArray(List<ItemIdCount> list)
	{
		if (list == null)
		{
			return null;
		}
		int[] array = new int[list.Count * 2];
		int num = 0;
		foreach (ItemIdCount item in list)
		{
			array[num++] = item.protoId;
			array[num++] = item.count;
		}
		return array;
	}

	public static void ShowTips(int tipsCode, int replaceStrId = -1)
	{
		string text = "?";
		string @string = PELocalization.GetString(replaceStrId);
		text = GetNoFormatString(PELocalization.GetString(tipsCode), @string);
		new PeTipMsg(text, PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Colony);
		string[] array = Regex.Split(text, "\\[-\\] ", RegexOptions.IgnoreCase);
		if (array.Length < 2)
		{
			CSUI_MainWndCtrl.ShowStatusBar(array[0], 10f);
		}
		else
		{
			CSUI_MainWndCtrl.ShowStatusBar(array[1], 10f);
		}
	}

	public static void ShowTips(int tipsCode, string replaceStr)
	{
		string text = "?";
		text = GetNoFormatString(PELocalization.GetString(tipsCode), replaceStr);
		new PeTipMsg(text, PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Colony);
		string[] array = Regex.Split(text, "\\[-\\] ", RegexOptions.IgnoreCase);
		if (array.Length < 2)
		{
			CSUI_MainWndCtrl.ShowStatusBar(array[0], 10f);
		}
		else
		{
			CSUI_MainWndCtrl.ShowStatusBar(array[1], 10f);
		}
	}

	public static void ShowTips(string str)
	{
		CSUI_MainWndCtrl.ShowStatusBar(str);
	}

	public static void ShowCannotWorkReason(ENpcUnableWorkType state, string fullname)
	{
		switch (state)
		{
		case ENpcUnableWorkType.HasRequest:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201070), fullname));
			break;
		case ENpcUnableWorkType.IsNeedMedicine:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201071), fullname));
			break;
		case ENpcUnableWorkType.IsHunger:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201072), fullname));
			break;
		case ENpcUnableWorkType.IsHpLow:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201092), fullname));
			break;
		case ENpcUnableWorkType.IsUncomfortable:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201073), fullname));
			break;
		case ENpcUnableWorkType.IsSleeepTime:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201074), fullname));
			break;
		case ENpcUnableWorkType.IsDinnerTime:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201075), fullname));
			break;
		default:
			CSUI_MainWndCtrl.ShowStatusBar(GetNoFormatString(PELocalization.GetString(82201093), fullname));
			break;
		}
	}
}
