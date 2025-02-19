using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TownEditGui_N : UIStaticWnd
{
	private static TownEditGui_N mInstance;

	public DragableBuildingFile_N mFilePerfab;

	public UIGrid mFileGrid;

	private List<DragableBuildingFile_N> mBuildFileList = new List<DragableBuildingFile_N>();

	private DragableBuildingFile_N mCurrentFile;

	private EditBuilding mOpBuilding;

	private Vector3 mPressMousePos = Vector3.zero;

	public static TownEditGui_N Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	public void ResetFileList()
	{
		List<string> list = new List<string>();
		foreach (int key in BlockBuilding.s_tblBlockBuildingMap.Keys)
		{
			list.Add(Path.GetFileNameWithoutExtension(BlockBuilding.s_tblBlockBuildingMap[key].mPath));
		}
		foreach (DragableBuildingFile_N mBuildFile in mBuildFileList)
		{
			if ((bool)mBuildFile)
			{
				Object.Destroy(mBuildFile.gameObject);
			}
		}
		mBuildFileList.Clear();
		foreach (string item in list)
		{
			DragableBuildingFile_N dragableBuildingFile_N = Object.Instantiate(mFilePerfab);
			dragableBuildingFile_N.SetFile(item, base.gameObject);
			dragableBuildingFile_N.transform.parent = mFileGrid.transform;
			dragableBuildingFile_N.transform.localScale = Vector3.one;
			dragableBuildingFile_N.transform.localRotation = Quaternion.identity;
			mBuildFileList.Add(dragableBuildingFile_N);
		}
		mFileGrid.Reposition();
	}

	private void OnFileDrag(DragableBuildingFile_N dragFile)
	{
		if (null == mOpBuilding && mCurrentFile != dragFile)
		{
			mCurrentFile = dragFile;
			TownEditor.Instance.OnCreateBuilding(mCurrentFile.FileName);
		}
	}

	public void SetOpBuild(EditBuilding editBuilding)
	{
		mOpBuilding = editBuilding;
		mCurrentFile = null;
	}

	private void Update()
	{
		if (!(null != mOpBuilding))
		{
			return;
		}
		if (Input.GetMouseButtonUp(0))
		{
			TownEditor.Instance.PutBuildingDown();
		}
		else if (Input.GetKeyUp(KeyCode.T))
		{
			TownEditor.Instance.TurnBuilding();
		}
		else if (Input.GetKeyUp(KeyCode.Escape))
		{
			TownEditor.Instance.CancelSelect();
		}
		else if (Input.GetKeyUp(KeyCode.Delete))
		{
			TownEditor.Instance.DeletBuilding();
		}
		else
		{
			if (!(null == UICamera.hoveredObject))
			{
				return;
			}
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float num = 10000f;
			RaycastHit[] array = Physics.RaycastAll(ray, 500f, 4096);
			if (array.Length <= 0)
			{
				return;
			}
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if (raycastHit.distance < num && !raycastHit.transform.name.Contains("B45Chnk"))
				{
					num = raycastHit.distance;
					TownEditor.Instance.SetOpBuildingPosition(BuildBlockManager.BestMatchPosition(raycastHit.point));
				}
			}
		}
	}
}
