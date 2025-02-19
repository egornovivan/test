using System;
using System.Collections.Generic;
using System.IO;
using BSTools;
using UnityEngine;

public class BSMiscBrush : BSSelectBrush
{
	public string IsoSavedName = "No Name";

	private bool _extruding;

	private float _startExtrudeTime;

	protected override bool AfterSelectionUpdate()
	{
		if (!m_Selecting)
		{
			if (m_SelectionBoxes.Count == 0)
			{
				return true;
			}
			if (!_extruding)
			{
				if (BSInput.s_Delete)
				{
					DeleteVoxel();
				}
				else if (Input.GetKeyDown(KeyCode.Period))
				{
					BSIsoData outData = null;
					SaveToIso(IsoSavedName, new byte[0], out outData);
				}
			}
			if (!_extruding)
			{
				if (BSInput.s_Shift && BSInput.s_Left)
				{
					ExtrudeSelection(-1, 0, 0);
					_startExtrudeTime = 0f;
				}
				if (BSInput.s_Shift && BSInput.s_Right)
				{
					ExtrudeSelection(1, 0, 0);
					_startExtrudeTime = 0f;
				}
				if (BSInput.s_Shift && BSInput.s_Up)
				{
					ExtrudeSelection(0, 1, 0);
					_startExtrudeTime = 0f;
				}
				if (BSInput.s_Shift && BSInput.s_Down)
				{
					ExtrudeSelection(0, -1, 0);
					_startExtrudeTime = 0f;
				}
				if (BSInput.s_Shift && BSInput.s_Forward)
				{
					ExtrudeSelection(0, 0, 1);
					_startExtrudeTime = 0f;
				}
				if (BSInput.s_Shift && BSInput.s_Back)
				{
					ExtrudeSelection(0, 0, -1);
					_startExtrudeTime = 0f;
				}
			}
			if (_extruding)
			{
				if (_startExtrudeTime > 0.3f)
				{
					_startExtrudeTime = 0f;
					_extruding = false;
					return true;
				}
				_startExtrudeTime += Time.deltaTime;
				return false;
			}
		}
		return true;
	}

	public void DeleteVoxel()
	{
		if (m_SelectionBoxes.Count == 0)
		{
			return;
		}
		List<BSVoxel> list = new List<BSVoxel>();
		List<IntVector3> list2 = new List<IntVector3>();
		List<BSVoxel> list3 = new List<BSVoxel>();
		foreach (BSTools.SelBox selectionBox in m_SelectionBoxes)
		{
			for (int i = selectionBox.m_Box.xMin; i <= selectionBox.m_Box.xMax; i++)
			{
				for (int j = selectionBox.m_Box.yMin; j <= selectionBox.m_Box.yMax; j++)
				{
					for (int k = selectionBox.m_Box.zMin; k <= selectionBox.m_Box.zMax; k++)
					{
						BSVoxel item = dataSource.Read(i, j, k);
						list.Add(default(BSVoxel));
						list2.Add(new IntVector3(i, j, k));
						list3.Add(item);
					}
				}
			}
		}
		ClearSelection(m_Action);
		if (list2.Count != 0)
		{
			BSVoxelModify bSVoxelModify = new BSVoxelModify(list2.ToArray(), list3.ToArray(), list.ToArray(), dataSource, EBSBrushMode.Subtract);
			m_Action.AddModify(bSVoxelModify);
			bSVoxelModify.Redo();
		}
	}

