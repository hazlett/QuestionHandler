using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
[XmlRoot]
public class Questions : MonoBehaviour {
    
    [XmlElement]
    public List<Question> questions = new List<Question>();

    [XmlIgnore]
    private string addQuestionURL = "http://hazlett206.ddns.net/QuestionManager/AddQuestion.php",
    getQuestionsURL = "http://hazlett206.ddns.net/QuestionManager/GetQuestions.php",
    getQuestionURL = "http://hazlett206.ddns.net/QuestionManager/GetQuestion.php";
    [XmlIgnore]
    private Questions instance;
    [XmlIgnore]
    public Questions Instance { get { return instance; } }
    public Questions() { }
	
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start () {
        Refresh();
	}
	
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("REFRESH"))
        {
            Refresh();
        }
        if (GUILayout.Button("SAVE"))
        {
            SaveToServer();
        }
        GUILayout.EndHorizontal();

        foreach (Question q in questions)
        {
            GUILayout.Label(q.ID);
        }
    }
    void Refresh()
    {
        StartCoroutine(GetQuestions());
    }
    void SaveLocal()
    {
        foreach (Question question in questions)
        {

        }
    }
    void SaveToServer()
    {
        foreach (Question question in questions)
        {
            StartCoroutine(SendToServer(question));
        }
    }
    private IEnumerator SendToServer(Question question)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", question.ID);
        form.AddField("contents", question.ToXml());
        WWW www = new WWW(addQuestionURL, form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log("Successful upload");
        }
        else
        {
            Debug.Log("Error uploading: " + www.error);
        }
    }
    private IEnumerator GetQuestions()
    {
        WWW www = new WWW(getQuestionsURL);
        yield return www;
        List<string> names = new List<string>();
        if (www.error == null)
        {
            Debug.Log("Successful download");
            names = DeserializeToList(www.text);
            questions = new List<Question>();
            foreach (string name in names)
            {
                StartCoroutine(AddQuestionFromServer(name));
            }
        }
        else
        {
            Debug.Log("Error downloading: " + www.error);
        }
    }
    private IEnumerator AddQuestionFromServer(string name)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", name);
        WWW www = new WWW(getQuestionURL, form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log("Successful upload");
            questions.Add(DeserializeToQuestion(www.text));
        }
        else
        {
            Debug.Log("Error uploading: " + www.error);
        }
    }
    private List<string> DeserializeToList(string text)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);

        List<string> obj = new List<string>();
        XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
        XmlReader reader = new XmlNodeReader(doc);

        obj = serializer.Deserialize(reader) as List<string>;

        return obj;
    }
    private Question DeserializeToQuestion(string text)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);

        Question obj = new Question();
        XmlSerializer serializer = new XmlSerializer(typeof(Question));
        XmlReader reader = new XmlNodeReader(doc);

        obj = serializer.Deserialize(reader) as Question;

        return obj;
    }
}
