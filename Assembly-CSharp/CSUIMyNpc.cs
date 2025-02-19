using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class CSUIMyNpc
{
	private List<CSUIMySkill> m_OwnSkills = new List<CSUIMySkill>();

	private int m_State = -1;

	private bool m_IsRandom;

	private bool m_HasOccupation;

	private string m_Name = "Free";

	private PeSex m_Sex = PeSex.Male;

	private Texture m_RandomNpcFace;

	private int m_Health;

	private int m_HealthMax;

	private int m_Stamina;

	private int m_Stamina_max;

	private int m_Hunger;

	private int m_Hunger_max;

	private int m_Comfort;

	private int m_Comfort_max;

	private int m_Oxygen;

	private int m_Oxygen_max;

	private int m_Shield;

	private int m_Shield_max;

	private int m_Energy;

	private int m_Energy_max;

	private int m_Strength;

	private int m_Strength_max;

	private int m_Attack;

	private int m_Defense;

	private string m_AddHealth = string.Empty;

	private string m_AddStrength = string.Empty;

	private string m_AddHunger = string.Empty;

	private string m_AddStamina = string.Empty;

	private string m_AddOxygen = string.Empty;

	public List<CSUIMySkill> OwnSkills
	{
		get
		{
			return m_OwnSkills;
		}
		set
		{
			m_OwnSkills = value;
		}
	}

	public int State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public bool IsRandom
	{
		get
		{
			return m_IsRandom;
		}
		set
		{
			m_IsRandom = value;
		}
	}

	public bool HasOccupation
	{
		get
		{
			return m_HasOccupation;
		}
		set
		{
			m_HasOccupation = value;
		}
	}

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
		}
	}

	public PeSex Sex
	{
		get
		{
			return m_Sex;
		}
		set
		{
			m_Sex = value;
		}
	}

	public Texture RandomNpcFace
	{
		get
		{
			return m_RandomNpcFace;
		}
		set
		{
			m_RandomNpcFace = value;
		}
	}

	public int Health
	{
		get
		{
			return m_Health;
		}
		set
		{
			m_Health = value;
		}
	}

	public int HealthMax
	{
		get
		{
			return m_HealthMax;
		}
		set
		{
			m_HealthMax = value;
		}
	}

	public int Stamina
	{
		get
		{
			return m_Stamina;
		}
		set
		{
			m_Stamina = value;
		}
	}

	public int Stamina_max
	{
		get
		{
			return m_Stamina_max;
		}
		set
		{
			m_Stamina_max = value;
		}
	}

	public int Hunger
	{
		get
		{
			return m_Hunger;
		}
		set
		{
			m_Hunger = value;
		}
	}

	public int Hunger_max
	{
		get
		{
			return m_Hunger_max;
		}
		set
		{
			m_Hunger_max = value;
		}
	}

	public int Comfort
	{
		get
		{
			return m_Comfort;
		}
		set
		{
			m_Comfort = value;
		}
	}

	public int Comfort_max
	{
		get
		{
			return m_Comfort_max;
		}
		set
		{
			m_Comfort_max = value;
		}
	}

	public int Oxygen
	{
		get
		{
			return m_Oxygen;
		}
		set
		{
			m_Oxygen = value;
		}
	}

	public int Oxygen_max
	{
		get
		{
			return m_Oxygen_max;
		}
		set
		{
			m_Oxygen_max = value;
		}
	}

	public int Shield
	{
		get
		{
			return m_Shield;
		}
		set
		{
			m_Shield = value;
		}
	}

	public int Shield_max
	{
		get
		{
			return m_Shield_max;
		}
		set
		{
			m_Shield_max = value;
		}
	}

	public int Energy
	{
		get
		{
			return m_Energy;
		}
		set
		{
			m_Energy = value;
		}
	}

	public int Energy_max
	{
		get
		{
			return m_Energy_max;
		}
		set
		{
			m_Energy_max = value;
		}
	}

	public int Strength
	{
		get
		{
			return m_Strength;
		}
		set
		{
			m_Strength = value;
		}
	}

	public int Strength_max
	{
		get
		{
			return m_Strength_max;
		}
		set
		{
			m_Strength_max = value;
		}
	}

	public int Attack
	{
		get
		{
			return m_Attack;
		}
		set
		{
			m_Attack = value;
		}
	}

	public int Defense
	{
		get
		{
			return m_Defense;
		}
		set
		{
			m_Defense = value;
		}
	}

	public string AddHealth
	{
		get
		{
			return m_AddHealth;
		}
		set
		{
			m_AddHealth = value;
		}
	}

	public string AddStrength
	{
		get
		{
			return m_AddStrength;
		}
		set
		{
			m_AddStrength = value;
		}
	}

	public string AddHunger
	{
		get
		{
			return m_AddHunger;
		}
		set
		{
			m_AddHunger = value;
		}
	}

	public string AddStamina
	{
		get
		{
			return m_AddStamina;
		}
		set
		{
			m_AddStamina = value;
		}
	}

	public string AddOxygen
	{
		get
		{
			return m_AddOxygen;
		}
		set
		{
			m_AddOxygen = value;
		}
	}
}
