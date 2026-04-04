using System.Collections.Generic;
using UnityEngine;

public class CameraVisibilityController : MonoBehaviour
{
    public int visibleClusterId; // Each camera has a designated cluster ID to render.
    private List<ClusterIdentifier> hiddenObjects = new List<ClusterIdentifier>();

    void OnPreRender()
    {
        // Find all objects with ClusterIdentifier
        ClusterIdentifier[] allCharacters = FindObjectsOfType<ClusterIdentifier>();

        foreach (ClusterIdentifier character in allCharacters)
        {
            if (character.clusterId != visibleClusterId)
            {
                Renderer[] renderers = character.GetComponentsInChildren<Renderer>(); // Get all types of renderers

                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = false; // Disable all renderers
                }
                hiddenObjects.Add(character);
            }
        }
    }

    void OnPostRender()
    {
        // Restore visibility after rendering
        foreach (ClusterIdentifier character in hiddenObjects)
        {
            Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
            Animator animator = character.GetComponent<Animator>();

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true; // Re-enable all renderers
            }
        }
        hiddenObjects.Clear();
    }
}
