using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadSpecialScene : PeLauncher.ILaunchable
{
	private SingleGameStory.StoryScene mType;

	public LoadSpecialScene(SingleGameStory.StoryScene type = SingleGameStory.StoryScene.MainLand)
	{
		mType = type;
	}

	void PeLauncher.ILaunchable.Launch()
	{
		if (mType == SingleGameStory.StoryScene.DienShip0)
		{
			Object @object = Resources.Load("Prefab/Other/DienShip");
			if (!(@object == null))
			{
				GameObject gameObject = Object.Instantiate(@object) as GameObject;
				gameObject.transform.position = new Vector3(14798f, 3f, 8344f);
			}
		}
		else if (mType == SingleGameStory.StoryScene.DienShip1)
		{
			Object object2 = Resources.Load("Prefab/Other/DienShip");
			if (!(object2 == null))
			{
				GameObject gameObject2 = Object.Instantiate(object2) as GameObject;
				gameObject2.transform.position = new Vector3(16545.25f, 3.93f, 10645.7f);
			}
		}
		else if (mType == SingleGameStory.StoryScene.DienShip2)
		{
			Object object3 = Resources.Load("Prefab/Other/DienShip");
			if (!(object3 == null))
			{
				GameObject gameObject3 = Object.Instantiate(object3) as GameObject;
				gameObject3.transform.position = new Vector3(2876f, 265.6f, 9750.3f);
			}
		}
		else if (mType == SingleGameStory.StoryScene.DienShip3)
		{
			Object object4 = Resources.Load("Prefab/Other/DienShip");
			if (!(object4 == null))
			{
				GameObject gameObject4 = Object.Instantiate(object4) as GameObject;
				gameObject4.transform.position = new Vector3(13765.5f, 75.7f, 15242.7f);
			}
		}
		else if (mType == SingleGameStory.StoryScene.DienShip4)
		{
			Object object5 = Resources.Load("Prefab/Other/DienShip");
			if (!(object5 == null))
			{
				GameObject gameObject5 = Object.Instantiate(object5) as GameObject;
				gameObject5.transform.position = new Vector3(12547.7f, 523.7f, 13485.5f);
			}
		}
		else if (mType == SingleGameStory.StoryScene.DienShip5)
		{
			Object object6 = Resources.Load("Prefab/Other/DienShip");
			if (!(object6 == null))
			{
				GameObject gameObject6 = Object.Instantiate(object6) as GameObject;
				gameObject6.transform.position = new Vector3(7750.4f, 349.7f, 14712.8f);
			}
		}
		else if (mType == SingleGameStory.StoryScene.L1Ship)
		{
			Object object7 = Resources.Load("Prefab/Other/old_scene_boatinside");
			if (!(object7 == null))
			{
				GameObject gameObject7 = Object.Instantiate(object7) as GameObject;
				gameObject7.transform.position = new Vector3(9661f, 88.8f, 12758f);
			}
		}
		else if (mType == SingleGameStory.StoryScene.PajaShip)
		{
			Object object8 = Resources.Load("Prefab/Other/paja_port_shipinside");
			if (!(object8 == null))
			{
				GameObject gameObject8 = Object.Instantiate(object8) as GameObject;
				gameObject8.transform.position = new Vector3(1471f, 101.3f, 7928.3f);
				Quaternion identity = Quaternion.identity;
				identity.eulerAngles = new Vector3(352f, 55f, 0f);
				gameObject8.transform.rotation = identity;
			}
		}
		else if (mType == SingleGameStory.StoryScene.LaunchCenter)
		{
			Object original = Resources.Load("Prefab/Other/paja_launch_center");
			GameObject gameObject9 = Object.Instantiate(original) as GameObject;
			gameObject9.transform.position = new Vector3(1713f, 140f, 10402f);
			Quaternion identity2 = Quaternion.identity;
			identity2.eulerAngles = new Vector3(0f, 180f, 0f);
			gameObject9.transform.rotation = identity2;
		}
	}
}
