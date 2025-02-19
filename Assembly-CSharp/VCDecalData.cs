using System.IO;
using UnityEngine;

public class VCDecalData : VCComponentData, IVCMultiphaseComponentData
{
	public static int s_ComponentId = 9901;

	public static string s_DecalPrefabPath = "Prefab/Decal Prefabs/Voxel Creation Decal";

	public ulong m_Guid;

	public int m_AssetIndex = -1;

	public float m_Size = 0.1f;

	public float m_Depth = 0.01f;

	public bool m_Mirrored;

	public int m_ShaderIndex;

	public Color m_Color = Color.white;

	public int Phase
	{
		get
		{
			return m_Mirrored ? 1 : 0;
		}
		set
		{
			m_Mirrored = value != 0;
		}
	}

	public VCDecalData()
	{
		m_ComponentId = s_ComponentId;
		m_Type = EVCComponent.cpDecal;
	}

	public void InversePhase()
	{
		m_Mirrored = !m_Mirrored;
	}

	public override GameObject CreateEntity(bool for_editor, Transform parent)
	{
		if (m_Entity != null)
		{
			DestroyEntity();
		}
		m_Entity = Object.Instantiate(Resources.Load(s_DecalPrefabPath) as GameObject);
		m_Entity.name = "Decal Image";
		if (for_editor)
		{
			m_Entity.transform.parent = VCEditor.Instance.m_DecalGroup.transform;
			VCEComponentTool component = m_Entity.GetComponent<VCEComponentTool>();
			component.m_IsBrush = false;
			component.m_InEditor = true;
			component.m_ToolGroup.SetActive(value: true);
			component.m_SelBound.enabled = false;
			component.m_SelBound.GetComponent<Collider>().enabled = false;
			component.m_SelBound.m_BoundColor = GLComponentBound.s_Blue;
			component.m_Data = this;
			Collider[] componentsInChildren = m_Entity.GetComponentsInChildren<Collider>(includeInactive: true);
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				if (collider.gameObject != component.m_SelBound.gameObject)
				{
					collider.enabled = false;
				}
			}
		}
		else
		{
			m_Entity.transform.parent = parent;
			Transform[] componentsInChildren2 = m_Entity.GetComponentsInChildren<Transform>(includeInactive: true);
			Transform[] array2 = componentsInChildren2;
			foreach (Transform transform in array2)
			{
				transform.gameObject.layer = VCConfig.s_ProductLayer;
			}
		}
		UpdateEntity(for_editor);
		if (!for_editor)
		{
			UpdateComponent();
		}
		return m_Entity;
	}

	public override void UpdateEntity(bool for_editor)
	{
		if (for_editor)
		{
			VCEComponentTool component = m_Entity.GetComponent<VCEComponentTool>();
			component.m_Data = this;
		}
		m_Rotation = VCEMath.NormalizeEulerAngle(m_Rotation);
		m_Entity.transform.localPosition = m_Position;
		m_Entity.transform.localEulerAngles = m_Rotation;
		m_Entity.transform.localScale = m_Scale;
		VCDecalHandler component2 = m_Entity.GetComponent<VCDecalHandler>();
		if (for_editor)
		{
			component2.m_Guid = m_Guid;
			component2.m_Iso = null;
			component2.m_AssetIndex = -1;
		}
		else
		{
			component2.m_Guid = 0uL;
			component2.m_Iso = m_CurrIso;
			component2.m_AssetIndex = m_AssetIndex;
		}
		component2.m_Size = m_Size;
		component2.m_Depth = m_Depth;
		component2.m_Mirrored = m_Mirrored;
		component2.m_ShaderIndex = m_ShaderIndex;
		component2.m_Color = m_Color;
	}

	public override void Validate()
	{
		PositionValidate();
		m_Scale = Vector3.one;
	}

	public override void Import(byte[] buffer)
	{
		if (buffer == null)
		{
			return;
		}
		using MemoryStream input = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		m_ComponentId = num & 0xFFFF;
		m_ExtendData = num >> 16;
		m_Type = (EVCComponent)binaryReader.ReadInt32();
		m_Position.x = binaryReader.ReadSingle();
		m_Position.y = binaryReader.ReadSingle();
		m_Position.z = binaryReader.ReadSingle();
		m_Rotation.x = binaryReader.ReadSingle();
		m_Rotation.y = binaryReader.ReadSingle();
		m_Rotation.z = binaryReader.ReadSingle();
		m_Scale.x = binaryReader.ReadSingle();
		m_Scale.y = binaryReader.ReadSingle();
		m_Scale.z = binaryReader.ReadSingle();
		m_Visible = binaryReader.ReadBoolean();
		m_Guid = binaryReader.ReadUInt64();
		m_AssetIndex = binaryReader.ReadInt32();
		m_Size = binaryReader.ReadSingle();
		m_Depth = binaryReader.ReadSingle();
		m_Mirrored = binaryReader.ReadBoolean();
		m_ShaderIndex = binaryReader.ReadInt32();
		m_Color.r = binaryReader.ReadSingle();
		m_Color.g = binaryReader.ReadSingle();
		m_Color.b = binaryReader.ReadSingle();
		m_Color.a = binaryReader.ReadSingle();
		binaryReader.Close();
	}

	public override byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(m_ComponentId | (m_ExtendData << 16));
		binaryWriter.Write((int)m_Type);
		binaryWriter.Write(m_Position.x);
		binaryWriter.Write(m_Position.y);
		binaryWriter.Write(m_Position.z);
		binaryWriter.Write(m_Rotation.x);
		binaryWriter.Write(m_Rotation.y);
		binaryWriter.Write(m_Rotation.z);
		binaryWriter.Write(m_Scale.x);
		binaryWriter.Write(m_Scale.y);
		binaryWriter.Write(m_Scale.z);
		binaryWriter.Write(m_Visible);
		binaryWriter.Write(m_Guid);
		binaryWriter.Write(m_AssetIndex);
		binaryWriter.Write(m_Size);
		binaryWriter.Write(m_Depth);
		binaryWriter.Write(m_Mirrored);
		binaryWriter.Write(m_ShaderIndex);
		binaryWriter.Write(m_Color.r);
		binaryWriter.Write(m_Color.g);
		binaryWriter.Write(m_Color.b);
		binaryWriter.Write(m_Color.a);
		binaryWriter.Close();
		return memoryStream.ToArray();
	}
}
