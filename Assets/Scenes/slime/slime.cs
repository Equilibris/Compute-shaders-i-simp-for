using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slime : BaseRenderer
{
	private int KERNEL_ID_Render;
	private int KERNEL_ID_Init;
	private int KERNEL_ID_Update;

	public int count = 14;
	private int InitBlockLength = 64;

	private int stride = 24;

	private int SLIME_BLOCK_COUNT;

	private Vector2 ThreadBlockSize = new Vector2(8, 8);

	private ComputeBuffer mainBuffer;

	private void Awake()
	{
		KERNEL_ID_Render = MainShader.FindKernel("Render");
		KERNEL_ID_Init = MainShader.FindKernel("Init");
		KERNEL_ID_Update = MainShader.FindKernel("Update");

		SLIME_BLOCK_COUNT = Mathf.CeilToInt(count / InitBlockLength);
	}

	private void Start() {
		InitComputeBuffer();
	}

	private void InitComputeBuffer()
	{
		mainBuffer = new ComputeBuffer(count, stride);

		SetShaderParams();

		MainShader.SetBuffer(KERNEL_ID_Init, "Agents", mainBuffer);
		MainShader.Dispatch(KERNEL_ID_Init, SLIME_BLOCK_COUNT, 1, 1);
	}

	public override void Render(RenderTexture destination)
	{
		MainShader.SetBuffer(KERNEL_ID_Update, "Agents", mainBuffer);
		MainShader.SetTexture(KERNEL_ID_Update, "Result", Result);
		MainShader.SetTexture(KERNEL_ID_Render, "Result", Result);

		int threadGroupsX = Mathf.CeilToInt(WIDTH / ThreadBlockSize.x);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / ThreadBlockSize.y);

		MainShader.Dispatch(KERNEL_ID_Render, threadGroupsX, threadGroupsY, 1);
		MainShader.Dispatch(KERNEL_ID_Update, SLIME_BLOCK_COUNT, 1, 1);

		Graphics.Blit(Result, destination);
	}
	public override void SetShaderParams()
	{
		MainShader.SetInt("count", count);
		MainShader.SetFloat("speed", 10);
	}

	public override void InitRenderTexture() =>
		createTexture(ref Result, WIDTH, HEIGHT);

}
