using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cell : MonoBehaviour
{
	public ComputeShader MainShader;

	private RenderTexture Result;

	private int KERNEL_ID_Init;
	private int KERNEL_ID_Render;
	private int KERNEL_ID_LifeCycle;

	public float scaleFactor = 1;
	private int WIDTH, HEIGHT;

	private Vector2 ThreadBlockSize = new Vector2(8, 8);

	private void Awake()
	{
		KERNEL_ID_Render = MainShader.FindKernel("Render");
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		WIDTH = (int)(Screen.width / scaleFactor);
		HEIGHT = (int)(Screen.height / scaleFactor);

		SetShaderParams();
		Render(destination);
	}
	private void SetShaderParams()
	{
		MainShader.SetInt("WIDTH", WIDTH);
		MainShader.SetInt("HEIGHT", HEIGHT);
		MainShader.SetFloat("now", Time.time);

		var useTime = Time.time / 10;

		var points = new Vector4[60];

		for (int i = 0; i < points.Length; i++)
			points[i] =
				new Vector4(
					Mathf.PerlinNoise(10000 + i * Mathf.PI * 50, useTime) * WIDTH,
					Mathf.PerlinNoise(10600 + i * Mathf.PI * 50, useTime) * HEIGHT,
					Mathf.PerlinNoise(10000 + (i+1) * Mathf.PI * 50, useTime) * WIDTH,
					Mathf.PerlinNoise(10600 + (i+1) * Mathf.PI * 50, useTime) * HEIGHT);

		MainShader.SetVectorArray("points", points);
	}
	private void Render(RenderTexture destination)
	{
		InitRenderTexture();

		int threadGroupsX = Mathf.CeilToInt(WIDTH / ThreadBlockSize.x);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / ThreadBlockSize.y);

		MainShader.SetTexture(KERNEL_ID_Render, "Result", Result);
		MainShader.Dispatch(KERNEL_ID_Render, threadGroupsX, threadGroupsY, 1);

		Graphics.Blit(Result, destination);
	}
	private bool createTexture(ref RenderTexture tex, int width, int height)
	{
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

	private void InitRenderTexture()
	{
		createTexture(ref Result, WIDTH, HEIGHT);
	}
}
