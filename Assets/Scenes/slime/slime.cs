using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slime : BaseRenderer
{
	private int KERNEL_ID_Render;
	private int KERNEL_ID_Init;
    private int KERNEL_ID_Update;

    private int count = 10000;
    private int InitBlockLength = 64;

    private int stride = 24;

    private int SLIME_BLOCK_COUNT;

    private ComputeBuffer mainBuffer;

	private void Awake()
	{
		KERNEL_ID_Render = MainShader.FindKernel("Render");
		KERNEL_ID_Init = MainShader.FindKernel("Init");
		KERNEL_ID_Update = MainShader.FindKernel("Update");

		SLIME_BLOCK_COUNT = Mathf.CeilToInt(count / InitBlockLength);

		InitComputeBuffer();
	}
	private void InitComputeBuffer()
	{
		mainBuffer = new ComputeBuffer(count, stride);

		SetShaderParams();

		MainShader.SetBuffer(KERNEL_ID_Init, "Boids", mainBuffer);
		MainShader.Dispatch(KERNEL_ID_Init, SLIME_BLOCK_COUNT, 1, 1);
	}



}
