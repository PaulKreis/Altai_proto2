using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
namespace SBPScripts
{
    public class CyclistAnimController : MonoBehaviour
    {
        [SerializeField]  BicycleController bicycleController;
        Animator anim;
        string clipInfoCurrent, clipInfoLast;
        [HideInInspector]
        public float speed;
        [HideInInspector]
        public bool isAirborne;

        public GameObject hipIK, chestIK, leftFootIK, leftFootIdleIK, rightFootIK, headIK;
        [SerializeField] BicycleStatus bicycleStatus;
        Rig rig;
        bool onOffBike;
        [Header("Character Switching")]
        [Space]
        public GameObject cyclist;
        float waitTime, prevLocalPosX;

        public PlayerCharacter playerCharacter;



        void Start()
        {
            //bicycleController = FindObjectOfType<BicycleController>();
            //bicycleStatus = FindObjectOfType<BicycleStatus>();

            rig = hipIK.transform.parent.gameObject.GetComponent<Rig>();
            if (bicycleStatus != null)
                onOffBike = bicycleStatus.onBike;
            if (cyclist != null)
                cyclist.SetActive(bicycleStatus.onBike);
            if (playerCharacter != null)
                playerCharacter.ToggleThirdPersonMode(!bicycleStatus.onBike);
            anim = GetComponent<Animator>();
            leftFootIK.GetComponent<TwoBoneIKConstraint>().weight = 0;
            chestIK.GetComponent<TwoBoneIKConstraint>().weight = 0;
            hipIK.GetComponent<MultiParentConstraint>().weight = 0;
            headIK.GetComponent<MultiAimConstraint>().weight = 0;
        }

        public void ToggleGetOnBike(PlayerCharacter player)
        {
            playerCharacter = player;

            waitTime = 1.5f;
            playerCharacter.transform.position = cyclist.transform.root.position - transform.right * 0.5f + transform.forward * 0.1f;
            bicycleStatus.onBike = !bicycleStatus.onBike;
            if (bicycleStatus.onBike)
            {
                if (prevLocalPosX < 0)
                    anim.Play("OnBike");
                else
                    anim.Play("OnBikeFlipped");
                StartCoroutine(AdjustRigWeight(0));
            }
            else
            {
                anim.Play("OffBike");
                StartCoroutine(AdjustRigWeight(1));
            }
        }

        void Update()
        {
            if (cyclist != null && playerCharacter != null)
            {
                prevLocalPosX = playerCharacter.transform.localPosition.x;
            }

            waitTime -= Time.deltaTime;
            waitTime = Mathf.Clamp(waitTime, 0, 1.5f);

            speed = bicycleController.transform.InverseTransformDirection(bicycleController.rb.velocity).z;
            isAirborne = bicycleController.isAirborne;
            anim.SetFloat("Speed", speed);
            anim.SetBool("isAirborne", isAirborne);
            if (bicycleStatus != null)
            {
                if (bicycleStatus.dislodged == false)
                {
                    if (!bicycleController.isAirborne && bicycleStatus.onBike)
                    {
                        clipInfoCurrent = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                        if (clipInfoCurrent == "IdleToStart" && clipInfoLast == "Idle")
                            StartCoroutine(LeftFootIK(0));
                        if (clipInfoCurrent == "Idle" && clipInfoLast == "IdleToStart")
                            StartCoroutine(LeftFootIK(1));
                        if (clipInfoCurrent == "Idle" && clipInfoLast == "Reverse")
                            StartCoroutine(LeftFootIdleIK(0));
                        if (clipInfoCurrent == "Reverse" && clipInfoLast == "Idle")
                            StartCoroutine(LeftFootIdleIK(1));

                        clipInfoLast = clipInfoCurrent;
                    }
                }
                else
                {
                    cyclist.SetActive(false);
                    if(Input.GetKeyDown(KeyCode.R))
                    {
                        cyclist.SetActive(true);
                        bicycleStatus.dislodged = false;
                    }
                }
            }
            else
            {
                if (!bicycleController.isAirborne)
                {
                    clipInfoCurrent = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                    if (clipInfoCurrent == "IdleToStart" && clipInfoLast == "Idle")
                        StartCoroutine(LeftFootIK(0));
                    if (clipInfoCurrent == "Idle" && clipInfoLast == "IdleToStart")
                        StartCoroutine(LeftFootIK(1));
                    if (clipInfoCurrent == "Idle" && clipInfoLast == "Reverse")
                        StartCoroutine(LeftFootIdleIK(0));
                    if (clipInfoCurrent == "Reverse" && clipInfoLast == "Idle")
                        StartCoroutine(LeftFootIdleIK(1));

                    clipInfoLast = clipInfoCurrent;
                }
            }
        }

        IEnumerator LeftFootIK(int offset)
        {
            float t1 = 0f;
            while (t1 <= 1f)
            {
                t1 += Time.fixedDeltaTime;
                leftFootIK.GetComponent<TwoBoneIKConstraint>().weight = Mathf.Lerp(-0.05f, 1.05f, Mathf.Abs(offset - t1));
                leftFootIdleIK.GetComponent<TwoBoneIKConstraint>().weight = 1 - leftFootIK.GetComponent<TwoBoneIKConstraint>().weight;
                yield return null;
            }

        }
        IEnumerator LeftFootIdleIK(int offset)
        {
            float t1 = 0f;
            while (t1 <= 1f)
            {
                t1 += Time.fixedDeltaTime;
                leftFootIdleIK.GetComponent<TwoBoneIKConstraint>().weight = Mathf.Lerp(-0.05f, 1.05f, Mathf.Abs(offset - t1));
                yield return null;
            }

        }
        IEnumerator AdjustRigWeight(int offset)
        {
            StartCoroutine(LeftFootIK(1));
            if (offset == 0)
            {
                cyclist.SetActive(true);
                playerCharacter.ToggleThirdPersonMode(false);
            }
            float t1 = 0f;
            while (t1 <= 1f)
            {
                t1 += Time.deltaTime;
                rig.weight = Mathf.Lerp(-0.05f, 1.05f, Mathf.Abs(offset - t1));
                yield return null;
            }
            if (offset == 1)
            {
                yield return new WaitForSeconds(0.2f);
                cyclist.SetActive(false);
                playerCharacter.ToggleThirdPersonMode(true);
                // Matching position and rotation to the best possible transform to get a seamless transition
                playerCharacter.transform.position = cyclist.transform.root.position - transform.right * 0.5f + transform.forward * 0.1f;
                playerCharacter.transform.rotation = Quaternion.Euler(playerCharacter.transform.rotation.eulerAngles.x, cyclist.transform.root.rotation.eulerAngles.y + 80, playerCharacter.transform.rotation.eulerAngles.z);
            }

        }
    }
}
