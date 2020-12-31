using Game;
using TMPro;
using UnityEngine;

namespace UI
{
    public class DepthCounter : MonoBehaviour
    {
        public TextMeshProUGUI DepthText;

        private void Awake()
        {
            Events.Events.OnLevelStart += UpdateDepthCounterText;
        }

        public void UpdateDepthCounterText()
        {
            DepthText.text = Stats.NPCurrentDepth.ToString();
        }

        private void OnDestroy()
        {
            Events.Events.OnLevelStart -= UpdateDepthCounterText;
        }
    }
}
