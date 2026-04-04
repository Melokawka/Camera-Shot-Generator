using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisibilityTester : MonoBehaviour
{
    public List<GameObject> idlePedestrianList;
    public static bool shouldCharactersBeInFrame = false;
    public static float requiredVisibleCharPercentageInFrame; // <- minimalny wymagany procent widocznych obiektów (np. 0.8)
    public static float cameraBoxColliderSize;

    public bool WillShotBeValid(List<CameraPose> cameraPoses)
    {
        GameObject cameraGhost = Instantiate(gameObject);
        cameraGhost.transform.SetParent(transform.parent, false);  // not sure its needed
        BoxCollider boxCollider = cameraGhost.AddComponent<BoxCollider>();
        boxCollider.size = Vector3.one * cameraBoxColliderSize;

        bool isShotValid = true;
        foreach (CameraPose cameraPose in cameraPoses)
        {
            cameraGhost.transform.position = cameraPose.position;  // fov is ignored
            cameraGhost.transform.rotation = cameraPose.rotation;
            Physics.SyncTransforms();  // what a joke!

            isShotValid = !IsCameraColliding(cameraGhost) && !ArePedestriansOccluded(cameraGhost);
            if (!isShotValid) break;

            if (GetComponent<Recorder2>().settings.shotType == ShotTypes.Static) break;  // only check once for static shots
        }

        Destroy(cameraGhost);
        return isShotValid;
    }

    public bool ArePedestriansOccluded(GameObject cameraGhost)
    {
        float visible = CheckVisibility(cameraGhost.GetComponent<Camera>());
        bool isOccluded = visible < requiredVisibleCharPercentageInFrame;

        if (isOccluded) Debug.LogWarning("The required threshold of visibility for idle pedestrians was not reached.");
        
        return isOccluded;
    }

    public bool IsCameraColliding(GameObject cameraGhost)
    {
        BoxCollider camCollider = cameraGhost.GetComponent<BoxCollider>();
        Collider[] overlaps = Physics.OverlapBox(
            camCollider.bounds.center,
            camCollider.bounds.extents,
            cameraGhost.transform.rotation,
            ~0, // layer 0
            QueryTriggerInteraction.Ignore // no triggers
        );

        bool isIntersecting = overlaps.Any(c => c.gameObject != cameraGhost && !c.gameObject.CompareTag("Pat"));

        if (isIntersecting) Debug.LogWarning("Shot invalid - camera would collide with an object.");

        return isIntersecting;
    }

    public float CheckVisibility(Camera cam)
    {
        if (idlePedestrianList.Count == 0) { Debug.LogWarning("Camera is missing the list of idle pedestrians."); return 1f; }

        int visibleCount = 0;
        foreach (GameObject obj in idlePedestrianList)
        {
            if (obj == null) { Debug.Log("Pedestrian is null."); continue;}

            Collider collider = obj.GetComponent<Collider>();
            if (collider == null) Debug.Log("Pedestrian is missing collider.");

            if (IsObjectVisible(cam, collider)) visibleCount++;
        }
        return (float)visibleCount / idlePedestrianList.Count;
    }

    public bool IsObjectVisible(Camera cam, Collider collider)
    {
        if (shouldCharactersBeInFrame)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            if (!GeometryUtility.TestPlanesAABB(planes, collider.bounds))
            {
                return false; // target object is outside of the frame
            }
        }

        Vector3 direction = cam.transform.position - collider.bounds.center;
        Ray ray = new Ray(collider.bounds.center, direction.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, direction.magnitude, ~0);
        //Debug.DrawRay(ray.origin, ray.direction * direction.magnitude, Color.red, 1f);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));  // raycasthits are randomly ordered by default

        foreach (RaycastHit hit in hits)  // what collider got hit first?
        {
            if (hit.collider.gameObject.CompareTag("Pat"))
            {
                continue; // pedestrian is not an obstacle, continue
            }

            if (hit.collider.gameObject == cam.gameObject) 
            {
                //Debug.Log("Trafiono kamere");
                return true;  // it hit camera so there are no obstacles in the way
            }
            else
            {
                //Debug.Log("Trafiono " + hit.collider.gameObject.name);
                return false; // the target object is occluded by the terrain  
            }
        }

        return false;
    }
}
