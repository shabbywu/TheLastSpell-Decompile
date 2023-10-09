namespace TheLastStand.Framework.UI.CoroutineTween;

internal interface ITweenValue
{
	bool IgnoreTimeScale { get; }

	float Duration { get; }

	void TweenValue(float floatPercentage);

	bool ValidTarget();
}
