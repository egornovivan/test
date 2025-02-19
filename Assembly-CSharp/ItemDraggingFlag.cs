public class ItemDraggingFlag : ItemDraggingArticle
{
	public override bool OnPutDown()
	{
		if (GameConfig.IsMultiClient)
		{
			if (VArtifactUtil.IsInTownBallArea(base.transform.position))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RequestDragFlag(itemDragging.itemObj.instanceId, base.transform.position, base.transform.rotation);
			}
			return true;
		}
		return base.OnPutDown();
	}
}
