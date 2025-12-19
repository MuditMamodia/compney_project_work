using System.Collections.Generic;
using UnityEngine;


public class Game_manager_m : MonoBehaviour
{
    public List<RectTransform> _Page_rects;


    private void Start()
    {
        Switch_page(1);
    }

    internal void Switch_page(int p_index)
    {
        if (_Page_rects == null || _Page_rects.Count == 0 || p_index < 0 || p_index >= _Page_rects.Count)
        {
            Debug.LogError($"Invalid page index: {p_index}");
            return;
        }

        for (int i = 0; i < _Page_rects.Count; i++)
        {
            if (_Page_rects[i] != null)
            {
                _Page_rects[i].gameObject.SetActive(i == p_index);
            }
        }

        if (p_index == 0)
        {
            // Write index 0 logic here
        }
        if (p_index == 1)
        {
            // Write index 1 logic here
        }
    }
}
