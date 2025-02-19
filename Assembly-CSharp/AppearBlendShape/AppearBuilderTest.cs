using System;
using System.Collections.Generic;
using CustomCharactor;
using UnityEngine;

namespace AppearBlendShape;

public class AppearBuilderTest : MonoBehaviour
{
	[Serializable]
	private class ModelInfo
	{
		[SerializeField]
		public GameObject m_boneRoot;

		public AvatarData clothed;

		public AvatarData nude;
	}

	[SerializeField]
	private ModelInfo female;

	[SerializeField]
	private ModelInfo male;

	private ModelInfo current;

	private AppearData mAppearData;

	private IEnumerable<string> CurrentParts => AvatarData.GetParts(current.clothed, current.nude);

	private AvatarData CurrentAvatar => current.clothed;

	private AvatarData CurrentNudeAvatar => current.nude;

	private ESex Sex { get; set; }

	private void Awake()
	{
		CustomMetaData.LoadData();
		female.m_boneRoot.transform.parent.gameObject.SetActive(value: false);
		male.m_boneRoot.transform.parent.gameObject.SetActive(value: false);
		SetMale();
		SetAppearData(new AppearData());
		female.clothed = new AvatarData();
		female.nude = new AvatarData();
		female.nude.SetFemaleBody();
		male.clothed = new AvatarData();
		male.nude = new AvatarData();
		male.nude.SetMaleBody();
	}

	private void SetAppearData(AppearData data)
	{
		mAppearData = data;
	}

	private void ChangeCurrent(ModelInfo cur)
	{
		if (current != null)
		{
			current.m_boneRoot.transform.parent.gameObject.SetActive(value: false);
		}
		current = cur;
		if (current != null)
		{
			current.m_boneRoot.transform.parent.gameObject.SetActive(value: true);
		}
	}

	private void SetMale()
	{
		Sex = ESex.Male;
		ChangeCurrent(male);
	}

	private void SetFemale()
	{
		Sex = ESex.Female;
		ChangeCurrent(female);
	}

	private void ToggleSex()
	{
		if (Sex == ESex.Male)
		{
			SetFemale();
		}
		else
		{
			SetMale();
		}
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, 0f, 500f, 500f));
		GUILayout.BeginVertical();
		DrawHeader();
		DrawHead();
		DrawCloth();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(1000f, 0f, 300f, 800f));
		DrawTwist();
		GUILayout.EndArea();
	}

	private void DrawCloth()
	{
		GUILayout.BeginVertical();
		for (int i = 1; i <= 10; i++)
		{
			GUILayout.BeginHorizontal();
			if (Sex == ESex.Female)
			{
				if (GUILayout.Button("helmet" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.HairF, $"Model/PlayerModel/female{i:D2}-Helmet");
				}
				if (GUILayout.Button("torso" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Torso, $"Model/PlayerModel/female{i:D2}-torso");
				}
				if (GUILayout.Button("legs" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Legs, $"Model/PlayerModel/female{i:D2}-legs");
				}
				if (GUILayout.Button("hands" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Hands, $"Model/PlayerModel/female{i:D2}-hands");
				}
				if (GUILayout.Button("feet" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Feet, $"Model/PlayerModel/female{i:D2}-feet");
				}
			}
			else
			{
				if (GUILayout.Button("helmet" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.HairF, $"Model/PlayerModel/male{i:D2}-Helmet");
				}
				if (GUILayout.Button("torso" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Torso, $"Model/PlayerModel/male{i:D2}-torso");
				}
				if (GUILayout.Button("legs" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Legs, $"Model/PlayerModel/male{i:D2}-legs");
				}
				if (GUILayout.Button("hands" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Hands, $"Model/PlayerModel/male{i:D2}-hands");
				}
				if (GUILayout.Button("feet" + i))
				{
					CurrentAvatar.SetPart(AvatarData.ESlot.Feet, $"Model/PlayerModel/male{i:D2}-feet");
				}
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	private void DrawHead()
	{
		GUILayout.BeginHorizontal();
		CustomMetaData customMetaData = ((Sex != 0) ? CustomMetaData.InstanceFemale : CustomMetaData.InstanceMale);
		for (int i = 0; i < customMetaData.GetHeadCount(); i++)
		{
			if (GUILayout.Button("head" + (i + 1)))
			{
				CurrentNudeAvatar.SetPart(AvatarData.ESlot.Head, customMetaData.GetHead(i).modelPath);
				Apply();
			}
		}
		GUILayout.EndHorizontal();
	}

	private void DrawHeader()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(Sex.ToString()))
		{
			ToggleSex();
		}
		GUILayout.EndHorizontal();
	}

	private void DrawTwist()
	{
		if (mAppearData == null)
		{
			return;
		}
		GUILayout.BeginVertical();
		for (int i = 0; i < 27; i++)
		{
			if (i != 21 && i != 22 && i != 26 && i != 25 && i != 20)
			{
				GUILayout.BeginHorizontal();
				EMorphItem eMorphItem = (EMorphItem)i;
				GUILayout.Label(eMorphItem.ToString());
				float weight = mAppearData.GetWeight(eMorphItem);
				float num = GUILayout.HorizontalSlider(weight, -1f, 1f);
				if (!Mathf.Approximately(weight, num))
				{
					mAppearData.SetWeight(eMorphItem, num);
					Apply();
				}
				GUILayout.EndHorizontal();
			}
		}
		DrawBuild();
		GUILayout.EndVertical();
	}

	private void DrawBuild()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("build"))
		{
			Apply();
		}
		if (GUILayout.Button("random"))
		{
			mAppearData.RandomMorphWeight();
			Apply();
		}
		GUILayout.EndHorizontal();
	}

	private void Apply()
	{
		AppearBuilder.Build(current.m_boneRoot, mAppearData, CurrentParts);
	}
}
