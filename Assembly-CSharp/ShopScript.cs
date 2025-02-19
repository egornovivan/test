public class ShopScript
{
	public void InitShop()
	{
	}

	public bool BuyItem(int id)
	{
		ShopData shopData = ShopRespository.GetShopData(id);
		if (shopData == null)
		{
			return false;
		}
		return true;
	}
}
