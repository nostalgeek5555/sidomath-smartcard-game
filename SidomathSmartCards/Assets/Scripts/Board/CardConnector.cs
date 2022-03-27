using UnityEngine;
using UnityEngine.EventSystems;

public class CardConnector : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform rectTransform;
    public CardConnectorType cardConnectorType;
    public Facing facing;
    public Animator animator;
    public new BoxCollider2D collider2D;
    public bool droppable = true;
    public bool dropped = false;
    public string cardConnectorTileIndex = "";

    public int droppedCardPairingIndex;
    public int parentCardPairingIndex;
    public Card.MatchedSide matchedSide;
    public Card.MatchedSide parentMatchedSide;
    public Card draggedCard = null;
    public Tile tileBelow = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();  
    }

    public bool CheckIfConnectedCardFlipped()
    {
        Card parentCard = transform.parent.parent.parent.GetComponent<Card>();

        if (parentCard.flipped)
        {
            return true;
        }

        else
        {
            return false;
        }
    }
    
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            Debug.Log($"collide with object == {collision.gameObject.name}");
            if (collision.TryGetComponent(out Card card))
            {
                if (!card.onTweeningBack)
                {
                    if (card.currentCardConnector != null)
                    {
                        card.currentCardConnector.animator.SetTrigger(HighlightType.Droppable.ToString());
                        card.currentCardConnector = this;
                        card.insideCardConnector = true;
                        animator.ResetTrigger(HighlightType.Droppable.ToString());
                        animator.SetTrigger(HighlightType.Highlight.ToString());
                    }

                    else
                    {
                        card.currentCardConnector = this;
                        card.insideCardConnector = true;
                        animator.ResetTrigger(HighlightType.Droppable.ToString());
                        animator.SetTrigger(HighlightType.Highlight.ToString());
                    }
                }
            }

            else
            {
                if (collision.TryGetComponent(out CardConnector cardConnector))
                {
                    Debug.Log($"card connector tile {cardConnector.cardConnectorTileIndex}");
                    Card _collidedParentCard = transform.parent.parent.parent.GetComponent<Card>();
                    if (_collidedParentCard.GetInstanceID() != GetInstanceID())
                    {
                        //Debug.Log($"collided with different card guid, this id {GetInstanceID()} & other card id {_collidedParentCard.GetInstanceID()}");
                        cardConnector.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.TryGetComponent(out Card card))
            {
                if (!card.onTweeningBack)
                {
                    card.currentCardConnector = this;
                    card.insideCardConnector = true;
                    animator.ResetTrigger(HighlightType.Droppable.ToString());
                    animator.SetTrigger(HighlightType.Highlight.ToString());
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.TryGetComponent(out Card card))
            {
                card.insideCardConnector = false;
                card.canvasGroup.blocksRaycasts = false;
                animator.ResetTrigger(HighlightType.Highlight.ToString());
                animator.SetTrigger(HighlightType.Droppable.ToString());
            }
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        var worldPoint = Vector3.zero;
        var isInside = RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            sp,
            eventCamera,
            out worldPoint
        );

        if (isInside)
            isInside = collider2D.OverlapPoint(worldPoint);
        return isInside;
    }

    public enum HighlightType
    {
        Normal = 0,
        Droppable = 1,
        Highlight = 2
    }

    public enum CardConnectorType
    {
        Top = 0,
        TopRight = 1,
        TopLeft = 2,
        Bottom = 3,
        BottomRight = 4,
        BottomLeft = 5
    }

    public enum Facing
    {
        VERTICAL = 0,
        HORIZONTAL = 1
    }
}