	public bool SaveToIso(string IsoName, byte[] icon_tex, out BSIsoData outData)
	{
		outData = null;
		if (m_SelectionBoxes.Count == 0)
		{
			return false;
		}
		if (pattern.type != EBSVoxelType.Block)
		{
			Debug.LogWarning("The iso is not support the Voxel");
			return false;
		}
		BSIsoData bSIsoData = new BSIsoData();
		bSIsoData.Init(pattern.type);
		bSIsoData.m_HeadInfo.Name = IsoName;
		BSTools.IntBox intBox = BSTools.SelBox.CalculateBound(m_SelectionBoxes);
		bSIsoData.m_HeadInfo.xSize = intBox.xMax - intBox.xMin + 1;
		bSIsoData.m_HeadInfo.ySize = intBox.yMax - intBox.yMin + 1;
		bSIsoData.m_HeadInfo.zSize = intBox.zMax - intBox.zMin + 1;
		bSIsoData.m_HeadInfo.IconTex = icon_tex;
		foreach (BSTools.SelBox selectionBox in m_SelectionBoxes)
		{
			for (int i = selectionBox.m_Box.xMin; i <= selectionBox.m_Box.xMax; i++)
			{
				for (int j = selectionBox.m_Box.yMin; j <= selectionBox.m_Box.yMax; j++)
				{
					for (int k = selectionBox.m_Box.zMin; k <= selectionBox.m_Box.zMax; k++)
					{
						BSVoxel bSVoxel = dataSource.SafeRead(i, j, k);
						int key = BSIsoData.IPosToKey(i - intBox.xMin, j - intBox.yMin, k - intBox.zMin);
						if (!dataSource.VoxelIsZero(bSVoxel, 1f))
						{
							bSIsoData.m_Voxels.Add(key, bSVoxel);
						}
					}
				}
			}
		}
		if (bSIsoData.m_Voxels.Count == 0)
		{
			return false;
		}
		bSIsoData.CaclCosts();
		string file_path = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath;
		SaveFile(file_path, bSIsoData);
		if (SaveFile(file_path, bSIsoData))
		{
			ClearSelection(m_Action);
			outData = bSIsoData;
			return true;
		}
		return false;
	}

	private bool SaveFile(string file_path, BSIsoData iso)
	{
		if (!Directory.Exists(file_path))
		{
			Directory.CreateDirectory(file_path);
		}
		file_path = file_path + iso.m_HeadInfo.Name + BuildingMan.s_IsoExt;
		try
		{
			using (FileStream output = new FileStream(file_path, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter binaryWriter = new BinaryWriter(output);
				byte[] buffer = iso.Export();
				binaryWriter.Write(buffer);
				binaryWriter.Close();
			}
			Debug.Log("Save building ISO successfully");
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	protected void ExtrudeSelection(int x, int y, int z)
	{
		if ((x == 0 && y == 0 && z == 0) || m_PrevDS != dataSource)
		{
			return;
		}
		_extruding = true;
		Dictionary<IntVector3, byte> dictionary = new Dictionary<IntVector3, byte>();
		List<IntVector3> list = new List<IntVector3>();
		List<BSVoxel> list2 = new List<BSVoxel>();
		List<BSVoxel> list3 = new List<BSVoxel>();
		Dictionary<IntVector3, int> dictionary2 = new Dictionary<IntVector3, int>();
		foreach (KeyValuePair<IntVector3, byte> selection in m_Selections)
		{
			IntVector3 intVector = new IntVector3(selection.Key);
			intVector.x += x;
			intVector.y += y;
			intVector.z += z;
			BSVoxel item = dataSource.SafeRead(selection.Key.x, selection.Key.y, selection.Key.z);
			BSVoxel item2 = dataSource.SafeRead(intVector.x, intVector.y, intVector.z);
			list.Add(intVector);
			list2.Add(item2);
			list3.Add(item);
			dictionary.Add(intVector, selection.Value);
			dictionary2.Add(intVector, 0);
		}
		BSBrush.FindExtraExtendableVoxels(dataSource, list3, list2, list, dictionary2);
		BSVoxelModify bSVoxelModify = new BSVoxelModify(list.ToArray(), list2.ToArray(), list3.ToArray(), dataSource, EBSBrushMode.Add);
		m_Action.AddModify(bSVoxelModify);
		bSVoxelModify.Redo();
		Dictionary<IntVector3, byte> old_value = new Dictionary<IntVector3, byte>(m_Selections);
		BSSelectedBoxModify bSSelectedBoxModify = new BSSelectedBoxModify(old_value, dictionary, this);
		m_Action.AddModify(bSSelectedBoxModify);
		bSSelectedBoxModify.Redo();
	}
}
