namespace PeCustom;

public interface ISceneController
{
	HashBinder Binder { get; }

	void OnNotification(ESceneNoification msg_type, params object[] data);
}
