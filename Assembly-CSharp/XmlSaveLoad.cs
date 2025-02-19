using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class XmlSaveLoad : MonoBehaviour
{
	private Rect _Save;

	private Rect _Load;

	private Rect _SaveMSG;

	private Rect _LoadMSG;

	private bool _ShouldSave;

	private bool _ShouldLoad;

	private bool _SwitchSave;

	private bool _SwitchLoad;

	private string _FileLocation;

	private string _FileName;

	public GameObject _Player;

	private UserData myData;

	private string _PlayerName;

	private string _data;

	private Vector3 VPosition;

	private void Start()
	{
		_Save = new Rect(10f, 80f, 100f, 20f);
		_Load = new Rect(10f, 100f, 100f, 20f);
		_SaveMSG = new Rect(10f, 120f, 400f, 40f);
		_LoadMSG = new Rect(10f, 140f, 400f, 40f);
		_FileLocation = Application.dataPath;
		_FileName = "SaveData.xml";
		_PlayerName = "Joe Schmoe";
		myData = new UserData();
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		if (Application.isEditor && GUI.Button(_Load, "Load"))
		{
			GUI.Label(_LoadMSG, "Loading from: " + _FileLocation);
			LoadXML();
			if (_data.ToString() != string.Empty)
			{
				myData = (UserData)DeserializeObject(_data);
				VPosition = new Vector3(myData._iUser.x, myData._iUser.y, myData._iUser.z);
				_Player.transform.position = VPosition;
				Debug.Log(myData._iUser.name);
			}
			if (GUI.Button(_Save, "Save"))
			{
				GUI.Label(_SaveMSG, "Saving to: " + _FileLocation);
				myData._iUser.x = _Player.transform.position.x;
				myData._iUser.y = _Player.transform.position.y;
				myData._iUser.z = _Player.transform.position.z;
				myData._iUser.name = _PlayerName;
				_data = SerializeObject(myData);
				CreateXML();
				Debug.Log(_data);
			}
		}
	}

	private string UTF8ByteArrayToString(byte[] characters)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		return uTF8Encoding.GetString(characters);
	}

	private byte[] StringToUTF8ByteArray(string pXmlString)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		return uTF8Encoding.GetBytes(pXmlString);
	}

	private string SerializeObject(object pObject)
	{
		string text = null;
		MemoryStream stream = new MemoryStream();
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserData));
		XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8);
		xmlSerializer.Serialize(xmlTextWriter, pObject);
		stream = (MemoryStream)xmlTextWriter.BaseStream;
		return UTF8ByteArrayToString(stream.ToArray());
	}

	private object DeserializeObject(string pXmlizedString)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserData));
		MemoryStream stream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
		return xmlSerializer.Deserialize(stream);
	}

	private void CreateXML()
	{
		FileInfo fileInfo = new FileInfo(_FileLocation + "/" + _FileName);
		StreamWriter streamWriter;
		if (!fileInfo.Exists)
		{
			streamWriter = fileInfo.CreateText();
		}
		else
		{
			fileInfo.Delete();
			streamWriter = fileInfo.CreateText();
		}
		streamWriter.Write(_data);
		streamWriter.Close();
		Debug.Log("File written.");
	}

	private void LoadXML()
	{
		StreamReader streamReader = File.OpenText(_FileLocation + "/" + _FileName);
		string data = streamReader.ReadToEnd();
		streamReader.Close();
		_data = data;
		Debug.Log("File Read");
	}
}
