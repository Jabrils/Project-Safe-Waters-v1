using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class ctrl : MonoBehaviour
{
    public GameObject pho, water, chooseUI;
    public GetHeight getH;
    public TMP_InputField genNumb;
    public int count = 10;
    int img;
    TextMeshProUGUI depth;
    float pT = 1;
    bool take = true;
    string[] write;
    int mode = 0;

    string grab = "Grabs", data = "Data", imgs = "Imgs";

    GameObject[] v = new GameObject[3];
    Vector3 wH;

    // Start is called before the first frame update
    void Start()
    {
        v[0] = GameObject.Find("B");
        v[1] = GameObject.Find("X");
        v[2] = GameObject.Find("Z");

        water = GameObject.Find("Water");
        wH = water.transform.position;
        depth = GameObject.Find("slope").GetComponent<TextMeshProUGUI>();
        depth.gameObject.SetActive(false);

        write = new string[count];

        if (!Directory.Exists(Path.Combine(Application.dataPath, grab)))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, grab));
        }

        if (!Directory.Exists(Path.Combine(Application.dataPath, grab, data)))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, grab, data));
        }

        if (!Directory.Exists(Path.Combine(Application.dataPath, grab, imgs)))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, grab, imgs));
        }

        PlacePhotographer();
    }

    void PlacePhotographer()
    {

        Vector3 rand = new Vector3(Random.Range(0f, 350f), 0, Random.Range(0f, 350f));

        Ray r = new Ray(v[0].transform.position - rand, Vector3.down);
        RaycastHit hit;

        water.layer = 2;

        // 
        if (Physics.Raycast(r, out hit))
        {
            pho.transform.position = hit.point + Vector3.up;
            pho.transform.eulerAngles = new Vector3(0, Random.value * 360, 0);

            float off = Random.Range(-1.5f, 1.5f);

            water.transform.position = new Vector3(wH.x, hit.point.y + off, wH.z);
        }

        water.layer = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (mode == 1)
        {
            if (getH.current.feet == 0 || pho.transform.position.y >= 344 || getH.current.feet >= 25)//Input.GetKeyDown(KeyCode.S))
            {
                PlacePhotographer();
            }
            else if (take && pT <= .1f)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, grab, imgs, $"Grab{img}.png"));
                write[img] = $"{getH.current.raw}";
                img++;
                take = false;
            }
            else if (img < count && pT <= 0)
            {
                PlacePhotographer();
                pT = .2f;
                take = true;
            }
            else if (img == count)
            {
                using (StreamWriter sW = new StreamWriter(Path.Combine(Application.dataPath, grab, data, "labels.txt")))
                {
                    foreach (string i in write)
                    {
                        sW.WriteLine(i);
                    }
                }
            }
            else
            {
                pT -= Time.deltaTime;
            }
        }
    }

    public void Generate()
    {
        count = int.Parse(genNumb.text);
        mode++;
        chooseUI.SetActive(false);
        write = new string[count];
    }
}
