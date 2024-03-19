using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pfc.Fulldome
{
    public class DesktopEnabler : MonoBehaviour
    {   
        void Awake()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
