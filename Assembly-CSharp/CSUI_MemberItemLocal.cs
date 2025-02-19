using UnityEngine;

public class CSUI_MemberItemLocal : MonoBehaviour
{
	[SerializeField]
	private UISprite iconSpr;

	private MyMemberInf mMemberInfo;

	public MyMemberInf MemberInfoLocal
	{
		get
		{
			return mMemberInfo;
		}
		set
		{
			mMemberInfo = value;
			InitLocalInfo();
		}
	}

	private void InitLocalInfo()
	{
		if (mMemberInfo != null)
		{
			SetIcon(mMemberInfo.memberName);
		}
	}

	private void SetIcon(string s)
	{
		if (!(iconSpr == null))
		{
			iconSpr.spriteName = s;
			iconSpr.MakePixelPerfect();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
