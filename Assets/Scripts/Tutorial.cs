using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private List<GameObject> tutorial;
    [SerializeField] private List<int> skipscenes;
    [SerializeField] private Button button;
    private int active = 0;
    //private int currentscene =1;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            tutorial[0].gameObject.SetActive(true);
            //NewSwipeSystem.instance.Buttons(0);
            foreach (GameObject g in tutorial)
            {
                g.SetActive(tutorial.IndexOf(g) == 0);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Next()
    {
        if(!PlayerPrefs.HasKey("Tutorial"))
        {
            active += 1;
            //if (skipscenes.Contains(active))
            //{
            //    currentscene += 1;
            //}
            foreach (GameObject g in tutorial)
            {
                g.SetActive(tutorial.IndexOf(g) == active);
            }
            if(active == tutorial.Count)
            {
                PlayerPrefs.SetInt("Tutorial", 1);
                gameObject.SetActive(false);
            }
        }
    }
}
