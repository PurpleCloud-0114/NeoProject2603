using System;
using UnityEngine;

[Serializable,CreateAssetMenu]
public class MapSize : ScriptableObject {
	public Vector3 map_center = Vector3.zero;
	public float boundaryRadius = 70f;
}
