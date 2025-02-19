using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using UnityEngine;

public static class VCConfig
{
	public static Dictionary<EVCCategory, VCCategoryInfo> s_Categories = null;

	public static Dictionary<EVCComponent, VCPartTypeInfo> s_PartTypes = null;

	public static Dictionary<int, VCPartInfo> s_Parts = null;

	public static Dictionary<int, VCEffectInfo> s_Effects = null;

	public static List<VCESceneSetting> s_EditorScenes = null;

	public static Dictionary<int, VCMatterInfo> s_Matters = null;

	public static string s_MaterialPath;

	public static string s_DecalPath;

	public static string s_IsoPath;

	public static string s_CreationPath;

	public static string s_CreationNetCachePath;

	public static string s_MaterialFileExt = ".vcmat";

	public static string s_DecalFileExt = ".vcdcl";

	public static string s_IsoFileExt = ".vciso";

	public static string s_ObsoleteIsoFileExt = ".peiso";

	public static string s_CreationFileExt = ".vcres";

	public static string s_CreationNetCacheFileExt = ".~vcres";

	public static int s_EditorLayer = 18;

	public static int s_ProductLayer = 19;

	public static int s_WheelLayer = 17;

	public static int s_SceneLayer = 29;

	public static int s_UILayer = 28;

	public static int s_MatGenLayer = 27;

	public static int s_EditorLayerMask = (1 << s_EditorLayer) | (1 << s_SceneLayer);

	public static int s_ProductLayerMask = 1 << s_ProductLayer;

	public static int s_UILayerMask = 1 << s_UILayer;

	public static int s_MatGenLayerMask = 1 << s_MatGenLayer;

	public static int s_DyeID = 1038;

	public static VCESceneSetting FirstSceneSetting
	{
		get
		{
			foreach (VCESceneSetting s_EditorScene in s_EditorScenes)
			{
				if (s_EditorScene.m_Category != 0)
				{
					return s_EditorScene;
				}
			}
			return null;
		}
	}

	public static void InitConfig()
	{
		BuildDirectories();
		LoadCategories();
		LoadEditorScenes();
		LoadMatters();
		LoadPartTypes();
		LoadParts();
		LoadEffects();
	}

	private static void BuildDirectories()
	{
		string empty = string.Empty;
		empty = GameConfig.GetUserDataPath();
		string text = "/PlanetExplorers/VoxelCreationData";
		s_MaterialPath = empty + text + "/Materials/";
		s_DecalPath = empty + text + "/Decals/";
		s_IsoPath = empty + text + "/Isos/";
		s_CreationPath = empty + text + "/Creations/";
		s_CreationNetCachePath = empty + text + "/Creations/NetCache/";
		if (!Directory.Exists(s_MaterialPath))
		{
			Directory.CreateDirectory(s_MaterialPath);
		}
		if (!Directory.Exists(s_DecalPath))
		{
			Directory.CreateDirectory(s_DecalPath);
		}
		if (!Directory.Exists(s_IsoPath))
		{
			Directory.CreateDirectory(s_IsoPath);
		}
		if (!Directory.Exists(s_CreationPath))
		{
			Directory.CreateDirectory(s_CreationPath);
		}
		if (!Directory.Exists(s_CreationNetCachePath))
		{
			Directory.CreateDirectory(s_CreationNetCachePath);
		}
	}

	private static string GetUserDataPath()
	{
		return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
	}

