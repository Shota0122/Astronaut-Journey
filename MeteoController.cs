using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoController : MonoBehaviour
{
    
    GameObject toTargetObj;
    GameObject meteoManager;
    GameObject player;
   
    Vector3 moveDir;
    bool moveStart = false;
    bool isRed = false;
    bool curveMeteo = false;

    float moveSpeed = 15.0f;
    float rotX;
    float rotY;
    float rotZ;
    float elapsed = 0;
    float curveProbability = 0;
    

    [SerializeField] Material redMat;
    Material originalMat;

    Ray ray;
    RaycastHit hit;


    AudioSource myAudio;
    [SerializeField] AudioClip SE_Curve;
    
    

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        meteoManager = GameObject.Find("MeteoManager");

        //プレイ時間が進むに連れて隕石がカーブする確率が上がる
        if (player != null)
        {
            curveProbability = player.GetComponent<PlayerController>().playTime / 400.0f;
            curveProbability = Mathf.Clamp(curveProbability, 0, 0.25f);

            if (Random.value <= curveProbability && !gameObject.CompareTag("Bottle")) Invoke("ChangeTarget", 0.3f);
        }

        //隕石のスピードを徐々にあげて臨場感を出す
        StartCoroutine("SpeedUp");
        



        if (!gameObject.CompareTag("Bottle"))
        {
            originalMat = GetComponent<Renderer>().material;

            //プレイヤーが無造作に回転するように調整
            rotX = Random.Range(150.0f, 180.0f);
            rotY = Random.Range(150.0f, 180.0f);
            rotZ = Random.Range(150.0f, 180.0f);
            if (Random.value < 0.5f) rotX *= -1.0f;
            if (Random.value < 0.5f) rotY *= -1.0f;
            if (Random.value < 0.5f) rotZ *= -1.0f;
        }

    }

    void LockOnTarget(GameObject targetObj)
    {
        toTargetObj = targetObj;
        moveDir = (toTargetObj.transform.position - transform.position).normalized;
        moveStart = true;

        if(!gameObject.CompareTag("Bottle")) StartCoroutine("CalDistance");
    }

    void ChangeTarget()
    {
        myAudio = GetComponent<AudioSource>();
        myAudio.PlayOneShot(SE_Curve);
        toTargetObj = meteoManager.GetComponent<MeteoManager>().meteoGeneratorsList[0].transform.parent.gameObject;
        moveDir = (toTargetObj.transform.position - transform.position).normalized;

        rotX *= 3.0f;
        rotY *= 3.0f;
        rotZ *= 3.0f;
    }
    
    IEnumerator CalDistance()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            ray = new Ray(transform.position, moveDir);
            Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.green, 1, false);
        }
    }

    IEnumerator SpeedUp()
    {
        while (true)
        {
            moveSpeed++;
            yield return new WaitForSeconds(0.2f);
        }

        
    }

    
    void Update()
    {
        if (!moveStart) return;

        
         
        transform.Rotate(rotX * Time.deltaTime, rotY * Time.deltaTime, rotZ * Time.deltaTime);
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (transform.position.z < -5.0f) Destroy(gameObject);





        //以下の処理は隕石のみに適用
        if (gameObject.CompareTag("Bottle")) return;
        

        if (Physics.Raycast(ray,out hit, 100.0f))
        {
            if (hit.collider.gameObject.CompareTag("Player") && !isRed)
            {
                isRed = true;

            } else if (!hit.collider.gameObject.CompareTag("Player") && isRed)
            {
                isRed = false;
                elapsed = 0;
                GetComponent<Renderer>().material = originalMat;
            }
            
        }

        if (isRed)
        {
            elapsed += Time.deltaTime;
            elapsed %= 0.2f;
            GetComponent<Renderer>().material = (elapsed <= 0.1f) ? redMat : originalMat;
        }



       
    }
}
