using UnityEngine;
using System.Collections;



namespace TMPro
{

    public static class TMP_TextUtilities
    {
        // CHARACTER HANDLING

        /// <summary>
        /// Function returning the index of the character at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera"></param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        public static int FindIntersectingCharacter(TextMeshProUGUI text, Vector3 position, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible)
                    continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

            }
            return -1;
        }


        /// <summary>
        /// Function returning the index of the character at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera"></param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        public static int FindIntersectingCharacter(TextMeshPro text, Vector3 position, Camera camera, bool visibleOnly)
        {
            Transform textTransform = text.transform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible)
                    continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = textTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = textTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = textTransform.TransformPoint(cInfo.topRight);
                Vector3 br = textTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

            }

            return -1;
        }


        /// <summary>
        /// Function to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera"></param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        public static int FindNearestCharacter(TextMeshProUGUI text, Vector3 position, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible)
                    continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

                // Find the closest corner to position.
                float dbl = DistanceToLine(bl, tl, position);
                float dtl = DistanceToLine(tl, tr, position);
                float dtr = DistanceToLine(tr, br, position);
                float dbr = DistanceToLine(br, bl, position);

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
            }

            //Debug.Log("Returning nearest character at index: " + closest);

            return closest;
        }


        /// <summary>
        /// Function to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera"></param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        public static int FindNearestCharacter(TextMeshPro text, Vector3 position, Camera camera, bool visibleOnly)
        {
            Transform textTransform = text.transform;
            
            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible)
                    continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = textTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = textTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = textTransform.TransformPoint(cInfo.topRight);
                Vector3 br = textTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

                // Find the closest corner to position.
                float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
                float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
                float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
                float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
            }

            //Debug.Log("Returning nearest character at index: " + closest);

            return closest;
        }


        // WORD HANDLING

        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindIntersectingWord(TextMeshProUGUI text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    // Track maximum Ascender and minimum Descender for each word.
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.topLine);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.bottomLine);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0);
                        tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0);
                            tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0);

                            // Transform coordinates to be relative to transform and account min descender and max ascender.
                            bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                            tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                            br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindIntersectingWord(TextMeshPro text, Vector3 position, Camera camera)
        {
            Transform textTransform = text.transform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    // Track maximum Ascender and minimum Descender for each word.
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.topLine);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.bottomLine);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0);
                        tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0);
                            tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0);

                            // Transform coordinates to be relative to transform and account min descender and max ascender.
                            bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                            tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                            tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                            br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        // Reset maxAscender and minDescender for next word segment.
                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }
            }

            return -1;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindNearestWord(TextMeshProUGUI text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                // Find the closest line segment to position.
                float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
                float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
                float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
                float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return closest;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindNearestWord(TextMeshPro text, Vector3 position, Camera camera)
        {
            Transform textTransform = text.transform;

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
                                              currentCharInfo.lineNumber > text.maxVisibleLines ||
                                             (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0));
                        tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                            tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                 // Find the closest line segment to position.
                float dbl = DistanceToLine(bl, tl, position);
                float dtl = DistanceToLine(tl, tr, position);
                float dtr = DistanceToLine(tr, br, position);
                float dbr = DistanceToLine(br, bl, position);

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return closest;
           
        }


        /// <summary>
        /// Function returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindIntersectingLink(TextMeshProUGUI text, Vector3 position, Camera camera)
        {
            Transform rectTransform = text.transform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.characterCount; j++)
                {
                    int characterIndex = linkInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (linkInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }


        /// <summary>
        /// Function returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindIntersectingLink(TextMeshPro text, Vector3 position, Camera camera)
        {
            Transform textTransform = text.transform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.characterCount; j++)
                {
                    int characterIndex = linkInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0));
                        tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (linkInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                            tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindNearestLink(TextMeshProUGUI text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.characterCount; j++)
                {
                    int characterIndex = linkInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (linkInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                // Find the closest line segment to position.
                float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
                float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
                float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
                float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return closest;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static int FindNearestLink(TextMeshPro text, Vector3 position, Camera camera)
        {
            Transform textTransform = text.transform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.characterCount; j++)
                {
                    int characterIndex = linkInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.bottomLine, 0));
                        tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.topLine, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (linkInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                            tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.bottomLine, 0));
                        tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.topLine, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                // Find the closest line segment to position.
                float dbl = DistanceToLine(bl, tl, position);
                float dtl = DistanceToLine(tl, tr, position);
                float dtr = DistanceToLine(tr, br, position);
                float dbr = DistanceToLine(br, bl, position);

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }
            return closest;
        }


    
        /// <summary>
        /// Function to check if a Point is contained within a Rectangle.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool PointIntersectRectangle(Vector3 m, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Vector3 ab = b - a;
            Vector3 am = m - a;
            Vector3 bc = c - b;
            Vector3 bm = m - b;

            float abamDot = Vector3.Dot(ab, am);
            float bcbmDot = Vector3.Dot(bc, bm);

            return 0 <= abamDot && abamDot <= Vector3.Dot(ab, ab) && 0 <= bcbmDot && bcbmDot <= Vector3.Dot(bc, bc);
        }


        /// <summary>
        /// Method to convert ScreenPoint to WorldPoint aligned with Rectangle
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="screenPoint"></param>
        /// <param name="cam"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToWorldPointInRectangle(Transform transform, Vector2 screenPoint, Camera cam, out Vector3 worldPoint)
        {
            worldPoint = (Vector3)Vector2.zero;
            Ray ray = RectTransformUtility.ScreenPointToRay(cam, screenPoint);
            float enter;
            if (!new Plane(transform.rotation * Vector3.back, transform.position).Raycast(ray, out enter))
                return false;
            worldPoint = ray.GetPoint(enter);
            return true;
        }


        private struct LineSegment
        {
            public Vector3 Point1;
            public Vector3 Point2;

            public LineSegment(Vector3 p1, Vector3 p2)
            {
                Point1 = p1;
                Point2 = p2;
            }
        }


        /// <summary>
        /// Function returning the point of intersection between a line and a plane.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        /// <param name="intersectingPoint"></param>
        /// <returns></returns>
        private static bool IntersectLinePlane(LineSegment line, Vector3 point, Vector3 normal, out Vector3 intersectingPoint)
        {
            intersectingPoint = Vector3.zero;
            Vector3 u = line.Point2 - line.Point1;
            Vector3 w = line.Point1 - point;

            float D = Vector3.Dot(normal, u);
            float N = -Vector3.Dot(normal, w);

            if (Mathf.Abs(D) < Mathf.Epsilon)   // if line is parallel & co-planar to plane
            {
                if (N == 0)
                    return true;
                else
                    return false;
            }

            float sI = N / D;

            if (sI < 0 || sI > 1) // Line parallel to plane
                return false;

            intersectingPoint = line.Point1 + sI * u;

            return true;
        }


        /// <summary>
        /// Function returning the Square Distance from a Point to a Line.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float DistanceToLine(Vector3 a, Vector3 b, Vector3 point)
        {            
            Vector3 n = b - a;
            Vector3 pa = a - point;
 
            float c = Vector3.Dot( n, pa );
 
            // Closest point is a
            if ( c > 0.0f )
                return Vector3.Dot( pa, pa );
 
            Vector3 bp = point - b;
 
            // Closest point is b
            if (Vector3.Dot( n, bp ) > 0.0f )
                return Vector3.Dot( bp, bp );
 
            // Closest point is between a and b
            Vector3 e = pa - n * (c / Vector3.Dot( n, n ));
 
            return Vector3.Dot( e, e );
        }


        /// <summary>
        /// Function which returns a simple hashcode from a string.
        /// </summary>
        /// <returns></returns>
        public static int GetSimpleHashCode(string s)
        {
            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = (hashCode << 5) - hashCode + s[i];

            return hashCode;
        }

    }
}
