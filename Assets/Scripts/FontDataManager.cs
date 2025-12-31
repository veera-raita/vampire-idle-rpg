using System;
using TMPro;
using UnityEngine;

public class FontDataManager : MonoSingleton<FontDataManager>
{
    [SerializeField] private FontVariables standardFont;
    [SerializeField] private FontVariables fabledFont;
    [SerializeField] private FontVariables highbirthFont;
    [SerializeField] private FontVariables serifFont;
    
    public FontVariables GetStandardFont() => standardFont;
    public FontVariables GetFabledFont() => fabledFont;
    public FontVariables GetHighbirthFont() => highbirthFont;
    public FontVariables GetSerifFont() => serifFont;
}

[Serializable]
public class FontVariables
{
    public TMP_FontAsset fontAsset;
    public int fontSize;
}