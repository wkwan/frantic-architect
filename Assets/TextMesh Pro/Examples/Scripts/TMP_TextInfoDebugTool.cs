using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{

    [ExecuteInEditMode]
    public class TMP_TextInfoDebugTool : MonoBehaviour
    {
        enum objectType { None = 0, TextMeshPro = 1, TextMeshProUI = 2 };

        private objectType m_textObjectType = objectType.None;

        public bool ShowCharacters;
        public bool ShowWords;
        public bool ShowLinks;
        public bool ShowLines;
        public bool ShowBounds;
        [Space(10)]
        [TextArea(2, 2)]
        public string ObjectStats;


        private TextMeshPro m_TextMeshPro;
        private TextMeshProUGUI m_TextMeshProUI;

        private Transform m_Transform;


        void OnEnable()
        {
            m_TextMeshPro = gameObject.GetComponent<TextMeshPro>();
            m_TextMeshProUI = gameObject.GetComponent<TextMeshProUGUI>();

            if (m_TextMeshPro != null) m_textObjectType = objectType.TextMeshPro;
            else if (m_TextMeshProUI != null) m_textObjectType = objectType.TextMeshProUI;
            else m_textObjectType = objectType.None;

            if (m_Transform == null)
                m_Transform = gameObject.GetComponent<Transform>();
        }


        void OnDrawGizmos()
        {
            // Update Text Statistics
            switch (m_textObjectType)
            {
                case objectType.TextMeshPro:
                    TMP_TextInfo textInfo = m_TextMeshPro.textInfo;
                    ObjectStats = "Characters: " + textInfo.characterCount + "   Words: " + textInfo.wordCount + "   Spaces: " + textInfo.spaceCount + "   Sprites: " + textInfo.spriteCount + "   Links: " + textInfo.linkCount
                                + "\nLines: " + textInfo.lineCount + "   Pages: " + textInfo.pageCount;
                    break;
                case objectType.TextMeshProUI:
                    textInfo = m_TextMeshProUI.textInfo;
                    ObjectStats = "Characters: " + textInfo.characterCount + "   Words: " + textInfo.wordCount + "   Spaces: " + textInfo.spaceCount + "   Sprites: " + textInfo.spriteCount + "   Links: " + textInfo.linkCount
                                + "\nLines: " + textInfo.lineCount + "   Pages: " + textInfo.pageCount;
                    break;
            }

            // Draw Quads around each of the Characters
            #region Draw Characters
            if (ShowCharacters)
            {
                switch (m_textObjectType)
                {
                    case objectType.TextMeshPro:
                        DrawCharacters(m_TextMeshPro);
                        break;
                    case objectType.TextMeshProUI:
                        DrawCharacters(m_TextMeshProUI);
                        break;
                }
            }
            #endregion


            // Draw Quads around each of the words
            #region Draw Words
            if (ShowWords)
            {
                switch(m_textObjectType)
                {
                    case objectType.TextMeshPro:
                        DrawWords(m_TextMeshPro);
                        break;
                    case objectType.TextMeshProUI:
                        DrawWords(m_TextMeshProUI);
                        break;
                }
            }
            #endregion


            // Draw Quads around each of the words
            #region Draw Links
            if (ShowLinks)
            {
                switch (m_textObjectType)
                {
                    case objectType.TextMeshPro:
                        DrawLinks(m_TextMeshPro);
                        break;
                    case objectType.TextMeshProUI:
                        DrawLinks(m_TextMeshProUI);
                        break;
                }
            }
            #endregion


            // Draw Quads around each line
            #region Draw Lines
            if (ShowLines)
            {
                switch (m_textObjectType)
                {
                    case objectType.TextMeshPro:
                        DrawLines(m_TextMeshPro);
                        break;
                    case objectType.TextMeshProUI:
                        DrawLines(m_TextMeshProUI);
                        break;
                }
            }
            #endregion


            // Draw Quads around the bounds of the text
            #region Draw Bounds
            if (ShowBounds)
            {
                switch (m_textObjectType)
                {
                    case objectType.TextMeshPro:
                        DrawBounds(m_TextMeshPro);
                        break;
                    case objectType.TextMeshProUI:
                        DrawBounds(m_TextMeshProUI);
                        break;
                }
            }
            #endregion
        }


        /// <summary>
        /// Method to draw a rectangle around each character.
        /// </summary>
        /// <param name="text"></param>
        void DrawCharacters(TextMeshPro text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                // Draw visible as well as invisible characters
                TMP_CharacterInfo cInfo = textInfo.characterInfo[i];

                bool isCharacterVisible = i >= text.maxVisibleCharacters ||
                                          cInfo.lineNumber >= text.maxVisibleLines ||
                                          (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                if (!isCharacterVisible) continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bottomLeft = m_Transform.TransformPoint(cInfo.bottomLeft);
                Vector3 topLeft = m_Transform.TransformPoint(new Vector3(cInfo.topLeft.x, cInfo.topLeft.y, 0));
                Vector3 topRight = m_Transform.TransformPoint(cInfo.topRight);
                Vector3 bottomRight = m_Transform.TransformPoint(new Vector3(cInfo.bottomRight.x, cInfo.bottomRight.y, 0));

                Color color = cInfo.isVisible ? Color.yellow : Color.grey;
                DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, color);

                // Draw Ascender & Descender for each character.
                //Vector3 ascenderStart = new Vector3(topLeft.x, m_Transform.TransformPoint(new Vector3(0, cInfo.topLine, 0)).y, 0);
                //Vector3 ascenderEnd = new Vector3(topRight.x, m_Transform.TransformPoint(new Vector3(0, cInfo.topLine, 0)).y, 0);
                //Vector3 descenderStart = new Vector3(bottomLeft.x, m_Transform.TransformPoint(new Vector3(0, cInfo.bottomLine, 0)).y, 0);
                //Vector3 descenderEnd = new Vector3(bottomRight.x, m_Transform.TransformPoint(new Vector3(0, cInfo.bottomLine, 0)).y, 0);

                //Gizmos.color = Color.cyan;
                //Gizmos.DrawLine(ascenderStart, ascenderEnd);
                //Gizmos.DrawLine(descenderStart, descenderEnd);

            }
        }


        /// <summary>
        /// Method to draw a rectangle around each character.
        /// </summary>
        /// <param name="text"></param>
        void DrawCharacters(TextMeshProUGUI text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                // Draw visible as well as invisible characters
                TMP_CharacterInfo cInfo = textInfo.characterInfo[i];

                bool isCharacterVisible = i >= text.maxVisibleCharacters ||
                                          cInfo.lineNumber >= text.maxVisibleLines ||
                                          (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                if (!isCharacterVisible) continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bottomLeft = m_Transform.TransformPoint(cInfo.bottomLeft);
                Vector3 topLeft = m_Transform.TransformPoint(new Vector3(cInfo.topLeft.x, cInfo.topLeft.y, 0));
                Vector3 topRight = m_Transform.TransformPoint(cInfo.topRight);
                Vector3 bottomRight = m_Transform.TransformPoint(new Vector3(cInfo.bottomRight.x, cInfo.bottomRight.y, 0));

                Color color = cInfo.isVisible ? Color.yellow : Color.grey;
                DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, color);

                // Draw Ascender & Descender for each character.
                //Vector3 ascenderStart = new Vector3(topLeft.x, m_Transform.TransformPoint(new Vector3(0, cInfo.topLine, 0)).y, 0);
                //Vector3 ascenderEnd = new Vector3(topRight.x, m_Transform.TransformPoint(new Vector3(0, cInfo.topLine, 0)).y, 0);
                //Vector3 descenderStart = new Vector3(bottomLeft.x, m_Transform.TransformPoint(new Vector3(0, cInfo.bottomLine, 0)).y, 0);
                //Vector3 descenderEnd = new Vector3(bottomRight.x, m_Transform.TransformPoint(new Vector3(0, cInfo.bottomLine, 0)).y, 0);

                //Gizmos.color = Color.cyan;
                //Gizmos.DrawLine(ascenderStart, ascenderEnd);
                //Gizmos.DrawLine(descenderStart, descenderEnd);
            }
        }


        /// <summary>
        /// Method to draw rectangles around each word of the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawWords(TextMeshPro text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bottomLeft = Vector3.zero;
                Vector3 topLeft = Vector3.zero;
                Vector3 bottomRight = Vector3.zero;
                Vector3 topRight = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                Color wordColor = Color.green;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    // Track Max Ascender and Min Descender
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.topLine);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.bottomLine);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0);
                        topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                            bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                            bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                            topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                            // Draw Region
                            DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);
                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;

                    }
                }

                //Debug.Log(wInfo.GetWord(m_TextMeshPro.textInfo.characterInfo));
            }


        }


        /// <summary>
        /// Method to draw rectangles around each word of the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawWords(TextMeshProUGUI text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bottomLeft = Vector3.zero;
                Vector3 topLeft = Vector3.zero;
                Vector3 bottomRight = Vector3.zero;
                Vector3 topRight = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                Color wordColor = Color.green;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    // Track Max Ascender and Min Descender
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.topLine);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.bottomLine);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0);
                        topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                            bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                            bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                            topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                            // Draw Region
                            DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, wordColor);
                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;

                    }
                }

                //Debug.Log(wInfo.GetWord(textInfo.characterInfo));
            }
        }


        /// <summary>
        /// Draw rectangle around each of the links contained in the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawLinks(TextMeshPro text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bottomLeft = Vector3.zero;
                Vector3 topLeft = Vector3.zero;
                Vector3 bottomRight = Vector3.zero;
                Vector3 topRight = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                Color32 linkColor = Color.cyan;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.characterCount; j++)
                {
                    int characterIndex = linkInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    // Track Max Ascender and Min Descender
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.topLine);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.bottomLine);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0);
                        topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Link is one character
                        if (linkInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                            bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                            bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                            topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                            // Draw Region
                            DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Link
                    if (isBeginRegion && j == linkInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Link is split on more than one line.
                    else if (isBeginRegion && currentLine != textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;
                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log(wInfo.GetWord(m_TextMeshPro.textInfo.characterInfo));
            }
        }


        /// <summary>
        /// Draw rectangle around each of the links contained in the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawLinks(TextMeshProUGUI text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bottomLeft = Vector3.zero;
                Vector3 topLeft = Vector3.zero;
                Vector3 bottomRight = Vector3.zero;
                Vector3 topRight = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                Color32 linkColor = Color.cyan;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.characterCount; j++)
                {
                    int characterIndex = linkInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    // Track Max Ascender and Min Descender
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.topLine);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.bottomLine);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0);
                        topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Link is one character
                        if (linkInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                            bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                            bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                            topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                            // Draw Region
                            DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Link
                    if (isBeginRegion && j == linkInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Link is split on more than one line.
                    else if (isBeginRegion && currentLine != textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                        bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                        bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                        topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                        // Draw Region
                        DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, linkColor);

                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;
                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log(wInfo.GetWord(m_TextMeshPro.textInfo.characterInfo));
            }
        }


        /// <summary>
        /// Draw Rectangles around each lines of the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawLines(TextMeshPro text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.lineCount; i++)
            {
                TMP_LineInfo lineInfo = textInfo.lineInfo[i];

                bool isLineVisible = (lineInfo.characterCount == 1 && textInfo.characterInfo[lineInfo.firstCharacterIndex].character == 10) ||
                                      i > text.maxVisibleLines ||
                                     (text.OverflowMode == TextOverflowModes.Page && textInfo.characterInfo[lineInfo.firstCharacterIndex].pageNumber + 1 != text.pageToDisplay) ? false : true;

                if (!isLineVisible) continue;

                //if (!ShowLinesOnlyVisibleCharacters)
                //{
                    // Get Bottom Left and Top Right position of each line
                    float ascender = lineInfo.ascender;
                    float descender = lineInfo.descender;
                    Vector3 bottomLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLeft.x, descender, 0));
                    Vector3 topLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLeft.x, ascender, 0));
                    Vector3 topRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastCharacterIndex].topRight.x, ascender, 0));
                    Vector3 bottomRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastCharacterIndex].topRight.x, descender, 0));

                    DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, Color.green);

                    Vector3 baselineStart = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLeft.x, textInfo.characterInfo[lineInfo.firstCharacterIndex].baseLine, 0));
                    Vector3 baselineEnd = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastCharacterIndex].topRight.x, textInfo.characterInfo[lineInfo.lastCharacterIndex].baseLine, 0));

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(baselineStart, baselineEnd);
                //}
                //else
                //{
                    //// Get Bottom Left and Top Right position of each line
                    //float ascender = lineInfo.ascender;
                    //float descender = lineInfo.descender;
                    //Vector3 bottomLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].bottomLeft.x, descender, 0));
                    //Vector3 topLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].bottomLeft.x, ascender, 0));
                    //Vector3 topRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].topRight.x, ascender, 0));
                    //Vector3 bottomRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].topRight.x, descender, 0));

                    //DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, Color.green);

                    //Vector3 baselineStart = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].bottomLeft.x, textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].baseLine, 0));
                    //Vector3 baselineEnd = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].topRight.x, textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].baseLine, 0));

                    //Gizmos.color = Color.cyan;
                    //Gizmos.DrawLine(baselineStart, baselineEnd);
                //}
            }
        }


        /// <summary>
        /// Draw Rectangles around each lines of the text.
        /// </summary>
        /// <param name="text"></param>
        void DrawLines(TextMeshProUGUI text)
        {
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.lineCount; i++)
            {
                TMP_LineInfo lineInfo = textInfo.lineInfo[i];

                bool isLineVisible = (lineInfo.characterCount == 1 && textInfo.characterInfo[lineInfo.firstCharacterIndex].character == 10) ||
                                      i > text.maxVisibleLines ||
                                     (text.OverflowMode == TextOverflowModes.Page && textInfo.characterInfo[lineInfo.firstCharacterIndex].pageNumber + 1 != text.pageToDisplay) ? false : true;

                if (!isLineVisible) continue;

                // Get Bottom Left and Top Right position of each line
                float ascender = lineInfo.ascender; // textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLine;
                float descender = lineInfo.descender; // textInfo.characterInfo[lineInfo.lastCharacterIndex].topLine;
                Vector3 bottomLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLeft.x, descender, 0));
                Vector3 topLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLeft.x, ascender, 0));
                Vector3 topRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastCharacterIndex].topRight.x, ascender, 0));
                Vector3 bottomRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastCharacterIndex].topRight.x, descender, 0));

                DrawDottedRectangle(bottomLeft, topLeft, topRight, bottomRight, Color.green);

                Vector3 baselineStart = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLeft.x, textInfo.characterInfo[lineInfo.firstCharacterIndex].baseLine, 0));
                Vector3 baselineEnd = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastCharacterIndex].topRight.x, textInfo.characterInfo[lineInfo.firstCharacterIndex].baseLine, 0));

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(baselineStart, baselineEnd);
                
                // Draw first and last character of each line
                bottomLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].bottomLeft.x, descender, 0));
                topLeft = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].bottomLeft.x, ascender, 0));
                topRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].topRight.x, ascender, 0));
                bottomRight = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].topRight.x, descender, 0));

                DrawRectangle(bottomLeft, topLeft, topRight, bottomRight, new Color32(0, 255, 0, 255));

                //Vector3 baselineStart = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].bottomLeft.x, textInfo.characterInfo[lineInfo.firstVisibleCharacterIndex].baseLine, 0));
                //Vector3 baselineEnd = m_Transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].topRight.x, textInfo.characterInfo[lineInfo.lastVisibleCharacterIndex].baseLine, 0));

                //Gizmos.color = Color.cyan;
                //Gizmos.DrawLine(baselineStart, baselineEnd);
            }
        }



        void DrawBounds(TextMeshPro text)
        {
            // Get Bottom Left and Top Right position of each word
            //Vector3 bottomLeft = text.transform.position - text.bounds.extents + text.bounds.center;
            //Vector3 topRight = text.transform.position + text.bounds.extents + text.bounds.center;
            Vector3 bottomLeft = text.renderer.bounds.center - text.renderer.bounds.extents;
            Vector3 topRight = text.renderer.bounds.center + text.renderer.bounds.extents;


            DrawRectangle(bottomLeft, topRight, new Color(1, 0.5f, 0));
        }


        void DrawBounds(TextMeshProUGUI text)
        {
            // Get Bottom Left and Top Right position of each word
            Vector3 bottomLeft = text.transform.position - text.bounds.extents + text.bounds.center;
            Vector3 topRight = text.transform.position + text.bounds.extents + text.bounds.center;

            DrawRectangle(bottomLeft, topRight, new Color(1, 0.5f, 0));
        }


        // Draw Rectangles
        void DrawRectangle(Vector3 BL, Vector3 TR, Color color)
        {
            Gizmos.color = color;

            Gizmos.DrawLine(new Vector3(BL.x, BL.y, 0), new Vector3(BL.x, TR.y, 0));
            Gizmos.DrawLine(new Vector3(BL.x, TR.y, 0), new Vector3(TR.x, TR.y, 0));
            Gizmos.DrawLine(new Vector3(TR.x, TR.y, 0), new Vector3(TR.x, BL.y, 0));
            Gizmos.DrawLine(new Vector3(TR.x, BL.y, 0), new Vector3(BL.x, BL.y, 0));
        }


        // Draw Rectangles
        void DrawRectangle(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Color color)
        {
            Gizmos.color = color;

            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
        }


        // Draw Rectangles
        void DrawDottedRectangle(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Color color)
        {
#if UNITY_EDITOR
            var cam = Camera.current;
            float dotSpacing = (cam.WorldToScreenPoint(br).x - cam.WorldToScreenPoint(bl).x) / 75f;
            UnityEditor.Handles.color = color;

            UnityEditor.Handles.DrawDottedLine(bl, tl, dotSpacing);
            UnityEditor.Handles.DrawDottedLine(tl, tr, dotSpacing);
            UnityEditor.Handles.DrawDottedLine(tr, br, dotSpacing);
            UnityEditor.Handles.DrawDottedLine(br, bl, dotSpacing);
#endif
        }
    }
}
