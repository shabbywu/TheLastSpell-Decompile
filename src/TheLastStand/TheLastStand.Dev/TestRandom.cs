using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Dev;

public class TestRandom : MonoBehaviour
{
	[SerializeField]
	private int pixWidth = 128;

	[SerializeField]
	private int pixHeight = 128;

	[SerializeField]
	private bool usePerlin = true;

	private Texture2D noiseTex;

	private Color[] pix;

	private Renderer rend;

	private void Start()
	{
		rend = ((Component)this).GetComponent<Renderer>();
		InitTexture();
	}

	[ContextMenu("InitTexture")]
	private void InitTexture()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		noiseTex = new Texture2D(pixWidth, pixHeight);
		((Texture)noiseTex).filterMode = (FilterMode)(usePerlin ? 1 : 0);
		pix = (Color[])(object)new Color[((Texture)noiseTex).width * ((Texture)noiseTex).height];
		rend.material.mainTexture = (Texture)(object)noiseTex;
	}

	private void CalcNoise()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		float num = -1f;
		for (; i < ((Texture)noiseTex).height; i++)
		{
			for (int j = 0; j < ((Texture)noiseTex).width; j++)
			{
				num = RandomManager.GetHashedWhiteNoise(j, i);
				pix[i * ((Texture)noiseTex).width + j] = new Color(num, num, num);
			}
		}
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
	}

	private void Update()
	{
		CalcNoise();
	}
}
