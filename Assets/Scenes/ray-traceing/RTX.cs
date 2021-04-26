using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTX : BaseRenderer
{
	private int KERNEL_ID_Render;

	private Camera _Camara;

	public Texture SkyboxTexture;

	private uint _currentSample = 0;
	private Material _addMaterial;

	public Light DirectionalLight;

	private void Awake()
	{
		_Camara = GetComponent<Camera>();

		KERNEL_ID_Render = MainShader.FindKernel("Render");
	}

	private void Update()
	{
		if (transform.hasChanged)
		{
			_currentSample = 0;
			transform.hasChanged = false;
		}
		if (DirectionalLight.transform.hasChanged)
		{
			_currentSample = 0;
			DirectionalLight.transform.hasChanged = false;
		}
	}

	public override void Render(RenderTexture destination)
	{
		MainShader.SetTexture(KERNEL_ID_Render, "Result", Result);

		int threadGroupsX = Mathf.CeilToInt(WIDTH / 8.0f);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / 8.0f);

		MainShader.Dispatch(KERNEL_ID_Render, threadGroupsX, threadGroupsY, 1);

		if (_addMaterial == null)
			_addMaterial = new Material(Shader.Find("Hidden/AddShader"));

		_addMaterial.SetFloat("_Sample", _currentSample);

		Graphics.Blit(Result, destination, _addMaterial);

		_currentSample++;
	}
	public override void SetShaderParams()
	{
		MainShader.SetMatrix("_CameraToWorld", _Camara.cameraToWorldMatrix);
		MainShader.SetMatrix("_CameraInverseProjection", _Camara.projectionMatrix.inverse);

		MainShader.SetTexture(KERNEL_ID_Render, "_SkyboxTexture", SkyboxTexture);

		MainShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));

		var light = DirectionalLight.transform.forward;
		MainShader.SetVector("_DirectionalLight", new Vector4(light.x, light.y, light.z, DirectionalLight.intensity));
	}
	public override void InitRenderTexture()
	{
		createTexture(ref Result, (int)WIDTH, (int)HEIGHT);
	}
}
