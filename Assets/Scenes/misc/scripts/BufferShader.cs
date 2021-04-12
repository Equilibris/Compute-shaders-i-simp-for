using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferShader : MonoBehaviour
{
	private struct Agent
	{
		public Vector2 position;
		public float angle;
	}

	public int COUNT = 10000;

	private int KERNEL_ID_Update;
	private int KERNEL_ID_Process;
	private int KERNEL_ID_Build;

	public ComputeShader MainShader;

	private RenderTexture _target;
	private ComputeBuffer _agents;
	private Agent[] data;

	private const int WARP_SIZE = 256;

	private int WARP_COUNT;

	private void Awake()
	{
		SetInitalAgentData();

		WARP_COUNT = Mathf.CeilToInt((float)COUNT / WARP_SIZE);

		KERNEL_ID_Process = MainShader.FindKernel("Process");
		KERNEL_ID_Update = MainShader.FindKernel("Update");
		KERNEL_ID_Build = MainShader.FindKernel("BuildAgents");
	}

	private void SetInitalAgentData()
	{
		data = new Agent[COUNT];

		for (int i = 0; i < COUNT; i++)
		{
			var agent = new Agent
			{
				position = new Vector2(Random.Range(10, Screen.width), Random.Range(10, Screen.height)),
				angle = Random.Range(0, Mathf.PI * 2),
			};
			data[i] = agent;
		}

		_agents = new ComputeBuffer(data.Length, 4 * 3 * sizeof(float));
		_agents.SetData(data);

		// MainShader.SetInt("width", Screen.width);
		// MainShader.SetInt("height", Screen.height);

		// MainShader.SetInt("numAgents", COUNT);

		// MainShader.Dispatch(KERNEL_ID_Build, COUNT / 32, 1, 1);
	}

	private void OnDisable()
	{
		if (_agents != null) _agents.Release();
	}
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		DoRenderOp();

		Graphics.Blit(_target, destination);
	}
	private void DoRenderOp()
	{
		InitRenderTexture();
		SetShaderParameters();

		int threadGroupsX = Mathf.CeilToInt(Screen.width / 8f);
		int threadGroupsY = Mathf.CeilToInt(Screen.height / 8f);

		MainShader.Dispatch(KERNEL_ID_Process, threadGroupsX, threadGroupsY, 1);
		MainShader.Dispatch(KERNEL_ID_Update, WARP_COUNT, 1, 1);
	}
	private void SetShaderParameters()
	{
		MainShader.SetInt("width", Screen.width);
		MainShader.SetInt("height", Screen.height);

		MainShader.SetInt("numAgents", COUNT);
		MainShader.SetBuffer(KERNEL_ID_Update, "agents", _agents);

		MainShader.SetFloat("movementSpeed", 100);
		MainShader.SetTexture(KERNEL_ID_Update, "Result", _target);
		MainShader.SetTexture(KERNEL_ID_Process, "Result", _target);
		MainShader.SetFloat("deltaTime", Time.deltaTime);
	}
	private void InitTexture(ref RenderTexture tex, RenderTextureFormat format)
	{
		if (tex == null || tex.width != Screen.width || tex.height != Screen.height)
		{
			if (tex != null) tex.Release();

			tex = new RenderTexture(Screen.width, Screen.height, 0, format, RenderTextureReadWrite.Linear);
			tex.enableRandomWrite = true;
			tex.Create();
		}
	}

	private void InitRenderTexture() => InitTexture(ref _target, RenderTextureFormat.ARGBFloat);
}
