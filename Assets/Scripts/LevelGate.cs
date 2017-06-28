using UnityEngine;
using System.Collections;

public class LevelGate : MonoBehaviour {
	private string _label;

	public string label
	{
		set {
			_label = value;
			Transform labelObj = transform.FindChild("Label");
			TextMesh meshObj = labelObj.GetComponent<TextMesh>();
			meshObj.text = _label;
		}
		get { return _label; }
	}

	public int level;
}
