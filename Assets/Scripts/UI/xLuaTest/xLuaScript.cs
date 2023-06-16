using UnityEngine;
using XLua;

namespace UI.xLuaTest
{
    [Hotfix]
    public class xLuaScript : MonoBehaviour
    {
        public GameObject cube;
        
        public void OnGenObjBtnClick()
        {
            Instantiate(cube, new Vector3(0, 5, 10), Quaternion.identity);
        }
    }
}