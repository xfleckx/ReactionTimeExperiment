using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System;
public class Experiment : MonoBehaviour {

	public int TrialsToDo = 10;
	private int currentTrialCount = 0;

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

	void Start () {

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
		yield return new WaitForSecondsRealtime(3f);

		yield return RunTrials();

		yield return ReturnToStartMenu();
	}

	IEnumerator ReturnToStartMenu()
	{
		targetSphere.SetActive(false);

		focusCross.SetActive(false);

		userInterface.SetActive(true);

		yield return null;
	}

	IEnumerator RunTrials()
	{
		while(currentTrialCount > 0)
		{
			focusCross.SetActive(true);

			yield return new WaitForSecondsRealtime(1f);

			focusCross.SetActive(false);

			sphereRenderer.material = defaultMaterial;

			targetSphere.SetActive(true);

			float timeToWaitForTheNextColorChange = GetTimeBetweenColorChanges();

			yield return new WaitForSecondsRealtime(timeToWaitForTheNextColorChange);

			currentMaterial = ChangeColor();

			yield return new WaitForEndOfFrame();

			dataRecorder.Write(new string[] { Time.realtimeSinceStartup.ToString(), "Await button press" });

			BeginAwaitAnButtonPress();
			
			yield return new WaitForSecondsRealtime(1f);

			EndAwaitAnButtonPress();

			if (!aButtonHasBeenPressed)
			{
				dataRecorder.Write(new string[] { Time.realtimeSinceStartup.ToString(), "MISS" });
			}

			targetSphere.SetActive(false);

			currentTrialCount--;
		}
		
	}

	private void BeginAwaitAnButtonPress()
	{
		awaitAnButtonPress = true;

		arduino.AwaitButtonPress();

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
		
		string result = "MISS";

		if (currentMaterial == targetMaterial)
			result = "HIT";

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

	private float GetTimeBetweenColorChanges()
	{
		if(UseRandomRangeAsTimeBetweenColorChanges)
		{
			return UnityEngine.Random.Range(minTimeBetweenColorChanges, maxTimeBetweenColorChanges);
		}

		return timeBetweenColorChanges;
	}

}
