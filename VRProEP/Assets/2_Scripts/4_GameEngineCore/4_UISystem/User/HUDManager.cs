using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace VRProEP.GameEngineCore
{
    public class HUDManager : MonoBehaviour
    {

        public GameObject hudText;
        public GameObject hudGraphics;
        public GameObject hudEnableGraphics;
        public GameObject hudDisableGraphics;
        public bool enable = true;

        private TextMeshPro hudTextMesh;

        private void Start()
        {
            // Get HUD TextMesh
            hudTextMesh = hudText.GetComponent<TextMeshPro>();
        }

        private void Update()
        {
            // Animate HUD.
            hudGraphics.transform.Rotate(new Vector3(0.0f, 0.0f, 10.0f) * Time.deltaTime);
            if (enable && !hudEnableGraphics.activeSelf)
            {
                hudEnableGraphics.SetActive(true);
                hudDisableGraphics.SetActive(false);
            }
            if (!enable && !hudDisableGraphics.activeSelf)
            {
                hudEnableGraphics.SetActive(false);
                hudDisableGraphics.SetActive(true);
            }
        }

        public void MinimizeHUD()
        {
            hudText.SetActive(false);
            hudGraphics.transform.localScale = new Vector3(0.5f, 0.5f);
        }

        public void MaximizeHUD()
        {
            hudText.SetActive(true);
            hudGraphics.transform.localScale = new Vector3(1.0f, 1.0f);
        }

        public void DisplayText(string text, float time)
        {
            MaximizeHUD();
            StartCoroutine(DisplayTextCoroutine(text, time));
        }

        public void DisplayText(string text)
        {
            MaximizeHUD();
            hudTextMesh.text = text;
        }

        public void ClearText()
        {
            hudTextMesh.text = "";
            MinimizeHUD();
        }

        private IEnumerator DisplayTextCoroutine(string text, float time)
        {
            if (hudTextMesh != null && text != null)
                hudTextMesh.text = text;
            yield return new WaitForSecondsRealtime(time);
            hudTextMesh.text = "";
            MinimizeHUD();
        }
    }
}