using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTX : BaseRenderer
{
	private int KERNEL_ID_Render;

	private Camera _Camara;

	private void Awake()
	{
		_Camara = GetComponent<Camera>();

		KERNEL_ID_Render = MainShader.FindKernel("Render");
	}

	public override void Render(RenderTexture destination)
	{
		MainShader.SetTexture(KERNEL_ID_Render, "Result", Result);

		int threadGroupsX = Mathf.CeilToInt(WIDTH / 8.0f);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / 8.0f);

		MainShader.Dispatch(KERNEL_ID_Render, threadGroupsX, threadGroupsY, 1);

		Graphics.Blit(Result, destination);
	}
	public override void SetShaderParams()
	{
		MainShader.SetMatrix("_CameraToWorld", _Camara.cameraToWorldMatrix);
		MainShader.SetMatrix("_CameraInverseProjection", _Camara.projectionMatrix.inverse);
	}
	public override void InitRenderTexture()
	{
		createTexture(ref Result, (int)WIDTH, (int)HEIGHT);
	}
}
