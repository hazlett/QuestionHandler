using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
[XmlRoot]
public class Questions : MonoBehaviour {
    
    [XmlElement]
    public List<Question> questions = new List<Question>();

    [XmlIgnore]
    private string addQuestionURL = "http://hazlett206.ddns.net/QuestionManager/AddQuestion.php",
    getQuestionsURL = "http://hazlett206.ddns.net/QuestionManager/GetQuestions.php",
    getQuestionURL = "http://hazlett206.ddns.net/QuestionManager/GetQuestion.php",
    getQuestionsXMLURL = "http://hazlett206.ddns.net/QuestionManager/GetQuestionsXML.php",
    addQuestionsXMLURL = "http://hazlett206.ddns.net/QuestionManager/AddQuestions.php",
    getCategoriesURL = "http://hazlett206.ddns.net/QuestionManager/GetCategories.php",
    sendCategoriesURL = "http://hazlett206.ddns.net/QuestionManager/SetCategories.php",
    message = "";
    [XmlIgnore]
    private Questions instance;
    [XmlIgnore]
    public Questions Instance { get { return instance; } }
    [XmlIgnore]
    private Question current;
    [XmlIgnore]
    public string[] categories;
    [XmlIgnore]
    private int loading = 0;
    [XmlIgnore]
    private float timer = 0;
    private bool saving = false, loaded = false, load = false;
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
        current = new Question(3);
        Refresh();
	}
	
    void Update()
    {
        if (loading > 0) 
        {
            message = "SAVING TO SERVER";
        }
        else if ((loading == 0) && saving)
        {
            saving = false;
            message = "QUESTIONS SAVED";
            timer = 0;
        }
        else if (message != "")
        {
            timer += Time.deltaTime;
            if (timer > 5)
            {
                timer = 0;
                message = "";
            }
        }
        else if (load)
        {
            message = "...LOADING QUESTIONS...";
        }
        else if (loaded)
        {
            timer += Time.deltaTime;
            if (timer > 5)
            {
                timer = 0;
                message = "";
                loaded = false;
            }
        }

    }
    void OnGUI()
    {
        GUILayout.Label(message);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("NEW QUESTION"))
        {
            current = new Question(3);
        }
        if (GUILayout.Button("REFRESH"))
        {
            Refresh();
        }
        if (GUILayout.Button("SAVE QUESTIONS (Not including current)"))
        {
            SaveToServer();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10.0f);
        GUILayout.Label("<b>QUESTIONS</b>\nClick to select/modify");
        GUILayout.BeginHorizontal();
        Question remove = null;
        foreach (Question q in questions)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button(q.ID))
            {
                current = q;
            }
            if (GUILayout.Button("REMOVE"))
            {
                remove = q;
            }
            GUILayout.EndVertical();
        }
        if (remove != null)
        {
            questions.Remove(remove);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10.0f);
        GUILayout.Label("QUESTION ID/NAME");
        current.ID = GUILayout.TextField(current.ID);
        GUILayout.Label("QUESTION");
        current.QuestionText = GUILayout.TextField(current.QuestionText);

        GUILayout.Label("CORRECT ANSWER");
        current.CorrectAnswer = GUILayout.TextField(current.CorrectAnswer);
        GUILayout.BeginHorizontal();
        GUILayout.Label("WRONG ANSWERS");
        if (GUILayout.Button("ADD ANSWER"))
        {
            current.WrongAnswers.Add("");
        }
        if (GUILayout.Button("REMOVE ANSWER"))
        {
            current.WrongAnswers.RemoveAt(0);
        }
        GUILayout.EndHorizontal();
        for (int i = 0; i < current.WrongAnswers.Count; i++)
        {
            current.WrongAnswers[i] = GUILayout.TextField(current.WrongAnswers[i]);
        }
        GUILayout.Label("CATEGORY: " + categories[int.Parse(current.Category)] + "\nClick a category to set it"); 
        GUILayout.BeginHorizontal();
        for (int i = 0; i < categories.Length; i++)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button(categories[i]))
            {
                current.Category = i.ToString();
            }
            categories[i] = GUILayout.TextField(categories[i]);
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10.0f);
        if (GUILayout.Button("ADD TO QUESTIONS LIST"))
        {
            questions.Add(current);
            current = new Question(3);
        }
    }
    void Refresh()
    {
        timer = 0;
        loaded = false;
        load = true;
        StartCoroutine(GetQuestionsXML());
        StartCoroutine(GetCategories());
        //StartCoroutine(GetQuestions());
    }

    private IEnumerator GetCategories()
    {
        WWW www = new WWW(getCategoriesURL);
        yield return www;
        if (www.error == null)
        {
            Debug.Log("Cat: " + www.text);
            categories = DeseralizeToCategories(www.text);
        }
        else
        {
            Debug.Log("Error downloading: " + www.error);
        }
    }
    private IEnumerator SendCategories()
    {
        WWWForm form = new WWWForm();
        string xml = CategoriesToXml(categories);
        Debug.Log("CATS: " + xml);
        form.AddField("contents", xml);
        WWW www = new WWW(sendCategoriesURL, form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log("Successful question upload: " + www.text);
        }
        else
        {
            Debug.Log("Error uploading: " + www.error);
        }
    }



    void SaveLocal()
    {
        foreach (Question question in questions)
        {

        }
    }
    void SaveToServer()
    {
        saving = true;
        foreach (Question question in questions)
        {
            //loading++;
            StartCoroutine(SendToServer(question));
        }
        StartCoroutine(SendQuestionsXML());
        StartCoroutine(SendCategories());
    }
    private IEnumerator SendToServer(Question question)
    {

        WWWForm form = new WWWForm();
        Debug.Log("ID: " + question.ID);
        form.AddField("name", question.ID);
        Debug.Log(question.ToXml());
        form.AddField("contents", question.ToXml());
        WWW www = new WWW(addQuestionURL, form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log("Successful question upload: " + www.text);
        }
        else
        {
            Debug.Log("Error uploading: " + www.error);
        }
        //loading--;
    }

    private IEnumerator GetQuestionsXML()
    {
        WWW www = new WWW(getQuestionsXMLURL);
        yield return www;
        questions = new List<Question>();
        if (www.error == null)
        {
            Debug.Log("Successful list download: " + www.text);
            questions = DeserializeToQuestions(www.text);
        }
        else
        {
            Debug.Log("Error downloading: " + www.error);
        }
        loaded = true;
        load = false;
        message = "QUESTIONS LOADED";
    }
    private IEnumerator SendQuestionsXML()
    {
        WWWForm form = new WWWForm();
        form.AddField("contents", QuestionsToXml(questions));
        WWW www = new WWW(addQuestionsXMLURL, form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log("Successful questions upload: " + www.text);
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
            Debug.Log("Successful list download: " + www.text);
            names = DeserializeToList(www.text);
            questions = new List<Question>();
            foreach (string name in names)
            {
                StartCoroutine(AddQuestionFromServer(name.Replace(".xml", "")));
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
        Debug.Log("GETTING: " + name);
        WWW www = new WWW(getQuestionURL, form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log("Successful question download: " + www.text);
            if (www.text != "")
            {
                questions.Add(DeserializeToQuestion(www.text));
            }
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
        List<string> list = new List<string>();
        foreach (string file in obj)
        {
            list.Add(file.Replace(".xml", ""));
        }
        return list;
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
    private string QuestionsToXml(List<Question> list)
    {
        XmlSerializer xmls = new XmlSerializer(typeof(List<Question>));
        StringWriter writer = new StringWriter();
        xmls.Serialize(writer, list);
        writer.Close();
        return writer.ToString();  
    }
    private string CategoriesToXml(string[] list)
    {
        XmlSerializer xmls = new XmlSerializer(typeof(string[]));
        StringWriter writer = new StringWriter();
        xmls.Serialize(writer, list);
        writer.Close();
        return writer.ToString();
    }
    private string[] DeseralizeToCategories(string text)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);

        XmlSerializer serializer = new XmlSerializer(typeof(string[]));
        XmlReader reader = new XmlNodeReader(doc);

        string[] obj = serializer.Deserialize(reader) as string[];

        return obj;
    }
    private List<Question> DeserializeToQuestions(string text)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);

        List<Question> obj = new List<Question>();
        XmlSerializer serializer = new XmlSerializer(typeof(List<Question>));
        XmlReader reader = new XmlNodeReader(doc);

        obj = serializer.Deserialize(reader) as List<Question>;

        return obj;
    }
}
