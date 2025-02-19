using System.IO;
using UnityEngine;

public class VCTriphaseFixedPartData : VCFixedPartData, IVCMultiphaseComponentData
{
	public int m_Phase;

	public int Phase
	{
		get
		{
			return m_Phase;
		}
		set
		{
			m_Phase = value % 3;
		}
	}

	public void InversePhase()
	{
		if (m_Phase == 0)
		{
			m_Phase = 0;
		}
		else if (m_Phase == 1)
		{
			m_Phase = 2;
		}
		else if (m_Phase == 2)
		{
			m_Phase = 1;
		}
	}

	public override void Validate()
	{
		PositionValidate();
		m_Rotation = Vector3.zero;
		m_Scale = Vector3.one;
		m_Phase %= 3;
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
		m_Phase = binaryReader.ReadInt32();
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
		binaryWriter.Write(m_Phase);
		binaryWriter.Close();
		return memoryStream.ToArray();
	}

	public override void UpdateEntity(bool for_editor)
	{
		VCEComponentTool component = m_Entity.GetComponent<VCEComponentTool>();
		if (component != null)
		{
			component.SetPhase(m_Phase);
			if (for_editor)
			{
				component.m_Data = this;
			}
		}
		m_Rotation = VCEMath.NormalizeEulerAngle(m_Rotation);
		m_Entity.transform.localPosition = m_Position;
		m_Entity.transform.localEulerAngles = m_Rotation;
		m_Entity.transform.localScale = m_Scale;
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
