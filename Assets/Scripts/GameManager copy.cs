// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class GameManager : MonoBehaviour
// {
//     public static GameManager instance;

//     public bool placeMultiple = false;

//     [SerializeField]
//     private Toggle multipleToggle;
//     [SerializeField]
//     private FloatingJoystick floatingJoystick;
//     [SerializeField]
//     private TextMeshProUGUI speedText;

//     public float speed = .005f;

//     private void Awake()
//     {
//         if (instance == null)
//             instance = this;
//         else
//             Destroy(gameObject);

//         DontDestroyOnLoad(gameObject);
//     }

//     void Start()
//     {
//         multipleToggle.onValueChanged.AddListener((value) =>
//         {
//             placeMultiple = value;
//             floatingJoystick.gameObject.SetActive(!value);
//         });

//         StartCoroutine(MyScript.waiter(2f, () =>
//         {
//             TextToSpeech.convert("Click on a flat surface to Place the environment.");
//         }));
//     }


//     private void Update()
//     {
//         // if (PlaceObject.instance.spawned.Count <= 0) return;

//         // foreach (GameObject go in PlaceObject.instance.spawned)
//         // {
//         //     Transform ant = go.transform.GetChild(0);
//         //     ant.position = new Vector3(ant.position.x + floatingJoystick.Direction.x * speed, ant.position.y, ant.position.z + floatingJoystick.Direction.y * speed);

//         //     if (floatingJoystick.Direction.magnitude <= .1f)
//         //     {
//         //         ant.GetComponent<Animator>().CrossFade("Happy Idle", 0);
//         //     }
//         //     else
//         //     {
//         //         ant.GetComponent<Animator>().CrossFade("Running", 0);
//         //         ant.rotation = Quaternion.Euler(0, Mathf.Atan2(floatingJoystick.Direction.x, floatingJoystick.Direction.y) * Mathf.Rad2Deg, 0);
//         //     }
//         // }
//     }

//     // public void enableJoystick(float newSpeed)
//     // {
//     //     if (!placeMultiple)
//     //     {
//     //         floatingJoystick.gameObject.SetActive(true);
//     //         newSpeed *= .05f;
//     //         speed = newSpeed;
//     //         speedText.text = "speed : " + newSpeed;
//     //     }
//     // }
// }
