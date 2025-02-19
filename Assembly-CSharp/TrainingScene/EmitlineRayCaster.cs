using UnityEngine;

namespace TrainingScene;

public class EmitlineRayCaster : MonoBehaviour
{
	private GameObject eff_true;

	private GameObject eff_false;

	private Transform receiver1;

	private Transform receiver2;

	private LineRenderer lr;

	private bool isX;

	private RaycastHit rh;

	private EmitlineTask et;

	private void Start()
	{
		eff_true = base.transform.FindChild("effect_true").gameObject;
		eff_false = base.transform.FindChild("effect_false").gameObject;
		et = EmitlineTask.Instance;
		receiver1 = et.receivers[0].colreceiver;
		receiver2 = et.receivers[1].colreceiver;
		lr = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		Physics.Raycast(base.transform.position - base.transform.right * 15f, base.transform.right, out rh, 20f, 4096);
		if (EmitlineTask.Instance.missionComplete)
		{
			isX = false;
		}
		else
		{
			isX = rh.transform == receiver1 || rh.transform == receiver2;
		}
		UpdateLine();
	}

	private void UpdateLine()
	{
		Vector3 vector = (Vector3.Distance(rh.point, base.transform.position) - 0.01f) * Vector3.right;
		lr.SetPosition(0, vector);
		if (isX)
		{
			eff_false.transform.localPosition = vector;
			et.testScore -= 100;
		}
		else
		{
			eff_true.transform.localPosition = vector;
			et.testScore++;
		}
		eff_true.SetActive(!isX);
		eff_false.SetActive(isX);
	}
}
