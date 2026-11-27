using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class NemesisDungeonPortal : MonoBehaviour
{
    public string destinationScene = "AshenDungeon";
    public bool requiresBossDefeated;

    public void Enter()
    {
        if (requiresBossDefeated)
        {
            var enemies = FindObjectsByType<NemesisEnemy>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.name.Contains("Hollow Knight"))
                {
                    NemesisHUD.Instance?.SetMessage("ПУТЬ ПЕРЕГОРОЖЕН: ПОЛЫЙ РЫЦАРЬ ЕЩЁ ЖИВ");
                    return;
                }
            }
        }
        if (Application.CanStreamedLevelBeLoaded(destinationScene))
            SceneManager.LoadScene(destinationScene);
        else
            NemesisHUD.Instance?.SetMessage("ПУТЬ ЗАПЕЧАТАН: ЛОКАЦИЯ НЕ ВКЛЮЧЕНА В СБОРКУ");
    }
}
