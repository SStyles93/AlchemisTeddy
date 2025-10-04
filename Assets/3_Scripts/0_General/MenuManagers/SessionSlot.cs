using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SessionSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image slotImage;
    [SerializeField] private TMP_Text slotText;

    private string sessionID;

    public void InitializeSessionSlot(SessionSaveData sessionData)
    {
        sessionID = sessionData.sessionID;
        slotText.text = $"{sessionData.sessionID} \n{sessionData.timestamp.ToShortDateString()}";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SessionManager.Instance.LoadSession(sessionID);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        slotBackground.color = Color.gray;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotBackground.color = Color.black;
    }
}
