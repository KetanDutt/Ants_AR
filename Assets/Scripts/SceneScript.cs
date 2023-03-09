using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class SceneScript : MonoBehaviour
{
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

    [SerializeField] private GameObject billboardPrefab;

    public bool QuestionState = true;
    public GameObject Ant;
    public GameObject billboard;
    private string Answer = "";
    private Camera arCamera;
    private Animator AntAnaimator;

    public void setQuestion(Question question)
    {
        Answer = question.question;
        text1.text = question.Answers[0];
        text2.text = question.Answers[1];
        text3.text = question.Answers[2];
    }

    public void SpawnAnt(GameObject ant)
    {
        Transform parent = Ant.transform.parent;
        Ant.transform.SetParent(null);
        Vector3 scale = Ant.transform.localScale;
        Ant = Instantiate(ant, Ant.transform.position, Ant.transform.rotation);
        Ant.transform.localScale = scale;
        Ant.transform.SetParent(parent);
        AntAnaimator = Ant.transform.GetChild(0).gameObject.GetComponent<Animator>();

        // billboard = Instantiate(billboardPrefab, new Vector3(Ant.transform.position.x, Ant.transform.position.y + .2f, Ant.transform.position.z), Ant.transform.rotation);
        // billboard.transform.localScale = scale * .5f;
        // billboard.transform.SetParent(parent);
    }

    private void Start()
    {
        arCamera = GameManager.instance.arCamera;
        StartCoroutine(MyScript.waiter(1f, () =>
        {
            QuestionState = true;
        }));
    }
    float initialDistance;
    Vector3 initialScale;
    private void Update()
    {
        Vector3 position = Ant.transform.position;
        Vector3 cameraPosition = arCamera.transform.position;
        Vector3 direction = cameraPosition - position;
        Vector3 targetRotationEuler = Quaternion.LookRotation(-direction).eulerAngles;
        Vector3 scaledEuler = Vector3.Scale(targetRotationEuler, Ant.transform.up.normalized);
        Quaternion targetRotation = Quaternion.Euler(scaledEuler);
        Ant.transform.rotation = targetRotation;


        if (Input.touchCount == 2)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            // if any one of touchzero or touchOne is cancelled or maybe ended then do nothing
            if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
            touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
            {
                return; // basically do nothing
            }

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                initialScale = gameObject.transform.localScale;
            }
            else // if touch is moved
            {
                var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
                //if accidentally touched or pinch movement is very very small
                if (Mathf.Approximately(initialDistance, 0))
                {
                    return; // do nothing if it can be ignored where inital distance is very close to zero
                }
                var factor = currentDistance / initialDistance;
                transform.localScale = initialScale * factor;
            }
        }


        if (!QuestionState)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                string text = hit.transform.parent.GetChild(1).gameObject.GetComponent<TextMeshPro>().text;
                QuestionState = false;
                AntAnaimator.CrossFade("JumpingLoop", .1f);
                hit.transform.parent.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = selectedMat;
                Ant.transform.DOJump(hit.transform.parent.GetChild(2).position, transform.localScale.x * 6, 1, 2).OnComplete(() =>
                {
                    AntAnaimator.CrossFade("Happy Idle", .1f);
                    if (Answer.Equals(text))
                    {
                        TextToSpeech.convert(GameManager.instance.configData.CorrectText);
                        hit.transform.parent.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = greenMat;
                    }
                    else
                    {
                        TextToSpeech.convert(GameManager.instance.configData.IncorrectText);
                        hit.transform.parent.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = redMat;
                    }
                    StartCoroutine(MyScript.waiter(.5f, () =>
                    {
                        AntAnaimator.CrossFade("JumpingLoop", .1f);
                        Ant.transform.DOJump(spawnPoint.transform.position, transform.localScale.x * 6, 1, 2).OnComplete(() =>
                        {
                            AntAnaimator.CrossFade("Happy Idle", .1f);
                            QuestionState = true;
                            hit.transform.parent.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = normalMat;
                            GameManager.instance.loadNextQuestion();
                        });
                    }));
                });
            }
        }
    }
}
