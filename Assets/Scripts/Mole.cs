using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Mole : MonoBehaviour
{
    [Header("Graphics")]
    [SerializeField] private Sprite mole;
    [SerializeField] private Sprite moleHardHat;
    [SerializeField] private Sprite moleHatBroken;
    [SerializeField] private Sprite moleHit;
    [SerializeField] private Sprite moleHatHit;

    [Header("GameManager")]
    [SerializeField] private GameManager gameManager;

    private BoxCollider2D boxCollider2D;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 startPosition = new Vector2(0f, -5f);
    private Vector2 endPosition = new Vector2(0f, -0.40f);
    private float showDuration = 0.5f;
    private float duration = 1f;
    private bool hittable = true;
    private Vector2 boxOffset;
    private Vector2 boxSize;
    private Vector2 boxOffsetHidden;
    private Vector2 boxSizeHidden;



    //creating a enum to change the moles sprite 
    private enum MoleType { Standard, HardHat, Bomb};
    private MoleType moleType;
    private float hardRate = 0.25f;
    private float bombRate = 0f;
    private int lives;
    private int moleIndex; 







    // Start is called before the first frame update
    public void Activate(int level)
    {
        SetLevel(level);
        CreateNext();
        StartCoroutine(ShowHide(startPosition, endPosition));
    }

    // Update is called once per frame
    void Update()
    {

    }



    private void SetLevel(int level)
    {
        //increase the bombrate but not more than 0.25
        bombRate = Mathf.Min(level * 0.25f, 0.25f);
        //increase the number of moles with hat as the level goes up
        hardRate = Mathf.Min(level * 0.25f, 1f);

        float durationMin = Mathf.Clamp(1 - level * 0.1f, 0.01f, 1f);
        float durationMax = Mathf.Clamp(2 - level * 0.1f, 0.01f, 2f);
        duration = Random.Range(durationMin, durationMax);

    }
    
    private void CreateNext()
    {
        float random = Random.Range(0f,1f);

        if(random < bombRate)
        {
            moleType = MoleType.Bomb;
            animator.enabled = true;
        }
        else {
            animator.enabled = false;
            if (random < hardRate)
            {
                //if it is below the hardness value than it is a mole with a hat
                moleType = MoleType.HardHat;
                spriteRenderer.sprite = moleHardHat;
                //moles with hats dies after 2 clicks so lives value for it must be 2
                lives = 2;
            }
            else
            {
                moleType = MoleType.Standard;
                spriteRenderer.sprite = mole;
                lives = 1;
            }
        }
        

        //after assigning a type for the mole and before starting the animation make sure this new mole is hittable
        hittable = true;

    }

    private void OnMouseDown()
    {
       if(hittable)
        {
            switch(moleType) { 
                case MoleType.Standard:
                    spriteRenderer.sprite = moleHit;
                    gameManager.AddScore(moleIndex);
                    StopAllCoroutines();
                    StartCoroutine(QuickHide());
                    hittable = false; break;
                case MoleType.HardHat:
                    if (lives == 2)
                    {
                        spriteRenderer.sprite = moleHatBroken;
                        lives--;
                    }
                    else
                    {
                        spriteRenderer.sprite = moleHatHit;
                        gameManager.AddScore(moleIndex);
                        StopAllCoroutines();
                        StartCoroutine(QuickHide());
                        hittable = false;
                    }
                    break;
                case MoleType.Bomb:
                    gameManager.GameOver(1);
                    break;
                default: break;

            }
        }
    }

    private IEnumerator QuickHide()
    {
        //wait a bit before hiding the mole 
        yield return new WaitForSeconds(0.25f);
        
        //just to make sure we are not hiding a alive mole check if it is not hittable
        //game might spawn another mole before the dead mole hide animation ends
        if (!hittable)
        {
            //to return the mole to original position call hide function
            Hide();
        }
    }

    public void Hide()
    {
        //return to original position
        transform.position = startPosition;
        //change the values of collider so player cant click it again 
        boxCollider2D.offset = boxOffsetHidden;
        boxCollider2D.size = boxSizeHidden;
    }

    private void Awake()
    {
        //getting all the components 
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        //getting the collider values we'll need
        boxOffset = boxCollider2D.offset;
        boxSize = boxCollider2D.size;
        //change the y values of the collider so it turns into something that player cant click by accident 
        boxOffsetHidden = new Vector2(boxOffset.x, -startPosition.y/2f);
        boxSizeHidden = new Vector2(boxSize.x, 0f);  
    }



    private IEnumerator ShowHide(Vector2 start, Vector2 end)
    {
        transform.localPosition = start;
        float elapsed = 0f;
        while (elapsed < showDuration)
        {
            //changing the size and position of the collider with the mole animation 
            transform.localPosition = Vector2.Lerp(start, end, elapsed / showDuration);
            
            boxCollider2D.size= Vector2.Lerp(boxSizeHidden, boxSize, elapsed / showDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = end;
        boxCollider2D.offset = boxOffset;
        boxCollider2D.size = boxSize;

        yield return new WaitForSeconds(duration);

        elapsed = 0f;

        while (elapsed < showDuration)
        {
            transform.localPosition = Vector2.Lerp(end, start, elapsed / showDuration);
            boxCollider2D.offset = Vector2.Lerp(boxOffset, boxOffsetHidden, elapsed / showDuration);
            boxCollider2D.size = Vector2.Lerp(boxSize, boxSizeHidden, elapsed / showDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = start;
        boxCollider2D.offset = boxOffsetHidden;
        boxCollider2D.size = boxSizeHidden;

        //if the mole animation is complete and not disturbed by mouse click 
        //means player missed the mole
        if (hittable)
        {
            hittable = false;
            gameManager.Missed(moleIndex, moleType != MoleType.Bomb);
        }
    }

    public void StopGame()
    {
        hittable=false;
        StopAllCoroutines();    
    }


    public void SetIndex(int index)
    {
        moleIndex = index;
    }
}
