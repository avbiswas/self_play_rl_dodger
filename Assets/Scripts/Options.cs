using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OptionPicker{
    string type;
    float value;

    public OptionPicker(string t, float v){
        type = t;
        value = v;
    }
}
public class Options : MonoBehaviour
{
    [SerializeField] GameObject OptionsMenu;

    [SerializeField] TextMeshProUGUI[] bs;

    // Start is called before the first frame update
    void Start()
    {
        OptionsMenu.SetActive(false);
    }

    public void OnClickChoice(int idx){
        Debug.Log(bs[idx].text);
    }
    public void ShowMenu(){
        Time.timeScale = 0f;
        OptionsMenu.SetActive(true);
    }

}
