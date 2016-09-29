using UnityEngine;
using System.Collections;
using Assets.LSL4Unity.Scripts;

public class FlaschScene : MonoBehaviour {

	public GameObject target;

	LSLMarkerStream marker;

	private int currentFrame = 0;

	private bool running = false;

	public bool autoStart = true;

	void Start () {
		Application.targetFrameRate = 60;
		marker = FindObjectOfType<LSLMarkerStream>();

		if (autoStart)
		{
			startFlashSequence();
		}
	}

	void Update()
	{
		if (!running && Input.GetKeyUp(KeyCode.Return))
		{
			startFlashSequence();
		}
	}

	private void startFlashSequence()
	{
		StartCoroutine(Run());
	}

	public void writeMarker(string moment)
	{
		marker.Write(string.Format("{0}; {1}", moment, Time.renderedFrameCount));
	}

	IEnumerator Run()
	{
		running = true;
		writeMarker("Wait 0.1 sec");
		yield return new WaitForSecondsRealtime(0.1f);

		writeMarker("SetActive");

		target.SetActive(true);

		yield return new WaitForEndOfFrame();
		writeMarker("EndOfFrame");

		yield return new WaitForEndOfFrame();
		writeMarker("EndOfFrame");

		target.SetActive(false);
		writeMarker("SetInActive");

		currentFrame = Time.renderedFrameCount;

		yield return new WaitUntil(() => currentFrame != Time.renderedFrameCount);

		writeMarker("NewFrame");
	}
	
}
