using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class TutorialFlag : MonoBehaviour
    {
        public GameObject pcGroup, mobileGroup;

        // Start is called before the first frame update
        void Start()
        {
            if (pcGroup)
                pcGroup.SetActive(GameManager.Instance.controllerType == ControllerType.PC);
            if (mobileGroup)
                mobileGroup.SetActive(GameManager.Instance.controllerType == ControllerType.Mobile);
        }
    }
}