using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using System;

[System.Serializable]
public class ScrollTextView : MonoBehaviour
{
    private static int TEXT_BUFFER_SIZE = 8192;

    public GameObject OutputText;
    public GameObject OutputScrollView;
    public GameObject OutputScrollBar;

    private LinkedList<string> m_outputLines;
    private StringBuilder m_outputScratchBuffer;

    private Text m_outputTextComponent;
    private TextGenerationSettings m_outputTextSettings;
    private Vector2 m_outputViewExtents;
    private Vector2 m_outputTextExtents;
    private Scrollbar m_outputScrollBar;

    public void Start()
    {
        m_outputTextComponent = OutputText.GetComponent<Text>();
        m_outputScrollBar = OutputScrollBar.GetComponent<Scrollbar>();
        m_outputViewExtents = OutputScrollView.GetComponent<RectTransform>().rect.size;
        m_outputTextExtents = OutputText.GetComponent<RectTransform>().rect.size;
        m_outputTextSettings =
            m_outputTextComponent.GetGenerationSettings(m_outputTextExtents);

        m_outputScratchBuffer = new StringBuilder(TEXT_BUFFER_SIZE);
        m_outputLines = new LinkedList<string>();

        m_outputTextComponent.text = "";
    }

    public void Clear()
    {
        m_outputLines.Clear();
        m_outputScratchBuffer.Length = 0;
        m_outputTextComponent.text = "";
        m_outputScrollBar.value = 1.0f;
    }

    public void AppendLine(
        string text)
    {
        bool fitsInExtents = false;
        float chatOutputHeight = 0.0f;

        // Append the new line to the end of the linked list
        m_outputLines.AddLast(text);

        // Delete old chat output until it fits
        while (!fitsInExtents)
        {
            // Bake the linked list of lines into a single string
            string chatOutputText = BakeOutputString();

            // Compute the height of the chat text
            chatOutputHeight =
                m_outputTextComponent.cachedTextGenerator.GetPreferredHeight(
                    chatOutputText, m_outputTextSettings);

            // Strip off the oldest line if the resulting text doesn't fit in  the text extents.
            // But if there is only one line left than just accept the overflow.
            if (chatOutputHeight > m_outputTextExtents.y && m_outputLines.Count > 1)
            {
                m_outputLines.RemoveFirst();
            }
            else
            {
                m_outputTextComponent.text = chatOutputText;
                fitsInExtents = true;
            }
        }

        // Auto scroll the scroll bar as the text fills up
        // 1.0 = scroll bar at the top
        // 0.0 = scroll bar at the bottom
        if (chatOutputHeight > m_outputViewExtents.y)
        {
            m_outputScrollBar.value =
                Math.Min(
                    Math.Max(
                        1.0f - ((chatOutputHeight - m_outputViewExtents.y) / (m_outputTextExtents.y - m_outputViewExtents.y)),
                        0.0f),
                    1.0f);
        }
    }

    private string BakeOutputString()
    {
        // Clear out the scratch buffer
        m_outputScratchBuffer.Length = 0;

        foreach (string line in m_outputLines)
        {
            m_outputScratchBuffer.AppendLine(line);
        }

        return m_outputScratchBuffer.ToString();
    }
}
