using UnityEngine;
using Nemesis.Player;

namespace Nemesis.Interaction
{
    public sealed class NemesisWorldInteractable : MonoBehaviour
    {
        [SerializeField] private string displayName = "Interactable";
        [SerializeField] private string prompt = "Interact";
        [SerializeField, TextArea] private string message = "Nothing happens.";
        [SerializeField, Min(0)] private int essenceReward;
        [SerializeField] private bool singleUse;
        private bool consumed;

        public string DisplayName => displayName;
        public string Prompt => consumed ? "Already used" : prompt;
        public bool CanInteract => !singleUse || !consumed;

        private void Awake()
        {
            SphereCollider trigger = GetComponent<SphereCollider>();
            if (trigger == null) trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.25f;
            trigger.center = new Vector3(0f, .9f, 0f);
        }

        public void Configure(string title, string actionPrompt, string response,
            int rewardEssence = 0, bool oneUse = false)
        {
            displayName = title;
            prompt = actionPrompt;
            message = response;
            essenceReward = Mathf.Max(0, rewardEssence);
            singleUse = oneUse;
            consumed = false;
        }

        public string Interact(GameObject player)
        {
            if (!CanInteract) return displayName + ": здесь больше нечего взять.";
            string result = message;
            if (essenceReward > 0)
            {
                PlayerResources resources = player != null ? player.GetComponent<PlayerResources>() : null;
                if (resources != null)
                {
                    resources.RestoreEssence(essenceReward);
                    result += "\nПолучено: +" + essenceReward + " Essence.";
                }
            }
            if (singleUse) consumed = true;
            return result;
        }
    }
}
