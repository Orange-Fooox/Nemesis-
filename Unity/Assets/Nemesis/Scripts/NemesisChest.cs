using UnityEngine;

public class NemesisChest : MonoBehaviour
{
    public bool opened;
    public void Open()
    {
        if(opened){NemesisHUD.Instance?.SetMessage("В сундуке ничего нет.");return;}
        opened=true;
        var sr=GetComponent<SpriteRenderer>(); if(sr!=null)sr.color=new Color(0.35f,0.28f,0.18f);
        NemesisHUD.Instance?.SetMessage("Найден пепельный знак  |  +1 реликт");
        NemesisHUD.Instance?.AddRelic();
    }
}
