using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClusterAnimationUpdater : MonoBehaviour
{
    public List<GameObject> runningPedestrianList;
    public List<GameObject> idlePedestrianList;

    public List<AnimType> whichIdleAnimTypeForPeds;
    public List<int> whichUncombinableAnimTypeForPeds;
    public static List<AnimationClip> idleAnimations;
    public static List<AnimationClip> sittingIdleAnimations;
    public static List<AnimationClip> standingIdleAnimations;
    public static List<AnimationClip> uncombinableIdleAnimations;

    public static float animMinSpeed;
    public static float animMaxSpeed;

    private Dictionary<GameObject, float> lastCycle = new Dictionary<GameObject, float>();

    void Start()
    {
        for (int i = 0; i < idlePedestrianList.Count; i++)
        {
            GameObject ped = idlePedestrianList[i];

            Animator animator = ped.GetComponent<Animator>();

            animator.speed = Random.Range(animMinSpeed, animMaxSpeed);
            
            AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);

            AnimationClip idleClip;
            if (whichIdleAnimTypeForPeds[i] == AnimType.sittingIdleAnimations)
                idleClip = sittingIdleAnimations[Random.Range(0, sittingIdleAnimations.Count)];
            if (whichIdleAnimTypeForPeds[i] == AnimType.standingIdleAnimations)
                idleClip = standingIdleAnimations[Random.Range(0, standingIdleAnimations.Count)];    
            else // == uncombinable
                idleClip = uncombinableIdleAnimations[whichUncombinableAnimTypeForPeds[i]];    

            aoc["Idle"] = idleClip;
            animator.runtimeAnimatorController = aoc;
        }
    }

    void Update()  
    {
        foreach (GameObject ped in runningPedestrianList) {  // adds running animation by adjusting speed parameter for each characters animator
            NavMeshAgent agent = ped.GetComponent<NavMeshAgent>();

            float speed = agent.velocity.magnitude;
            Animator animator = agent.GetComponent<Animator>();
            animator.SetFloat("Speed", speed);

            float expectedSpeed = 2.0f; // dostosuj do animacji biegu/chodu
            animator.speed = speed / expectedSpeed;
        }

        // randomizes each idle animation speed with every cycle  // we dont need to randomize it for running chars because the varying speeds provide enough variety anyway
        for (int i = 0; i < idlePedestrianList.Count; i++)
        {
            GameObject ped = idlePedestrianList[i];

            Animator animator = ped.GetComponent<Animator>();

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            float currentCycle = Mathf.Floor(stateInfo.normalizedTime);

            if (!lastCycle.ContainsKey(ped))
            {
                lastCycle.Add(ped, currentCycle);
            }

            if (currentCycle > lastCycle[ped])  // an animation cycle for a character ended // losuj nową animację idle
            {
                animator.speed = Random.Range(animMinSpeed, animMaxSpeed);
                
                AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);

                AnimationClip idleClip;
                if (whichIdleAnimTypeForPeds[i] == AnimType.sittingIdleAnimations)
                    idleClip = sittingIdleAnimations[Random.Range(0, sittingIdleAnimations.Count)];
                if (whichIdleAnimTypeForPeds[i] == AnimType.standingIdleAnimations)
                    idleClip = standingIdleAnimations[Random.Range(0, standingIdleAnimations.Count)];    
                else // == uncombinable
                    idleClip = uncombinableIdleAnimations[whichUncombinableAnimTypeForPeds[i]];    

                aoc["Idle"] = idleClip;
                animator.runtimeAnimatorController = aoc;

                lastCycle[ped] = currentCycle;
            }
        }
    }
}
