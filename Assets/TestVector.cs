using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestVector : MonoBehaviour {
    List<Vector3> list = new List<Vector3>();
    public int i;
    public Vector3 vect3;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < 100000; i++)
        {
            list.Add(new Vector3(0, 0, 0));
        }
	}
}
