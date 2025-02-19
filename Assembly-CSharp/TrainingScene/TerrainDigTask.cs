using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace TrainingScene;

public class TerrainDigTask : MonoBehaviour
{
	private static TerrainDigTask s_instance;

	public TerrainDigAppearance appearance;

	private HoloCameraControl hcc;

	private Transform terrain;

	public static TerrainDigTask Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		hcc = HoloCameraControl.Instance;
	}

	private void OnDestroy()
	{
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			PeSingleton<PeCreature>.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
		}
		if (GameUI.Instance != null && GameUI.Instance.mBuildBlock != null)
		{
			GameUI.Instance.mBuildBlock.mMenuCtrl.onMenuBtn -= OnBuildMenuBtn;
		}
	}

	private IEnumerator FindDigTerrain()
	{
		yield return new WaitForSeconds(2f);
		terrain = GameObject.Find("Chnk_0_0_0_0").transform;
		if (terrain != null)
		{
			Debug.Log("找到了");
		}
	}

	public void InitScene()
	{
		CreatTerrian();
		PeSingleton<PeCreature>.Instance.mainPlayer.equipmentCmpt.changeEventor.Subscribe(OnEquip);
	}

	public void InitMenuBtn()
	{
		GameUI.Instance.mBuildBlock.mMenuCtrl.onMenuBtn += OnBuildMenuBtn;
	}

	private void OnBuildMenuBtn()
	{
		TrainingTaskManager.Instance.CompleteMission();
		GameUI.Instance.mBuildBlock.mMenuCtrl.onMenuBtn -= OnBuildMenuBtn;
	}

	private void OnEquip(object sender, EquipmentCmpt.EventArg arg)
	{
		if (arg.isAdd && PEUtil.IsChildItemType(arg.itemObj.protoData.editorTypeId, 5))
		{
			if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
			{
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4078 });
				GameUI.Instance.mNPCTalk.PreShow();
			}
			else
			{
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 4078 }, null, IsClearTalkList: false);
			}
			PeSingleton<PeCreature>.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
		}
	}

	public void DestroyScene()
	{
		DestroyTerrian();
	}

	private void CloseMission()
	{
		DestroyTerrian();
	}

	public void ChangeRenderTarget(MeshRenderer newTarget)
	{
		hcc.renderObjs2.Clear();
		hcc.renderObjs2.Add(newTarget);
	}

	private void CreatTerrian()
	{
		VFVoxel voxel = new VFVoxel(byte.MaxValue, 1);
		VFVoxel voxel2 = new VFVoxel(210, 1);
		VFVoxel voxel3 = new VFVoxel(190, 1);
		VFVoxel voxel4 = new VFVoxel(170, 1);
		VFVoxel voxel5 = new VFVoxel(150, 1);
		VFVoxel voxel6 = new VFVoxel(130, 1);
		VFVoxel voxel7 = new VFVoxel(110, 1);
		for (int i = -2; i <= 15; i++)
		{
			for (int j = 2; j <= 21; j++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(i, 0, j, voxel);
			}
		}
		for (int k = -1; k <= 14; k++)
		{
			for (int l = 3; l <= 20; l++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(k, 1, l, voxel2);
			}
		}
		for (int m = 4; m <= 8; m++)
		{
			for (int n = 8; n <= 15; n++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(m, 2, n, voxel3);
			}
		}
		for (int num = 10; num <= 12; num++)
		{
			VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, num, voxel7);
		}
		VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 13, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 14, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 9, voxel5);
		for (int num2 = 5; num2 <= 6; num2++)
		{
			for (int num3 = 10; num3 <= 12; num3++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(num2, 3, num3, voxel3);
			}
		}
		VFVoxelTerrain.self.Voxels.SafeWrite(5, 3, 13, voxel5);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 13, voxel5);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 9, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 11, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 10, voxel5);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 9, voxel6);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 12, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 13, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 14, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 11, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 10, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 9, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(5, 2, 7, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 7, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 2, 7, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 6, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 11, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 12, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 14, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 13, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 11, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 12, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 10, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 13, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 9, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 14, voxel7);
	}

	private IEnumerator FindTerrian()
	{
		yield return new WaitForSeconds(1f);
		Transform terrain = GameObject.Find("Chnk_0_0_0_0").transform;
		if (terrain != null)
		{
			appearance.orgterrain = terrain;
			terrain.localScale = Vector3.zero;
			terrain.gameObject.SetActive(value: false);
		}
		appearance.gameObject.SetActive(value: true);
		appearance.orgterrain.gameObject.SetActive(value: true);
		appearance.orgterrain.GetComponent<MeshCollider>().GetComponent<Collider>().enabled = true;
		hcc.renderObjs2.Add(appearance.orgterrain.GetComponent<MeshRenderer>());
		appearance.produce = true;
	}

	private void DestroyTerrian()
	{
		VFVoxel voxel = new VFVoxel(0, 0);
		VFVoxel voxel2 = new VFVoxel(0, 0);
		VFVoxel voxel3 = new VFVoxel(0, 0);
		VFVoxel voxel4 = new VFVoxel(0, 0);
		VFVoxel voxel5 = new VFVoxel(0, 0);
		VFVoxel voxel6 = new VFVoxel(0, 0);
		VFVoxel voxel7 = new VFVoxel(0, 0);
		for (int i = -2; i <= 15; i++)
		{
			for (int j = 2; j <= 21; j++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(i, 0, j, voxel);
			}
		}
		for (int k = -1; k <= 14; k++)
		{
			for (int l = 3; l <= 20; l++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(k, 1, l, voxel2);
			}
		}
		for (int m = 4; m <= 8; m++)
		{
			for (int n = 8; n <= 15; n++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(m, 2, n, voxel3);
			}
		}
		for (int num = 10; num <= 12; num++)
		{
			VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, num, voxel7);
		}
		VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 13, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 14, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(9, 2, 9, voxel5);
		for (int num2 = 5; num2 <= 6; num2++)
		{
			for (int num3 = 10; num3 <= 12; num3++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(num2, 3, num3, voxel3);
			}
		}
		VFVoxelTerrain.self.Voxels.SafeWrite(5, 3, 13, voxel5);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 13, voxel5);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 3, 9, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 11, voxel4);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 10, voxel5);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 9, voxel6);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 12, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 13, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(3, 2, 14, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 11, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 10, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(2, 2, 9, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(5, 2, 7, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 7, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 2, 7, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(6, 2, 6, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 11, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 12, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 14, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(4, 3, 13, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 11, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 12, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 10, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 13, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 9, voxel7);
		VFVoxelTerrain.self.Voxels.SafeWrite(7, 3, 14, voxel7);
	}
}
