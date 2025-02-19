using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	private static PrefabManager self;

	public GameObject PlayerCreator;

	public GameObject PlayerOwner;

	public GameObject PlayerProxy;

	public GameObject BaseCreator;

	public GameObject BaseOwner;

	public GameObject BaseProxy;

	public GameObject CreationNetwork;

	public GameObject AiMonster;

	public GameObject AiMissionMonster;

	public GameObject AiGroupMonster;

	public GameObject AiTDMonster;

	public GameObject AiTowerNetworkSeed;

	public GameObject AiTowerDefense;

	public GameObject AiAdNpcNetworkSeed;

	public GameObject AiSceneStaticObject;

	public GameObject ItemFetchNetworkSeed;

	public GameObject AiGroupNetwork;

	public GameObject ChannelNetwork;

	public GameObject MapObjNetwork;

	public GameObject NativeTowerNetwork;

	public GameObject NativeTowerGiftNetwork;

	public GameObject NativeStaticNetwork;

	public GameObject ColonyNetwork;

	public GameObject AiFlagNetwork;

	public static PrefabManager Self => self;

	private void Awake()
	{
		self = this;
	}
}
