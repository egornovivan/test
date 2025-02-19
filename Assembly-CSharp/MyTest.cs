using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class MyTest : MonoBehaviour
{
	public Texture[] linshiyong;

	private List<CSUIMyNpc> allRandomNpcs = new List<CSUIMyNpc>();

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			Debug.Log("创建数据开始");
			CSUI_Train.Instance.GetAllRandomNpcsEvent += SendAllRandomNpcs;
			CreatRandomNpcs(10);
		}
	}

	private void SendAllRandomNpcs()
	{
		if (allRandomNpcs.Count > 0)
		{
			CSUI_Train.Instance.GetAllRandomNpcsMethod(allRandomNpcs);
		}
	}

	private void CreatRandomNpcs(int numbers)
	{
		if (numbers <= 5)
		{
			return;
		}
		List<CSUIMySkill> list = new List<CSUIMySkill>();
		CSUIMySkill cSUIMySkill = new CSUIMySkill();
		cSUIMySkill.name = "skill1";
		cSUIMySkill.iconImg = "npc_big_GerdyHooke";
		list.Add(cSUIMySkill);
		CSUIMySkill cSUIMySkill2 = new CSUIMySkill();
		cSUIMySkill2.name = "skill2";
		cSUIMySkill2.iconImg = "npc_big_AgnesCopperfield";
		list.Add(cSUIMySkill2);
		CSUIMySkill cSUIMySkill3 = new CSUIMySkill();
		cSUIMySkill3.name = "skill3";
		cSUIMySkill3.iconImg = "npc_big_BenYin";
		list.Add(cSUIMySkill3);
		CSUIMySkill cSUIMySkill4 = new CSUIMySkill();
		cSUIMySkill4.name = "skill4";
		cSUIMySkill4.iconImg = "npc_big_AvaAzniv";
		list.Add(cSUIMySkill4);
		for (int i = 0; i < numbers - 6; i++)
		{
			CSUIMyNpc cSUIMyNpc = new CSUIMyNpc();
			cSUIMyNpc.OwnSkills = list;
			cSUIMyNpc.IsRandom = true;
			cSUIMyNpc.HasOccupation = false;
			cSUIMyNpc.Health = 90;
			cSUIMyNpc.HealthMax = 100;
			cSUIMyNpc.Name = "xueyuan";
			cSUIMyNpc.RandomNpcFace = linshiyong[i];
			cSUIMyNpc.Sex = PeSex.Female;
			if (i == 3)
			{
				cSUIMyNpc.State = 10;
			}
			allRandomNpcs.Add(cSUIMyNpc);
		}
		for (int j = 4; j < numbers; j++)
		{
			CSUIMyNpc cSUIMyNpc2 = new CSUIMyNpc();
			cSUIMyNpc2.OwnSkills = list;
			cSUIMyNpc2.IsRandom = true;
			cSUIMyNpc2.HasOccupation = true;
			cSUIMyNpc2.AddHealth = "6~10";
			cSUIMyNpc2.AddHunger = "3~5";
			cSUIMyNpc2.Name = "jiaolian";
			cSUIMyNpc2.RandomNpcFace = linshiyong[j];
			cSUIMyNpc2.Sex = PeSex.Male;
			allRandomNpcs.Add(cSUIMyNpc2);
		}
		Debug.Log("生成的allRandomNpcs：" + allRandomNpcs.Count);
	}
}
