using System;
using UnityEngine;

public enum GameState
{
    None,
    WaitingForPlayer,
    Dissolve,
    Dream,
}

public class GameFlowManager : Singleton<GameFlowManager>
{
    [SerializeField] private GameState m_StartState;
    
    private GameState m_GameState = GameState.None;
    public GameState GameState { get { return m_GameState; } set { m_GameState = value; } }
    
    public static event Action<GameState> OnGameStateChanged;

    private void Start()
    {
        ChangeState(m_StartState);
    }

    public void ChangeState(GameState newState)
    {
        if (m_GameState == newState) return;
        
        m_GameState = newState;
        OnGameStateChanged?.Invoke(m_GameState);
    }
}
