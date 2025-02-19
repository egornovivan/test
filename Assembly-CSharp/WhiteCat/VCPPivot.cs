using UnityEngine;

namespace WhiteCat;

public class VCPPivot : VCSimpleObjectPart
{
	[SerializeField]
	private UIFilledSprite fillSprite;

	[GetSet("Angle")]
	[SerializeField]
	private float angle;

	private Transform rootTrans;

	private Vector3 originalPos;

	private bool direction;

	private float time01;

	public float Angle
	{
		get
		{
			return angle;
		}
		set
		{
			if ((bool)fillSprite)
			{
				if (value < 0f)
				{
					fillSprite.transform.localEulerAngles = new Vector3(0f, 180f, 0f - value);
				}
				else
				{
					fillSprite.transform.localEulerAngles = new Vector3(0f, 0f, value);
				}
				fillSprite.fillAmount = Mathf.Abs(value) / 360f;
			}
			angle = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
	}

	public override CmdList GetCmdList()
	{
		CmdList cmdList = base.GetCmdList();
		cmdList.Add("Rotate Pivot", Rotate);
		return cmdList;
	}

	public void Init(Transform rootTrans)
	{
		originalPos = rootTrans.localPosition;
		base.enabled = false;
		this.rootTrans = rootTrans;
		direction = false;
		time01 = 0f;
	}

	private void Rotate()
	{
		direction = !direction;
		base.enabled = true;
	}

	private void FixedUpdate()
	{
		float num = Time.deltaTime * PEVCConfig.instance.pivotRotateSpeed / Mathf.Abs(angle);
		if (direction)
		{
			time01 += num;
		}
		else
		{
			time01 -= num;
		}
		time01 = Mathf.Clamp01(time01);
		float num2 = Mathf.SmoothStep(0f, angle, time01);
		rootTrans.localPosition = originalPos;
		rootTrans.localRotation = Quaternion.identity;
		rootTrans.RotateAround(rootTrans.parent.position, Vector3.up, num2);
		if ((direction && time01 == 1f) || (!direction && time01 == 0f))
		{
			base.enabled = false;
		}
	}
}
