using TPLib.Log;
using UnityEngine;

public class CLoggerTest : CLogger<CLoggerTest>
{
	public void Start()
	{
		Test();
	}

	public void Test()
	{
		base.Log((object)"TEST LOG 1", (CLogLevel)1, false, false);
		base.Log((object)"TEST LOG 2", (CLogLevel)0, true, false);
		base.Log((object)"TEST LOG 3", (CLogLevel)1, true, false);
		base.Log((object)"TEST LOG 4", (CLogLevel)2, true, false);
		base.LogFormat((object)"{0} {1} {2} {3}", (CLogLevel)2, true, false, new object[4] { "TEST", "LOG", "FORMAT", "5" });
		base.LogFormat((object)"{0} {1} {2} {3}", (Object)(object)this, (CLogLevel)2, true, false, new object[4] { "TEST", "LOG", "FORMAT", "6" });
		base.Log((object)"TEST LOG 7", (Object)(object)this, (CLogLevel)2, true, false);
		base.LogWarning((object)"TEST LOGWARNING 1", (CLogLevel)1, true, false);
		base.LogWarning((object)"TEST LOGWARNING 2", (CLogLevel)0, true, false);
		base.LogWarning((object)"TEST LOGWARNING 3", (CLogLevel)1, true, false);
		base.LogWarning((object)"TEST LOGWARNING 4", (CLogLevel)2, true, false);
		base.LogWarningFormat("{0} {1} {2} {3}", (CLogLevel)2, true, false, new object[4] { "TEST", "LOGWARNING", "FORMAT", "5" });
		base.LogWarningFormat("{0} {1} {2} {3}", (Object)(object)this, (CLogLevel)2, true, false, new object[4] { "TEST", "LOGWARNING", "FORMAT", "6" });
		base.LogWarning((object)"TEST LOGWARNING 7", (Object)(object)this, (CLogLevel)2, true, false);
		base.LogError((object)"TEST LOGERROR 1", (CLogLevel)1, true, true);
		base.LogError((object)"TEST LOGERROR 2", (CLogLevel)0, true, true);
		base.LogError((object)"TEST LOGERROR 3", (CLogLevel)1, true, true);
		base.LogError((object)"TEST LOGERROR 4", (CLogLevel)2, true, true);
		base.LogErrorFormat("{0} {1} {2} {3}", (CLogLevel)2, true, true, new object[4] { "TEST", "LOGERROR", "FORMAT", "5" });
		base.LogErrorFormat("{0} {1} {2} {3}", (Object)(object)this, (CLogLevel)2, true, true, new object[4] { "TEST", "LOGERROR", "FORMAT", "6" });
		base.LogError((object)"TEST LOGERROR 7", (Object)(object)this, (CLogLevel)2, true, true);
	}
}
