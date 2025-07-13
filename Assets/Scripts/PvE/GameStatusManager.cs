using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameStatusManager : MonoBehaviour
{
    [Header("Tower and Icon names must match")]
    public Tower[] Towers;
    public Image[] TowerIcons;
    public Color destroyedTowerOverlayColor = Color.gray;

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
            //TODO: victory / defeat | pause screens 
        }
        else if (AllTowersDown("T1"))
        {

        }
    }

    public bool AllTowersDown(string TeamPointer)
    {
        Tower findT = Towers.Where(i => i != null && i.name.Contains(TeamPointer.ToUpper())).FirstOrDefault();
        return findT == null;
    }
}
