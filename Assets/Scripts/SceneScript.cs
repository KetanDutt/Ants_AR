using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls one placed quiz world: its ant, three answer platforms, touch input,
/// feedback animation, and pinch-to-resize gesture.
/// </summary>
public class SceneScript : MonoBehaviour
{
    private const float MinimumWorldScale = 0.05f;
    private const float MaximumWorldScale = 0.5f;
    private const float AnswerJumpDuration = 1.25f;

    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject jumpPoint1;
    [SerializeField] private GameObject jumpPoint2;
    [SerializeField] private GameObject jumpPoint3;

    [SerializeField] private TextMeshPro text1;
    [SerializeField] private TextMeshPro text2;
    [SerializeField] private TextMeshPro text3;

    [SerializeField] private Material greenMat;
    [SerializeField] private Material redMat;
    [SerializeField] private Material normalMat;
    [SerializeField] private Material selectedMat;

    // Retained so existing scene/prefab data remains compatible. The optional
    // billboard is not required for the core quiz interaction.
    [SerializeField] private GameObject billboardPrefab;

    [HideInInspector] public bool QuestionState;
    public GameObject Ant;
    public GameObject billboard;

    private Camera arCamera;
    private Animator antAnimator;
    private string correctAnswer = string.Empty;
    private bool hasValidQuestion;
    private float initialPinchDistance;
    private Vector3 initialWorldScale;
    private Tween activeTween;

    private readonly Transform[] answerPlatformRoots = new Transform[3];
    private readonly Transform[] answerJumpPoints = new Transform[3];
    private readonly TextMeshPro[] answerLabels = new TextMeshPro[3];
    private readonly Renderer[] answerRenderers = new Renderer[3];
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    private void Awake()
    {
        if (GameManager.instance != null)
            arCamera = GameManager.instance.arCamera;

        if (arCamera == null)
            arCamera = Camera.main;

        answerJumpPoints[0] = jumpPoint1 != null ? jumpPoint1.transform : null;
        answerJumpPoints[1] = jumpPoint2 != null ? jumpPoint2.transform : null;
        answerJumpPoints[2] = jumpPoint3 != null ? jumpPoint3.transform : null;

        answerLabels[0] = text1;
        answerLabels[1] = text2;
        answerLabels[2] = text3;

        for (int index = 0; index < answerJumpPoints.Length; index++)
        {
            Transform jumpPoint = answerJumpPoints[index];
            if (jumpPoint == null)
                continue;

            answerPlatformRoots[index] = jumpPoint.parent;
            if (answerPlatformRoots[index] != null)
                answerRenderers[index] = answerPlatformRoots[index].GetComponentInChildren<Renderer>(true);

            if (answerLabels[index] != null)
                answerLabels[index].richText = false;
        }
    }

    private void Update()
    {
        if (Ant != null && arCamera != null)
            FaceCamera();

        HandlePinchGesture();

        if (!QuestionState || !hasValidQuestion || Ant == null || antAnimator == null)
            return;

        Vector2 pointerPosition;
        if (!TryGetPointerDown(out pointerPosition) || IsPointerOverUI(pointerPosition))
            return;

        Ray ray = arCamera.ScreenPointToRay(pointerPosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
            return;

        int answerIndex = FindAnswerIndex(hit.transform);
        if (answerIndex >= 0)
            SelectAnswer(answerIndex);
    }

    private void FaceCamera()
    {
        // The mesh is authored facing backwards relative to the root transform,
        // so the root faces away from the camera to make the ant look toward it.
        Vector3 direction = Vector3.ProjectOnPlane(Ant.transform.position - arCamera.transform.position, transform.up);
        if (direction.sqrMagnitude > 0.0001f)
            Ant.transform.rotation = Quaternion.LookRotation(direction.normalized, transform.up);
    }

    private void HandlePinchGesture()
    {
        if (Input.touchCount != 2)
        {
            initialPinchDistance = 0f;
            return;
        }

        UnityEngine.Touch touchZero = Input.GetTouch(0);
        UnityEngine.Touch touchOne = Input.GetTouch(1);
        if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
            touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
        {
            initialPinchDistance = 0f;
            return;
        }

        float currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
        if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began || initialPinchDistance <= 0f)
        {
            initialPinchDistance = currentDistance;
            initialWorldScale = transform.localScale;
            return;
        }

        if (currentDistance <= 0f || initialPinchDistance <= 0f)
            return;

        float targetScale = initialWorldScale.x * (currentDistance / initialPinchDistance);
        targetScale = Mathf.Clamp(targetScale, MinimumWorldScale, MaximumWorldScale);
        transform.localScale = Vector3.one * targetScale;
    }

