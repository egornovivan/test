using UnityEngine;

public class CSUI_CampItem : MonoBehaviour
{
	public enum Typecamp
	{
		Earth,
		Mars
	}

	public delegate void CampChose(object sender);

	[SerializeField]
	private UILabel mCampNameLb;

	[SerializeField]
	private GameObject ChoseBg;

	private Typecamp mCamp;

	public Typecamp Camp
	{
		get
		{
			return mCamp;
		}
		set
		{
			mCamp = value;
		}
	}

	public event CampChose e_ItemOnClick;

	private void Awake()
	{
		ChoseBg.SetActive(value: false);
	}

	private void Start()
	{
	}

	public void SetCampName(string Name)
	{
		if (Name.Length < 15)
		{
			mCampNameLb.text = Name;
		}
	}

	private void OnTooltip(bool show)
	{
	}

	public string GetCampName()
	{
		return mCampNameLb.text;
	}

	public void SetChoeBg(bool Show)
	{
		ChoseBg.SetActive(Show);
	}

	private void OnCampChose()
	{
		if (this.e_ItemOnClick != null)
		{
			this.e_ItemOnClick(this);
		}
	}

	private void Update()
	{
	}
}
