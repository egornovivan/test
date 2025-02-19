using System;
using System.Collections.Generic;
using System.IO;
using Pathea.Operate;
using UnityEngine;
using WhiteCat;

public static class CreationMgr
{
	public const int VERSION = 8193;

	private static Dictionary<int, CreationData> m_Creations;

	private static CreationData lastCreationData;

	private static GameObject lastProduct;

	public static void Init()
	{
		Clear();
		m_Creations = new Dictionary<int, CreationData>();
	}

	public static void Clear()
	{
		if (m_Creations == null)
		{
			return;
		}
		foreach (KeyValuePair<int, CreationData> creation in m_Creations)
		{
			if (creation.Value != null)
			{
				creation.Value.Destroy();
			}
		}
		m_Creations.Clear();
	}

	public static int QueryNewId()
	{
		int num = 100000000;
		if (m_Creations.Count == 0)
		{
			return 100000001;
		}
		foreach (KeyValuePair<int, CreationData> creation in m_Creations)
		{
			if (creation.Value.m_ObjectID > num)
			{
				num = creation.Value.m_ObjectID;
			}
		}
		return num + 1;
	}

	public static void AddCreation(CreationData creation_data)
	{
		if (m_Creations == null)
		{
			Debug.LogError("CreationMgr haven't initialized!");
		}
		else if (m_Creations.ContainsKey(creation_data.m_ObjectID))
		{
			Debug.LogWarning("You want to add a creation instance, but it already exist, the old creation_data has been replaced!");
			m_Creations[creation_data.m_ObjectID] = creation_data;
		}
		else
		{
			m_Creations.Add(creation_data.m_ObjectID, creation_data);
		}
	}

	public static CreationData NewCreation(int object_id, ulong res_hash, float rand_seed)
	{
		CreationData creationData = new CreationData();
		creationData.m_ObjectID = object_id;
		creationData.m_HashCode = res_hash;
		creationData.m_RandomSeed = rand_seed;
		if (creationData.LoadRes())
		{
			creationData.GenCreationAttr();
			creationData.BuildPrefab();
			creationData.Register();
			AddCreation(creationData);
			Debug.Log("new creation succeed !");
			creationData.UpdateUseTime();
			return creationData;
		}
		Debug.LogError("creation generate failed.");
		return null;
	}

	public static void RemoveCreation(int id)
	{
		if (m_Creations == null)
		{
			Debug.LogError("CreationMgr haven't initialized!");
		}
		else if (m_Creations.ContainsKey(id))
		{
			m_Creations[id]?.Destroy();
			m_Creations.Remove(id);
		}
		else
		{
			Debug.LogError("You want to remove a creation instance, but it doesn't exist!");
		}
	}

	public static CreationData GetCreation(int id)
	{
		if (m_Creations == null)
		{
			Debug.LogError("CreationMgr haven't initialized!");
			return null;
		}
		if (m_Creations.ContainsKey(id))
		{
			m_Creations[id].UpdateUseTime();
			return m_Creations[id];
		}
		if (GameConfig.IsMultiMode)
		{
			return SteamWorkShop.GetCreation(id);
		}
		return null;
	}

