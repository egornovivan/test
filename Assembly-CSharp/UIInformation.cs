public class UIInformation : UIStaticWnd
{
	public enum infoType
	{
		it_Item,
		it_SystemWaning,
		it_SystemInfo,
		it_Mission,
		it_Conloy
	}

	private static UIInformation mIntence;

	public static UIInformation Intence => mIntence;

	public void AddInfo(infoType type, string info)
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
