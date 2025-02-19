using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VCEScene
{
	public struct ISOMat
	{
		public Material m_EditorMat;

		public RenderTexture m_DiffTex;

		public RenderTexture m_BumpTex;

		public Texture2D m_PropertyTex;

		public void Init()
		{
			m_EditorMat = VCEditor.Instance.m_TempIsoMat;
			m_DiffTex = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
			m_BumpTex = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
			m_PropertyTex = new Texture2D(16, 4, TextureFormat.ARGB32, mipmap: false, linear: false);
			m_DiffTex.anisoLevel = 4;
			m_DiffTex.filterMode = FilterMode.Trilinear;
			m_DiffTex.useMipMap = true;
			m_DiffTex.wrapMode = TextureWrapMode.Repeat;
			m_BumpTex.anisoLevel = 4;
			m_BumpTex.filterMode = FilterMode.Trilinear;
			m_BumpTex.useMipMap = true;
			m_BumpTex.wrapMode = TextureWrapMode.Repeat;
			m_PropertyTex.anisoLevel = 0;
			m_PropertyTex.filterMode = FilterMode.Point;
		}

		public void Destroy()
		{
			m_EditorMat.SetTexture("_DiffuseTex", null);
			m_EditorMat.SetTexture("_BumpTex", null);
			m_EditorMat.SetTexture("_PropertyTex", null);
			if (m_DiffTex != null)
			{
				UnityEngine.Object.Destroy(m_DiffTex);
				m_DiffTex = null;
			}
			if (m_BumpTex != null)
			{
				UnityEngine.Object.Destroy(m_BumpTex);
				m_BumpTex = null;
			}
			if (m_PropertyTex != null)
			{
				UnityEngine.Object.Destroy(m_PropertyTex);
				m_PropertyTex = null;
			}
		}
	}

	public VCESceneSetting m_Setting;

	public VCIsoData m_IsoData;

	public VCIsoData m_Stencil;

	public ISOMat m_TempIsoMat;

	public VCMCComputer m_MeshComputer;

	public CreationAttr m_CreationAttr;

	public string m_DocumentPath = string.Empty;

	public static event Action OnSaveIso;

	public VCEScene(VCESceneSetting setting)
	{
		New(setting);
	}

	public VCEScene(string iso_path)
	{
		m_IsoData = new VCIsoData();
		if (!LoadIso(iso_path))
		{
			Destroy();
			throw new Exception("Load ISO error");
		}
		VCESceneSetting vCESceneSetting = m_IsoData.m_HeadInfo.FindSceneSetting();
		if (vCESceneSetting != null)
		{
			m_Setting = vCESceneSetting;
			m_Stencil = new VCIsoData();
			m_Stencil.Init(33751041, m_Setting, new VCIsoOption(editor: true));
			m_TempIsoMat.Init();
			m_DocumentPath = iso_path;
			VCEditor.Instance.m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
			m_MeshComputer = new VCMCComputer();
			m_MeshComputer.Init(m_Setting.m_EditorSize, VCEditor.Instance.m_MeshMgr);
			m_CreationAttr = new CreationAttr();
			return;
		}
		Destroy();
		throw new Exception("Scene setting error");
	}

	public VCEScene(VCESceneSetting setting, int template)
	{
		TextAsset textAsset = Resources.Load<TextAsset>("Isos/" + setting.m_Id + "/" + template);
		if (textAsset == null)
		{
			New(setting);
			return;
		}
		m_IsoData = new VCIsoData();
		if (!m_IsoData.Import(textAsset.bytes, new VCIsoOption(editor: true)))
		{
			Destroy();
			throw new Exception("Load Template ISO error");
		}
		VCESceneSetting vCESceneSetting = m_IsoData.m_HeadInfo.FindSceneSetting();
		if (vCESceneSetting != null)
		{
			m_Setting = vCESceneSetting;
			m_Stencil = new VCIsoData();
			m_Stencil.Init(33751041, m_Setting, new VCIsoOption(editor: true));
			m_TempIsoMat.Init();
			m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled" + VCConfig.s_IsoFileExt;
			int num = 2;
			while (File.Exists(VCConfig.s_IsoPath + m_DocumentPath))
			{
				m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled (" + num + ")" + VCConfig.s_IsoFileExt;
				num++;
			}
			VCEditor.Instance.m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
			m_MeshComputer = new VCMCComputer();
			m_MeshComputer.Init(m_Setting.m_EditorSize, VCEditor.Instance.m_MeshMgr);
			m_CreationAttr = new CreationAttr();
			return;
		}
		Destroy();
		throw new Exception("Scene setting error");
	}

	private void New(VCESceneSetting setting)
	{
		m_Setting = setting;
		m_IsoData = new VCIsoData();
		m_IsoData.Init(33751041, setting, new VCIsoOption(editor: true));
		m_IsoData.m_HeadInfo.Category = setting.m_Category;
		m_Stencil = new VCIsoData();
		m_Stencil.Init(33751041, setting, new VCIsoOption(editor: true));
		m_TempIsoMat.Init();
		m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled" + VCConfig.s_IsoFileExt;
		int num = 2;
		while (File.Exists(VCConfig.s_IsoPath + m_DocumentPath))
		{
			m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled (" + num + ")" + VCConfig.s_IsoFileExt;
			num++;
		}
		VCEditor.Instance.m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
		m_MeshComputer = new VCMCComputer();
		m_MeshComputer.Init(m_Setting.m_EditorSize, VCEditor.Instance.m_MeshMgr);
		m_CreationAttr = new CreationAttr();
	}

	public void BuildScene()
	{
		GenerateIsoMat();
		foreach (VCComponentData component in m_IsoData.m_Components)
		{
			component.CreateEntity(for_editor: true, null);
		}
		foreach (KeyValuePair<int, VCVoxel> voxel in m_IsoData.m_Voxels)
		{
			m_MeshComputer.AlterVoxel(voxel.Key, voxel.Value);
		}
	}

	public void Destroy()
	{
		if (m_IsoData != null)
		{
			m_IsoData.Destroy();
			m_IsoData = null;
		}
		if (m_Stencil != null)
		{
			m_Stencil.Destroy();
			m_Stencil = null;
		}
		m_TempIsoMat.Destroy();
		if (m_MeshComputer != null)
		{
			m_MeshComputer.Destroy();
			m_MeshComputer = null;
		}
		VCEditor.Instance.m_MeshMgr.FreeGameObjects();
	}

	public void PreventRenderTextureLost()
	{
		if (!m_TempIsoMat.m_DiffTex.IsCreated() || !m_TempIsoMat.m_BumpTex.IsCreated())
		{
			GenerateIsoMat();
		}
	}

	public void GenerateIsoMat()
	{
		if (VCMatGenerator.Instance != null)
		{
			VCMaterial[] array = null;
			array = ((m_IsoData == null || m_IsoData.m_Materials == null) ? new VCMaterial[16] : m_IsoData.m_Materials);
			VCMatGenerator.Instance.GenMeshMaterial(array, bForEditor: true);
		}
	}

	public bool SaveIso()
	{
		try
		{
			string text = VCConfig.s_IsoPath + m_DocumentPath;
			string fullName = new FileInfo(text).Directory.FullName;
			if (!Directory.Exists(fullName))
			{
				Directory.CreateDirectory(fullName);
			}
			using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write))
			{
				byte[] array = m_IsoData.Export();
				fileStream.Write(array, 0, array.Length);
				fileStream.Close();
			}
			VCEHistory.s_Modified = false;
			if (VCEScene.OnSaveIso != null)
			{
				VCEScene.OnSaveIso();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public bool SaveIsoAs(string name)
	{
		m_DocumentPath = name;
		return SaveIso();
	}

	private bool LoadIso(string path)
	{
		try
		{
			m_DocumentPath = path;
			string path2 = VCConfig.s_IsoPath + m_DocumentPath;
			using FileStream fileStream = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] array = new byte[(int)fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			return m_IsoData.Import(array, new VCIsoOption(editor: true));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Loading ISO Error : " + ex.ToString());
			return false;
		}
	}
}
