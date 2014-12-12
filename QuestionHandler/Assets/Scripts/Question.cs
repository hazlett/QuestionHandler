using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
[XmlRoot]
public class Question {
    [XmlAttribute]
    public string ID = "";
    [XmlAttribute]
    public string QuestionText = "";
    [XmlAttribute]
    public string Category = "0";
    [XmlElement]
    public List<string> WrongAnswers = new List<string>();
    [XmlAttribute]
    public string CorrectAnswer = "";

    public Question() { }
    public Question(int wrongAnswers)
    {
        for (int i = 0; i < wrongAnswers; i++)
        {
            WrongAnswers.Add("");
        }
    }
    public Question(string text, string category)
    {
        QuestionText = text;
        Category = category;
    }
    public string ToXml()
    {
        XmlSerializer xmls = new XmlSerializer(typeof(Question));
        StringWriter writer = new StringWriter();
        xmls.Serialize(writer, this);
        writer.Close();
        return writer.ToString();       
    }
}
