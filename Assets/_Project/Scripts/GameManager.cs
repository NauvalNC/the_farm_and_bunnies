using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    [Header("Crates Manager")]
    [SerializeField]
    protected Transform crateLister;
    public List<Crate> crates = new List<Crate>();
    public Text numOfItemTxt;
    int m_currentTotalNumberOfItem, m_totalNumberOfItem;

    [Header("Enemy Wave Manager")]
    public int numberOfWave = 2;
    public Transform wavePortalLister;
    public Text waveTxt, numberOfEnemyTxt;
    protected List<WavePortal> m_wavePortal = new List<WavePortal>();
    int m_wave;
    int m_currWaveOutOfDuties, m_currWaveTotalEnemies;

    GameObject m_systemTopPriority;
    bool m_isPointerOverUI;

    bool m_isGameOver = false, m_isGameOverInvoked = false;

    private void Awake()
    {
        SetupAttributes();
    }

    private void Start()
    {
        StartWaveDelay();
    }

    private void Update()
    {
        ManageWave();
        ManageCrates();
        CheckPointerOverUI();
        UpdateGraphics();

        if (m_isGameOver) OnGameOver();
    }

    void SetupAttributes()
    {
        m_totalNumberOfItem = 0;
        Crate m_temp;
        foreach (Transform child in crateLister)
        {
            m_temp = child.GetComponent<Crate>();
            m_totalNumberOfItem += m_temp.GetNumberOfItems();
            crates.Add(m_temp);
        }

        foreach(Transform child in wavePortalLister)
        {
            m_wavePortal.Add(child.GetComponent<WavePortal>());
        }
    }

    void UpdateGraphics()
    {
        waveTxt.text = "Wave " + m_wave;
        numberOfEnemyTxt.text = m_currWaveOutOfDuties + "/" + m_currWaveTotalEnemies;
        numOfItemTxt.text = m_currentTotalNumberOfItem + "/" + m_totalNumberOfItem;
    }

    #region Request System Priority

    public void GainTopPriority(GameObject gameObject)
    {
        if (IsPriorityAvailable(gameObject) == false) return;
        m_systemTopPriority = gameObject;
    }

    public void ReleaseTopPriority(GameObject gameObject)
    {
        if (m_systemTopPriority != gameObject) return;
        m_systemTopPriority = null;
    }

    public bool IsPriorityAvailable(GameObject gameObject)
    {
        if (gameObject == m_systemTopPriority) return true;
        return m_systemTopPriority == null;
    }

    #endregion

    #region Input Output Manager

    void CheckPointerOverUI()
    {
        PointerEventData m_e = new PointerEventData(EventSystem.current);
        m_e.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> m_currentUICatched = new List<RaycastResult>();
        EventSystem.current.RaycastAll(m_e, m_currentUICatched);
        m_isPointerOverUI = m_currentUICatched.Count > 0;
    }

    public bool IsPointerOverUI()
    {
        return m_isPointerOverUI;
    }

    #endregion

    #region Enemy Wave Manager

    void StartWaveDelay()
    {
        HUDManager.Instance.highlightTitle.text = "Wave " + (m_wave + 1);
        HUDManager.Instance.highlightSubTitle.text = "Wave Started!";
        HUDManager.Instance.GetHUDAC().Play("wave_in");
    }

    public void StartNextWave()
    {
        if (m_wave + 1 > numberOfWave) return;
        m_wave++;

        m_currWaveOutOfDuties = m_currWaveTotalEnemies = 0;
        foreach (WavePortal portal in m_wavePortal)
        {
            m_currWaveTotalEnemies += portal.enemyWaves[m_wave-1].enemiesToSpawn.Count;
            portal.StartPortal(m_wave);
        }
    }

    void ManageWave()
    {
        // This is when no wave has been started, then nothing need to manage
        if (m_wave <= 0 || m_isGameOver) return;

        bool m_waveEnded = true;
        m_currWaveOutOfDuties = 0;

        foreach (WavePortal portal in m_wavePortal)
        {
            if (portal.IsWaveEnded() == false)
            {
                m_waveEnded = false;
            }

            m_currWaveOutOfDuties += portal.GetNumberOutOfDutyEnemies();
        }

        if (m_waveEnded && m_wave < numberOfWave) StartWaveDelay();
        else if (m_waveEnded && m_wave >= numberOfWave) m_isGameOver = true;
    }

    public int GetWave() { return m_wave; }

    #endregion

    #region Crate

    void ManageCrates()
    {
        m_currentTotalNumberOfItem = 0;
        foreach (Crate m_crate in crates)
        {
            m_currentTotalNumberOfItem = m_crate.GetActualNumberOfItems();
        }
    }

    #endregion

    #region Game Over Manager
    
    void OnGameOver()
    {
        if (m_isGameOverInvoked) return;
        m_isGameOverInvoked = true;

        HUDManager.Instance.highlightTitle.text = "Game Over!";
        HUDManager.Instance.highlightSubTitle.text = "The game is over!";
        HUDManager.Instance.GetHUDAC().Play("wave_in");
    }

    #endregion
}
