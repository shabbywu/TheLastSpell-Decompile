using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Model;

namespace TheLastStand.Controller;

public interface IDamageableController : IEffectTargetSkillActionController
{
	IDamageable Damageable { get; }

	float GainHealth(float amount, bool refreshHud = true);

	void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true);

	void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true);

	void OnHit(ISkillCaster attacker);
}
