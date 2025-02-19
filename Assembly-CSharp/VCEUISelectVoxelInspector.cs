using System.Collections.Generic;
using UnityEngine;

public class VCEUISelectVoxelInspector : VCEUISelectInspector
{
	public VCESelectVoxel m_SelectBrush;

	public GameObject m_FunctionGroup;

	public UICheckbox m_BoxMethodCheck;

	public GameObject m_VolumeGroup;

	public UILabel m_VolumeLabel;

	public UISlider m_VolumeSlider;

	public GameObject m_TextureBtnGO;

	public GameObject m_ColorBtnGO;

	public GameObject m_EraseBtnGO;

	private VCEAction m_VolModifyAction;

	private float oldVol;

	private int oldKey = -1;

	private float VolumeSliderValue
	{
		get
		{
			return Mathf.Clamp(m_VolumeSlider.sliderValue, 0.004f, 1f);
		}
		set
		{
			m_VolumeSlider.sliderValue = value;
		}
	}

	public void Update()
	{
		int count = VCEditor.Instance.m_VoxelSelection.m_Selection.Count;
		if (count == 0)
		{
			m_SelectInfo.text = "[00FFFF]0[-] " + "voxel selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(value: false);
		}
		else if (count == 1)
		{
			m_SelectInfo.text = "[00FFFF]1[-] " + "voxel selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(value: true);
		}
		else if (count < 10000)
		{
			m_SelectInfo.text = "[00FFFF]" + count.ToString("#,###") + "[-] " + "voxels selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(value: true);
		}
		else
		{
			m_SelectInfo.text = "[00FFFF]" + count.ToString("#,###") + "[-]\r\n" + "voxels selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(value: true);
		}
		if (count == 1)
		{
			m_VolumeGroup.SetActive(value: true);
			int num = -1;
			VCVoxel vCVoxel = default(VCVoxel);
			VCVoxel vCVoxel2 = default(VCVoxel);
			foreach (KeyValuePair<int, byte> item5 in VCEditor.Instance.m_VoxelSelection.m_Selection)
			{
				num = item5.Key;
				vCVoxel = VCEditor.s_Scene.m_IsoData.GetVoxel(item5.Key);
			}
			if (num != oldKey)
			{
				m_VolModifyAction = null;
				oldKey = num;
			}
			if (num >= 0)
			{
				if (m_VolModifyAction == null)
				{
					m_VolModifyAction = new VCEAction();
					VolumeSliderValue = vCVoxel.VolumeF;
					oldVol = VolumeSliderValue;
				}
				m_VolumeLabel.text = "Voxel volume".ToLocalizationString() + " = [FFFF00]" + (VolumeSliderValue * 200f - 100f).ToString("0") + "[-]";
				vCVoxel2 = vCVoxel;
				vCVoxel2.VolumeF = VolumeSliderValue;
				if (Input.GetMouseButtonUp(0) && Mathf.Abs(oldVol - VolumeSliderValue) > 0.002f)
				{
					oldVol = VolumeSliderValue;
					VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
					if (VCEditor.s_Mirror.Enabled_Masked)
					{
						VCEditor.s_Mirror.MirrorVoxel(VCIsoData.KeyToIPos(num));
						for (int i = 0; i < VCEditor.s_Mirror.OutputCnt; i++)
						{
							if (!VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]))
							{
								continue;
							}
							int num2 = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
							VCVoxel voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(num2);
							if (voxel.Volume == 0)
							{
								continue;
							}
							VCVoxel vCVoxel3 = voxel;
							vCVoxel3.VolumeF = VolumeSliderValue;
							if (m_VolModifyAction.Modifies.Count == 0)
							{
								VCEAlterVoxel item = new VCEAlterVoxel(num2, voxel, vCVoxel3);
								m_VolModifyAction.Modifies.Add(item);
								m_VolModifyAction.Do();
								continue;
							}
							bool flag = false;
							foreach (VCEModify modify in m_VolModifyAction.Modifies)
							{
								if (modify is VCEAlterVoxel vCEAlterVoxel && vCEAlterVoxel.m_Pos == num2)
								{
									vCEAlterVoxel.m_New = vCVoxel3;
									flag = true;
								}
							}
							if (!flag)
							{
								VCEAlterVoxel item2 = new VCEAlterVoxel(num2, voxel, vCVoxel3);
								m_VolModifyAction.Modifies.Add(item2);
							}
							m_VolModifyAction.DoButNotRegister();
						}
					}
					else if (m_VolModifyAction.Modifies.Count == 0)
					{
						VCEAlterVoxel item3 = new VCEAlterVoxel(num, vCVoxel, vCVoxel2);
						m_VolModifyAction.Modifies.Add(item3);
						m_VolModifyAction.Do();
					}
					else
					{
						bool flag2 = false;
						foreach (VCEModify modify2 in m_VolModifyAction.Modifies)
						{
							if (modify2 is VCEAlterVoxel vCEAlterVoxel2 && vCEAlterVoxel2.m_Pos == num)
							{
								vCEAlterVoxel2.m_New = vCVoxel2;
								flag2 = true;
							}
						}
						if (!flag2)
						{
							VCEAlterVoxel item4 = new VCEAlterVoxel(num, vCVoxel, vCVoxel2);
							m_VolModifyAction.Modifies.Add(item4);
						}
						m_VolModifyAction.DoButNotRegister();
					}
				}
			}
		}
		else
		{
			m_VolumeGroup.SetActive(value: false);
			m_VolModifyAction = null;
			oldKey = -1;
			oldVol = 0f;
		}
		if (m_BoxMethodCheck.isChecked)
		{
			m_SelectBrush.SelectMethod = EVCESelectMethod.Box;
		}
		else
		{
			m_SelectBrush.SelectMethod = EVCESelectMethod.None;
		}
		if (VCEditor.Instance.m_UI.m_MaterialTab.isChecked)
		{
			if (VCEditor.SelectedVoxelType >= 0)
			{
				m_TextureBtnGO.SetActive(value: true);
			}
			else
			{
				m_TextureBtnGO.SetActive(value: false);
			}
			m_ColorBtnGO.SetActive(value: false);
			m_EraseBtnGO.SetActive(value: false);
		}
		else if (VCEditor.Instance.m_UI.m_PaintTab.isChecked)
		{
			m_TextureBtnGO.SetActive(value: false);
			m_ColorBtnGO.SetActive(value: true);
			m_EraseBtnGO.SetActive(value: true);
		}
		if (m_FunctionGroup.activeSelf)
		{
			m_FunctionGroup.GetComponent<UIGrid>().Reposition();
		}
	}

	public void CancelAllMethod()
	{
		m_BoxMethodCheck.isChecked = false;
	}

	private void OnDeleteClick()
	{
		m_SelectBrush.DeleteSelection();
	}

	private void OnTextureClick()
	{
		m_SelectBrush.TextureSelection();
	}

	private void OnColorClick()
	{
		m_SelectBrush.ColorSelection(VCEditor.SelectedColor);
	}

	private void OnEraseColorClick()
	{
		m_SelectBrush.ColorSelection(VCIsoData.BLANK_COLOR);
	}
}
