using Pathea;
using UnityEngine;

public class PENative : MonoBehaviour
{
	[SerializeField]
	private NativeProfession profession;

	[SerializeField]
	private NativeSex sex;

	[SerializeField]
	private NativeAge age;

	public NativeProfession Profession
	{
		get
		{
			return profession;
		}
		set
		{
			profession = value;
		}
	}

	public NativeSex Sex => sex;

	public NativeAge Age => age;
}
