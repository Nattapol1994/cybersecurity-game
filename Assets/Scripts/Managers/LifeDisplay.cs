using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // For animations

public class LifeDisplay : MonoBehaviour
{
    [SerializeField] private Transform livesContainer; // Assign LivesContainer
    [SerializeField] private GameObject lifePrefab; // Assign LifeIcon prefab (Image only)
    [SerializeField] private Sprite normalLaptop;
    [SerializeField] private Sprite explodedLaptop;

    // We now store both the Image and its parent container for animation
    private class LifeIconEntry
    {
        public Image image;
        public Transform container;
    }

    private List<LifeIconEntry> lifeIcons = new List<LifeIconEntry>();

    // Track the last number of lives to know when a life is lost
    private int previousLives = -1; // Initialize to an invalid number

    public void InitializeLives(int totalLives)
    {
        // Clear old icons if any
        foreach (Transform child in livesContainer)
            Destroy(child.gameObject);
        lifeIcons.Clear();

        // Spawn icons
        for (int i = 0; i < totalLives; i++)
        {
            // Create a container to hold the Image
            GameObject container = new GameObject("LifeIconContainer", typeof(RectTransform));
            container.transform.SetParent(livesContainer, false);

            // Instantiate the Image inside the container
            GameObject icon = Instantiate(lifePrefab, container.transform, false);
            Image img = container.GetComponentInChildren<Image>();
            img.sprite = normalLaptop;

            // Store reference
            lifeIcons.Add(new LifeIconEntry { image = img, container = container.transform });
        }

        // Set initial lives count
        previousLives = totalLives;
    }

    public void UpdateLives(int currentLives)
    {
        // If a life has been lost (currentLives < previousLives)
        if (currentLives < previousLives)
        {
            // Find the lost life (the one that is now missing)
            int lostLifeIndex = previousLives - 1; // This is the index of the lost life

            // Animate only the most recent lost life (the one that was removed)
            lifeIcons[lostLifeIndex].image.sprite = explodedLaptop;  // Set exploded sprite
            lifeIcons[lostLifeIndex].container.DOPunchScale(Vector3.one * 0.2f, 1f, 1, 1);  // Play animation
        }

        // Update the sprites for the remaining lives
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (i < currentLives)
            {
                lifeIcons[i].image.sprite = normalLaptop;  // Normal sprite for active lives
            }
            else
            {
                lifeIcons[i].image.sprite = explodedLaptop;  // Exploded sprite for lost lives
            }
        }

        // Update the previousLives variable for the next update
        previousLives = currentLives;
    }
}
