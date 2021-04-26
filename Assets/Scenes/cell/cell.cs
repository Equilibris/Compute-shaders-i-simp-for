using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class cell : BaseRenderer
{
	private int KERNEL_ID_Init;
	private int KERNEL_ID_Render;
	private int KERNEL_ID_LifeCycle;

	private Vector2 ThreadBlockSize = new Vector2(8, 8);

	private void Awake() { KERNEL_ID_Render = MainShader.FindKernel("Render"); }

	public override void SetShaderParams()
	{
		var useTime = Time.time / 20;
		// var useTime = Mathf.Sin(Time.time * Mathf.PI / 15);

		var points = new Vector4[120];

		for (int i = 0; i < points.Length; i++)
			points[i] =
				new Vector4(
					Mathf.PerlinNoise(10000 + i * Mathf.PI * 50, useTime) * WIDTH,
					Mathf.PerlinNoise(10600 + i * Mathf.PI * 50, useTime) * HEIGHT,
					Mathf.PerlinNoise(10000 + (i + 1) * Mathf.PI * 50, useTime) * WIDTH,
					Mathf.PerlinNoise(10600 + (i + 1) * Mathf.PI * 50, useTime) * HEIGHT);

		MainShader.SetVectorArray("points", points);
	}
	public override void Render(RenderTexture destination)
	{
		int threadGroupsX = Mathf.CeilToInt(WIDTH / ThreadBlockSize.x);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / ThreadBlockSize.y);

		MainShader.SetTexture(KERNEL_ID_Render, "Result", Result);
		MainShader.Dispatch(KERNEL_ID_Render, threadGroupsX, threadGroupsY, 1);

		Graphics.Blit(Result, destination);
	}
	public override void InitRenderTexture() => createTexture(ref Result, (int)WIDTH, (int)HEIGHT);
}
