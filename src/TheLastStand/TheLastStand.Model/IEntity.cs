using TPLib.Log;

namespace TheLastStand.Model;

public interface IEntity
{
	int RandomId { get; }

	string UniqueIdentifier { get; }

	void Log(object message, CLogLevel logLevel = 1, bool forcePrintInUnity = false, bool printStackTrace = false);

	void LogError(object message, CLogLevel logLevel = 1, bool forcePrintInUnity = true, bool printStackTrace = true);

	void LogWarning(object message, CLogLevel logLevel = 1, bool forcePrintInUnity = true, bool printStackTrace = false);
}
