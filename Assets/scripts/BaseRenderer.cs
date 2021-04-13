using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRenderer : MonoBehaviour
{
	public ComputeShader MainShader;

	protected RenderTexture Result;

	protected int WIDTH, HEIGHT;
	public float scaleFactor = 1;

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		WIDTH = (int)(Screen.width / scaleFactor);
		HEIGHT = (int)(Screen.height / scaleFactor);

		MainShader.SetInt("WIDTH", WIDTH);
		MainShader.SetInt("HEIGHT", HEIGHT);
		MainShader.SetFloat("now", Time.time);
		MainShader.SetFloat("deltaTime", Time.deltaTime);

		SetShaderParams();
		InitRenderTexture();
		Render(destination);
	}

	public abstract void InitRenderTexture();
	public abstract void SetShaderParams();
	public abstract void Render(RenderTexture destination);
	protected bool createTexture(ref RenderTexture tex, int width, int height)
	{
		if (width == 0 || height == 0) return false;

		if (tex == null || tex.width != width || tex.height != height)
		{
			if (tex != null)
				tex.Release();
			tex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			tex.enableRandomWrite = true;
			tex.Create();
			return true;
		}
		return false;
	}
}
