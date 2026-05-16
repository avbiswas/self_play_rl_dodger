using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvSettingsManager : MonoBehaviour
{
    Dictionary<string, List<float>> options_list = new Dictionary<string, List<float>>();
    int[] num_bullets_choices = {2, 5, 7, 8};
    float[] bullet_min_time_choices = {0.5f, 0.25f, 0.15f, 0.1f};
    float[] dash_cooldown_choices = {1.5f, 1f, 0.75f, 0.5f};

    SkillsController p1;
    SkillsController p2;

    List<OptionPicker> CreateRandomSetting(int num=2) {
        List<OptionPicker> opts = new List<OptionPicker>();
        List<string> optionNames = new List<string>(options_list.Keys);
        int numOptions = Mathf.Min(num, optionNames.Count);

        for (int i=0; i < numOptions; i ++){
            string optionName = optionNames[i];
            List<float> choices = options_list[optionName];
            float value = choices[Random.Range(0, choices.Count)];
            opts.Add(new OptionPicker(optionName, value));
        }
        return opts;
    }
    void Awake(){
        options_list.Add("Bullets", new List<float> {2.0f, 5.0f, 7.0f, 8.0f});
        options_list.Add("Bullet Refill", new List<float> {0.5f, 0.25f, 0.15f, 0.1f});
        options_list.Add("Dash Cooldown", new List<float> {1.5f, 1f, 0.75f, 0.5f});
    }

    void Start(){
        SkillsController[] ps = GetComponentsInChildren<SkillsController>();
        foreach (SkillsController p in ps){
            if (p.gameObject.name == "AI"){
                p2 = p;
            }
            else{
                p1 = p;
            }
        }    
    }
    
}
