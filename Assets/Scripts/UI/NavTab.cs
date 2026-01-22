using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NavTab : MonoBehaviour
{
    public static bool isUnderReview;
    [SerializeField]
    List<Button> TabButtons;
    [SerializeField]
    List<Image> ContentIcons;
    [SerializeField] 
    List<bool> HideOnReview;
    [SerializeField]
    Color SelectedPanelColor;
    [SerializeField]
    Color DisSelectedPanelColor;
    [SerializeField]
    Color SelectedContentColor;
    [SerializeField]
    Color DisSelectedContentColor;

    List<Graphic> UnderUpdateGraphics;
    

    private bool isSomethingUnderUpdate;
   
    private void OnEnable()
    {
        UnderUpdateGraphics = new List<Graphic>();

        SelectButton(0);
        CheckButtonsForReview();
    }
    private void CheckButtonsForReview()
    {
        for (int i = 0; i < TabButtons.Count; i++)
        {
            if (HideOnReview[i] && isUnderReview)
            {
              //  Debug.Log("Should Hide");
               foreach(Button btn in  TabButtons[i].GetComponentsInChildren<Button>())
                {
                    btn.interactable = false;
                }
                Graphic _targetContentImage = TabButtons[i].transform.GetChild(0).gameObject.GetComponent<Graphic>();
                _targetContentImage.color = new Color(1, 1, 1, 0);

            }
              
        }
    }
    public void SelectButton(int _index)
    {
        if(UnderUpdateGraphics?.Count == 0)
            for (int i = 0; i < TabButtons.Count; i++)
            {

                if (_index == i)
                {
                    Graphic _targetContentImage = TabButtons[_index].transform.GetChild(0).gameObject.GetComponent<Graphic>();
                    Graphic _targetPanelImage = TabButtons[_index].GetComponent<Graphic>();
                    StartCoroutine(SlerpColorsAB(_targetContentImage, SelectedContentColor, 0.3f));
                    StartCoroutine(SlerpColorsAB(_targetPanelImage, SelectedPanelColor, 0.3f));
                }
                else
                {
                    if (HideOnReview[i] && isUnderReview)
                    {
                        Graphic _targetPanelImage = TabButtons[i].GetComponent<Graphic>();
                        StartCoroutine(SlerpColorsAB(_targetPanelImage, DisSelectedPanelColor, 0.5f));
                    }
                    else
                    {


                        
                        Graphic _targetContentImage = TabButtons[i].transform.GetChild(0).gameObject.GetComponent<Graphic>();
                        Graphic _targetPanelImage = TabButtons[i].GetComponent<Graphic>();
                        StartCoroutine(SlerpColorsAB(_targetContentImage, DisSelectedContentColor, 0.5f));
                        StartCoroutine(SlerpColorsAB(_targetPanelImage, DisSelectedPanelColor, 0.5f));
                    }
                }

            }
        else
        {
           
            this.CallWithDelay(()=>SelectButton(_index), 0.2f);
        }
    }
   
 

    private IEnumerator SlerpColorsAB(Graphic _targetGraphic, Color _color2, float _step)
    {
        if (!UnderUpdateGraphics.Contains(_targetGraphic))
        {
            UnderUpdateGraphics.Add(_targetGraphic);
            while ((Mathf.Abs((_targetGraphic.color - _color2).r) > 0.01f) || (Mathf.Abs((_targetGraphic.color - _color2).g) > 0.01f) || (Mathf.Abs((_targetGraphic.color - _color2).b) > 0.01f) || (Mathf.Abs((_targetGraphic.color - _color2).a) > 0.01f))
            {
                _targetGraphic.color = Color.Lerp(_targetGraphic.color, _color2, _step);
                yield return new WaitForSecondsRealtime(0.01f);

            }
            UnderUpdateGraphics.Remove(_targetGraphic);

        }
        yield return null;
    }
}
