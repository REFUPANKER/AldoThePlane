using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStatusManager : MonoBehaviour
{
    [Header("Tower and Icon names must match")]
    public Tower[] Towers;
    public Image[] TowerIcons;
    public Color destroyedTowerOverlayColor = Color.gray;

    public GameObject GameEndedScreen;
    public GameObject VictoryScreen;
    public GameObject DefeatScreen;

    public void TowerDown(Tower t)
    {
        Tower findT = Towers.Where(i => i == t).FirstOrDefault();
        findT?.TowerExplosion();
        Image findTI = TowerIcons.Where(i => i.name.StartsWith(t.name)).FirstOrDefault();
        if (findTI != null)
        {
            findTI.color = destroyedTowerOverlayColor;
        }

        if (AllTowersDown("T2"))
        {
            GameIsEnded("T1");
        }
        else if (AllTowersDown("T1"))
        {
            GameIsEnded("T2");
        }
    }

    public bool AllTowersDown(string TeamPointer)
    {
        Tower findT = Towers.Where(i => i != null && i.name.Contains(TeamPointer.ToUpper())).FirstOrDefault();
        return findT == null;
    }

    void Start()
    {
        GameEndedScreen.SetActive(false);
        DefeatScreen.SetActive(false);
        VictoryScreen.SetActive(false);
    }
    void GameIsEnded(string Winner)
    {
        Time.timeScale = 0;
        // stop game (spawners)
        GameEndedScreen.SetActive(true);
        if (Winner == "T1")
        {
            VictoryScreen.SetActive(true);
        }
        else
        {
            DefeatScreen.SetActive(true);
        }
    }
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
