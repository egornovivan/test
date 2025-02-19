using UnityEngine;

public class BuildGui_N : UIStaticWnd
{
	private const int NumPerPage = 22;

	public static BuildGui_N mInstance;

	public GameObject mOperationWnd;

	public UISlicedSprite mBgSpr;

	private int mCurrentBrushID = 1;

	private int mCurrentItemId = 30200001;

	private int mMatPage;

	private int mBrushPage;

	private void Awake()
	{
		mInstance = this;
	}

	private void Update()
	{
	}

	private void OnOpWndChange()
	{
		mOperationWnd.SetActive(!mOperationWnd.activeSelf);
	}

	private void OnOpButtonClick()
	{
	}
}
