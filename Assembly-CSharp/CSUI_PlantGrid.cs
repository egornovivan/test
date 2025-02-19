using UnityEngine;

public class CSUI_PlantGrid : MonoBehaviour
{
	public delegate void PlantGridEvent(CSUI_PlantGrid plantGrid);

	public FarmPlantLogic m_Plant;

	[SerializeField]
	private UIGrid m_StateRoot;

	[SerializeField]
	private UISlicedSprite m_WateringState;

	[SerializeField]
	private UISlicedSprite m_WeedingState;

	[SerializeField]
	private UISlicedSprite m_GainState;

	[SerializeField]
	private UISlicedSprite m_DeadState;

	[SerializeField]
	private UISlicedSprite m_IconSprite;

	[SerializeField]
	private UIButton m_DeleteBtn;

	[SerializeField]
	private UISlider m_LifeSlider;

	public PlantGridEvent OnDestroySelf;

	private int m_State;

	public string IconSpriteName
	{
		get
		{
			return m_IconSprite.spriteName;
		}
		set
		{
			m_IconSprite.spriteName = value;
		}
	}

	private void OnActivate(bool active)
	{
		m_DeleteBtn.gameObject.SetActive(active);
	}

	private void OnDeleteBtn()
	{
		if (!(m_Plant == null))
		{
			if (FarmManager.Instance != null)
			{
				FarmManager.Instance.RemovePlant(m_Plant.mPlantInstanceId);
			}
			DragArticleAgent.Destory(m_Plant.mPlantInstanceId);
			if (OnDestroySelf != null)
			{
				OnDestroySelf(this);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_Plant == null)
		{
			return;
		}
		int num = 0;
		if (m_Plant.IsRipe)
		{
			m_WateringState.gameObject.SetActive(value: false);
			m_WeedingState.gameObject.SetActive(value: false);
			m_GainState.gameObject.SetActive(value: true);
			m_DeadState.gameObject.SetActive(value: false);
			num = 1;
		}
		else if (m_Plant.mDead)
		{
			m_WateringState.gameObject.SetActive(value: false);
			m_WeedingState.gameObject.SetActive(value: false);
			m_GainState.gameObject.SetActive(value: false);
			m_DeadState.gameObject.SetActive(value: true);
			num = 5;
		}
		else
		{
			m_GainState.gameObject.SetActive(value: false);
			m_DeadState.gameObject.SetActive(value: false);
			m_WateringState.gameObject.SetActive(m_Plant.NeedWater);
			m_WeedingState.gameObject.SetActive(m_Plant.NeedClean);
			if (m_Plant.NeedWater)
			{
				num = 2;
			}
			else if (m_Plant.NeedClean)
			{
				num = 3;
			}
			else if (m_Plant.NeedWater && m_Plant.NeedClean)
			{
				num = 4;
			}
		}
		if (m_State != num)
		{
			m_StateRoot.repositionNow = true;
			m_State = num;
		}
		if (!m_Plant.mDead)
		{
			m_LifeSlider.sliderValue = m_Plant.GetRipePercent();
		}
		else
		{
			m_LifeSlider.sliderValue = 0f;
		}
	}
}
