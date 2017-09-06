using System.Collections.Generic;
using UnityEngine;
using System;


public class GearBehavior: MonoBehaviour
{
	private Graph _graph;
    private Node _selected;

    public enum State { move, run }
    public State state = State.move;

    public void toggle_state()
    {
        if (state == State.move)
        {
            run();
            state = State.run;
        }
        else state = State.move;
    }

    bool run()
    {
        bool all = true;
        foreach (Node n in _graph._nodes.FindAll(x => x.isMain))
        {
            if (!_graph.calculate_velocity(n))
            {
                Debug.Log("STOP RIGHT THERE! YOU CRIMINAL SCUM!");
                _graph.stop_recursive(n);
            }
        }
        return all;
    }

    void Start()
    {
		build_graph ();
        run();
        
	}

    void Moving()
    {
        if (_graph._nodes[0].joint.animator.speed == 0f) {}
        else if (Math.Abs(_graph._nodes[0].joint.animator.speed) > 0f)
            foreach (Node n in _graph._nodes.FindAll(n => !n.stopped))
            {
                n.joint.animator.speed -= (float)(Time.deltaTime * n.angular_velocity / 3);
            }
        else
            foreach (Node n in _graph._nodes.FindAll(n => !n.stopped))
            {
                n.joint.animator.speed = 0f;
            }
        if (Input.GetMouseButtonDown(0))
        {
            if (_selected == null)
            {
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if ((_selected = _graph.find(pos)) != null && _selected.isNotMoving) _selected = null;
            }
            else
            {
                _graph.delete(_selected);
                Vector2 old_pos = _selected.joint.pos;
                _selected.joint.pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (!_graph.insert(_selected))
                {
                    _selected.joint.pos = old_pos;
                    _graph.insert(_selected);
                }
                _selected = null;
            }
        }
    }

    void Update()
    {
        if (state == State.move)
            Moving();

        if (state == State.run)
        {
            Node node = _graph._nodes[0];
            if (node.joint.animator.speed == node.angular_velocity) {}
            else if (Math.Abs(node.joint.animator.speed) < Math.Abs(node.angular_velocity))
                foreach (Node n in _graph._nodes.FindAll(n => !n.stopped))
                {
                    n.joint.animator.speed += (float)(Time.deltaTime*n.angular_velocity/3);
                }
            else
                foreach (Node n in _graph._nodes.FindAll(n => !n.stopped))
                {
                    n.joint.animator.speed = (float)n.angular_velocity;
                }
        }
            
	}

	void build_graph()
    {
		_graph = new Graph();
        Animator[] anim = gameObject.GetComponentsInChildren<Animator>();
        byte i = 0;
		foreach (Node n in gameObject.GetComponentsInChildren<Node>())
        {
            anim[i].speed = 0;
            n.neighbors = new List<Node>();
            n.joint = new Joint(anim[i++]);
            _graph.insert(n);
        }
    }
}