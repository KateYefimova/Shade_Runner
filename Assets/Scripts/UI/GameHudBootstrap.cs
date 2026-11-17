using UnityEngine;

[RequireComponent(typeof(GameHudView))]
public class GameHudBootstrap : MonoBehaviour
{
    private GameHudPresenter presenter;

    private void Start()
    {
        GameManager game = GameManager.Instance;
        if (game != null)
            presenter = new GameHudPresenter(game.Model, GetComponent<GameHudView>(), game);
    }

    private void OnDestroy()
    {
        presenter?.Dispose();
    }
}
