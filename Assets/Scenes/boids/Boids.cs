using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : BaseRenderer
{
	private ComputeBuffer mainBuffer;

	private int KERNEL_ID_Init;
	private int KERNEL_ID_Update;
	private int KERNEL_ID_Render;

	public int count = 100;
	private int stride = 4 * 4;

	private Vector2 ThreadBlockSize = new Vector2(8, 8);
	private float InitBlockLength = 64;

	private void Awake()
	{
		KERNEL_ID_Render = MainShader.FindKernel("Render");
		KERNEL_ID_Init = MainShader.FindKernel("Init");
		KERNEL_ID_Update = MainShader.FindKernel("Update");

		InitComputeBuffer();
	}
	private void InitComputeBuffer()
	{
		mainBuffer = new ComputeBuffer(count, stride);

		SetShaderParams();

		MainShader.SetBuffer(KERNEL_ID_Init, "Boids", mainBuffer);
		MainShader.Dispatch(KERNEL_ID_Init, Mathf.CeilToInt(count / InitBlockLength), 1, 1);
	}

	private void OnDestroy()
	{
		if (mainBuffer != null) mainBuffer.Release();
		if (Result != null) Result.Release();
	}
	public override void SetShaderParams()
	{
		MainShader.SetInt("count", count);
	}
	public override void Render(RenderTexture destination)
	{
		int threadGroupsX = Mathf.CeilToInt(WIDTH / ThreadBlockSize.x);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / ThreadBlockSize.y);

		MainShader.SetTexture(KERNEL_ID_Render, "Result", Result);
		MainShader.Dispatch(KERNEL_ID_Render, threadGroupsX, threadGroupsY, 1);

		Graphics.Blit(Result, destination);
	}
	public override void InitRenderTexture()
	{
		createTexture(ref Result, WIDTH, HEIGHT);
	}
}
