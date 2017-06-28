using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public float speed;
	public Text countText;
	public Text winText;
	public Text lastRunText;
	public GameObject pickupPrefab;
	public GameObject levelGatePrefab;

	private int level;
	private int count;
	private int totalPickups;
	private float levelStart;
	private LevelGate levelGate;

	private float lastPositionSend;

	void Start()
	{
		Application.runInBackground = true;

		_setupLevel(level);

		lastPositionSend = Time.fixedTime;
	}
	
	void FixedUpdate()
	{
		if (Input.GetButtonUp("Stop")) {
			Input.ResetInputAxes();

			GetComponent<Rigidbody>().velocity = new Vector3();
		} else {
			float moveHorizontal = Input.GetAxis("Horizontal");
			float moveVertical = Input.GetAxis("Vertical");

			Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

			GetComponent<Rigidbody>().AddForce(movement * speed * Time.deltaTime);
		}

		float now = Time.fixedTime;
		if ((now - lastPositionSend) > 0.03333) {
			lastPositionSend = now;
			NetController.SendPosition(transform.position);
		}

		if (levelGate != null) {
			Vector3 distance = levelGate.transform.position - transform.position;
			if (distance.magnitude > 1) {
				levelGate.transform.rotation = Quaternion.LookRotation(distance.normalized);
			}
		}
	}

	void _setupLevel(int newLevel)
	{
		count = 0;
		level = newLevel;
		winText.text = "";
		int numPickups = 2 + Mathf.RoundToInt(Mathf.Pow(1.8f, level));
		float radius = 6;
		for (int i = 0; i < numPickups; ++i)
		{
			float angle = i * Mathf.PI * 2 / numPickups;
			Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0.5f, Mathf.Sin(angle) * radius);
			Instantiate (pickupPrefab, pos, Quaternion.identity);
		}
		totalPickups = GameObject.FindGameObjectsWithTag("Pickup").Length;
		_setCountText();
		
		if (PlayerPrefs.HasKey("LastRunDuration" + level.ToString())) {
			lastRunText.text = "Last Run: " + PlayerPrefs.GetFloat("LastRunDuration" + level.ToString()).ToString("F2");
		} else {
			lastRunText.text = "Last Run: N/A";
		}
		
		levelStart = Time.timeSinceLevelLoad;
	}

	void _setCountText()
	{
		countText.text = "Pickups: " + count.ToString() + " / " + totalPickups.ToString();
	}

	Vector3 _levelGateVector()
	{
		return new Vector3(0.0f, 0.5f, 0.0f);
	}

	float _levelDuration()
	{
		return Time.timeSinceLevelLoad - levelStart;
	}

	void _checkWinLevel()
	{
		if (count >= totalPickups) {
			float runTime = _levelDuration();
			winText.text = "<size=48>YOU WIN!</size>\n<size=24>Time: " + runTime.ToString("F2") + "</size>";
			PlayerPrefs.SetFloat("LastRunDuration" + level.ToString(), runTime);

			Vector3 position = _levelGateVector();
			Quaternion angle = Quaternion.LookRotation((position - transform.position).normalized);

			GameObject obj = Instantiate(levelGatePrefab, position, angle) as GameObject;
			levelGate = obj.GetComponent<LevelGate>();
			levelGate.label = "Next Level";
			levelGate.level = level + 1;
		}
	}

	void _handlePickup(GameObject pickup)
	{
		Destroy(pickup);
		++count;
		_setCountText();
		_checkWinLevel();
	}

	void _handleLevelGate(GameObject obj)
	{
		GameObject gate = obj.transform.parent.gameObject;
		LevelGate scr = gate.GetComponent<LevelGate>();
		_setupLevel(scr.level);
		levelGate = null;
		Destroy(gate);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Pickup") {
			_handlePickup(other.gameObject);
		} else if (other.gameObject.tag == "LevelGate") {
			_handleLevelGate(other.gameObject);
		}
	}
}
