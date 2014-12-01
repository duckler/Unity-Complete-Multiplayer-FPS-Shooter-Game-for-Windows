using UnityEngine;
using System.Collections;

public class BeamOrigin : MonoBehaviour {
	public GameObject beamPrefab;
	public GameObject shrapnelPrefab;
	
	[RPC]
	void Beam(Vector3 dest, Vector3 normal ) {
		GameObject goBeam = (GameObject)Instantiate(beamPrefab, transform.position, Quaternion.FromToRotation(Vector3.forward, dest-transform.position) );
		Vector3 s = new Vector3(1, 1, Vector3.Distance(transform.position, dest) );
		goBeam.transform.localScale = s;
		/*LineRenderer lrBeam = goBeam.GetComponent<LineRenderer>();
		lrBeam.useWorldSpace=true;
		lrBeam.SetVertexCount(2);
		lrBeam.SetPosition(0, orig);
		lrBeam.SetPosition(1, dest);*/
		BeamLock bl = goBeam.GetComponent<BeamLock>();
		bl.orig = transform;
		bl.dest = dest;
		Destroy (goBeam, .1f);
		
		
		Instantiate(shrapnelPrefab, dest, Quaternion.FromToRotation(Vector3.up, normal));
	}
	
}
