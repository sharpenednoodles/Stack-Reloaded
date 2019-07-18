using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject prevBlock;
    [SerializeField] private GameObject currentBlock;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Camera mainCamera;
    [Header("Text Displays")]
    [SerializeField] private Text scoreDisplay;
    [SerializeField] private Text gameOverDisplay;
    [Header("Canvas Items")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject gameUICanvas;

    //Define the max position of block movement
    private const int transformBounds = 12;
    private const float speed = 20.0f;

    private bool gameStart = true;
    private bool invertMove = false;

    private float cameraHeightTarget;
    private int score;

    private float cameraAnimTime = 0.6f;
    private float currentLerpTimer;

    //Start with different colors each game
    private float colorOffset;

    //private float startTime;


    // Start is called before the first frame update
    void Start()
    {
        //Setup various things if needed
        //startTime = Time.time;
        score = 0;
        scoreDisplay.text = "";
        cameraHeightTarget = mainCamera.transform.position.y;

        //Set colour
        colorOffset = Random.Range(0f, 255f);
        CreateBlock(6);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart)
        {
            AnimateBlock(invertMove);

            //Space for debugging, remove after testing
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown("space"))
                PlaceBlock();

            if (mainCamera.transform.position.y < cameraHeightTarget)
                AnimateCamera();
            else
                currentLerpTimer = 0;
        }
    }

    /// <summary>
    /// Move the current block between two points
    /// TODO: Reset animation each new block
    /// </summary>
    void AnimateBlock(bool invertMove)
    {
        if (!invertMove)
            currentBlock.transform.position = new Vector3(
                Mathf.PingPong(Time.time * speed, 2 * transformBounds) - transformBounds,
                currentBlock.transform.position.y, currentBlock.transform.position.z);
        else
            currentBlock.transform.position = new Vector3(
                currentBlock.transform.position.x, currentBlock.transform.position.y, 
                Mathf.PingPong(Time.time * speed, 2 * transformBounds) - transformBounds);
    }

    /// <summary>
    /// Place the block in the game world
    /// </summary>
    void PlaceBlock()
    {
        invertMove = !invertMove;

        Debug.Log("<color=purple>Placing Block</color>");
        //Store positional reference to previous block for block size calculations
        Vector3 basePosition = prevBlock.transform.position;
        Vector3 baseScale = prevBlock.transform.localScale;

        //Update block
        prevBlock = currentBlock;

        //Calculate block bounds (x < y)
        Vector2 baseBlockXBounds = new Vector2(basePosition.x - (baseScale.x / 2),
                                               basePosition.x + (baseScale.x / 2));

        Vector2 baseBlockZBounds = new Vector2(basePosition.z - (baseScale.z / 2),
                                               basePosition.z + (baseScale.z / 2));

        Vector2 prevBlockXBounds = new Vector2(prevBlock.transform.position.x - (prevBlock.transform.localScale.x / 2), 
                                               prevBlock.transform.position.x + (prevBlock.transform.localScale.x / 2));

        Vector2 prevBlockZBounds = new Vector2(prevBlock.transform.position.z - (prevBlock.transform.localScale.z / 2),
                                               prevBlock.transform.position.z + (prevBlock.transform.localScale.z / 2));


        Debug.Log("<color=yellow>INFO-X</color> base: "+baseBlockXBounds);
        Debug.Log("<color=yellow>INFO-X</color> prev: " + prevBlockXBounds);
        

        //Determine whether our ranges overlap
        //Need to check this based upon X and Z coordinates!!!
        if (!(baseBlockXBounds.x < prevBlockXBounds.y && prevBlockXBounds.x < baseBlockXBounds.y))
        {
            Debug.Log("<color=red>We missed the block on X!</color>");
            gameStart = false;
            ToggleCanvas(gameUICanvas, gameOverCanvas);
            prevBlock.AddComponent<Rigidbody>();
            return;
        }
        else
        {
            //Create new wedge
            CreateBlock(prevBlock.transform.position.y + 2);
        }

     
        
        //Create split wedge block
        GameObject splitBlock = Instantiate(blockPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        splitBlock.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((score + colorOffset) / 100f % 1f, 1f, 1f));
        

        if (invertMove)
        {
            ///TEMP FIX FOR DEBUGGIN X MOVEMENT ONLY
            invertMove = false;

            //Modify x fields
            float overlapX = 0;

            //Calculate split coordinate, as the prev coordinate that isn't in the range of base
            float splitX = 0;
            if (prevBlockXBounds.x > baseBlockXBounds.x && prevBlockXBounds.x < baseBlockXBounds.y)
            {
                splitX = baseBlockXBounds.y;
            }
            else
            {
                splitX = baseBlockXBounds.x;
            }

            float overLapXNeg = Mathf.Min(baseBlockXBounds.y, baseBlockXBounds.x) - Mathf.Max(prevBlockXBounds.x, prevBlockXBounds.y);
            float overLapXPos = Mathf.Max(baseBlockXBounds.y, baseBlockXBounds.x) - Mathf.Min(prevBlockXBounds.x, prevBlockXBounds.y);

            //Calculate overlap based upon positive, or negative boundaries
            if (prevBlock.transform.position.x < 0)
            {
                overlapX = Mathf.Min(baseBlockXBounds.y, baseBlockXBounds.x) - Mathf.Max(prevBlockXBounds.x, prevBlockXBounds.y);
                Debug.Log("<color=orange>-ve boundry</color>");
            }
            else
            {
                overlapX = Mathf.Max(baseBlockXBounds.y, baseBlockXBounds.x) - Mathf.Min(prevBlockXBounds.x, prevBlockXBounds.y);
                Debug.Log("<color=orange>+ve boundry</color>");
            }
            
            if (overlapX > baseScale.x)
            {
                Debug.Log("CORRECTION CALL");
                overlapX = Mathf.Min(overLapXNeg, overLapXPos);
            }

            Debug.Log("<color=green>OverlapX</color>: " + overlapX + " <color=green>Min:</color>" + Mathf.Min(baseBlockXBounds.y, baseBlockXBounds.x) + " <color=green>Max:</color>" + Mathf.Max(prevBlockXBounds.x, prevBlockXBounds.y));
            //change scale of previous block in x
            prevBlock.transform.localScale = new Vector3(Mathf.Abs(overlapX), 
                                                         prevBlock.transform.localScale.y, 
                                                         prevBlock.transform.localScale.z);

            
            //Set scale of split block
            if (baseScale.x - Mathf.Abs(overlapX) < 0)
            {
                Debug.LogError("Negative boudary for block split");
            }
            splitBlock.transform.localScale = new Vector3(baseScale.x - Mathf.Abs(overlapX),
                                                                   prevBlock.transform.localScale.y,
                                                                   prevBlock.transform.localScale.z);
            //Set position of wedge
            splitBlock.transform.position = new Vector3(splitBlock.transform.localScale.x/2 + splitX,
                                                        prevBlock.transform.position.y,
                                                        prevBlock.transform.position.z);

            splitBlock.AddComponent<Rigidbody>();
            

            float bigCord = splitX + (prevBlock.transform.localScale.x / 2);
            float smallCord = splitX - (prevBlock.transform.localScale.x / 2);

            if (bigCord > baseBlockXBounds.x && bigCord < baseBlockXBounds.y)
            {
                //Modify x pos of prev
                prevBlock.transform.position = new Vector3(bigCord,
                                                       prevBlock.transform.position.y,
                                                       prevBlock.transform.position.z);

                Debug.Log("Splitting block bounds at " + splitX + " with positive, \nmoving prev to " + bigCord +" small cord:" +smallCord);
            }
            else
            {
                //Modify x pos of prev
                prevBlock.transform.position = new Vector3(smallCord,
                                                       prevBlock.transform.position.y,
                                                       prevBlock.transform.position.z);

                Debug.Log("Splitting block bounds at " + splitX + " with negative, \nmoving prev to " +smallCord + " big cord:" +bigCord );
            }

            //Modify x pos of new block to patch prev
            currentBlock.transform.position = new Vector3(prevBlock.transform.position.x, 
                                                          currentBlock.transform.position.y, 
                                                          currentBlock.transform.position.z);
            //Catching errors
            //THESE FIELDS ARE THE SAME VALUE FOR SOME REASON
            Debug.LogWarning("Prev Scale =" + prevBlock.transform.localScale.x + " Base Scale =" + baseScale.x);
            if (prevBlock.transform.localScale.x > baseScale.x)
            {
                Debug.LogError("<color=red>Block has grown in size!</color>");
            }
        }
        else
        {
            //Modify x fields
            float overlapZ = 0;

            //Calculate split coordinate, as the prev coordinate that isn't in the range of base
            float splitZ = 0;
            if (prevBlockZBounds.x > baseBlockZBounds.x && prevBlockZBounds.x < baseBlockZBounds.y)
            {
                splitZ = baseBlockZBounds.y;
            }
            else
            {
                splitZ = baseBlockZBounds.x;
            }

            float overLapZNeg = Mathf.Min(baseBlockZBounds.y, baseBlockZBounds.x) - Mathf.Max(prevBlockZBounds.x, prevBlockZBounds.y);
            float overLapZPos = Mathf.Max(baseBlockZBounds.y, baseBlockZBounds.x) - Mathf.Min(prevBlockZBounds.x, prevBlockZBounds.y);

            //Calculate overlap based upon positive, or negative boundaries
            if (prevBlock.transform.position.z < 0)
            {
                overlapZ = Mathf.Min(baseBlockZBounds.y, baseBlockZBounds.x) - Mathf.Max(prevBlockZBounds.x, prevBlockZBounds.y);
                Debug.Log("<color=orange>-ve boundry</color>");
            }
            else
            {
                overlapZ = Mathf.Max(baseBlockZBounds.y, baseBlockZBounds.x) - Mathf.Min(prevBlockZBounds.x, prevBlockZBounds.y);
                Debug.Log("<color=orange>+ve boundry</color>");
            }

            if (overlapZ > baseScale.x)
            {
                Debug.Log("CORRECTION CALL");
                overlapZ = Mathf.Min(overLapZNeg, overLapZPos);
            }

            Debug.Log("<color=green>OverlapX</color>: " + overlapZ + " <color=green>Min:</color>" + Mathf.Min(baseBlockZBounds.y, baseBlockZBounds.x) + " <color=green>Max:</color>" + Mathf.Max(prevBlockZBounds.x, prevBlockZBounds.y));
            //change scale of previous block in x
            prevBlock.transform.localScale = new Vector3(prevBlock.transform.localScale.x,
                                                         prevBlock.transform.localScale.y,
                                                         Mathf.Abs(overlapZ));


            //Set scale of split block
            splitBlock.transform.localScale = new Vector3(prevBlock.transform.localScale.x,
                                                          prevBlock.transform.localScale.y,
                                                          baseScale.z - Mathf.Abs(overlapZ));
            //Set position of wedge
            splitBlock.transform.position = new Vector3(prevBlock.transform.position.x,
                                                        prevBlock.transform.position.y,
                                                        splitBlock.transform.localScale.z / 2 + splitZ);

            splitBlock.AddComponent<Rigidbody>();


            float bigCord = splitZ + (prevBlock.transform.localScale.z / 2);
            float smallCord = splitZ - (prevBlock.transform.localScale.z / 2);

            if (bigCord > baseBlockZBounds.x && bigCord < baseBlockZBounds.y)
            {
                //Modify x pos of prev
                prevBlock.transform.position = new Vector3(prevBlock.transform.position.x,
                                                           prevBlock.transform.position.y,
                                                           bigCord);

                Debug.Log("Splitting block bounds at " + splitZ + " with positive, \nmoving prev to " + bigCord + " small cord:" + smallCord);
            }
            else
            {
                //Modify x pos of prev
                prevBlock.transform.position = new Vector3(prevBlock.transform.position.x,
                                                           prevBlock.transform.position.y,
                                                           smallCord);

                Debug.Log("Splitting block bounds at " + splitZ + " with negative, \nmoving prev to " + smallCord + " big cord:" + bigCord);
            }

            //Modify x pos of new block to patch prev
            currentBlock.transform.position = new Vector3(currentBlock.transform.position.x,
                                                          currentBlock.transform.position.y,
                                                          prevBlock.transform.position.z);
        }

        //Set new block scale
        currentBlock.transform.localScale = new Vector3(prevBlock.transform.localScale.x, prevBlock.transform.localScale.y, prevBlock.transform.localScale.z);

        //Move camera
        cameraHeightTarget += 2;
        //Update score

        //Check if game is still running
        score += 1;
        scoreDisplay.text = score.ToString();

    }

    void CreateBlock(float spawnHeight)
    {
        currentBlock = Instantiate(blockPrefab, new Vector3(0, spawnHeight, 0), new Quaternion(0, 0, 0, 0));
        currentBlock.name = "Block Level " + score;
        currentBlock.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((score + colorOffset) / 100f % 1f, 1f, 1f));
    }


    void AnimateCamera()
    {
        currentLerpTimer += Time.deltaTime;

        if (currentLerpTimer > cameraAnimTime)
            currentLerpTimer = cameraAnimTime;

        //Lerp percentage
        float perc = currentLerpTimer / cameraAnimTime;
        Vector3 oldCameraPostion = mainCamera.transform.position;

        //Use a coserp function to ease into the camera rise
        mainCamera.transform.position = new Vector3(oldCameraPostion.x, 
            Mathf.Lerp(oldCameraPostion.y, oldCameraPostion.y + 2, 1f - Mathf.Cos(perc * Mathf.PI * 0.5f)), 
            oldCameraPostion.z);
    }

    void ToggleCanvas(GameObject disable, GameObject enable)
    {
        if (disable != null)
        {
            disable.SetActive(false);
        }

        if (enable != null)
        {
            enable.SetActive(true);
        }
    }

    //Restart game
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
