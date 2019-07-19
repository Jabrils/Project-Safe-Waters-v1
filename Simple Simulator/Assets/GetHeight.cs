using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GetHeight : MonoBehaviour
{
    public Camera cam;
    GameObject water;
    TextMeshProUGUI depth;
    public Measure current = new Measure(0);

    // Start is called before the first frame update
    void Start()
    {
        water = GameObject.Find("Water");
        depth = GameObject.Find("slope").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        current = CheckWaterDepth();
        depth.text = $"Depth: {current.TheMeasure()}";
    }

    Measure CheckWaterDepth()
    {
        float actorHeight = 6;
        float gameHeight = 2;

        float norm = actorHeight / gameHeight;

        //Vector3 from = transform.position + (Vector3.up);
        Vector3 from = cam.transform.position;

        //Ray rD = new Ray(from, (-transform.up + transform.forward).normalized);
        Ray rD = new Ray(from, cam.transform.forward);
        //Ray rU = new Ray(from + (Vector3.up*5), cam.transform.forward);
        RaycastHit hit;

        bool isWater = false;
        Vector3 ground = Vector3.zero;

        // 
        if (Physics.Raycast(rD, out hit))
        {
            isWater = hit.collider.gameObject == water;
        }

        // 
        if (isWater)
        {
            water.layer = 2;
        }
        else
        {
            //if (Physics.Raycast(rU, out hit))
            //{
            //    isWater = hit.collider.gameObject == water;
            //}

            //if (isWater)
            //{
            //    float above = Mathf.Abs((water.transform.position - ground).y) * norm;

            //    return new Measure(above);
            //}
            //else
            //{
                return new Measure(0);
            //}
        }

        // 
        if (Physics.Raycast(rD, out hit))
        {
            ground = hit.point;
        }

        float deep = Mathf.Abs((water.transform.position - ground).y) * norm;

        water.layer = 0;

        return new Measure(deep);
    }

}

public class Measure
{
    float _raw;
    public float raw { get { return _raw; } }
    int _feet;
    public int feet {get {return _feet;}}
    float _inches;
    public float inches { get { return _inches; } }

    public Measure(float m)
    {

        _raw = m;

        _feet = (int)m;

        _inches = Mathf.Round(Remainder(m)*100);
    }

    public string TheMeasure()
    {
        return $"{feet}'{inches}\"";
    }

    float Remainder(float inp)
    {
        int t = (int)inp;

        return inp - t;
    }
}