	private static void LoadCategories()
	{
		s_Categories = new Dictionary<EVCCategory, VCCategoryInfo>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_category");
		while (sqliteDataReader.Read())
		{
			VCCategoryInfo vCCategoryInfo = new VCCategoryInfo();
			vCCategoryInfo.m_Category = (EVCCategory)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			vCCategoryInfo.m_Name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("name"));
			vCCategoryInfo.m_DefaultPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("default_path"));
			List<string> list = VCUtils.ExplodeString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("part_types")), ';');
			vCCategoryInfo.m_PartTypes = new List<EVCComponent>();
			foreach (string item in list)
			{
				vCCategoryInfo.m_PartTypes.Add((EVCComponent)Convert.ToInt32(item));
			}
			s_Categories.Add(vCCategoryInfo.m_Category, vCCategoryInfo);
		}
	}

	private static void LoadEditorScenes()
	{
		s_EditorScenes = new List<VCESceneSetting>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_scene");
		while (sqliteDataReader.Read())
		{
			VCESceneSetting vCESceneSetting = new VCESceneSetting();
			vCESceneSetting.m_Id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			vCESceneSetting.m_ParentId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("parentid")));
			vCESceneSetting.m_Name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("name"));
			vCESceneSetting.m_Category = (EVCCategory)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("category")));
			List<string> list = null;
			list = VCUtils.ExplodeString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("editorsize")), ';');
			vCESceneSetting.m_EditorSize = new IntVector3();
			vCESceneSetting.m_EditorSize.x = Convert.ToInt32(list[0]);
			vCESceneSetting.m_EditorSize.y = Convert.ToInt32(list[1]);
			vCESceneSetting.m_EditorSize.z = Convert.ToInt32(list[2]);
			list.Clear();
			vCESceneSetting.m_VoxelSize = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("voxelsize")));
			list = VCUtils.ExplodeString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("interval")), ';');
			vCESceneSetting.m_MajorInterval = Convert.ToInt32(list[0]);
			vCESceneSetting.m_MinorInterval = Convert.ToInt32(list[1]);
			list.Clear();
			list = VCUtils.ExplodeString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("cost")), ';');
			vCESceneSetting.m_BlockUnit = Convert.ToInt32(list[0]);
			vCESceneSetting.m_DyeUnit = Convert.ToInt32(list[1]);
			list.Clear();
			s_EditorScenes.Add(vCESceneSetting);
		}
	}

	private static void LoadMatters()
	{
		s_Matters = new Dictionary<int, VCMatterInfo>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_material ORDER BY cast(sort as int) ASC");
		while (sqliteDataReader.Read())
		{
			VCMatterInfo vCMatterInfo = new VCMatterInfo();
			vCMatterInfo.ItemIndex = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			vCMatterInfo.Order = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sort")));
			vCMatterInfo.ItemId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemid")));
			vCMatterInfo.Attack = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("attack")));
			vCMatterInfo.Defence = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("defence")));
			vCMatterInfo.Durability = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("durability")));
			vCMatterInfo.Hp = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("hp")));
			vCMatterInfo.SellPrice = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("value")));
			vCMatterInfo.Density = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("density")));
			vCMatterInfo.Elasticity = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("elasticity")));
			vCMatterInfo.DefaultBumpStrength = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bumpstrength")));
			vCMatterInfo.DefaultSpecularStrength = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("specularstrength")));
			vCMatterInfo.DefaultSpecularPower = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("specularpower")));
			vCMatterInfo.DefaultTile = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("tile")));
			vCMatterInfo.DefaultDiffuseRes = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("diffusemap"));
			vCMatterInfo.DefaultBumpRes = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bumpmap"));
			List<string> list = null;
			list = VCUtils.ExplodeString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("specularcolor")), ';');
			vCMatterInfo.DefaultSpecularColor.r = (byte)Convert.ToInt32(list[0]);
			vCMatterInfo.DefaultSpecularColor.g = (byte)Convert.ToInt32(list[1]);
			vCMatterInfo.DefaultSpecularColor.b = (byte)Convert.ToInt32(list[2]);
			vCMatterInfo.DefaultSpecularColor.a = byte.MaxValue;
			list.Clear();
			list = VCUtils.ExplodeString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("emissivecolor")), ';');
			vCMatterInfo.DefaultEmissiveColor.r = (byte)Convert.ToInt32(list[0]);
			vCMatterInfo.DefaultEmissiveColor.g = (byte)Convert.ToInt32(list[1]);
			vCMatterInfo.DefaultEmissiveColor.b = (byte)Convert.ToInt32(list[2]);
			vCMatterInfo.DefaultEmissiveColor.a = byte.MaxValue;
			list.Clear();
			s_Matters.Add(vCMatterInfo.ItemIndex, vCMatterInfo);
		}
	}

	private static void LoadPartTypes()
	{
		s_PartTypes = new Dictionary<EVCComponent, VCPartTypeInfo>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_part_type");
		while (sqliteDataReader.Read())
		{
			VCPartTypeInfo vCPartTypeInfo = new VCPartTypeInfo();
			vCPartTypeInfo.m_Type = (EVCComponent)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			vCPartTypeInfo.m_Name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("name"));
			vCPartTypeInfo.m_ShortName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("short_name"));
			vCPartTypeInfo.m_InspectorRes = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("inspector"));
			vCPartTypeInfo.m_RotateMask = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rotmask")));
			s_PartTypes.Add(vCPartTypeInfo.m_Type, vCPartTypeInfo);
		}
	}

	private static void LoadParts()
	{
		s_Parts = new Dictionary<int, VCPartInfo>();
		string sqlQuery = " SELECT a.id, a.itemid, a.costcount, a.type, b._engName as name, a.path as respath,         b._iconId as iconpath, b.currency_value as sellprice, a.weight, a.volume, a.mirror_mask, a.symmetric  FROM vc_part a, PrototypeItem b WHERE a.itemid = b.id  ORDER BY cast(a.type as int) ASC, cast(a.sort as int) ASC";
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ExecuteQuery(sqlQuery);
		while (sqliteDataReader.Read())
		{
			VCPartInfo vCPartInfo = new VCPartInfo();
			try
			{
				vCPartInfo.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
				vCPartInfo.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemid")));
				vCPartInfo.m_CostCount = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("costcount")));
				vCPartInfo.m_Type = (EVCComponent)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("type")));
				vCPartInfo.m_ResPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("respath"));
				vCPartInfo.m_ResObj = Resources.Load(vCPartInfo.m_ResPath) as GameObject;
				vCPartInfo.m_IconPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("iconpath"));
				vCPartInfo.m_SellPrice = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sellprice")));
				vCPartInfo.m_Weight = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("weight")));
				vCPartInfo.m_Volume = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("volume")));
				vCPartInfo.m_MirrorMask = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("mirror_mask")));
				vCPartInfo.m_Symmetric = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("symmetric")));
				s_Parts.Add(vCPartInfo.m_ID, vCPartInfo);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Exception on load parts:" + vCPartInfo.m_ID + "\n" + ex);
			}
		}
	}

	private static void LoadEffects()
	{
		s_Effects = new Dictionary<int, VCEffectInfo>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_effects ORDER BY cast(sort as int) ASC");
		while (sqliteDataReader.Read())
		{
			VCEffectInfo vCEffectInfo = new VCEffectInfo();
			vCEffectInfo.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			vCEffectInfo.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemid")));
			vCEffectInfo.m_Type = (EVCEffect)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("type")));
			vCEffectInfo.m_Name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("name"));
			vCEffectInfo.m_ResPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("respath"));
			vCEffectInfo.m_ResObj = Resources.Load(vCEffectInfo.m_ResPath) as GameObject;
			vCEffectInfo.m_IconPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("iconpath"));
			vCEffectInfo.m_IconTex = Resources.Load(vCEffectInfo.m_IconPath) as Texture2D;
			vCEffectInfo.m_SellPrice = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sellprice")));
			s_Effects.Add(vCEffectInfo.m_ID, vCEffectInfo);
		}
	}
}
