using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lifeFromLaptop : MonoBehaviour
{
	public ComputeShader MainShader;

	private RenderTexture Intermediary;
	private RenderTexture Result;
	public RenderTexture Life;

	private int KERNEL_ID_Init;
	private int KERNEL_ID_Render;
	private int KERNEL_ID_LifeCycle;

	private int WIDTH, HEIGHT;

	private Vector2 ThreadBlockSize = new Vector2(8, 8);
	private Vector2 LifeDimentions = new Vector2(10000, 10000);

	public float speed = 10;
	public float dampening = 0.9f;
	public Vector2 displacement = new Vector2(0, 0);
	private Vector2 acceleration = new Vector2(0, 0);

	public float zoomSpeed = 1;
	private float zoomAcceleration = 0;
	public float zoomFactor = 1;
	public bool lifeIsActive = true;

	private void Awake()
	{
		KERNEL_ID_Init = MainShader.FindKernel("Init");
		KERNEL_ID_Render = MainShader.FindKernel("Render");
		KERNEL_ID_LifeCycle = MainShader.FindKernel("LifeCycle");

		StartCoroutine(doLife());
	}

	private IEnumerator doLife()
	{
		while (true)
		{
			if (!lifeIsActive) yield return new WaitForEndOfFrame();
			else
			{
				Graphics.Blit(Life, Intermediary);

				SetShaderParams();

				MainShader.SetTexture(KERNEL_ID_LifeCycle, "Life", Life);
				MainShader.SetTexture(KERNEL_ID_LifeCycle, "Intermediary", Intermediary);

				int threadGroupsX = Mathf.CeilToInt(LifeDimentions.x / ThreadBlockSize.x);
				int threadGroupsY = Mathf.CeilToInt(LifeDimentions.y / ThreadBlockSize.y);

				MainShader.Dispatch(KERNEL_ID_LifeCycle, threadGroupsX, threadGroupsY, 1);

				yield return new WaitForSeconds(0.05f);
				// yield return new WaitForEndOfFrame();
			}
		}
	}

	private void Update()
	{
		bool tokyoDrift = Input.GetKey(KeyCode.LeftShift),
					 up = Input.GetKey(KeyCode.W),
				   down = Input.GetKey(KeyCode.S),
				  right = Input.GetKey(KeyCode.D),
				   left = Input.GetKey(KeyCode.A),
				   zoom = Input.GetKey(KeyCode.Q),
				 unZoom = Input.GetKey(KeyCode.E);

		if (Input.GetKeyDown(KeyCode.Space)) lifeIsActive ^= true;

		float mainSpeed = speed * zoomFactor;

		if (zoom) zoomAcceleration += zoomSpeed;
		if (unZoom) zoomAcceleration -= zoomSpeed;

		if (up) acceleration.y += mainSpeed;
		if (down) acceleration.y -= mainSpeed;

		if (right) acceleration.x += mainSpeed;
		if (left) acceleration.x -= mainSpeed;

		if (!tokyoDrift) acceleration *= dampening;
		zoomAcceleration *= dampening;

		displacement += acceleration * Time.deltaTime;
		zoomFactor = Mathf.Clamp(zoomFactor + zoomAcceleration, 0.1f, 1f);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		WIDTH = Screen.width;
		HEIGHT = Screen.height;

		SetShaderParams();
		Render(destination);
	}
	private void SetShaderParams()
	{
		MainShader.SetInt("M_WIDTH", WIDTH);
		MainShader.SetInt("M_HEIGHT", HEIGHT);

		MainShader.SetInt("L_WIDTH", (int)LifeDimentions.x);
		MainShader.SetInt("L_HEIGHT", (int)LifeDimentions.y);

		MainShader.SetFloat("now", Time.time);

		MainShader.SetVector("displacement", displacement);
	}
	private void Render(RenderTexture destination)
	{
		InitRenderTexture();

		int threadGroupsX = Mathf.CeilToInt(WIDTH / ThreadBlockSize.x);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / ThreadBlockSize.y);

		MainShader.SetTexture(KERNEL_ID_Render, "Life", Life);
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
		if (createTexture(ref Life, (int)LifeDimentions.x, (int)LifeDimentions.y))
		{
			MainShader.SetTexture(KERNEL_ID_Init, "Life", Life);
			int threadGroupsX = Mathf.CeilToInt(LifeDimentions.x / ThreadBlockSize.x);
			int threadGroupsY = Mathf.CeilToInt(LifeDimentions.y / ThreadBlockSize.y);
			MainShader.Dispatch(KERNEL_ID_Init, threadGroupsX, threadGroupsY, 1);
		}
		createTexture(ref Intermediary, (int)LifeDimentions.x, (int)LifeDimentions.y);
		createTexture(ref Result, (int)(WIDTH * zoomFactor), (int)(HEIGHT * zoomFactor));
	}
}
