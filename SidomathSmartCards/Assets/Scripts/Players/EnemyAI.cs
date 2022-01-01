public class EnemyAI : Player
{
    public override void StateController(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.GET_TURN:
                break;
            case PlayerState.PROCESSING:
                break;
            case PlayerState.END_TURN:
                break;
            case PlayerState.SKIP_TURN:
                break;
            case PlayerState.WIN:
                break;
            case PlayerState.GAME_OVER:
                break;
            default:
                break;
        }
    }

    public override void OnGettingTurn()
    {
        playerState = PlayerState.GET_TURN;
    }

    public override void OnEndingTurn()
    {
        playerState = PlayerState.END_TURN;
    }

    public override void OnSkipTurn()
    {
        playerState = PlayerState.SKIP_TURN;
    }




}
