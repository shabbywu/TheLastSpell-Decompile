using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View;

[RequireComponent(typeof(SpriteRenderer))]
public class BodyPartView : MonoBehaviour
{
	[SerializeField]
	private BodyPartDefinition.E_Orientation orientation = BodyPartDefinition.E_Orientation.Front;

	[SerializeField]
	private bool keepOwnMaterial;

	private SpriteRenderer bodyPartRenderer;

	public bool IsDirty { get; set; } = true;


	public BodyPart BodyPart { get; set; }

	public BodyPartDefinition.E_Orientation Orientation => orientation;

	public static Sprite GetSprite(BodyPart bodyPart, string faceId, string gender, BodyPartDefinition.E_Orientation orientation)
	{
		if (bodyPart == null)
		{
			TPDebug.LogError((object)"BodyPartView.GetSprite() => bodyPart can't be null! Aborting...", (Object)null);
			return null;
		}
		if (bodyPart.AdditionalConstraints.Contains("Hide"))
		{
			return null;
		}
		string spritePath = bodyPart.GetSpritePath(faceId, gender, orientation);
		if (string.IsNullOrEmpty(spritePath))
		{
			return null;
		}
		return ResourcePooler.LoadOnce<Sprite>(spritePath, failSilently: false);
	}

	public Sprite GetSprite()
	{
		return bodyPartRenderer.sprite;
	}

	public void Init()
	{
		InitRenderer();
	}

	public void Refresh(string faceId, string gender, bool forceRefresh = false)
	{
		if ((IsDirty || forceRefresh) && !((Object)(object)bodyPartRenderer == (Object)null))
		{
			Sprite sprite = GetSprite(faceId, gender);
			bodyPartRenderer.sprite = sprite;
			((Renderer)bodyPartRenderer).enabled = (Object)(object)sprite != (Object)null;
			IsDirty = false;
		}
	}

	public void SetMaterial(Material material)
	{
		if (!keepOwnMaterial)
		{
			InitRenderer();
			((Renderer)bodyPartRenderer).material = material;
		}
	}

	public void Tint(Color bodyPartColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		bodyPartRenderer.color = bodyPartColor;
	}

	private void Awake()
	{
		InitRenderer();
	}

	private Sprite GetSprite(string faceId, string gender)
	{
		return GetSprite(BodyPart, faceId, gender, orientation);
	}

	private void InitRenderer()
	{
		if (!((Object)(object)bodyPartRenderer != (Object)null))
		{
			bodyPartRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		}
	}
}
