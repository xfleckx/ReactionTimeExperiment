using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System;
using Assets.LSL4Unity.Scripts;

public class Experiment : MonoBehaviour {

	public const string MISS = "MISS";

	public const string HIT = "HIT";

	public int TrialsToDo = 10;
	private int currentTrialCount = 0;

	public float timeShowingFocusCross = 1f;

	public float timeResponseTimeOut = 1f;

	public float timeBetweenColorChanges = 2f;
	
	public bool UseRandomRangeAsTimeBetweenColorChanges = false;

	public float maxTimeBetweenColorChanges = 2f;

	public float minTimeBetweenColorChanges = 2f;

	public AnimationCurve distribution;

	public Material defaultMaterial;
	public Material targetMaterial;
	public Material nonTargetMaterial;

	public GameObject focusCross;
	public GameObject targetSphere;
	private MeshRenderer sphereRenderer;

	public GameObject userInterface;

	public ArduinoController arduino;
	public CSVFileWriter dataRecorder;
	private Material currentMaterial;
	private bool awaitAnButtonPress;
	private bool aButtonHasBeenPressed;

	private LSLMarkerStream marker;

	void Start () {

		marker = GetComponent<LSLMarkerStream>();

		Assert.IsNotNull(marker);

		Assert.IsNotNull(focusCross);
		Assert.IsNotNull(targetSphere);
		Assert.IsNotNull(targetMaterial);
		Assert.IsNotNull(nonTargetMaterial);
		Assert.IsNotNull(userInterface);
		Assert.IsNotNull(arduino);
		Assert.IsNotNull(dataRecorder);

		sphereRenderer = targetSphere.GetComponent<MeshRenderer>();
		defaultMaterial = sphereRenderer.material;
	}

	public void StartExperiment()
	{
		userInterface.SetActive(false);
		
		currentTrialCount = TrialsToDo;

		StartCoroutine(RunExperiment());
	}

	IEnumerator RunExperiment()
	{
		marker.Write("Start Experiment");

		yield return new WaitForSecondsRealtime(3f);

		yield return RunTrials();

		yield return ReturnToStartMenu();
	}

	IEnumerator RunTrials()
	{
		marker.Write("Begin Trials");
		while(currentTrialCount > 0)
		{
			focusCross.SetActive(true);

			yield return new WaitForSecondsRealtime(timeShowingFocusCross);

			marker.Write("Enable Sphere");

			focusCross.SetActive(false);

			sphereRenderer.material = defaultMaterial;

			targetSphere.SetActive(true);

			float timeToWaitForTheNextColorChange = GetRandomOffsetForColorChange();

			yield return new WaitForSecondsRealtime(timeToWaitForTheNextColorChange);

			marker.Write("Change Color");
			currentMaterial = ChangeColor();

			yield return new WaitForEndOfFrame();

			marker.Write("End of Frame");

			dataRecorder.Write(new string[] { Time.realtimeSinceStartup.ToString(), "Await button press" });

			BeginAwaitAnButtonPress();
			
			yield return new WaitForSecondsRealtime(timeResponseTimeOut);

			EndAwaitAnButtonPress();

			if (!aButtonHasBeenPressed)
			{
				dataRecorder.Write(new string[] { Time.realtimeSinceStartup.ToString(), MISS });
			}

			targetSphere.SetActive(false);

			currentTrialCount--;
		}
		marker.Write("End Trials");
		dataRecorder.WriteEverythingToFile();
	}

	IEnumerator ReturnToStartMenu()
	{
		targetSphere.SetActive(false);

		focusCross.SetActive(false);

		userInterface.SetActive(true);

		yield return null;
	}
	
	private void BeginAwaitAnButtonPress()
	{
		awaitAnButtonPress = true;
		marker.Write("Begin Send Signal");
		arduino.AwaitButtonPress();
		marker.Write("End Send Signal");
		aButtonHasBeenPressed = false;
	}

	private void EndAwaitAnButtonPress()
	{
		awaitAnButtonPress = false;
	}

	public void OnArduinoEvent(ArduinoEvent evt)
	{
		if (!awaitAnButtonPress)
			return;
		
		string result = MISS;

		if (currentMaterial == targetMaterial)
			result = HIT;

		dataRecorder.Write(new string[] { Time.realtimeSinceStartup.ToString(), evt.reactionTime.ToString(), result });

		aButtonHasBeenPressed = true;

		awaitAnButtonPress = false;
	}

	private Material ChangeColor()
	{
		float random = UnityEngine.Random.value;

		float colorIndex = distribution.Evaluate(random);

		if (colorIndex >= 0.5)
			sphereRenderer.material = targetMaterial;
		else
			sphereRenderer.material = nonTargetMaterial;

		return sphereRenderer.sharedMaterial;
	}

	private float GetRandomOffsetForColorChange()
	{
		if(UseRandomRangeAsTimeBetweenColorChanges)
		{
			return UnityEngine.Random.Range(minTimeBetweenColorChanges, maxTimeBetweenColorChanges);
		}

		return timeBetweenColorChanges;
	}

}
