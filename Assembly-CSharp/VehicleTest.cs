using UnityEngine;
using VehiclePhysics;

public class VehicleTest : MonoBehaviour
{
	[SerializeField]
	private Camera mMainCamera;

	[SerializeField]
	private Transform ExportPosVehicle;

	[SerializeField]
	private Transform ExportPosAircraft;

	[SerializeField]
	private Transform ExportPosBoat;

	private CreationData new_creation;

	private GameObject creation_obj;

	private void Start()
	{
		CreationMgr.Init();
	}

	private void OpenEditor()
	{
		VCEditor.Open();
	}

	private void ExportCreation()
	{
		if (VCEditor.s_Scene == null || VCEditor.s_Scene.m_IsoData == null)
		{
			Debug.Log("VCEditor IsoData is null!");
			return;
		}
		new_creation = new CreationData();
		new_creation.m_ObjectID = CreationMgr.QueryNewId();
		new_creation.m_RandomSeed = Random.value;
		new_creation.m_Resource = VCEditor.s_Scene.m_IsoData.Export();
		new_creation.ReadRes();
		new_creation.GenCreationAttr();
		if (new_creation.SaveRes())
		{
			new_creation.BuildPrefab();
			new_creation.Register();
			CreationMgr.AddCreation(new_creation);
			GameObject gameObject = CreationMgr.InstantiateCreation(new_creation.m_ObjectID, 0, init: true, null);
			if (gameObject != null)
			{
				if (new_creation.m_Attribute.m_Type == ECreation.Vehicle)
				{
					gameObject.transform.localPosition = ExportPosVehicle.position;
				}
				if (new_creation.m_Attribute.m_Type == ECreation.Aircraft)
				{
					gameObject.transform.localPosition = ExportPosAircraft.position;
				}
				if (new_creation.m_Attribute.m_Type == ECreation.Boat)
				{
					gameObject.transform.localPosition = ExportPosBoat.position;
				}
				ZXCameraCtrl component = mMainCamera.GetComponent<ZXCameraCtrl>();
				component.Following = gameObject.transform;
			}
			creation_obj = gameObject;
		}
		else
		{
			Debug.Log("Save creation resource file failed !");
			new_creation.Destroy();
		}
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(50f, 50f, 100f, 30f), "Clear Monos"))
		{
			MonoBehaviour[] componentsInChildren = creation_obj.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
			MonoBehaviour[] array = componentsInChildren;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				if (!(monoBehaviour is CreationMeshLoader) && !(monoBehaviour is VCMeshMgr))
				{
					Object.Destroy(monoBehaviour);
				}
			}
			MonoBehaviour[] array2 = componentsInChildren;
			foreach (MonoBehaviour monoBehaviour2 in array2)
			{
				if (!(monoBehaviour2 is CreationMeshLoader) && !(monoBehaviour2 is VCMeshMgr))
				{
					Object.Destroy(monoBehaviour2);
				}
			}
		}
		if (!GUI.Button(new Rect(50f, 100f, 100f, 30f), "Init Test"))
		{
			return;
		}
		VehicleEngine vehicleEngine = creation_obj.AddComponent<VehicleEngine>();
		creation_obj.AddComponent<VehicleDemo>().engine = vehicleEngine;
		WheelCollider[] componentsInChildren2 = creation_obj.GetComponentsInChildren<WheelCollider>();
		WheelCollider[] array3 = componentsInChildren2;
		foreach (WheelCollider wheelCollider in array3)
		{
			wheelCollider.enabled = true;
		}
		vehicleEngine.wheels = new VehicleWheel[componentsInChildren2.Length];
		for (int l = 0; l < vehicleEngine.wheels.Length; l++)
		{
			vehicleEngine.wheels[l] = new VehicleWheel();
			vehicleEngine.wheels[l].wheel = componentsInChildren2[l];
			vehicleEngine.wheels[l].model = componentsInChildren2[l].transform.parent.GetChild(3);
			vehicleEngine.wheels[l].maxMotorTorque = 5000f;
			vehicleEngine.wheels[l].staticBrakeTorque = 1200f;
			vehicleEngine.wheels[l].dynamicBrakeTorque = 400f;
			vehicleEngine.wheels[l].footBrakeTorque = 6000f;
			vehicleEngine.wheels[l].handBrakeTorque = 10000000f;
		}
		vehicleEngine.maxMotorTorque = 24000f;
		vehicleEngine.maxPower = 3000f;
		vehicleEngine.realMass = 15000f;
		Transform[] componentsInChildren3 = creation_obj.GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] array4 = componentsInChildren3;
		foreach (Transform transform in array4)
		{
			if (transform.gameObject.name == "MCollider")
			{
				Object.Destroy(transform.gameObject);
			}
		}
		creation_obj.GetComponent<Rigidbody>().mass = 15000f;
		creation_obj.GetComponent<Rigidbody>().isKinematic = false;
	}
}
