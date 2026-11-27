using UnityEngine;
using Nemesis.Controls;

namespace Nemesis.Interaction
{
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField, Min(.5f)] private float interactionRange = 2.7f;

        private NemesisWorldInteractable currentTarget;
        private string feedback;
        private float feedbackUntil;
        private GUIStyle promptStyle;
        private GUIStyle feedbackStyle;

        private void Update()
        {
            currentTarget = FindNearestTarget();

            if (currentTarget != null &&
                NemesisInput.WasPressed(NemesisInputAction.Interact))
            {
                feedback = currentTarget.Interact(gameObject);
                feedbackUntil = Time.time + 4f;

                if (!currentTarget.CanInteract)
                    currentTarget = null;
            }
        }

        private NemesisWorldInteractable FindNearestTarget()
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position + Vector3.up,
                interactionRange,
                ~0,
                QueryTriggerInteraction.Collide);

            NemesisWorldInteractable nearest = null;
            float nearestDistance = interactionRange;

            foreach (Collider hit in hits)
            {
                NemesisWorldInteractable candidate =
                    hit.GetComponentInParent<NemesisWorldInteractable>();

                if (candidate == null || !candidate.CanInteract) continue;

                float distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void OnGUI()
        {
            if (currentTarget != null && currentTarget.CanInteract)
            {
                EnsureStyles();
                const float width = 360f;
                GUI.Box(
                    new Rect((Screen.width - width) * .5f, Screen.height - 78f, width, 38f),
                    currentTarget.DisplayName + "   [E] " + currentTarget.Prompt,
                    promptStyle);
            }

            if (!string.IsNullOrEmpty(feedback) && Time.time < feedbackUntil)
            {
                EnsureStyles();
                const float width = 470f;
                const float height = 94f;
                Rect panel = new Rect(
                    (Screen.width - width) * .5f,
                    Screen.height - 190f,
                    width,
                    height);
                GUI.Box(panel, GUIContent.none);
                GUI.Label(new Rect(panel.x + 16f, panel.y + 10f, panel.width - 32f, panel.height - 20f),
                    feedback, feedbackStyle);
            }
        }

        private void EnsureStyles()
        {
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    normal = { textColor = new Color(.95f, .89f, .75f) }
                };
                feedbackStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    normal = { textColor = new Color(.95f, .89f, .75f) }
                };
            }
        }
    }
}
