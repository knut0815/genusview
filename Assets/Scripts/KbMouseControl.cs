﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MouseMode { Look, Stick, Rotate };

public class KbMouseControl : MonoBehaviour {
	public GameObject camera;
	public GameObject stickHolder;
	public GameObject surface;
	public GameObject helpScreenParent;
	public GameObject h2view;
	public float turnRange = 180f;
	public float stickRange = 90f;
	private StickBehavior sb;
	private h2viewcontrol h2c;
	private MouseMode mouseMode = MouseMode.Look;
	private MouseMode savedMode = MouseMode.Look;
	private Quaternion stickInitQ, cameraInitQ, surfaceInitQ;
	private PaintableTexture pt = null;
	private HelpScreen helpScreen;
	private Vector3 surfaceDelta;

	public float speed = 10.0f;
	public float mouseSpeed = 4.0f;

	private Vector2 mpos = new Vector2(0,0);

	void Start() {
		pt = PaintableTexture.Instance;
		// Todo: replace below with singletons to avoid need for linking in the editor
		sb = stickHolder.GetComponent<StickBehavior>();
		helpScreen = helpScreenParent.GetComponent<HelpScreen>();
		h2c = h2view.GetComponent<h2viewcontrol>();
		stickInitQ = stickHolder.transform.localRotation;
		cameraInitQ = camera.transform.localRotation;
		surfaceInitQ = surface.transform.localRotation;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void doQuit() {
		#if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
         #else
         Application.Quit();
         #endif
	}

	void OnApplicationQuit() {
		Cursor.lockState = CursorLockMode.None;
	}

	Vector2 RelMousePos() {
		mpos.x += 20*Input.GetAxis("Mouse X") / Screen.width;
		mpos.y += 20*Input.GetAxis("Mouse Y") / Screen.height;
		return mpos;
	}

	void ResetMousePos() {
		mpos.x = 0f;
		mpos.y = 0f;
		stickInitQ = stickHolder.transform.localRotation;
		cameraInitQ = camera.transform.localRotation;
		surfaceInitQ = surface.transform.localRotation;
	}

	void Update () {
		float dt = Time.deltaTime;

		if  ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Q)) {
			doQuit();
		}

		if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.F1)) {
			helpScreen.ToggleVisibility();
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			helpScreen.Hide();
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			ResetMousePos();
			// Space bar toggles mode (walk/draw)
			if (mouseMode == MouseMode.Look) {
				mouseMode = MouseMode.Stick;
				sb.makeVisible();
			} else if (mouseMode == MouseMode.Stick) {
				sb.makeInvisible();
				mouseMode = MouseMode.Look;
			}
		}

		if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetMouseButtonDown(1)) {
			ResetMousePos();
			savedMode = mouseMode;
			if (mouseMode == MouseMode.Stick) {
				sb.makeInvisible();
			}
			mouseMode = MouseMode.Rotate;
			surfaceDelta = surface.transform.position - camera.transform.position; 
		}
		if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt) || Input.GetMouseButtonUp(1)) {
			ResetMousePos();
			mouseMode = savedMode;
			if (mouseMode == MouseMode.Stick) {
				sb.makeVisible();
			}
		}

		if (Input.GetKeyDown(KeyCode.Z)) {
			pt.Clear();
		}

		if (Input.GetKeyDown(KeyCode.K)) {
			h2c.Toggle();
			h2c.ExportMode();
		}

		// Walk & strafe according to keyboard
		float horiz = Input.GetAxis ("Horizontal") * speed;
		float depth = Input.GetAxis ("Vertical") * speed;
		camera.transform.Translate (horiz * dt, 0f, depth * dt);
	    if (mouseMode == MouseMode.Rotate) {
			surface.transform.position = camera.transform.position + surfaceDelta;
		}

		if (mouseMode == MouseMode.Look) {
			Vector2 mp = RelMousePos ();
			camera.transform.localRotation = cameraInitQ * Quaternion.Euler(0,turnRange*mp.x,0);
		}
		if (mouseMode == MouseMode.Stick) {
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(0)) {
				sb.startDrawing();
			} else {
				sb.stopDrawing();
			}
			Vector2 mp = RelMousePos ();
			stickHolder.transform.localRotation = stickInitQ * Quaternion.Euler(-0.5f*stickRange*mp.y,0,-0.5f*stickRange*mp.x);
		}
		if (mouseMode == MouseMode.Rotate) {
			Vector2 mp = RelMousePos ();
			surface.transform.localRotation = Quaternion.AngleAxis(turnRange*mp.y,camera.transform.right) * Quaternion.AngleAxis(-turnRange*mp.x,camera.transform.up) * surfaceInitQ;
		}
	}
}
