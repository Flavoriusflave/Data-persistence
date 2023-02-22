using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public Text HighscoreText;
    public GameObject GameOverText;
    
    private bool m_Started = false;
    private int m_Points;
    private int m_Highscore;
    private string m_Name;
    
    private bool m_GameOver = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    private void GameStart()
    {
        const float step = 0.6f;
        int perLine = Mathf.FloorToInt(4.0f / step);
        
        int[] pointCountArray = new [] {1,1,2,2,5,5};
        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                brick.PointValue = pointCountArray[i];
                brick.onDestroyed.AddListener(AddPoint);
            }
        }

        Ball = GameObject.Find("Ball").GetComponent<Rigidbody>();
        ScoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        HighscoreText = GameObject.Find("HighscoreText").GetComponent<Text>();
        GameOverText = GameObject.Find("GameoverText");
        LoadHighscore();
        GameOverText.SetActive(false);

    }

    private void Update()
    {
        if (!m_Started)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameStart();
                m_Started = true;
                float randomDirection = Random.Range(-1.0f, 1.0f);
                Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                forceDir.Normalize();

                Ball.transform.SetParent(null);
                Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
            }
        }
        else if (m_GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Started = false;
                m_GameOver = false;
                m_Points = 0;
                SceneManager.LoadScene(0);
            }
        }
    }

    void AddPoint(int point)
    {
        m_Points += point;
        ScoreText.text = $"Score : {m_Name} : {m_Points}";
    }

    public void GameOver()
    {
        SaveHighscore();
        m_GameOver = true;
        GameOverText.SetActive(true);
    }

    class SaveData
    {
        public int Highscore;
        public string Name;
    }

    public void ExitGame()
    {
        SaveHighscore();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit(); // original code to quit Unity player
#endif
    }

    public void ToMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void ToGame()
    {
        SceneManager.LoadScene(0);
    }

    public void GetName(TMP_InputField input)
    {
        m_Name = input.text;
    }

    public void SaveHighscore()
    {
        SaveData data = new SaveData();
        if (m_Points > m_Highscore)
        {
            data.Highscore = m_Points;
            data.Name = m_Name;
        
            string json = JsonUtility.ToJson(data);

            File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        }

        
    }

    public void LoadHighscore()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            m_Highscore = data.Highscore;
            print(data.Name);

            HighscoreText.text = $"Highscore : {data.Name} : {data.Highscore}";
        }
    }
}
