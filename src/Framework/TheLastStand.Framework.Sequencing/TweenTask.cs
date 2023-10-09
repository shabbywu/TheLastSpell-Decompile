using DG.Tweening;

namespace TheLastStand.Framework.Sequencing;

public class TweenTask : Task
{
	private Tween tween;

	public TweenTask(Tween tween)
	{
		this.tween = tween;
		TweenExtensions.Pause<Tween>(this.tween);
	}

	public override void StartTask()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.StartTask();
		TweenSettingsExtensions.OnComplete<Tween>(TweenExtensions.Play<Tween>(tween), (TweenCallback)delegate
		{
			Complete();
		});
	}

	public override string ToString()
	{
		return base.ToString() + ": " + ((object)tween).GetType().ToString();
	}
}
