using System.Collections.Generic;
using AppearBlendShape;
using CustomCharactor;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
	public GameObject mMode;

	public GameObject mModeRoot;

	public Transform mHeadTran;

	[HideInInspector]
	public AppearData mAppearData;

	[HideInInspector]
	public AvatarData mNude;

	[HideInInspector]
	public AvatarData mClothed;

	private IEnumerable<string> CurrentParts => AvatarData.GetParts(mClothed, mNude);

	private AvatarData CurrentAvatar => mClothed;

	private AvatarData CurrentNudeAvatar => mNude;

	private void Awake()
	{
	}

	public void BuildModel()
	{
		AppearBuilder.Build(mModeRoot, mAppearData, CurrentParts);
	}

	public void ApplyColor()
	{
		AppearBuilder.ApplyColor(mModeRoot, mAppearData);
	}
}
