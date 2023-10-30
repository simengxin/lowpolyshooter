using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PhoenixaStudio
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        public int testLevelMap = 1;

        //void Awake()
        //{
        //    Instance = this;

        //    if (FindObjectOfType<LevelWave>())
        //    {
        //        Debug.LogError("Notice: There are a Level on this scene!");
        //        return;
        //    }

        //    if (GameMode.Instance)
        //    {
        //        //Instantiate(LevelMaps[GlobalValue.levelPlaying - 1], Vector2.zero, Quaternion.identity);
        //        var _go = Resources.Load("Levels/Level " + GlobalValue.levelPlaying) as GameObject;
        //        if (_go != null)
        //            Instantiate(_go, Vector2.zero, Quaternion.identity);
        //        else
        //            Debug.LogError("NO LEVEL IN RESOURCE: Level " + GlobalValue.levelPlaying);
        //    }
        //    else
        //    {
        //        Debug.LogError("PLAY GAME CORRECTLY FROM THE LOGO SCENE");
        //        var _go = Resources.Load("Levels/Level " + testLevelMap) as GameObject;
        //        if (_go != null)
        //            Instantiate(_go, Vector2.zero, Quaternion.identity);
        //        else
        //            Debug.LogError("NO LEVEL IN RESOURCE: Level " + testLevelMap);
        //    }
        //}
    }
}