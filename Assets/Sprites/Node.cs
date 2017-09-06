using System.Collections.Generic;
using UnityEngine;
using System;


public class Node : MonoBehaviour
{
    public Joint joint;
    public List<Node> neighbors { get; set; }
    public bool isNotMoving;
    public bool isMain;
    public double angular_velocity = 0.0;
    public void Start() {}
    public bool stopped;
    public bool marked;
}
