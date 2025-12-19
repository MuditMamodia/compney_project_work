using System;
using UnityEngine;

public class SessionIDHelper : MonoBehaviour
{
    public static SessionIDHelper _SIH;

    internal string _Session_ID;
    internal int _question_count, _correct_answers_count;

    void Awake()
    {
        if (_SIH)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _SIH = this;
            DontDestroyOnLoad(gameObject);

            _Session_ID = Guid.NewGuid().ToString();
        }
    }
}
