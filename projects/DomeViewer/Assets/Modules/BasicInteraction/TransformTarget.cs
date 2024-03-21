using UnityEngine;

namespace pfc.Fulldome
{
    public class TransformTarget : MonoBehaviour
    {
        public void ScaleTarget(float value) 
        {
            gameObject.transform.localScale = new Vector3(value,value,value);
        }
    }
}
