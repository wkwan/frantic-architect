using UnityEngine;
using System.Collections;

namespace Unibill.Demo {
	public class ComboBox
	{
	    private static bool forceToUnShow = false; 
	    private static int useControlID = -1;
	    private bool isClickedComboButton = false;  
	    
	    private int selectedItemIndex = 0;  
	    
	    public int List( Rect rect, string buttonText, GUIContent[] listContent, GUIStyle listStyle )
	    {
	        return List( rect, new GUIContent( buttonText ), listContent, "button", "box", listStyle );
	    }
	    
	    public int List( Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle )
	    {
	        return List( rect, buttonContent, listContent, "button", "box", listStyle);
	    }
	    
	    public int List( Rect rect, string buttonText, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle )
	    {
	        return List( rect, new GUIContent( buttonText ), listContent, buttonStyle, boxStyle, listStyle );
	    }
	    
	    public int List( Rect rect, GUIContent buttonContent, GUIContent[] listContent,
	                    GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle )
	    {
	        if( forceToUnShow )
	        {
	            forceToUnShow = false;
	            isClickedComboButton = false;           
	        }
	        
	        bool done = false;
	        int controlID = GUIUtility.GetControlID( FocusType.Passive );       
	        
	        switch( Event.current.GetTypeForControl(controlID) )
	        {
	        case EventType.mouseUp:
	        {
	            if( isClickedComboButton )
	            {
	                done = true;
	            }
	        }
	            break;
	        }       
	        
	        if( GUI.Button( rect, buttonContent, buttonStyle ) )
	        {
	            if( useControlID == -1 )
	            {
	                useControlID = controlID;
	                isClickedComboButton = false;
	            }
	            
	            if( useControlID != controlID )
	            {
	                forceToUnShow = true;
	                useControlID = controlID;
	            }
	            isClickedComboButton = true;
	        }
	        
	        if( isClickedComboButton )
	        {
	            Rect listRect = new Rect( rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1.0f),
	                                     rect.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length );
	            
	            GUI.Box( listRect, "", boxStyle );
	            int newSelectedItemIndex = GUI.SelectionGrid( listRect, selectedItemIndex, listContent, 1, listStyle );
	            if( newSelectedItemIndex != selectedItemIndex )
	                selectedItemIndex = newSelectedItemIndex;
	        }
	        
	        if( done )
	            isClickedComboButton = false;
	        
	        return GetSelectedItemIndex();
	    }
	    
	    public int GetSelectedItemIndex()
	    {
	        return selectedItemIndex;
	    }
	}
}