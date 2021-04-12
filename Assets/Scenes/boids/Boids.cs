using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : BaseRenderer
{
	private int KERNEL_ID_Init;
	private int KERNEL_ID_Render;

	public int length = 100;
	private int stride = 4 * 4;

	private Vector2 ThreadBlockSize = new Vector2(8, 8);

	private void Awake()
	{
		KERNEL_ID_Render = MainShader.FindKernel("Render");

        // TODO: ADD BUFFER
	}


	public override void SetShaderParams()
	{

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