	public static void Import(byte[] buffer)
	{
		if (buffer == null || buffer.Length < 8)
		{
			return;
		}
		Init();
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		if (num != 8193)
		{
			Debug.LogWarning("The version of CreationMgr is newer than the record.");
		}
		int num2 = num;
		if (num2 == 8193)
		{
			int num3 = binaryReader.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				CreationData creationData = new CreationData();
				creationData.m_ObjectID = binaryReader.ReadInt32();
				creationData.m_HashCode = binaryReader.ReadUInt64();
				creationData.m_RandomSeed = binaryReader.ReadSingle();
				if (creationData.LoadRes())
				{
					creationData.GenCreationAttr();
					creationData.BuildPrefab();
					creationData.Register();
					m_Creations.Add(creationData.m_ObjectID, creationData);
				}
				else
				{
					creationData.Destroy();
					creationData = null;
				}
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public static void Export(BinaryWriter w)
	{
		if (m_Creations == null)
		{
			Debug.LogError("CreationMgr haven't initialized!");
			return;
		}
		w.Write(8193);
		w.Write(m_Creations.Count);
		foreach (KeyValuePair<int, CreationData> creation in m_Creations)
		{
			w.Write(creation.Value.m_ObjectID);
			w.Write(creation.Value.m_HashCode);
			w.Write(creation.Value.m_RandomSeed);
		}
	}

	private static GameObject InstantiateCreationModel(CreationData crd, Action<Transform> initTransform)
	{
		if (crd == null)
		{
			return null;
		}
		if (crd.m_Prefab == null)
		{
			return null;
		}
		GameObject gameObject = null;
		if ((bool)lastProduct)
		{
			ItemDraggingBase component = lastProduct.GetComponent<ItemDraggingBase>();
			if ((bool)component)
			{
				if (lastCreationData == crd)
				{
					UnityEngine.Object.Destroy(component);
					gameObject = lastProduct;
				}
				else
				{
					UnityEngine.Object.Destroy(lastProduct);
				}
			}
			lastProduct = null;
			lastCreationData = null;
		}
		if (!gameObject)
		{
			gameObject = (lastProduct = UnityEngine.Object.Instantiate(crd.m_Prefab));
			lastCreationData = crd;
		}
		gameObject.transform.SetParent(null, worldPositionStays: false);
		gameObject.SetActive(value: true);
		if (initTransform != null)
		{
			initTransform(gameObject.transform);
		}
		else
		{
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.rotation = Quaternion.identity;
		}
		if ((bool)lastProduct)
		{
			CreationController component2 = gameObject.GetComponent<CreationController>();
			component2.enabled = true;
			if (crd.m_IsoData.m_HeadInfo.Category == EVCCategory.cgDbSword)
			{
				VCMeshMgr[] componentsInChildren = gameObject.GetComponentsInChildren<VCMeshMgr>();
				CreationMeshLoader[] array = new CreationMeshLoader[2];
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].m_ColorMap = crd.m_IsoData.m_Colors;
					componentsInChildren[i].Init();
					array[i] = componentsInChildren[i].gameObject.AddComponent<CreationMeshLoader>();
					array[i].m_Meshdagger = crd.m_IsoData.m_HeadInfo.Category == EVCCategory.cgDbSword;
					array[i].Init(component2);
				}
			}
			else
			{
				VCMeshMgr componentInChildren = gameObject.GetComponentInChildren<VCMeshMgr>();
				componentInChildren.m_ColorMap = crd.m_IsoData.m_Colors;
				componentInChildren.Init();
				CreationMeshLoader creationMeshLoader = componentInChildren.gameObject.AddComponent<CreationMeshLoader>();
				creationMeshLoader.Init(component2);
			}
			VCDecalHandler[] componentsInChildren2 = gameObject.GetComponentsInChildren<VCDecalHandler>(includeInactive: true);
			VCDecalHandler[] array2 = componentsInChildren2;
			foreach (VCDecalHandler vCDecalHandler in array2)
			{
				vCDecalHandler.m_Iso = crd.m_IsoData;
			}
		}
		return gameObject;
	}

	public static GameObject InstantiateCreation(int objectid, int itemInstanceId, bool init, Action<Transform> initTransform)
	{
		CreationData creation = GetCreation(objectid);
		GameObject gameObject = null;
		if (creation == null)
		{
			Debug.LogError("Can not find CreationData");
			return null;
		}
		if (creation.m_Attribute.m_Type == ECreation.Vehicle)
		{
			gameObject = InstantiateCarrier<VehicleController>(itemInstanceId, init, creation, initTransform);
		}
		else if (creation.m_Attribute.m_Type == ECreation.Aircraft)
		{
			gameObject = InstantiateCarrier<HelicopterController>(itemInstanceId, init, creation, initTransform);
		}
		else if (creation.m_Attribute.m_Type == ECreation.Boat)
		{
			gameObject = InstantiateCarrier<BoatController>(itemInstanceId, init, creation, initTransform);
		}
		else if (creation.m_Attribute.m_Type == ECreation.SimpleObject)
		{
			gameObject = InstantiateSimpleObject(itemInstanceId, init, creation, initTransform);
		}
		else if (creation.m_Attribute.m_Type == ECreation.Robot)
		{
			gameObject = InstantiateRobot(itemInstanceId, init, creation, initTransform);
		}
		else if (creation.m_Attribute.m_Type == ECreation.AITurret)
		{
			gameObject = InstantiateAITurret(itemInstanceId, init, creation, initTransform);
		}
		else if (creation.m_Attribute.m_Type == ECreation.ArmorArmAndLeg || creation.m_Attribute.m_Type == ECreation.ArmorBody || creation.m_Attribute.m_Type == ECreation.ArmorDecoration || creation.m_Attribute.m_Type == ECreation.ArmorHandAndFoot || creation.m_Attribute.m_Type == ECreation.ArmorHead)
		{
			gameObject = InstantiateArmor(itemInstanceId, init, creation, initTransform);
		}
		else if (creation.m_Attribute.m_Type == ECreation.HandGun || creation.m_Attribute.m_Type == ECreation.Rifle || creation.m_Attribute.m_Type == ECreation.Shield || creation.m_Attribute.m_Type == ECreation.Sword || creation.m_Attribute.m_Type == ECreation.SwordLarge || creation.m_Attribute.m_Type == ECreation.SwordDouble || creation.m_Attribute.m_Type == ECreation.Axe || creation.m_Attribute.m_Type == ECreation.Bow)
		{
			if (init)
			{
				PEGun component = creation.m_Prefab.GetComponent<PEGun>();
				if (!component || component.m_AmmoType != AmmoType.Energy)
				{
					return null;
				}
				gameObject = new GameObject("Shit");
				PEEnergyGunLogic pEEnergyGunLogic = gameObject.AddComponent<PEEnergyGunLogic>();
				pEEnergyGunLogic.m_Magazine = new Magazine(component.m_Magazine);
				pEEnergyGunLogic.m_RechargeDelay = component.m_RechargeDelay;
				pEEnergyGunLogic.m_RechargeEnergySpeed = component.m_RechargeEnergySpeed;
			}
			else
			{
				gameObject = InstantiateCreationModel(creation, initTransform);
			}
		}
		return gameObject;
	}

