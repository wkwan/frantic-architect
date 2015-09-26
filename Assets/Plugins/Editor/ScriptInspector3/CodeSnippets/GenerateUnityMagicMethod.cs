/* SCRIPT INSPECTOR 3
 * version 3.0.6, September 2015
 * Copyright © 2012-2015, Flipbook Games
 * 
 * Unity's legendary editor for C#, UnityScript, Boo, Shaders, and text,
 * now transformed into an advanced C# IDE!!!
 * 
 * Follow me on http://twitter.com/FlipbookGames
 * Like Flipbook Games on Facebook http://facebook.com/FlipbookGames
 * Join discussion in Unity forums http://forum.unity3d.com/threads/138329
 * Contact info@flipbookgames.com for feedback, bug reports, or suggestions.
 * Visit http://flipbookgames.com/ for more info.
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ScriptInspector
{

class GenerateUnityMagicMethod : ISnippetProvider
{
	private static TypeDefinitionBase contextType;
	
	interface IMagicMethod
	{
		string GetParametersString();
		bool IsCoroutine { get; }
		SymbolDefinition BaseSymbol { get; set; }
	}
	
	class MagicMethod<T> : SnippetCompletion, IMagicMethod
	{
		private static Texture2D icon = UnityEditorInternal.InternalEditorUtility.GetIconForFile("fakeScene.unity");
		
		public SymbolDefinition baseSymbol;
		public SymbolDefinition BaseSymbol {
			get { return baseSymbol; }
			set { baseSymbol = value; }
		}
		
		private readonly string signature;
		
		public MagicMethod(string methodSignature)
			: base(GetMethodName(methodSignature))
		{
			displayFormat = GetDisplayName(methodSignature);
			signature = methodSignature;
			cachedIcon = icon;
		}
		
		private static string GetMethodName(string signature)
		{
			var i = signature.IndexOf(' ');
			var j = signature.IndexOf('(', i + 1);
			i = signature.LastIndexOf(' ', j);
			var methodName = signature.Substring(i + 1, j - i - 1);
			return methodName;
		}
		
		private static string GetDisplayName(string signature)
		{
			if (signature.StartsWith("IEnumerator", System.StringComparison.Ordinal))
				return "IEnumerator {0}(...)";
			else
				return "{0}(...)";
		}
		
		public override string Expand()
		{
			string comment = "";
			if (SISettings.magicMethods_insertWithComments && UnitySymbols.summaries.Count > 0)
			{
				var commentKey = typeof(T).Name + '.' + name;
				if (UnitySymbols.summaries.TryGetValue(commentKey, out comment))
					comment = "// " + comment + "\n";
				else
					comment = "";
			}
			
			string modifiers = contextType.IsSealed ? "" : "protected ";
			
			string baseCall = "";
			if (baseSymbol != null)
			{
				modifiers += "new ";
				if (baseSymbol.kind == SymbolKind.Method)
				{
					baseCall = "base." + name + "(";
					var parameters = baseSymbol.GetParameters();
					var separator = "";
					foreach (var p in parameters)
					{
						baseCall += separator;
						baseCall += p.name;
						separator = ", ";
					}
					baseCall += ")";
					
					var returnType = baseSymbol.TypeOf();
					var baseIsCoroutine = returnType != null && returnType.name == "IEnumerator";
					if (baseIsCoroutine)
					{
						baseCall = "StartCoroutine(" + baseCall + ")";
						if (IsCoroutine)
							baseCall = "yield return " + baseCall;
					}
					
					baseCall += ";";
				}
			}
			
			string expandedCode;
			expandedCode = string.Format("{0}{1}{2}{3}{{\n\t{4}$end$\n}}",
				comment,
				modifiers,
				signature,
				SISettings.magicMethods_openingBraceOnSameLine ? " " : "\n",
				baseCall);
			return expandedCode;
		}
		
		public string GetParametersString()
		{
			var openIndex = signature.IndexOf('(');
			return signature.Substring(openIndex + 1, signature.Length - openIndex - 2);
		}
		
		public bool IsCoroutine { get { return signature.StartsWith("IEnumerator", System.StringComparison.Ordinal); } }
	}
	
	private static List<SnippetCompletion> monoBehaviourMagicMethods = new List<SnippetCompletion> {
		new MagicMethod<MonoBehaviour>("void Awake()"),
		new MagicMethod<MonoBehaviour>("void Start()"),
		new MagicMethod<MonoBehaviour>("IEnumerator Start()"),
		new MagicMethod<MonoBehaviour>("void Update()"),
		new MagicMethod<MonoBehaviour>("void LateUpdate()"),
		new MagicMethod<MonoBehaviour>("void FixedUpdate()"),
		new MagicMethod<MonoBehaviour>("void OnGUI()"),
		new MagicMethod<MonoBehaviour>("void OnEnable()"),
		new MagicMethod<MonoBehaviour>("void OnDisable()"),
		new MagicMethod<MonoBehaviour>("void OnDestroy()"),
		new MagicMethod<MonoBehaviour>("void Reset()"),
		new MagicMethod<MonoBehaviour>("void OnValidate()"),
		// Physics
		new MagicMethod<MonoBehaviour>("void OnTriggerEnter(Collider other)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnTriggerEnter(Collider other)"),
		new MagicMethod<MonoBehaviour>("void OnTriggerEnter2D(Collider2D other)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnTriggerEnter2D(Collider2D other)"),
		new MagicMethod<MonoBehaviour>("void OnTriggerExit(Collider other)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnTriggerExit(Collider other)"),
		new MagicMethod<MonoBehaviour>("void OnTriggerExit2D(Collider2D other)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnTriggerExit2D(Collider2D other)"),
		new MagicMethod<MonoBehaviour>("void OnTriggerStay(Collider other)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnTriggerStay(Collider other)"),
		new MagicMethod<MonoBehaviour>("void OnTriggerStay2D(Collider2D other)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnTriggerStay2D(Collider2D other)"),
		new MagicMethod<MonoBehaviour>("void OnCollisionEnter(Collision collisionInfo)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnCollisionEnter(Collision collisionInfo)"),
		new MagicMethod<MonoBehaviour>("void OnCollisionEnter2D(Collision2D collisionInfo)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnCollisionEnter2D(Collision2D collisionInfo)"),
		new MagicMethod<MonoBehaviour>("void OnCollisionExit(Collision collisionInfo)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnCollisionExit(Collision collisionInfo)"),
		new MagicMethod<MonoBehaviour>("void OnCollisionExit2D(Collision2D collisionInfo)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnCollisionExit2D(Collision2D collisionInfo)"),
		new MagicMethod<MonoBehaviour>("void OnCollisionStay(Collision collisionInfo)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnCollisionStay(Collision collisionInfo)"),
		new MagicMethod<MonoBehaviour>("void OnCollisionStay2D(Collision2D collisionInfo)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnCollisionStay2D(Collision2D collisionInfo)"),
		new MagicMethod<MonoBehaviour>("void OnControllerColliderHit(ControllerColliderHit hit)"),
		new MagicMethod<MonoBehaviour>("void OnJointBreak(float breakForce)"),
		new MagicMethod<MonoBehaviour>("void OnParticleCollision(GameObject other)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnParticleCollision(GameObject other)"),
		// Mouse
		new MagicMethod<MonoBehaviour>("void OnMouseEnter()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnMouseEnter()"),
		new MagicMethod<MonoBehaviour>("void OnMouseOver()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnMouseOver()"),
		new MagicMethod<MonoBehaviour>("void OnMouseExit()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnMouseExit()"),
		new MagicMethod<MonoBehaviour>("void OnMouseDown()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnMouseDown()"),
		new MagicMethod<MonoBehaviour>("void OnMouseUp()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnMouseUp()"),
		new MagicMethod<MonoBehaviour>("void OnMouseUpAsButton()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnMouseUpAsButton()"),
		new MagicMethod<MonoBehaviour>("void OnMouseDrag()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnMouseDrag()"),
		// Playback
		new MagicMethod<MonoBehaviour>("void OnLevelWasLoaded(int level)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnLevelWasLoaded(int level)"),
		new MagicMethod<MonoBehaviour>("void OnApplicationFocus(bool focus)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnApplicationFocus(bool focus)"),
		new MagicMethod<MonoBehaviour>("void OnApplicationPause(bool pause)"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnApplicationPause(bool pause)"),
		new MagicMethod<MonoBehaviour>("void OnApplicationQuit()"),
		// Rendering
		new MagicMethod<MonoBehaviour>("void OnBecameVisible()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnBecameVisible()"),
		new MagicMethod<MonoBehaviour>("void OnBecameInvisible()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnBecameInvisible()"),
		new MagicMethod<MonoBehaviour>("void OnPreCull()"),
		new MagicMethod<MonoBehaviour>("void OnPreRender()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnPreRender()"),
		new MagicMethod<MonoBehaviour>("void OnPostRender()"),
		new MagicMethod<MonoBehaviour>("IEnumerator OnPostRender()"),
		new MagicMethod<MonoBehaviour>("void OnRenderObject()"),
		new MagicMethod<MonoBehaviour>("void OnWillRenderObject()"),
		new MagicMethod<MonoBehaviour>("void OnRenderImage(RenderTexture source, RenderTexture destination)"),
		// Gizmos
		new MagicMethod<MonoBehaviour>("void OnDrawGizmosSelected()"),
		new MagicMethod<MonoBehaviour>("void OnDrawGizmos()"),
		// Network
		new MagicMethod<MonoBehaviour>("void OnPlayerConnected(NetworkPlayer player)"),
		new MagicMethod<MonoBehaviour>("void OnServerInitialized()"),
		new MagicMethod<MonoBehaviour>("void OnConnectedToServer()"),
		new MagicMethod<MonoBehaviour>("void OnPlayerDisconnected(NetworkPlayer player)"),
		new MagicMethod<MonoBehaviour>("void OnDisconnectedFromServer(NetworkDisconnection info)"),
		new MagicMethod<MonoBehaviour>("void OnFailedToConnect(NetworkConnectionError error)"),
		new MagicMethod<MonoBehaviour>("void OnFailedToConnectToMasterServer(NetworkConnectionError info)"),
		new MagicMethod<MonoBehaviour>("void OnMasterServerEvent(MasterServerEvent msEvent)"),
		new MagicMethod<MonoBehaviour>("void OnNetworkInstantiate(NetworkMessageInfo info)"),
		new MagicMethod<MonoBehaviour>("void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)"),
		// Animation
		new MagicMethod<MonoBehaviour>("void OnAnimatorIK(int layerIndex)"),
		new MagicMethod<MonoBehaviour>("void OnAnimatorMove()"),
		new MagicMethod<MonoBehaviour>("void ()"),
		new MagicMethod<MonoBehaviour>("void ()"),
		new MagicMethod<MonoBehaviour>("void ()"),
		// Audio
		new MagicMethod<MonoBehaviour>("void OnAudioFilterRead(float[] data, int channels)"),
		// Scene Hierarchy
		new MagicMethod<MonoBehaviour>("void OnTransformChildrenChanged()"),
		new MagicMethod<MonoBehaviour>("void OnTransformParentChanged()"),
	};
	
	private static List<SnippetCompletion> editorWindowMagicMethods = new List<SnippetCompletion> {
		new MagicMethod<EditorWindow>("void OnDestroy()"),
		new MagicMethod<EditorWindow>("void OnFocus()"),
		new MagicMethod<EditorWindow>("void OnGUI()"),
		new MagicMethod<EditorWindow>("void OnHierarchyChange()"),
		new MagicMethod<EditorWindow>("void OnInspectorUpdate()"),
		new MagicMethod<EditorWindow>("void OnLostFocus()"),
		new MagicMethod<EditorWindow>("void OnProjectChange()"),
		new MagicMethod<EditorWindow>("void OnSelectionChnage()"),
		new MagicMethod<EditorWindow>("void Update()"),
		// Undocumented
		new MagicMethod<EditorWindow>("void ShowButton(Rect buttonRect)"),
		// Inherited
		new MagicMethod<ScriptableObject>("void OnDisable()"),
		new MagicMethod<ScriptableObject>("void OnEnable()"),
	};
	
	private static List<SnippetCompletion> scriptableObjectMagicMethods = new List<SnippetCompletion> {
		new MagicMethod<ScriptableObject>("void OnDestroy()"),
		new MagicMethod<ScriptableObject>("void OnDisable()"),
		new MagicMethod<ScriptableObject>("void OnEnable()"),
	};
	
	private static List<SnippetCompletion> scriptableWizardMagicMethods = new List<SnippetCompletion> {
		new MagicMethod<ScriptableWizard>("void OnWizardCreate()"),
		new MagicMethod<ScriptableWizard>("void OnWizardOtherButton()"),
		new MagicMethod<ScriptableWizard>("void OnWizardUpdate()"),
		// Inherited
		new MagicMethod<EditorWindow>("void OnDestroy()"),
		new MagicMethod<EditorWindow>("void OnFocus()"),
		new MagicMethod<EditorWindow>("void OnGUI()"),
		new MagicMethod<EditorWindow>("void OnHierarchyChange()"),
		new MagicMethod<EditorWindow>("void OnInspectorUpdate()"),
		new MagicMethod<EditorWindow>("void OnLostFocus()"),
		new MagicMethod<EditorWindow>("void OnProjectChange()"),
		new MagicMethod<EditorWindow>("void OnSelectionChnage()"),
		new MagicMethod<EditorWindow>("void Update()"),
		new MagicMethod<ScriptableObject>("void OnDisable()"),
		new MagicMethod<ScriptableObject>("void OnEnable()"),
	};
		
	private static List<SnippetCompletion> editorMagicMethods = new List<SnippetCompletion> {
		new MagicMethod<Editor>("void OnSceneGUI()"),
		new MagicMethod<Editor>("bool RequiresConstantRepaint()"),
		new MagicMethod<Editor>("bool UseDefaultMargins()"),
		// Inherited
		new MagicMethod<ScriptableObject>("void OnDestroy()"),
		new MagicMethod<ScriptableObject>("void OnDisable()"),
		new MagicMethod<ScriptableObject>("void OnEnable()"),
	};
	
	private static List<SnippetCompletion> assetPostprocessorMagicMethods = new List<SnippetCompletion> {
		new MagicMethod<AssetPostprocessor>("Material OnAssignMaterialModel(Material material, Renderer renderer)"),
		new MagicMethod<AssetPostprocessor>("static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)"),
		new MagicMethod<AssetPostprocessor>("void OnPostprocessAssetbundleNameChanged(string assetPath, string previousAssetBundleName, string newAssetBundleName)"),
		new MagicMethod<AssetPostprocessor>("void OnPostprocessAudio(AudioClip clip)"),
		new MagicMethod<AssetPostprocessor>("void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propNames, object[] values)"),
		new MagicMethod<AssetPostprocessor>("void OnPostprocessModel(GameObject root)"),
		new MagicMethod<AssetPostprocessor>("void OnPostprocessSpeedTree(GameObject root)"),
		new MagicMethod<AssetPostprocessor>("void OnPostprocessTexture(Texture2D texture)"),
		new MagicMethod<AssetPostprocessor>("void OnPreprocessAnimation()"),
		new MagicMethod<AssetPostprocessor>("void OnPreprocessAudio()"),
		new MagicMethod<AssetPostprocessor>("void OnPreprocessModel()"),
		new MagicMethod<AssetPostprocessor>("void OnPreprocessSpeedTree()"),
		new MagicMethod<AssetPostprocessor>("void OnPreprocessTexture()"),
	};
	
	private static TypeDefinitionBase monoBehaviourType =
		ReflectedTypeReference.ForType(typeof(MonoBehaviour)).definition as TypeDefinitionBase;
	private static TypeDefinitionBase editorWindowType =
		ReflectedTypeReference.ForType(typeof(EditorWindow)).definition as TypeDefinitionBase;
	private static TypeDefinitionBase scriptableWizardType =
		ReflectedTypeReference.ForType(typeof(ScriptableWizard)).definition as TypeDefinitionBase;
	private static TypeDefinitionBase scriptableObjectType =
		ReflectedTypeReference.ForType(typeof(ScriptableObject)).definition as TypeDefinitionBase;
	private static TypeDefinitionBase editorType =
		ReflectedTypeReference.ForType(typeof(Editor)).definition as TypeDefinitionBase;
	private static TypeDefinitionBase assetPostprocessorType =
		ReflectedTypeReference.ForType(typeof(AssetPostprocessor)).definition as TypeDefinitionBase;
	
	public IEnumerable<SnippetCompletion> EnumSnippets(
		SymbolDefinition context,
		FGGrammar.TokenSet expectedTokens,
		SyntaxToken tokenLeft,
		Scope scope)
	{
		if (tokenLeft == null || tokenLeft.parent == null || tokenLeft.parent.parent == null)
			yield break;
		
		if (tokenLeft.tokenKind != SyntaxToken.Kind.Punctuator)
			yield break;
		
		if (tokenLeft.text != "{" && tokenLeft.text != "}" && tokenLeft.text != ";")
			yield break;
		
		var bodyScope = scope as BodyScope;
		if (bodyScope == null)
			yield break;
		
		contextType = bodyScope.definition as TypeDefinitionBase;
		if (contextType == null || contextType.kind != SymbolKind.Class)
			yield break;
		
		List<SnippetCompletion> magicMethods;
		
		if (contextType.DerivesFrom(monoBehaviourType))
			magicMethods = monoBehaviourMagicMethods;
		else if (contextType.DerivesFrom(scriptableWizardType))
			magicMethods = scriptableWizardMagicMethods;
		else if (contextType.DerivesFrom(editorWindowType))
			magicMethods = editorWindowMagicMethods;
		else if (contextType.DerivesFrom(editorType))
			magicMethods = editorMagicMethods;
		else if (contextType.DerivesFrom(assetPostprocessorType))
			magicMethods = assetPostprocessorMagicMethods;
		else if (contextType.DerivesFrom(scriptableObjectType))
			magicMethods = scriptableObjectMagicMethods;
		else
			yield break;
		
		var baseType = contextType.BaseType();
		
		var tempLeaf = new ParseTree.Leaf() { token = new SyntaxToken(SyntaxToken.Kind.Identifier, "") };
		foreach (var magic in magicMethods)
		{
			((IMagicMethod) magic).BaseSymbol = null;
			
			if (contextType.FindName(magic.name, -1, false) != null)
				continue;
			
			tempLeaf.token.text = magic.name;
			baseType.ResolveMember(tempLeaf, scope, -1, false);
			var baseSymbol = tempLeaf.resolvedSymbol;
			if (baseSymbol == null || baseSymbol.kind == SymbolKind.Error)
			{
				yield return magic;
				continue;
			}
			
			var asMethodGroup = baseSymbol as MethodGroupDefinition;
			if (baseSymbol.kind != SymbolKind.MethodGroup || asMethodGroup == null)
			{
				if (!baseSymbol.IsPrivate)
					((IMagicMethod) magic).BaseSymbol = baseSymbol;
				yield return magic;
				continue;
			}
			
			bool yield = true;
			var magicSignature = ((IMagicMethod) magic).GetParametersString();
			foreach (var baseMethod in asMethodGroup.methods)
			{
				if (baseMethod.PrintParameters(baseMethod.GetParameters(), true) == magicSignature)
				{
					if (baseMethod.IsOverride || baseMethod.IsVirtual || baseMethod.IsAbstract)
					{
						yield = false;
						break;
					}
					
					if (!baseMethod.IsPrivate)
					{
						var returnType = baseMethod.ReturnType();
						if (returnType == null || returnType.kind == SymbolKind.Error)
						{
							((IMagicMethod) magic).BaseSymbol = asMethodGroup;
						}
						else
						{
							var baseIsCoroutine = returnType.name == "IEnumerator";
							var returnsVoid = baseMethod.ReturnType() == SymbolDefinition.builtInTypes_void;
							if (!baseIsCoroutine && !returnsVoid)
								((IMagicMethod) magic).BaseSymbol = asMethodGroup;
							else
								((IMagicMethod) magic).BaseSymbol = baseMethod;
						}
					}
					break;
				}
			}
			
			if (yield)
				yield return magic;
		}
	}
	
	public string Get(
		string shortcut,
		SymbolDefinition context,
		FGGrammar.TokenSet expectedTokens,
		Scope scope)
	{
		return null;
	}
}
	
}
