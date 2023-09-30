using System;
using System.Xml.Linq;
using System.Xml.Serialization;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedSettings : SerializedContainer
{
	public bool AlwaysDisplayMaxStatValue;

	public bool EdgePan = true;

	public bool EdgePanOverUI = true;

	public bool FocusCamOnSelections = true;

	public SerializedScreenSettings ScreenSettings;

	public SerializedSoundSettings SoundSettings;

	public string Language;

	public bool[] EndTurnWarnings;

	public bool? ShowSkillsHotkeys;

	public float? SpeedScale;

	public bool SmartCast;

	public SettingsManager.E_SpeedMode SpeedMode;

	public SettingsManager.E_InputDeviceType InputDeviceType;

	public int CurrentProfile;

	public bool HideCompendium;

	public bool AlwaysDisplayUnitPortraitAttribute;

	public static void HandleUnknownXMLElement(object sender, XmlElementEventArgs e)
	{
		CLoggerManager.Log((object)("Found unknown " + e.Element.Name + " element, trying to handle it with " + typeof(SerializedSettings).Name + "'s Unkown XML Element Handler..."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
		if (!(e.ObjectBeingDeserialized is SerializedSettings serializedSettings))
		{
			CLoggerManager.Log((object)(typeof(SerializedSettings).Name + "'s Unkown XML Element Handler has been used for an unkown type, aborting. Something wrong happened in the registration, this should not happen."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XElement val = XElement.Parse(e.Element.OuterXml);
		if (!(val.Name.LocalName == "TurnEndWarningsEnabled"))
		{
			return;
		}
		try
		{
			bool flag = bool.Parse(val.Value);
			if (serializedSettings.EndTurnWarnings == null)
			{
				serializedSettings.EndTurnWarnings = new bool[Enum.GetNames(typeof(SettingsManager.E_EndTurnWarning)).Length];
				for (int i = 0; i < serializedSettings.EndTurnWarnings.Length; i++)
				{
					serializedSettings.EndTurnWarnings[i] = flag;
				}
			}
		}
		catch (Exception ex)
		{
			CLoggerManager.Log((object)$"An error occured when manually handling {val.Name} :\n{ex.Message}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}

	public override byte GetSaveVersion()
	{
		return SaveManager.SettingsSaveVersion;
	}
}
