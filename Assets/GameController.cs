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
    /// Invokes the game as over
    /// </summary>
    void GameOver()
    {
        Debug.Log("<color=red>We missed the block</color>");
        gameStart = false;
        ToggleCanvas(gameUICanvas, gameOverCanvas);
        prevBlock.AddComponent<Rigidbody>();
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


        //Debug.Log("<color=yellow>INFO-X</color> base: "+baseBlockXBounds);
        //Debug.Log("<color=yellow>INFO-X</color> prev: " + prevBlockXBounds);

        float overlap, split;

        //TODO rewrite to be generalised. Allow code to be reused for both cases
        if (invertMove)
        {
            //Keep on X bounds only for debugging
            //invertMove = false;
            overlap = Mathf.Min(baseBlockXBounds.y, prevBlockXBounds.y) - Mathf.Max(baseBlockXBounds.x, prevBlockXBounds.x);
            Debug.Log("Overlap interval =" + overlap);

            //We missed the block, invoke gameover
            if (overlap < 0)
            {
                GameOver();
                return;
            }

            
            //Calculate the split point
            if (baseBlockXBounds.x < prevBlockXBounds.x)
            {
                split = baseBlockXBounds.y;
                float blockPos, splitPos;
                blockPos = split - 0.5f * overlap;
                prevBlock.transform.localScale = new Vector3(overlap, prevBlock.transform.localScale.y, prevBlock.transform.localScale.z);
                prevBlock.transform.position = new Vector3(blockPos, prevBlock.transform.position.y, prevBlock.transform.position.z);

                //This SHOULD be positive only!!
                float splitScale = baseScale.x - overlap;
                splitPos = split + 0.5f * splitScale;
                CreateSplit(new Vector3(splitPos, prevBlock.transform.position.y, prevBlock.transform.position.z), new Vector3(splitScale, prevBlock.transform.localScale.y, prevBlock.transform.localScale.z));
                
            }
            else if (baseBlockXBounds.x > prevBlockXBounds.x)
            {
                split = baseBlockXBounds.x;
                float blockPos, splitPos;
                blockPos = split + 0.5f * overlap;
                prevBlock.transform.localScale = new Vector3(overlap, prevBlock.transform.localScale.y, prevBlock.transform.localScale.z);
                prevBlock.transform.position = new Vector3(blockPos, prevBlock.transform.position.y, prevBlock.transform.position.z);

                //This SHOULD be positive only!!
                float splitScale = baseScale.x - overlap;
                splitPos = split - 0.5f * splitScale;
                CreateSplit(new Vector3(splitPos, prevBlock.transform.position.y, prevBlock.transform.position.z), new Vector3(splitScale, prevBlock.transform.localScale.y, prevBlock.transform.localScale.z));
            }
            else
            {
                Debug.Log("No split!");
            }
        }
        else if (!invertMove)
        {
            overlap = Mathf.Min(baseBlockZBounds.y, prevBlockZBounds.y) - Mathf.Max(baseBlockZBounds.x, prevBlockZBounds.x);
            Debug.Log("Block overlap =" + overlap);

            //We missed the block, invoke gameover
            if (overlap < 0)
            {
                GameOver();
                return;
            }

          
            //Calculate the split point
            if (baseBlockZBounds.x < prevBlockZBounds.x)
            {
                split = baseBlockZBounds.y;
                float blockPos, splitPos;
                blockPos = split - 0.5f * overlap;
                prevBlock.transform.localScale = new Vector3(prevBlock.transform.localScale.x, prevBlock.transform.localScale.y, overlap);
                prevBlock.transform.position = new Vector3(prevBlock.transform.position.x, prevBlock.transform.position.y, blockPos);

                //This SHOULD be positive only!!
                float splitScale = baseScale.z - overlap;
                splitPos = split + 0.5f * splitScale;
                CreateSplit(new Vector3(prevBlock.transform.position.x, prevBlock.transform.position.y, splitPos), new Vector3(prevBlock.transform.localScale.x, prevBlock.transform.localScale.y, splitScale));

            }
            else if (baseBlockZBounds.x > prevBlockZBounds.x)
            {
                split = baseBlockZBounds.x;
                float blockPos, splitPos;
                blockPos = split + 0.5f * overlap;
                prevBlock.transform.localScale = new Vector3(prevBlock.transform.localScale.x, prevBlock.transform.localScale.y, overlap);
                prevBlock.transform.position = new Vector3(prevBlock.transform.position.x, prevBlock.transform.position.y, blockPos);

   
                float splitScale = baseScale.z - overlap;
                splitPos = split - 0.5f * splitScale;
                CreateSplit(new Vector3(prevBlock.transform.position.x, prevBlock.transform.position.y, splitPos), new Vector3(prevBlock.transform.localScale.x, prevBlock.transform.localScale.y, splitScale));
            }
            else
            {
                Debug.Log("No split!");
            }
        }

        //Set new block scale to prevblock
        CreateBlock(prevBlock.transform.position.y + 2);
        currentBlock.transform.localScale = prevBlock.transform.localScale;
        currentBlock.transform.position = new Vector3(prevBlock.transform.position.x, currentBlock.transform.position.y, prevBlock.transform.position.z);

        //Move camera
        cameraHeightTarget += 2;

        //Update score
        score += 1;
        scoreDisplay.text = score.ToString();
    }

    void CreateBlock(float spawnHeight)
    {
        currentBlock = Instantiate(blockPrefab, new Vector3(0, spawnHeight, 0), new Quaternion(0, 0, 0, 0));
        currentBlock.name = "Block Level " + score;
        currentBlock.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((score + colorOffset) / 100f % 1f, 1f, 1f));
    }

    void CreateSplit(Vector3 SpawnPos, Vector3 Scale)
    {
        GameObject splitblock = Instantiate(blockPrefab, new Vector3(0,0,0), new Quaternion(0, 0, 0, 0));
        splitblock.name = "Block split " + score;
        splitblock.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((score + colorOffset) / 100f % 1f, 1f, 1f));
        splitblock.transform.localScale = Scale;
        splitblock.transform.position = SpawnPos;
        splitblock.AddComponent<Rigidbody>();
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
