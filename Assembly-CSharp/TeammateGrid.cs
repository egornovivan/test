using Pathea;
using UnityEngine;

public class TeammateGrid : MonoBehaviour
{
	[SerializeField]
	private UILabel mName;

	[SerializeField]
	private UISlider mUISlider;

	[HideInInspector]
	public PlayerNetwork mPlayer;

	private Vector3 mPos;

	private float maxHpTemp;

	public void SetInfo(PlayerNetwork _player)
	{
		mPlayer = _player;
		mName.text = _player.RoleName;
	}

	private void Update()
	{
		if (mPlayer != null && mPlayer.PlayerEntity != null)
		{
			maxHpTemp = mPlayer.PlayerEntity.GetAttribute(AttribType.HpMax);
			mUISlider.sliderValue = ((!(maxHpTemp > 0f)) ? 0f : (mPlayer.PlayerEntity.GetAttribute(AttribType.Hp) / maxHpTemp));
			mPos = mPlayer.PlayerEntity.position;
		}
	}

	private void OnTooltip(bool show)
	{
		if (!show)
		{
			ToolTipsMgr.ShowText(null);
			return;
		}
		string empty = string.Empty;
		empty = "Name: " + mName.text + "\r\nPosition: " + mPos.ToString();
		ToolTipsMgr.ShowText(empty);
	}
}