	private static GameObject InstantiateCarrier<T>(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform) where T : CarrierController
	{
		GameObject gameObject = InstantiateCreationModel(crd, initTransform);
		if (gameObject == null)
		{
			return null;
		}
		if (init)
		{
			T val = gameObject.AddComponent<T>();
			val.InitController(itemInstanceId);
			CreationController component = gameObject.GetComponent<CreationController>();
			bool collidable = (component.visible = false);
			component.collidable = collidable;
		}
		else
		{
			gameObject.AddComponent<ItemDraggingCreation>();
		}
		return gameObject;
	}

	private static GameObject InstantiateRobot(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject gameObject = InstantiateCreationModel(crd, initTransform);
		if (gameObject == null)
		{
			return null;
		}
		if (init)
		{
			RobotController robotController = gameObject.AddComponent<RobotController>();
			robotController.InitController(itemInstanceId);
			CreationController component = gameObject.GetComponent<CreationController>();
			bool collidable = (component.visible = false);
			component.collidable = collidable;
		}
		else
		{
			gameObject.AddComponent<ItemDraggingCreation>();
		}
		return gameObject;
	}

	private static GameObject InstantiateAITurret(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject gameObject = InstantiateCreationModel(crd, initTransform);
		if (gameObject == null)
		{
			return null;
		}
		if (init)
		{
			AITurretController aITurretController = gameObject.AddComponent<AITurretController>();
			aITurretController.InitController(itemInstanceId);
			CreationController component = gameObject.GetComponent<CreationController>();
			bool collidable = (component.visible = false);
			component.collidable = collidable;
		}
		else
		{
			gameObject.AddComponent<ItemDraggingCreation>();
		}
		return gameObject;
	}

	private static GameObject InstantiateSimpleObject(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject gameObject = InstantiateCreationModel(crd, initTransform);
		if (gameObject == null)
		{
			return null;
		}
		if (init)
		{
			gameObject.AddComponent<DragItemLogicCreationSimpleObj>();
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			rigidbody.drag = 0f;
			rigidbody.angularDrag = 1000000f;
			rigidbody.mass = 1000000f;
			rigidbody.centerOfMass = Vector3.zero;
			CreationController component = gameObject.GetComponent<CreationController>();
			GameObject gameObject2 = component.meshRoot.gameObject;
			gameObject2.AddComponent<VCPCommonPart>();
			gameObject2.AddComponent<DragItemMousePickCreationSimpleObject>();
			gameObject2.AddComponent<ItemScript>();
			VCPBed[] componentsInChildren = gameObject.GetComponentsInChildren<VCPBed>();
			VCPPivot componentInChildren = gameObject.GetComponentInChildren<VCPPivot>();
			VCPSimpleLight[] componentsInChildren2 = gameObject.GetComponentsInChildren<VCPSimpleLight>();
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				PESleep[] array = (gameObject.AddComponent<PEBed>().sleeps = new PESleep[componentsInChildren.Length]);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					array[i] = componentsInChildren[i].sleepPivot;
					componentsInChildren[i].gameObject.AddComponent<DragItemMousePickCreationSimpleObject>().overridePriority = MousePicker.EPriority.Level3;
					componentsInChildren[i].gameObject.AddComponent<ItemScript>();
				}
			}
			if (componentInChildren != null)
			{
				componentInChildren.gameObject.AddComponent<DragItemMousePickCreationSimpleObject>().overridePriority = MousePicker.EPriority.Level3;
				componentInChildren.gameObject.AddComponent<ItemScript>();
				componentInChildren.Init(component.partRoot.parent);
			}
			if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
			{
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j].gameObject.AddComponent<DragItemMousePickCreationSimpleObject>().overridePriority = MousePicker.EPriority.Level3;
					componentsInChildren2[j].gameObject.AddComponent<ItemScript>();
				}
			}
			bool collidable = (component.visible = false);
			component.collidable = collidable;
		}
		else
		{
			gameObject.AddComponent<ItemDraggingCreation>();
		}
		return gameObject;
	}

	private static GameObject InstantiateArmor(int itemInstanceId, bool init, CreationData crd, Action<Transform> initTransform)
	{
		GameObject gameObject = InstantiateCreationModel(crd, initTransform);
		if (gameObject == null)
		{
			return null;
		}
		if (init)
		{
		}
		return gameObject;
	}

	public static void CheckForDel()
	{
		if (m_Creations == null)
		{
			return;
		}
		using Dictionary<int, CreationData>.Enumerator enumerator = m_Creations.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.Value.TooLongNoUse())
		{
		}
	}
}
