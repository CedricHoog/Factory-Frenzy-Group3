using UnityEngine;
using TMPro;

public class SaveFileNameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //TODO IMPLEMENT THIS. This is here to make the left part of the text hidden instead of the right when the file name is too long
    public static string FitLeftEllipsis(TMP_Text t, string full)
    {
        float max = t.rectTransform.rect.width - t.margin.x - t.margin.z;
        if (t.GetPreferredValues(full).x <= max) return full;

        int start = 1;
        while (start < full.Length &&
               t.GetPreferredValues("…" + full.Substring(start)).x > max)
            start++;

        return "…" + full.Substring(start);
    }
}
