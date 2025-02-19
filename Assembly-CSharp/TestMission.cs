using TrainingScene;
using UnityEngine;

public class TestMission : MonoBehaviour
{
	private GUIStyle archiveBtnStyle;

	private bool gather;

	private bool cut;

	private bool replicate;

	private bool dig;

	private bool build;

	private bool move;

	private bool newmove;

	private void InitStyle()
	{
		archiveBtnStyle = new GUIStyle();
		archiveBtnStyle.stretchHeight = true;
		archiveBtnStyle.stretchWidth = true;
	}

	private void Awake()
	{
		InitStyle();
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(300f, 0f, 140f, Screen.height));
		GetMission();
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(450f, 0f, 170f, Screen.height));
		CompleteMission();
		GUILayout.EndArea();
		GatherHerb();
	}

	private void GetMission()
	{
		if (GUILayout.Button("GetGatherMission") && !gather)
		{
			HoloherbTask.Instance.InitScene();
			gather = true;
		}
		if (GUILayout.Button("GetCutMission") && !cut)
		{
			HolotreeTask.Instance.InitScene();
			cut = true;
		}
		if (GUILayout.Button("GetCopyMission") && !replicate)
		{
			replicate = true;
		}
		if (GUILayout.Button("GetDigMission") && !dig)
		{
			TerrainDigTask.Instance.InitScene();
			dig = true;
		}
		if (GUILayout.Button("GetBuildMission") && !build)
		{
			EmitlineTask.Instance.InitScene();
			build = true;
		}
		if (GUILayout.Button("GetMoveMission") && !move)
		{
			MoveTask.Instance.InitScene();
			move = true;
		}
		if (GUILayout.Button("GetNewMoveMission") && !newmove)
		{
			NewMoveTask.Instance.InitMoveScene();
			newmove = true;
		}
	}

	private void CompleteMission()
	{
		if (GUILayout.Button("CompleteGatherMission") && gather)
		{
			HoloherbTask.Instance.DestroyScene();
			gather = false;
		}
		if (GUILayout.Button("CompleteCutMission") && cut)
		{
			HolotreeTask.Instance.DestroyScene();
			cut = false;
		}
		if (GUILayout.Button("CompleteCopyMission") && replicate)
		{
			replicate = false;
		}
		if (GUILayout.Button("CompleteDigMission") && dig)
		{
			TerrainDigTask.Instance.DestroyScene();
			dig = false;
		}
		if (GUILayout.Button("CompleteBuildMission") && build)
		{
			EmitlineTask.Instance.DestroyScene();
			build = false;
		}
		if (GUILayout.Button("CompleteMoveMission") && move)
		{
			MoveTask.Instance.DestroyScene();
			move = false;
		}
		if (GUILayout.Button("CompleteNewMoveMission") && newmove)
		{
			NewMoveTask.Instance.DestroyMoveScene();
			newmove = false;
		}
	}

	private void GatherHerb()
	{
		GUILayout.BeginArea(new Rect(630f, 0f, 50f, 40f));
		if (GUILayout.Button("herb1") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset1").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(685f, 0f, 50f, 40f));
		if (GUILayout.Button("herb2") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset2").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(740f, 0f, 50f, 40f));
		if (GUILayout.Button("herb3") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset3").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(795f, 0f, 50f, 40f));
		if (GUILayout.Button("herb4") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset4").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(850f, 0f, 50f, 40f));
		if (GUILayout.Button("herb5") && gather)
		{
			HoloherbTask.Instance.SubHerb(HoloherbTask.Instance.transform.FindChild("holoherbs").FindChild("offset5").GetComponent<HoloherbAppearance>());
		}
		GUILayout.EndArea();
	}
}
