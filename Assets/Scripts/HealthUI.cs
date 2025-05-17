using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour {
    private Sprite fullHeartSprite;
    private Sprite emptyHeartSprite;
    private Image heartPrefab;
    private List<Image> hearts = new List<Image>();

    public void Start() {
        heartPrefab = Resources.Load<Image>("Prefabs/Heart");
        Sprite[] hearts = Resources.LoadAll<Sprite>("UI/HeartSprite");
        emptyHeartSprite = hearts[0];
        fullHeartSprite = hearts[1];
    }
    public void SetMaxHearts(int maxHearts) {
        foreach (Image heart in hearts) {
            Destroy(heart.gameObject);
        }
        hearts.Clear();

        for (int i = 0; i < maxHearts; i++) {
            Image newHeart = Instantiate(heartPrefab, transform);
            newHeart.sprite = fullHeartSprite;
            hearts.Add(newHeart);
        }
    }

    public void UpdateHearts(int currentHealth) {
        for (int i = 0; i < hearts.Count; i++) {
            if (i < currentHealth) {
                hearts[i].sprite = fullHeartSprite;
            } else {
                hearts[i].sprite = emptyHeartSprite;
            }
        }
    }
}
