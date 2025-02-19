using System.Collections.Generic;
using Pathea;
using UnityEngine;
using WhiteCat;

public class CreationWaterMask : PeCmpt
{
	public GameObject m_WaterMask;

	private CreationData m_CreationData;

	private VCIsoData m_IsoData;

	private CreationAttr m_Attribute;

	private VCESceneSetting m_SceneSetting;

	private Vector3 m_MassCenter = Vector3.zero;

	private Material m_MaskMat;

	private Texture2D m_MaskTex;

	public override void Awake()
	{
		base.Awake();
		CreationController component = GetComponent<CreationController>();
		if (component != null)
		{
			m_CreationData = component.creationData;
			if (m_CreationData != null)
			{
				m_IsoData = m_CreationData.m_IsoData;
				m_SceneSetting = m_IsoData.m_HeadInfo.FindSceneSetting();
				m_Attribute = m_CreationData.m_Attribute;
				m_MassCenter = m_Attribute.m_CenterOfMass;
			}
		}
	}

	public override void Start()
	{
		base.Start();
		GameObject gameObject = Object.Instantiate(VCEditor.Instance.m_WaterMaskPrefab);
		gameObject.name = "Water Mask";
		gameObject.transform.parent = base.transform;
		gameObject.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
		gameObject.transform.localScale = new Vector3(m_SceneSetting.EditorWorldSize.x, m_SceneSetting.EditorWorldSize.z, 1f);
		m_WaterMask = gameObject;
		Material material = Object.Instantiate(gameObject.GetComponent<Renderer>().material);
		gameObject.GetComponent<Renderer>().material = material;
		m_MaskTex = new Texture2D(m_SceneSetting.m_EditorSize.x, m_SceneSetting.m_EditorSize.z, TextureFormat.ARGB32, mipmap: false);
		material.SetTexture("_MainTex", m_MaskTex);
		int num = 0;
		int y = m_SceneSetting.m_EditorSize.y;
		int[] array = new int[y];
		int[] array2 = new int[y];
		int[] array3 = new int[y];
		int[] array4 = new int[y];
		int[] array5 = new int[y];
		for (int i = 0; i < y; i++)
		{
			array[i] = (array3[i] = 0);
			array2[i] = (array4[i] = 1000000);
			array5[i] = 0;
		}
		foreach (KeyValuePair<int, VCVoxel> voxel in m_IsoData.m_Voxels)
		{
			IntVector3 intVector = VCIsoData.KeyToIPos(voxel.Key);
			if (intVector.y > num)
			{
				num = intVector.y;
			}
			if (intVector.y >= 0 && intVector.y < y)
			{
				if (intVector.x < array2[intVector.y])
				{
					array2[intVector.y] = intVector.x;
				}
				if (intVector.x > array[intVector.y])
				{
					array[intVector.y] = intVector.x;
				}
				if (intVector.z < array4[intVector.y])
				{
					array4[intVector.y] = intVector.z;
				}
				if (intVector.z > array3[intVector.y])
				{
					array3[intVector.y] = intVector.z;
				}
				array5[intVector.y]++;
			}
		}
		int num2 = Mathf.Min(Mathf.FloorToInt(m_MassCenter.y / m_SceneSetting.m_VoxelSize), num);
		int num3 = Mathf.Max(Mathf.FloorToInt(m_MassCenter.y / m_SceneSetting.m_VoxelSize), num);
		if (num3 > num2 + 16)
		{
			num3 = num2 + 16;
		}
		if (num3 >= y)
		{
			num3 = y - 1;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		int num4 = num2;
		int num5 = 0;
		int num6 = 1;
		int num7 = 0;
		for (int j = num2; j <= num3; j++)
		{
			int num8 = (array[j] - array2[j]) * (array3[j] - array4[j]);
			int num9 = array5[j];
			int num10 = (array[j] - array2[j]) * 2 + (array3[j] - array4[j]) * 2;
			if (num8 >= num5 && num9 >= num7)
			{
				num4 = j;
			}
			if (num8 >= num5)
			{
				num5 = num8;
			}
			if (num9 >= num7 && num9 < num10 * 5)
			{
				num7 = num9;
			}
			if (num8 > 100 && num9 >= num10 / 3 && j >= num4 + 5 && j <= num2 + 6 && num6 > 0)
			{
				num6--;
				num4 = j;
			}
			if (num8 < num5 / 9 && j > num2 + 4)
			{
				break;
			}
		}
		Color32[] array6 = new Color32[m_MaskTex.width * m_MaskTex.height];
		for (int k = 0; k < array6.Length; k++)
		{
			ref Color32 reference = ref array6[k];
			reference = new Color32(0, 0, 0, byte.MaxValue);
		}
		int num11 = array2[num4];
		int num12 = array[num4];
		int num13 = array4[num4];
		int num14 = array3[num4];
		for (int l = num11; l <= num12; l++)
		{
			for (int m = num13; m <= num14; m++)
			{
				bool flag = false;
				for (int num15 = num4; num15 >= 0; num15--)
				{
					int pos = VCIsoData.IPosToKey(l, num15, m);
					if (m_IsoData.GetVoxel(pos).Volume >= 128)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					int num16 = m * m_SceneSetting.m_EditorSize.x + l;
					if (num16 < array6.Length)
					{
						ref Color32 reference2 = ref array6[num16];
						reference2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
					}
				}
			}
		}
		m_MaskTex.SetPixels32(array6);
		m_MaskTex.Apply();
		Vector3 vector = m_SceneSetting.EditorWorldSize * 0.5f;
		Vector3 localPosition = vector - m_MassCenter;
		localPosition.y = ((float)num4 + 0.5f) * m_SceneSetting.m_VoxelSize;
		gameObject.transform.localPosition = localPosition;
	}

	public override void OnDestroy()
	{
		if (m_MaskTex != null)
		{
			Object.Destroy(m_MaskTex);
			m_MaskTex = null;
		}
		if (m_MaskMat != null)
		{
			Object.Destroy(m_MaskMat);
			m_MaskMat = null;
		}
	}
}
