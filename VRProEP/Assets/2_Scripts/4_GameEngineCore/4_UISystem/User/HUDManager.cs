using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace VRProEP.GameEngineCore
{
    public class HUDManager : MonoBehaviour
    {
        public enum HUDColour
        {
            Blue,
            Red,
            Green,
            Yellow,
            Orange,
            Purple
        }

        public enum HUDCentreColour
        {
            None,
            Yellow
        }

        public GameObject hudText;
        public GameObject hudGraphics;
        public GameObject hudBlueGraphics;
        public GameObject hudRedGraphics;
        public GameObject hudGreenGraphics;
        public GameObject hudYellowGraphics;
        public GameObject hudOrangeGraphics;
        public GameObject hudPurpleGraphics;
        public GameObject hudCentreYellowGraphics;
        public HUDColour colour;
        public HUDCentreColour centreColour;

        private bool clearCentre = true;

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
            if (colour == HUDColour.Blue && !hudBlueGraphics.activeSelf)
            {
                hudRedGraphics.SetActive(false);
                hudGreenGraphics.SetActive(false);
                hudYellowGraphics.SetActive(false);
                hudOrangeGraphics.SetActive(false);
                hudPurpleGraphics.SetActive(false);
                hudBlueGraphics.SetActive(true);
            }
            if (colour == HUDColour.Red && !hudRedGraphics.activeSelf)
            {
                hudBlueGraphics.SetActive(false);
                hudGreenGraphics.SetActive(false);
                hudYellowGraphics.SetActive(false);
                hudOrangeGraphics.SetActive(false);
                hudPurpleGraphics.SetActive(false);
                hudRedGraphics.SetActive(true);
            }
            if (colour == HUDColour.Green && !hudGreenGraphics.activeSelf)
            {
                hudBlueGraphics.SetActive(false);
                hudRedGraphics.SetActive(false);
                hudYellowGraphics.SetActive(false);
                hudOrangeGraphics.SetActive(false);
                hudPurpleGraphics.SetActive(false);
                hudGreenGraphics.SetActive(true);
            }
            if (colour == HUDColour.Yellow && !hudYellowGraphics.activeSelf)
            {
                hudBlueGraphics.SetActive(false);
                hudRedGraphics.SetActive(false);
                hudYellowGraphics.SetActive(true);
                hudOrangeGraphics.SetActive(false);
                hudPurpleGraphics.SetActive(false);
                hudGreenGraphics.SetActive(false);
            }
            if (colour == HUDColour.Orange && !hudOrangeGraphics.activeSelf)
            {
                hudBlueGraphics.SetActive(false);
                hudRedGraphics.SetActive(false);
                hudYellowGraphics.SetActive(false);
                hudPurpleGraphics.SetActive(false);
                hudGreenGraphics.SetActive(false);
                hudOrangeGraphics.SetActive(true);
            }
            if (colour == HUDColour.Purple && !hudPurpleGraphics.activeSelf)
            {
                hudBlueGraphics.SetActive(false);
                hudRedGraphics.SetActive(false);
                hudYellowGraphics.SetActive(false);
                hudOrangeGraphics.SetActive(false);
                hudPurpleGraphics.SetActive(true);
                hudGreenGraphics.SetActive(false);
            }
            if(centreColour == HUDCentreColour.None && !clearCentre)
            {
                hudCentreYellowGraphics.SetActive(false);
            }
            if (centreColour == HUDCentreColour.Yellow && !hudCentreYellowGraphics.activeSelf)
            {
                hudCentreYellowGraphics.SetActive(true);
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
            hudGraphics.transform.localScale = new Vector3(3.0f, 3.0f);
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