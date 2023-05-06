using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Messages 
{
    public Message[] messages;
}

[System.Serializable]
public class Message
{
    public int id;
    public TextContent[] content;
}

[System.Serializable]
public class TextContent 
{
    public int language;
    public string[] texts;
}


