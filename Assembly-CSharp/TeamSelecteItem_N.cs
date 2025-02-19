using UnityEngine;

public class TeamSelecteItem_N : MonoBehaviour
{
	public int TeamId = -1;

	public UISlicedSprite mSprite;

	public UILabel mLabel;

	private void Awake()
	{
		mSprite = base.gameObject.GetComponentInChildren<UISlicedSprite>();
		mLabel = base.gameObject.GetComponentInChildren<UILabel>();
	}

	private void OnClick()
	{
		if (Input.GetMouseButtonUp(0))
		{
			RoomGui_N.Instance.ChangePlayerTeamToNet(TeamId, -1);
		}
	}
}
