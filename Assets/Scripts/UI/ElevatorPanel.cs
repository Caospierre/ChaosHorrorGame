using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElevatorPanel : MonoBehaviour
{
    [SerializeField] TMP_Text floorTitle;
    [SerializeField] Transform content;
    [SerializeField] GameObject buttonPrefab;

    private void Start()
    {
        int currentFloor = HotelLayoutManager.Instance.CurrentFloor;
        floorTitle.text = currentFloor == 0 ? "Lobby" : $"{currentFloor}";

        // Clear any old children
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < HotelLayoutManager.Instance.MaxFloor; i++)
        {
            var buttonContainer = Instantiate(buttonPrefab, content);
            var btn = buttonContainer.GetComponentInChildren<Button>();
            var label = buttonContainer.GetComponentInChildren<TMP_Text>();
            label.text = i == 0 ? "L" : i.ToString();

            int capturedFloor = i;
            btn.onClick.AddListener(() => OnSelectFloor(capturedFloor));

            // Don’t allow clicking the current floor
            if (i == HotelLayoutManager.Instance.CurrentFloor)
            {
                btn.interactable = false;

                var colors = btn.colors;
                colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, 1f);
                btn.colors = colors;
            }
        }

        // Make sure UI focus is set so gamepads/keyboard work
        EventSystem.current?.SetSelectedGameObject(content.childCount > 0 ? content.GetChild(0).gameObject : null);
    }

    void OnSelectFloor(int floor)
    {
        HotelLayoutManager.Instance.GoToFloor(floor);
    }
}
