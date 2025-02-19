using UnityEngine;

public class UIListHeaderItem : MonoBehaviour
{
	public delegate void e_OnClickSort(int index, int sortState);

	public GameObject mSplite;

	public UILabel mText;

	public int mIndex;

	public BoxCollider mBoxCollider;

	public int mBoxCliderHeight;

	public int mSortState = -1;

	public GameObject mSort;

	public GameObject mSortUp;

	public GameObject mSortDn;

	public GameObject mSortDefault;

	public bool mCanSort;

	public event e_OnClickSort eSortOnClick;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(string text, float pos_x, int width, int index)
	{
		mText.text = text;
		base.gameObject.transform.localPosition = new Vector3(pos_x, 0f, 0f);
		mSplite.transform.localPosition = new Vector3(width - 2, 0f, 0f);
		mIndex = index;
		if (mBoxCollider != null)
		{
			mBoxCollider.size = new Vector3(width - 2, mBoxCliderHeight, -2f);
			mBoxCollider.center = new Vector3(width / 2 - 1, 0f, 0f);
			mBoxCollider.enabled = false;
		}
		if (mSort != null)
		{
			mSort.transform.localPosition = new Vector3(width - 10, 0f, 0f);
		}
	}

	public void InitSort(bool CanSort)
	{
		mSort.SetActive(CanSort);
		mBoxCollider.enabled = CanSort;
		mCanSort = CanSort;
	}

	public void SetSortSatate(int sortState)
	{
		if (!mCanSort)
		{
			mSort.SetActive(value: false);
			return;
		}
		switch (sortState)
		{
		case 0:
			mSortUp.SetActive(value: false);
			mSortDn.SetActive(value: false);
			mSortDefault.SetActive(value: true);
			break;
		case 1:
			mSortUp.SetActive(value: true);
			mSortDn.SetActive(value: false);
			mSortDefault.SetActive(value: false);
			break;
		case 2:
			mSortUp.SetActive(value: false);
			mSortDn.SetActive(value: true);
			mSortDefault.SetActive(value: false);
			break;
		default:
			mSortState = -1;
			return;
		}
		mSortState = sortState;
	}

	private void OnClickSort()
	{
		if (!mCanSort)
		{
			mSort.SetActive(value: false);
		}
		else if (this.eSortOnClick != null)
		{
			this.eSortOnClick(mIndex, mSortState);
		}
	}
}
