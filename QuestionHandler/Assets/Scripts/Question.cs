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
    public string Category = "";
    [XmlElement]
    public List<string> Tags = new List<string>();

    public Question() { }
    public Question(string text, string category, List<string> tags)
    {
        QuestionText = text;
        Category = category;
        Tags = tags;
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
