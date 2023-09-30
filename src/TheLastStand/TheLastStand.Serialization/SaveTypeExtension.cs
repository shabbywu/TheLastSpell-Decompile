using TPLib;
using TPLib.Log;
using TheLastStand.Manager;

namespace TheLastStand.Serialization;

public static class SaveTypeExtension
{
	public static SaveInfo GetSaveInfo(this E_SaveType saveType)
	{
		switch (saveType)
		{
		case E_SaveType.App:
		{
			SaveInfo result = default(SaveInfo);
			result.Container = TPSingleton<ApplicationManager>.Instance.Serialize() as SerializedContainer;
			result.FilePath = SaveManager.AppSaveFilePath;
			result.BackupFilePath = SaveManager.AppSaveBackupFilePath;
			result.TemporaryBackupFilePath = SaveManager.AppSaveTemporaryBackupFilePath;
			result.UseEncryption = !SaveManager.IsSaveEncryptionDisabled;
			return result;
		}
		case E_SaveType.Game:
		{
			SaveInfo result = default(SaveInfo);
			result.Container = TPSingleton<GameManager>.Instance.Serialize() as SerializedGameState;
			result.FilePath = SaveManager.GameSaveFilePath;
			result.BackupFilePath = SaveManager.GameSaveBackupFilePath;
			result.TemporaryBackupFilePath = SaveManager.GameSaveTemporaryBackupFilePath;
			result.UseEncryption = !SaveManager.IsSaveEncryptionDisabled;
			return result;
		}
		case E_SaveType.Settings:
		{
			SaveInfo result = default(SaveInfo);
			result.Container = TPSingleton<SettingsManager>.Instance.Serialize() as SerializedSettings;
			result.FilePath = SaveManager.SettingsFilePath;
			result.BackupFilePath = string.Empty;
			result.TemporaryBackupFilePath = string.Empty;
			result.UseEncryption = false;
			return result;
		}
		default:
			((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).LogError((object)("Unexpected save type : " + saveType), (CLogLevel)1, true, true);
			return default(SaveInfo);
		}
	}
}