    private static bool TryGetPointerDown(out Vector2 screenPosition)
    {
        if (Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            screenPosition = touch.position;
            return touch.phase == TouchPhase.Began;
        }

        if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            return true;
        }

        screenPosition = Vector2.zero;
        return false;
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = screenPosition;
        uiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

    private int FindAnswerIndex(Transform hitTransform)
    {
        if (hitTransform == null)
            return -1;

        for (int index = 0; index < answerPlatformRoots.Length; index++)
        {
            Transform platformRoot = answerPlatformRoots[index];
            if (platformRoot != null && (hitTransform == platformRoot || hitTransform.IsChildOf(platformRoot)))
                return index;
        }

        return -1;
    }

    public bool SetQuestion(Question question)
    {
        hasValidQuestion = false;
        if (question == null)
            return false;

        string[] answers = question.GetAnswers();
        if (answers == null || answers.Length < answerLabels.Length)
            return false;

        for (int index = 0; index < answerLabels.Length; index++)
        {
            if (answerLabels[index] == null || string.IsNullOrWhiteSpace(answers[index]))
                return false;
        }

        correctAnswer = question.GetCorrectAnswer(answers);
        if (string.IsNullOrWhiteSpace(correctAnswer))
            return false;

        for (int index = 0; index < answerLabels.Length; index++)
            answerLabels[index].text = answers[index];

        hasValidQuestion = true;
        return true;
    }

    public bool SpawnAnt(GameObject antPrefab)
    {
        if (antPrefab == null || Ant == null)
        {
            Debug.LogError("SceneScript requires an ant prefab and a placeholder ant under the spawn point.");
            return false;
        }

        Transform previousAnt = Ant.transform;
        Transform parent = previousAnt.parent;
        Vector3 localPosition = previousAnt.localPosition;
        Quaternion localRotation = previousAnt.localRotation;
        Vector3 localScale = previousAnt.localScale;

        GameObject previousAntObject = Ant;
        Ant = Instantiate(antPrefab, parent, false);
        Ant.transform.localPosition = localPosition;
        Ant.transform.localRotation = localRotation;
        Ant.transform.localScale = localScale;
        Destroy(previousAntObject);

        antAnimator = Ant.GetComponentInChildren<Animator>(true);
        if (antAnimator == null)
        {
            Debug.LogError("The selected ant prefab does not contain an Animator.");
            return false;
        }

        return true;
    }

    public void BeginGame()
    {
        QuestionState = true;
    }

    private void SelectAnswer(int answerIndex)
    {
        if (answerRenderers[answerIndex] == null || answerJumpPoints[answerIndex] == null || spawnPoint == null)
            return;

        QuestionState = false;
        SetAnimation("JumpingLoop", 0.1f);
        answerRenderers[answerIndex].sharedMaterial = selectedMat;

        bool isCorrect = string.Equals(
            correctAnswer.Trim(),
            answerLabels[answerIndex].text.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();

        float jumpHeight = Mathf.Max(0.1f, transform.lossyScale.x * 6f);
        activeTween = Ant.transform
            .DOJump(answerJumpPoints[answerIndex].position, jumpHeight, 1, AnswerJumpDuration)
            .OnComplete(delegate
            {
                if (this == null || !isActiveAndEnabled)
                    return;

                SetAnimation("Happy Idle", 0.1f);
                answerRenderers[answerIndex].sharedMaterial = isCorrect ? greenMat : redMat;

                if (GameManager.instance != null)
                    GameManager.instance.RecordAnswer(isCorrect);

                StartCoroutine(ReturnAntToSpawn(answerIndex));
            });
    }

    private IEnumerator ReturnAntToSpawn(int answerIndex)
    {
        yield return new WaitForSeconds(0.6f);
        if (Ant == null || spawnPoint == null)
            yield break;

        SetAnimation("JumpingLoop", 0.1f);
        float jumpHeight = Mathf.Max(0.1f, transform.lossyScale.x * 6f);
        activeTween = Ant.transform
            .DOJump(spawnPoint.transform.position, jumpHeight, 1, AnswerJumpDuration)
            .OnComplete(delegate
            {
                if (this == null || !isActiveAndEnabled)
                    return;

                SetAnimation("Happy Idle", 0.1f);
                if (answerRenderers[answerIndex] != null)
                    answerRenderers[answerIndex].sharedMaterial = normalMat;

                QuestionState = true;
                if (GameManager.instance != null)
                    GameManager.instance.LoadNextQuestion();
            });
    }

    private void SetAnimation(string stateName, float transitionDuration)
    {
        if (antAnimator != null)
            antAnimator.CrossFade(Animator.StringToHash(stateName), transitionDuration);
    }

    private void OnDestroy()
    {
        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();
    }
}
