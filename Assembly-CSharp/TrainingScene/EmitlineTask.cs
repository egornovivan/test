using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace TrainingScene;

public class EmitlineTask : MonoBehaviour
{
	private static EmitlineTask s_instance;

	public EmitreceiverAppear[] receivers;

	[SerializeField]
	private GameObject[] recieverBase;

	[SerializeField]
	private Transform lineGroup;

	[SerializeField]
	private GameObject receiverGroup;

	[HideInInspector]
	public int testScore;

	[HideInInspector]
	public bool missionComplete;

	[HideInInspector]
	public bool isBuildLocked = true;

	private List<int[]> recordVoxelRemove;

	private GameObject isoCube1;

	private GameObject isoCube2;

	public static EmitlineTask Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void Update()
	{
		testScore = 0;
	}

	private void LateUpdate()
	{
		if (testScore > 0 && !missionComplete)
		{
			TrainingTaskManager.Instance.CompleteMission();
			missionComplete = true;
		}
	}

	private void OnDestroy()
	{
		if (MainPlayerCmpt.gMainPlayer != null)
		{
			MainPlayerCmpt.gMainPlayer.onBuildMode -= OnInBuildMode;
		}
	}

	public void InitInBuildScene()
	{
		MainPlayerCmpt.gMainPlayer.onBuildMode += OnInBuildMode;
	}

	private void OnInBuildMode(bool inorout)
	{
		if (inorout)
		{
			MainPlayerCmpt.gMainPlayer.onBuildMode -= OnInBuildMode;
			TrainingTaskManager.Instance.CompleteMission();
		}
	}

	public void InitScene()
	{
		recordVoxelRemove = new List<int[]>();
		BSBlock45Data.voxelWrite = (Action<int[]>)Delegate.Combine(BSBlock45Data.voxelWrite, new Action<int[]>(AddRecordVoxel));
		CreatTerrian();
		lineGroup.gameObject.SetActive(value: true);
		receiverGroup.SetActive(value: true);
		GameObject[] array = recieverBase;
		foreach (GameObject item in array)
		{
			HoloCameraControl.Instance.renderObjs1.Add(item);
		}
		EmitreceiverAppear[] array2 = receivers;
		foreach (EmitreceiverAppear emitreceiverAppear in array2)
		{
			emitreceiverAppear.produce = true;
		}
		EmitlineAppearance.Instance.StartFadeLine(0.3f, prod: true);
		isBuildLocked = false;
		missionComplete = false;
	}

	private void AddRecordVoxel(int[] tmp)
	{
		recordVoxelRemove.Add(tmp);
	}

	public void DestroyScene()
	{
		EmitreceiverAppear[] array = receivers;
		foreach (EmitreceiverAppear emitreceiverAppear in array)
		{
			emitreceiverAppear.destroy = true;
		}
		EmitlineAppearance.Instance.StartFadeLine(0.3f, prod: false);
		Invoke("CloseMission", receivers[0].fadeTime + 0.2f);
	}

	public void CloseMission_buildPoint()
	{
		HoloCameraControl.Instance.renderObjs1.Clear();
		lineGroup.gameObject.SetActive(value: false);
	}

	public void CreateExportIsoCube()
	{
		isoCube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		isoCube1.name = "IsoCube";
		isoCube1.transform.position = new Vector3(18.5f, 3f, 11.5f);
		isoCube1.transform.localScale = new Vector3(4f, 4f, 4f);
		isoCube1.GetComponent<Renderer>().material = Resources.Load<Material>("Material/BlueGizmoMat");
		UnityEngine.Object.Destroy(isoCube1.GetComponent<Collider>());
		isoCube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		isoCube2.name = "IsoCube";
		isoCube2.transform.position = new Vector3(11.5f, 3.5f, 10.5f);
		isoCube2.transform.localScale = new Vector3(4f, 4f, 4f);
		isoCube2.GetComponent<Renderer>().material = Resources.Load<Material>("Material/BlueGizmoMat");
		UnityEngine.Object.Destroy(isoCube2.GetComponent<Collider>());
	}

	public void CloseMission_buildCreateISO()
	{
		UnityEngine.Object.Destroy(isoCube1);
		UnityEngine.Object.Destroy(isoCube2);
		BSBlock45Data.voxelWrite = (Action<int[]>)Delegate.Remove(BSBlock45Data.voxelWrite, new Action<int[]>(AddRecordVoxel));
		BSVoxel voxel = new BSVoxel(0, 0);
		for (int i = 0; i < recordVoxelRemove.Count; i++)
		{
			int[] array = recordVoxelRemove[i];
			BuildingMan.Blocks.SafeWrite(voxel, array[0], array[1], array[2]);
		}
		MotionMgrCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
		if (cmpt != null)
		{
			cmpt.EndAction(PEActionType.Build);
		}
		receiverGroup.SetActive(value: false);
		DestroyTerrian();
	}

	private void CreatTerrian()
	{
		VFVoxel voxel = new VFVoxel(byte.MaxValue, 1);
		for (int i = 17; i <= 20; i++)
		{
			for (int j = 10; j <= 13; j++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(i, 1, j, voxel);
			}
		}
		for (int k = 10; k <= 13; k++)
		{
			for (int l = 2; l <= 5; l++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(k, l, 9, voxel);
			}
		}
	}

	private void DestroyTerrian()
	{
		VFVoxel voxel = new VFVoxel(0, 0);
		for (int i = 17; i <= 20; i++)
		{
			for (int j = 10; j <= 13; j++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(i, 1, j, voxel);
			}
		}
		for (int k = 10; k <= 13; k++)
		{
			for (int l = 2; l <= 5; l++)
			{
				VFVoxelTerrain.self.Voxels.SafeWrite(k, l, 9, voxel);
			}
		}
	}
}
