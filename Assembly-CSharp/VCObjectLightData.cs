using System.IO;
using UnityEngine;
using WhiteCat;

public class VCObjectLightData : VCPartData
{
	public Color m_Color = Color.white;

	public override void Validate()
	{
		PositionValidate();
		m_Scale.x = (m_Scale.y = (m_Scale.z = Mathf.Clamp(GetScaleValue(m_Scale), 0.25f, 4f)));
	}

	private static float GetScaleValue(Vector3 scale)
	{
		if (scale.x == scale.y)
		{
			return scale.z;
		}
		if (scale.x == scale.z)
		{
			return scale.y;
		}
		if (scale.y == scale.z)
		{
			return scale.x;
		}
		return Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
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
		m_Color.r = binaryReader.ReadSingle();
		m_Color.g = binaryReader.ReadSingle();
		m_Color.b = binaryReader.ReadSingle();
		m_Color.a = binaryReader.ReadSingle();
		binaryReader.ReadSingle();
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
		binaryWriter.Write(m_Color.r);
		binaryWriter.Write(m_Color.g);
		binaryWriter.Write(m_Color.b);
		binaryWriter.Write(m_Color.a);
		binaryWriter.Write(2f);
		binaryWriter.Close();
		return memoryStream.ToArray();
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
		VCPSimpleLight component2 = m_Entity.GetComponent<VCPSimpleLight>();
		if (component2 != null)
		{
			component2.color = m_Color;
		}
		Renderer[] componentsInChildren = m_Entity.GetComponentsInChildren<Renderer>(includeInactive: true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer is TrailRenderer)
			{
				renderer.enabled = true;
			}
			else if (renderer is ParticleRenderer)
			{
				renderer.enabled = true;
			}
			else if (renderer is ParticleSystemRenderer)
			{
				renderer.enabled = true;
			}
			else if (renderer is LineRenderer)
			{
				renderer.enabled = true;
			}
			else if (renderer is SpriteRenderer)
			{
				renderer.enabled = true;
			}
			else
			{
				renderer.enabled = m_Visible;
			}
		}
	}
}
