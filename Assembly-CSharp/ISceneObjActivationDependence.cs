public interface ISceneObjActivationDependence
{
	bool IsDependableForAgent(ISceneObjAgent agent, ref EDependChunkType type);
}
