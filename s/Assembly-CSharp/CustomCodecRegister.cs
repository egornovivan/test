using CustomData;
using ItemAsset;
using Jboy;
using TownData;
using uLink;
using UnityEngine;

internal class CustomCodecRegister
{
	internal static void Register()
	{
		BitStreamCodec.AddAndMakeArray<AlteredVoxel>(AlteredVoxel.ReadVoxel, AlteredVoxel.WriteVoxel);
		BitStreamCodec.AddAndMakeArray<BuildVoxel>(BuildVoxel.ReadBuildVoxel, BuildVoxel.WriteBuildVoxel);
		BitStreamCodec.AddAndMakeArray<TMsgInfo>(TMsgInfo.ReadMsg, TMsgInfo.WriteMsg);
		BitStreamCodec.AddAndMakeArray<RoleInfo>(RoleInfo.ReadRoleInfo, RoleInfo.WriteRoleInfo);
		BitStreamCodec.AddAndMakeArray<IntVector3>(IntVector3.DeserializeItem, IntVector3.SerializeItem);
		BitStreamCodec.AddAndMakeArray<IntVector2>(IntVector2.DeserializeItem, IntVector2.SerializeItem);
		BitStreamCodec.AddAndMakeArray<CustomAppearanceData>(CustomAppearanceData.ReadAppearanceData, CustomAppearanceData.WriteAppearanceData);
		BitStreamCodec.AddAndMakeArray<IntVector4>(IntVector4.DeserializeItem, IntVector4.SerializeItem);
		BitStreamCodec.AddAndMakeArray<ItemObject>(ItemObject.Deserialize, ItemObject.Serialize);
		BitStreamCodec.AddAndMakeArray<BattleInfo>(BattleInfo.Deserialize, BattleInfo.Serialize);
		BitStreamCodec.AddAndMakeArray<CreationOriginData>(CreationOriginData.Deserialize, CreationOriginData.Serialize);
		BitStreamCodec.AddAndMakeArray<PlayerBattleInfo>(PlayerBattleInfo.Deserialize, PlayerBattleInfo.Serialize);
		BitStreamCodec.AddAndMakeArray<ItemSample>(ItemSample.Deserialize, ItemSample.Serialize);
		BitStreamCodec.AddAndMakeArray<RegisteredISO>(RegisteredISO.Deserialize, RegisteredISO.Serialize);
		BitStreamCodec.AddAndMakeArray<MapObj>(MapObj.Deserialize, MapObj.Serialize);
		BitStreamCodec.AddAndMakeArray<HistoryStruct>(HistoryStruct.Deserialize, HistoryStruct.Serialize);
		BitStreamCodec.AddAndMakeArray<CompoudItem>(CompoudItem.Deserialize, CompoudItem.Serialize);
		BitStreamCodec.AddAndMakeArray<GlobalTreeInfo>(GlobalTreeInfo.Deserialize, GlobalTreeInfo.Serialize);
		BitStreamCodec.AddAndMakeArray<CreatItemInfo>(CreatItemInfo.DeserializeItemInfo, CreatItemInfo.SerializeItemInfo);
		BitStreamCodec.AddAndMakeArray<TownInfo>(TownInfo.DeserializeInfo, TownInfo.SerializeInfo);
		BitStreamCodec.AddAndMakeArray<TownNpcInfo>(TownNpcInfo.DeserializeInfo, TownNpcInfo.SerializeInfo);
		BitStreamCodec.AddAndMakeArray<BuildingID>(BuildingID.Deserialize, BuildingID.Serialize);
		BitStreamCodec.AddAndMakeArray<SceneObject>(SceneObject.Deserialize, SceneObject.Serialize);
		BitStreamCodec.AddAndMakeArray<FarmPlantLogic>(FarmPlantLogic.Deserialize, FarmPlantLogic.Serialize);
		BitStreamCodec.AddAndMakeArray<TradeObj>(TradeObj.Deserialize, TradeObj.Serialize);
		BitStreamCodec.AddAndMakeArray<TownTradeItemInfo>(TownTradeItemInfo.Deserialize, TownTradeItemInfo.Serialize);
		BitStreamCodec.AddAndMakeArray<ItemIdCount>(ItemIdCount.Deserialize, ItemIdCount.Serialize);
		BitStreamCodec.AddAndMakeArray<CSTreatment>(CSTreatment.Deserialize, CSTreatment.Serialize);
		Json.AddCodec<Vector3>(Vector3ToJson.JsonDeserializer, Vector3ToJson.JsonSerializer);
		Json.AddCodec<Quaternion>(QuaternionToJson.JsonDeserializer, QuaternionToJson.JsonSerializer);
	}
}
