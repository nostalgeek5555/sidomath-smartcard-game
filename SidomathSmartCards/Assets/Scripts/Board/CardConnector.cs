using UnityEngine;
using UnityEngine.EventSystems;

public class CardConnector : MonoBehaviour, IDropHandler, ICanvasRaycastFilter
{
    public RectTransform rectTransform;
    public CardConnectorType cardConnectorType;
    public Animator animator;
    public new BoxCollider2D collider2D;
    public bool droppable = true;
    public bool dropped = false;
    public string cardConnectorTileIndex = "";

    public Tile tileBelow = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();  
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("valid dropped");

        if (eventData.pointerDrag != null && dropped == false)
        {
            if (eventData.pointerDrag.TryGetComponent(out Card card))
            {
                
                if (!CheckIfConnectedCardFlipped())
                {
                    Card parentCard = transform.parent.parent.parent.GetComponent<Card>();
                    BoardManager.Instance.OnDropCard(card, parentCard, this);
                    Debug.Log("Dropped on valid card connector");
                }

                else
                {
                    Debug.Log("on card flipped tween back card");
                    card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                }
            }
        }
    }

    private bool CheckIfConnectedCardFlipped()
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
    
    

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            Debug.Log($"collide with object == {collision.gameObject.name}");
            if (collision.TryGetComponent(out Card card))
            {
                //card.canvasGroup.blocksRaycasts = true;
                Debug.Log($"block ray card == {card.canvasGroup.blocksRaycasts}");
                if (card.pointerEventData.dragging)
                {
                    if (card.currentCardConnector != null)
                    {
                        card.currentCardConnector.animator.SetTrigger(HighlightType.Droppable.ToString());
                        card.currentCardConnector = this;
                        animator.ResetTrigger(HighlightType.Droppable.ToString());
                        animator.SetTrigger(HighlightType.Highlight.ToString());
                    }
                    
                    else
                    {
                        card.currentCardConnector = this;
                        animator.ResetTrigger(HighlightType.Droppable.ToString());
                        animator.SetTrigger(HighlightType.Highlight.ToString());
                    }
                }
                
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.TryGetComponent(out Card card))
            {
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
}
