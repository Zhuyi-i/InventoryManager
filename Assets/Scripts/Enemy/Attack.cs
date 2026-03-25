using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    //    public bool isAttacking = false;
    //    public bool isCriticalAttack = false;
    //    private bool canAttack = true;
    //    private bool criticalAttackReady = false;
    //    private string expectedCriticalInput = "";
    //    private float lastAttackTime = 0f;

    //    [SerializeField] public float comboResetTime = 1.5f;
    //    private InputAction _attack, _heavyAttack;
    //    private List<string> attackTypeHistory = new List<string>();

    //    [Header("Hitbox GameObjects")]
    //    [SerializeField] private GameObject lightAttackHitbox;
    //    [SerializeField] private GameObject heavyAttackHitbox;
    //    [SerializeField] private GameObject criticalAttackHitbox;

    //    [Header("Hitbox Settings")]
    //    [SerializeField] private float hitboxActiveDuration = 0.3f;

    //    [Header("Backstep Critical")]
    //    [SerializeField] private float backstepCriticalWindow = 0.6f;
    //    private bool backstepCriticalReady = false;
    //    private Coroutine backstepWindowCoroutine;

    //    private PlayerController _playerController;
    //    private Coroutine currentHitboxCoroutine;

    //    void Awake()
    //    {
    //        _attack = InputSystem.actions.FindAction("Attack");
    //        _heavyAttack = InputSystem.actions.FindAction("HeavyAttack");
    //        _playerController = GetComponent<PlayerController>();

    //        lightAttackHitbox?.SetActive(false);
    //        heavyAttackHitbox?.SetActive(false);
    //        criticalAttackHitbox?.SetActive(false);
    //    }

    //    void Update()
    //    {
    //        if (Time.time - lastAttackTime > comboResetTime && attackTypeHistory.Count > 0)
    //            ResetCombo();

    //        // Detect when a backstep just happened and open the critical window
    //        if (_playerController != null && _playerController._wasBackstep)
    //        {
    //            _playerController._wasBackstep = false; // Consume the flag
    //            OpenBackstepCriticalWindow();
    //        }

    //        if (!canAttack || isAttacking) return;

    //        if (_attack.triggered)
    //            PerformAttack("light", lightAttackHitbox, 0.5f);
    //        else if (_heavyAttack.triggered)
    //            PerformAttack("heavy", heavyAttackHitbox, 0.7f);
    //    }

    //    void OpenBackstepCriticalWindow()
    //    {
    //        if (backstepWindowCoroutine != null)
    //            StopCoroutine(backstepWindowCoroutine);

    //        backstepCriticalReady = true;
    //        backstepWindowCoroutine = StartCoroutine(BackstepCriticalWindow());
    //        Debug.Log("Backstep critical window opened!");
    //    }

    //    IEnumerator BackstepCriticalWindow()
    //    {
    //        yield return new WaitForSeconds(backstepCriticalWindow);
    //        backstepCriticalReady = false;
    //        backstepWindowCoroutine = null;
    //        Debug.Log("Backstep critical window expired.");
    //    }

    //    void PerformAttack(string type, GameObject hitbox, float resetDelay)
    //    {
    //        isAttacking = true;
    //        lastAttackTime = Time.time;

    //        bool isBackstepCritical = backstepCriticalReady;
    //        bool isComboCritical = criticalAttackReady && expectedCriticalInput == type;

    //        isCriticalAttack = isBackstepCritical || isComboCritical;

    //        if (isBackstepCritical || isComboCritical)
    //        {
    //            string source = isBackstepCritical ? "backstep" : "combo";
    //            Debug.Log($"CRITICAL {type.ToUpper()} ATTACK! ({source})");
    //            StartHitboxCoroutine(criticalAttackHitbox);
    //            attackTypeHistory.Add("critical_" + type);

    //            if (isBackstepCritical)
    //            {
    //                backstepCriticalReady = false;
    //                if (backstepWindowCoroutine != null)
    //                {
    //                    StopCoroutine(backstepWindowCoroutine);
    //                    backstepWindowCoroutine = null;
    //                }
    //            }

    //            ResetCombo();
    //        }
    //        else
    //        {
    //            if (criticalAttackReady)
    //            {
    //                Debug.Log($"Combo broken - expected {expectedCriticalInput} but got {type}");
    //                attackTypeHistory.Clear();
    //                criticalAttackReady = false;
    //                expectedCriticalInput = "";
    //            }

    //            Debug.Log($"Regular {type} attack");
    //            attackTypeHistory.Add(type);
    //            StartHitboxCoroutine(hitbox);
    //            CheckForCriticalCombo();
    //        }

    //        Invoke(nameof(ResetAttackingState), resetDelay);
    //    }

    //    void CheckForCriticalCombo()
    //    {
    //        if (attackTypeHistory.Count < 3) return;

    //        var last3 = attackTypeHistory.GetRange(attackTypeHistory.Count - 3, 3);
    //        if (last3[0] == last3[1] && last3[1] == last3[2] && (last3[0] == "light" || last3[0] == "heavy"))
    //        {
    //            criticalAttackReady = true;
    //            expectedCriticalInput = last3[0] == "light" ? "heavy" : "light";
    //            Debug.Log($"COMBO READY! Next {expectedCriticalInput.ToUpper()} will be CRITICAL!");
    //        }
    //    }

    //    void StartHitboxCoroutine(GameObject hitbox)
    //    {
    //        if (currentHitboxCoroutine != null)
    //            StopCoroutine(currentHitboxCoroutine);

    //        lightAttackHitbox?.SetActive(false);
    //        heavyAttackHitbox?.SetActive(false);
    //        criticalAttackHitbox?.SetActive(false);

    //        currentHitboxCoroutine = StartCoroutine(ActivateHitboxTemporarily(hitbox));
    //    }

    //    IEnumerator ActivateHitboxTemporarily(GameObject hitbox)
    //    {
    //        hitbox.SetActive(true);
    //        yield return new WaitForSeconds(hitboxActiveDuration);
    //        hitbox.SetActive(false);
    //        currentHitboxCoroutine = null;
    //    }

    //    void ResetCombo()
    //    {
    //        attackTypeHistory.Clear();
    //        criticalAttackReady = false;
    //        expectedCriticalInput = "";
    //        Debug.Log("Combo reset.");
    //    }

    //    void ResetAttackingState() 
    //    {
    //        isAttacking = false;
    //        isCriticalAttack = false;
    //    }

    //    // Animation Event methods
    //    public void OnLightAttackHitboxActivate() { if (!criticalAttackReady && !backstepCriticalReady) StartHitboxCoroutine(lightAttackHitbox); }
    //    public void OnHeavyAttackHitboxActivate() { if (!criticalAttackReady && !backstepCriticalReady) StartHitboxCoroutine(heavyAttackHitbox); }
    //    public void OnCriticalAttackHitboxActivate() { if (criticalAttackReady || backstepCriticalReady) StartHitboxCoroutine(criticalAttackHitbox); }
    //    public void OnAttackEnd() => isAttacking = false;
}