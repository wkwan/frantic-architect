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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Debug = UnityEngine.Debug;


namespace ScriptInspector
{

public enum SymbolKind : byte
{
	None,
	Error,
	_Keyword,
	_Snippet,
	Namespace,
	Interface,
	Enum,
	Struct,
	Class,
	Delegate,
	Field,
	ConstantField,
	LocalConstant,
	EnumMember,
	Property,
	Event,
	Indexer,
	Method,
	ExtensionMethod,
	MethodGroup,
	Constructor,
	Destructor,
	Operator,
	Accessor,
	LambdaExpression,
	Parameter,
	CatchParameter,
	Variable,
	ForEachVariable,
	FromClauseVariable,
	TypeParameter,
	BaseTypesList,
	Instance,
	Null,
	Label,
	ImportedNamespace,
	TypeAlias,
}

[Flags]
public enum Modifiers
{
	None = 0,
	Public = 1 << 0,
	Internal = 1 << 1,
	Protected = 1 << 2,
	Private = 1 << 3,
	Static = 1 << 4,
	New = 1 << 5,
	Sealed = 1 << 6,
	Abstract = 1 << 7,
	ReadOnly = 1 << 8,
	Volatile = 1 << 9,
	Virtual = 1 << 10,
	Override = 1 << 11,
	Extern = 1 << 12,
	Ref = 1 << 13,
	Out = 1 << 14,
	Params = 1 << 15,
	This = 1 << 16,
	Partial = 1 << 17,
}

public enum AccessLevel : byte
{
	None = 0,
	Private = 1, // private
	ProtectedAndInternal = 2, // n/a
	ProtectedOrInternal = 4, // protected internal
	Protected, // protected
	Internal, // internal
	Public, // public
}

static class ToCSharpStringExtensions
{
	public static string ToCSharpString(this AccessLevel self)
	{
		switch (self)
		{
		case AccessLevel.Public:
			return "public";
		case AccessLevel.Internal:
			return "internal";
		case AccessLevel.Protected:
			return "protected";
		case AccessLevel.ProtectedOrInternal:
			return "protected internal";
		default:
			return "private";
		}
	}
}

[Flags]
public enum AccessLevelMask : byte
{
	None = 0,
	Private = 1, // private
	Protected = 2, // protected
	Internal = 4, // internal
	Public = 8, // public

	Any = Private | Protected | Internal | Public,
	NonPublic = Private | Protected | Internal,
}


public class SymbolReference
{
	protected SymbolReference() {}

	public SymbolReference(ParseTree.BaseNode node)
	{
		parseTreeNode = node;
	}

	public SymbolReference(SymbolDefinition definedSymbol)
	{
		_definition = definedSymbol;
	}

	protected ParseTree.BaseNode parseTreeNode;
	public ParseTree.BaseNode Node { get { return parseTreeNode; } }

	protected uint _resolvedVersion;
	protected SymbolDefinition _definition;
	protected bool resolving = false;
	public static bool dontResolveNow = false;
	public virtual SymbolDefinition definition
	{
		get
		{
			if (_definition != null &&
				(parseTreeNode != null && _resolvedVersion != ParseTree.resolverVersion || !_definition.IsValid()))
				_definition = null;
			
			if (_definition == null)
			{
			//	Debug.Log("Dereferencing " + parseTreeNode.Print());
				if (!resolving)
				{
					if (dontResolveNow)
						return SymbolDefinition.unknownSymbol;
					resolving = true;
					_definition = SymbolDefinition.ResolveNode(parseTreeNode);
					_resolvedVersion = ParseTree.resolverVersion;
					resolving = false;
				}
				else
				{
					return SymbolDefinition.unknownSymbol;
				}
				//var leaf = parseTreeNode as ParseTree.Leaf;
				//if (leaf != null && leaf.resolvedSymbol != null)
				//{
				//    _definition = leaf.resolvedSymbol;
				//}
				//else
				//{
				//    var node = parseTreeNode as ParseTree.Node;
				//    var scopeNode = node;
				//    while (scopeNode != null && scopeNode.scope == null)
				//        scopeNode = scopeNode.parent;
				//    if (scopeNode != null)
				//    {
				//        _definition = scopeNode.scope.ResolveNode(node);
				//    }
				//}
				if (_definition == null)
				{
				//	Debug.Log("Failed to resolve SymbolReference: " + parseTreeNode);
					_definition = SymbolDefinition.unknownType;
					_resolvedVersion = ParseTree.resolverVersion;
				}
			}
			return _definition;
		}
	}

	public bool IsBefore(ParseTree.Leaf leaf)
	{
		if (parseTreeNode == null)
			return true;
		var lastLeaf = parseTreeNode as ParseTree.Leaf;
		if (lastLeaf == null)
			lastLeaf = ((ParseTree.Node) parseTreeNode).GetLastLeaf();
		return lastLeaf != null && (lastLeaf.line < leaf.line || lastLeaf.line == leaf.line && lastLeaf.tokenIndex < leaf.tokenIndex);
	}

	public override string ToString()
	{
		return parseTreeNode != null ? parseTreeNode.Print() : _definition.GetName();
	}
}


public abstract class Scope
{
	public static ParseTree.BaseNode completionNode;
	public static string completionAssetPath;
	public static int completionAtLine;
	public static int completionAtTokenIndex;
	
	protected ParseTree.Node parseTreeNode;

	public Scope(ParseTree.Node node)
	{
		parseTreeNode = node;
	}

	public Scope _parentScope;
	public Scope parentScope {
		get {
			if (_parentScope != null || parseTreeNode == null)
				return _parentScope;
			for (var node = parseTreeNode.parent; node != null; node = node.parent)
				if (node.scope != null)
					return node.scope;
			return null;
		}
		set { _parentScope = value; }
	}
	
	public AssemblyDefinition GetAssembly()
	{
		for (Scope scope = this; scope != null; scope = scope.parentScope)
		{
			var cuScope = scope as CompilationUnitScope;
			if (cuScope != null)
				return cuScope.assembly;
		}
		throw new Exception("No Assembly for scope???");
	}

	public abstract SymbolDefinition AddDeclaration(SymbolDeclaration symbol);

	public abstract void RemoveDeclaration(SymbolDeclaration symbol);

	//public virtual SymbolDefinition AddDeclaration(SymbolKind symbolKind, ParseTree.Node definitionNode)
	//{
	//    var symbol = new SymbolDeclaration { scope = this, kind = symbolKind, parseTreeNode = definitionNode };
	//    var definition = AddDeclaration(symbol);
	//    return definition;
	//

	public virtual string CreateAnonymousName()
	{
		return parentScope != null ? parentScope.CreateAnonymousName() : null;
	}

	public virtual void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		leaf.resolvedSymbol = null;
		if (parentScope != null)
			parentScope.Resolve(leaf, numTypeArgs, asTypeOnly);
	}

	public virtual void ResolveAttribute(ParseTree.Leaf leaf)
	{
		leaf.resolvedSymbol = null;
		if (parentScope != null)
			parentScope.ResolveAttribute(leaf);
	}

	public virtual SymbolDefinition ResolveAsExtensionMethod(ParseTree.Leaf invokedLeaf, SymbolDefinition invokedSymbol, TypeDefinitionBase memberOf, ParseTree.Node argumentListNode, SymbolReference[] typeArgs, Scope context)
	{
		return parentScope != null ? parentScope.ResolveAsExtensionMethod(invokedLeaf, invokedSymbol, memberOf, argumentListNode, typeArgs, context) : null;
	}
	
	public abstract SymbolDefinition FindName(string symbolName, int numTypeParameters);
	//{
	//    return null;
	//}

	public virtual void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		if (parentScope != null)
			parentScope.GetCompletionData(data, fromInstance, assembly);
	}

	//public abstract void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, bool includePrivate, AssemblyDefinition assembly);

	public virtual TypeDefinition EnclosingType()
	{
		return parentScope != null ? parentScope.EnclosingType() : null;
	}

	public NamespaceScope EnclosingNamespaceScope()
	{
		for (var parent = parentScope; parent != null; parent = parent.parentScope)
		{
			var parentNamespaceScope = parent as NamespaceScope;
			if (parentNamespaceScope != null)
				return parentNamespaceScope;
		}
		return null;
	}
	
	public virtual void GetExtensionMethodsCompletionData(TypeDefinitionBase forType, Dictionary<string, SymbolDefinition> data)
	{
 		if (parentScope != null)
	 		parentScope.GetExtensionMethodsCompletionData(forType, data);
	}

	public virtual IEnumerable<NamespaceDefinition> VisibleNamespacesInScope()
	{
		if (parentScope != null)
			foreach (var ns in parentScope.VisibleNamespacesInScope())
				yield return ns;
	}
}


public class ReflectedMember : InstanceDefinition
{
	private readonly MemberInfo memberInfo;

	public ReflectedMember(MemberInfo info, SymbolDefinition memberOf)
	{
		MethodInfo getMethodInfo = null;
		MethodInfo setMethodInfo = null;
		MethodInfo addMethodInfo = null;
		MethodInfo removeMethodInfo = null;
		
		switch (info.MemberType)
		{
			case MemberTypes.Constructor:
			case MemberTypes.Method:
				throw new InvalidOperationException();

			case MemberTypes.Field:
				var fieldInfo = (FieldInfo) info;
				modifiers =
					fieldInfo.IsPublic ? Modifiers.Public :
					fieldInfo.IsFamilyOrAssembly ? Modifiers.Internal | Modifiers.Protected :
					fieldInfo.IsAssembly ? Modifiers.Internal :
					fieldInfo.IsFamily ? Modifiers.Protected :
					Modifiers.Private;
				if (fieldInfo.IsStatic)// && !fieldInfo.IsLiteral)
					modifiers |= Modifiers.Static;
				break;

			case MemberTypes.Property:
				var propertyInfo = (PropertyInfo) info;
				getMethodInfo = propertyInfo.GetGetMethod(true);
				setMethodInfo = propertyInfo.GetSetMethod(true);
				modifiers = GetAccessorModifiers(getMethodInfo, setMethodInfo);
				break;

			case MemberTypes.Event:
				var eventInfo = (EventInfo) info;
				addMethodInfo = eventInfo.GetAddMethod(true);
				removeMethodInfo = eventInfo.GetRemoveMethod(true);
				modifiers = GetAccessorModifiers(addMethodInfo, removeMethodInfo);
				break;

			default:
				break;
		}
		accessLevel = AccessLevelFromModifiers(modifiers);

		memberInfo = info;
		var generic = info.Name.IndexOf('`');
		name = generic < 0 ? info.Name : info.Name.Substring(0, generic);
		parentSymbol = memberOf;
		switch (info.MemberType)
		{
			case MemberTypes.Field:
				kind = ((FieldInfo) info).IsLiteral ?
					(memberOf.kind == SymbolKind.Enum ? SymbolKind.EnumMember : SymbolKind.ConstantField) :
					SymbolKind.Field;
				break;
			case MemberTypes.Property:
				var indexParams = ((PropertyInfo) info).GetIndexParameters();
				kind = indexParams.Length > 0 ? SymbolKind.Indexer : SymbolKind.Property;
				if (getMethodInfo != null)
				{
					var accessor = Create(SymbolKind.Accessor, "get");
					accessor.modifiers = setMethodInfo != null ? GetAccessorModifiers(getMethodInfo) : modifiers;
					AddMember(accessor);
				}
				if (setMethodInfo != null)
				{
					var accessor = Create(SymbolKind.Accessor, "set");
					accessor.modifiers = getMethodInfo != null ? GetAccessorModifiers(setMethodInfo) : modifiers;
					AddMember(accessor);
				}
				break;
			case MemberTypes.Event:
				kind = SymbolKind.Event;
				if (addMethodInfo != null)
				{
					var accessor = Create(SymbolKind.Accessor, "add");
					accessor.modifiers = removeMethodInfo != null ? GetAccessorModifiers(addMethodInfo) : modifiers;
					AddMember(accessor);
				}
				if (removeMethodInfo != null)
				{
					var accessor = Create(SymbolKind.Accessor, "remove");
					accessor.modifiers = addMethodInfo != null ? GetAccessorModifiers(removeMethodInfo) : modifiers;
					AddMember(accessor);
				}
				break;
			default:
				throw new InvalidOperationException("Importing a non-supported member type!");
		}
	}
	
	private Modifiers GetAccessorModifiers(MethodInfo accessor1, MethodInfo accessor2)
	{
		var union = GetAccessorModifiers(accessor1) | GetAccessorModifiers(accessor2);
		var result = (union & Modifiers.Public) != 0 ? Modifiers.Public : union & (Modifiers.Internal | Modifiers.Protected);
		if (result == Modifiers.None)
			result = Modifiers.Private;
		result |= union & (Modifiers.Abstract | Modifiers.Virtual | Modifiers.Static);
		return result;
	}
	
	private Modifiers GetAccessorModifiers(MethodInfo accessor)
	{
		if (accessor == null)
			return Modifiers.Private;
		
		var modifiers = 
			accessor.IsPublic ? Modifiers.Public :
			accessor.IsFamilyOrAssembly ? Modifiers.Internal | Modifiers.Protected :
			accessor.IsAssembly ? Modifiers.Internal :
			accessor.IsFamily ? Modifiers.Protected :
			Modifiers.Private;
		if (accessor.IsAbstract)
			modifiers |= Modifiers.Abstract;
		if (accessor.IsVirtual)
			modifiers |= Modifiers.Virtual;
		if (accessor.IsStatic)
			modifiers |= Modifiers.Static;
		var baseDefinition = accessor.GetBaseDefinition();
		if (baseDefinition != null && baseDefinition.DeclaringType != accessor.DeclaringType)
			modifiers = (modifiers & ~Modifiers.Virtual) | Modifiers.Override;
		return modifiers;
	}

	public override SymbolDefinition TypeOf()
	{
		if (memberInfo.MemberType == MemberTypes.Constructor)
			return parentSymbol.TypeOf();
		
		if (type != null && (type.definition == null || !type.definition.IsValid()))
			type = null;
		
		if (type == null)
		{
			Type memberType = null;
			switch (memberInfo.MemberType)
			{
				case MemberTypes.Field:
					memberType = ((FieldInfo) memberInfo).FieldType;
					break;
				case MemberTypes.Property:
					memberType = ((PropertyInfo) memberInfo).PropertyType;
					break;
				case MemberTypes.Event:
					memberType = ((EventInfo) memberInfo).EventHandlerType;
					break;
				case MemberTypes.Method:
					memberType = ((MethodInfo) memberInfo).ReturnType;
					break;
			}
			type = ReflectedTypeReference.ForType(memberType);
		}

		return type != null ? type.definition : unknownType;
	}

	//public override bool IsStatic
	//{
	//    get
	//    {
	//        switch (memberInfo.MemberType)
	//        {
	//            case MemberTypes.Method:
	//                return ((MethodInfo) memberInfo).IsStatic;
	//            case MemberTypes.Field:
	//                return ((FieldInfo) memberInfo).IsStatic;
	//            case MemberTypes.Property:
	//                return ((PropertyInfo) memberInfo).GetGetMethod(true).IsStatic;
	//            case MemberTypes.NestedType:
	//                return false; // TODO: Fix this!!!
	//            default:
	//                return false;
	//        }
	//    }
	//    set { }
	//}

	//public override bool IsPublic
	//{
	//    get
	//    {
	//        switch (memberInfo.MemberType)
	//        {
	//            case MemberTypes.Method:
	//                return ((MethodInfo) memberInfo).IsPublic;
	//            case MemberTypes.Field:
	//                return ((FieldInfo) memberInfo).IsPublic;
	//            case MemberTypes.Property:
	//                return ((PropertyInfo) memberInfo).GetGetMethod(true).IsPublic;
	//            case MemberTypes.NestedType:
	//                return ((Type) memberInfo).IsPublic;
	//            default:
	//                return false;
	//        }
	//    }
	//    set { }
	//}

	//public override bool IsProtected
	//{
	//    get
	//    {
	//        switch (memberInfo.MemberType)
	//        {
	//            case MemberTypes.Method:
	//                return ((MethodInfo) memberInfo).IsFamily;
	//            case MemberTypes.Field:
	//                return ((FieldInfo) memberInfo).IsFamily;
	//            case MemberTypes.Property:
	//                return ((PropertyInfo) memberInfo).GetGetMethod(true).IsFamily;
	//            case MemberTypes.NestedType:
	//                return ((Type) memberInfo).IsNestedFamily;
	//            default:
	//                return false;
	//        }
	//    }
	//    set { }
	//}
}


public class ReflectedTypeReference : SymbolReference
{
	protected Type reflectedType;
	protected ReflectedTypeReference(Type type)
	{
		reflectedType = type;
	}

	private static readonly Dictionary<Type, ReflectedTypeReference> allReflectedReferences = new Dictionary<Type,ReflectedTypeReference>();

	public static ReflectedTypeReference ForType(Type type)
	{
		ReflectedTypeReference result;
		if (allReflectedReferences.TryGetValue(type, out result))
			return result;
		result = new ReflectedTypeReference(type);
		allReflectedReferences[type] = result;
		return result;
	}

	public override SymbolDefinition definition
	{
		get
		{
			if (_definition != null && !_definition.IsValid())
				_definition = null;
			
			if (_definition == null)
			{
				if (reflectedType.IsArray)
				{
					var elementType = reflectedType.GetElementType();
					var elementTypeDefinition = ReflectedTypeReference.ForType(elementType).definition as TypeDefinitionBase;
					var rank = reflectedType.GetArrayRank();
					_definition = elementTypeDefinition.MakeArrayType(rank);
					return _definition;
				}

				if (reflectedType.IsGenericParameter)
				{
					var index = reflectedType.GenericParameterPosition;
					var reflectedDeclaringMethod = reflectedType.DeclaringMethod as MethodInfo;
					if (reflectedDeclaringMethod != null && reflectedDeclaringMethod.IsGenericMethod)
					{
						var declaringTypeRef = ForType(reflectedDeclaringMethod.DeclaringType);
						var declaringType = declaringTypeRef.definition as ReflectedType;
						if (declaringType == null)
							return _definition = SymbolDefinition.unknownType;
						var methodName = reflectedDeclaringMethod.Name;
						var typeArgs = reflectedDeclaringMethod.GetGenericArguments();
						var numTypeArgs = typeArgs.Length;
						var member = declaringType.FindName(methodName, numTypeArgs, false);
						if (member == null && numTypeArgs > 0)
							member = declaringType.FindName(methodName, 0, false);
						if (member != null && member.kind == SymbolKind.MethodGroup)
						{
							var methodGroup = (MethodGroupDefinition) member;
							foreach (var m in methodGroup.methods)
							{
								var reflectedMethod = m as ReflectedMethod;
								if (reflectedMethod != null && reflectedMethod.reflectedMethodInfo == reflectedDeclaringMethod)
								{
									member = reflectedMethod;
									break;
								}
							}
						}
						var methodDefinition = member as MethodDefinition;
						_definition = methodDefinition.typeParameters.ElementAtOrDefault(index);
						//	(methodDefinition != null && methodDefinition.typeParameters != null
						//	? methodDefinition.typeParameters.ElementAtOrDefault(index) : null)
						//	?? SymbolDefinition.unknownSymbol;
					}
					else
					{
						var reflectedDeclaringType = reflectedType.DeclaringType;
						while (true)
						{
							var parentType = reflectedDeclaringType.DeclaringType;
							if (parentType == null)
								break;
							var count = parentType.GetGenericArguments().Length;
							if (count <= index)
							{
								index -= count;
								break;
							}
							reflectedDeclaringType = parentType;
						}

						var declaringTypeRef = ForType(reflectedDeclaringType);
						var declaringType = declaringTypeRef.definition as TypeDefinition;
						if (declaringType == null)
							return _definition = SymbolDefinition.unknownType;

						_definition = declaringType.typeParameters[index];
					}
					return _definition;
				}

				if (reflectedType.IsGenericType && !reflectedType.IsGenericTypeDefinition)
				{
					var reflectedTypeDef = reflectedType.GetGenericTypeDefinition();
					var genericTypeDefRef = ForType(reflectedTypeDef);
					var genericTypeDef = genericTypeDefRef.definition as TypeDefinition;
					if (genericTypeDef == null)
						return _definition = SymbolDefinition.unknownType;

					var reflectedTypeArgs = reflectedType.GetGenericArguments();
					var numGenericArgs = reflectedTypeArgs.Length;
					var declaringType = reflectedType.DeclaringType;
					if (declaringType != null && declaringType.IsGenericType)
					{
						var parentArgs = declaringType.GetGenericArguments();
						numGenericArgs -= parentArgs.Length;
					}

					var typeArguments = new ReflectedTypeReference[numGenericArgs];
					for (int i = typeArguments.Length - numGenericArgs, j = 0; i < typeArguments.Length; ++i)
						typeArguments[j++] = ForType(reflectedTypeArgs[i]);
					_definition = genericTypeDef.ConstructType(typeArguments);
					return _definition;
				}

				var tn = reflectedType.Name;
				SymbolDefinition declaringSymbol = null;
				
				if (reflectedType.IsNested)
				{
					declaringSymbol = ForType(reflectedType.DeclaringType).definition;
				}
				else
				{
					var assemblyDefinition = AssemblyDefinition.FromAssembly(reflectedType.Assembly);
					if (assemblyDefinition != null)
						declaringSymbol = assemblyDefinition.FindNamespace(reflectedType.Namespace);
				}

				if (declaringSymbol != null && declaringSymbol.kind != SymbolKind.Error)
				{
					var rankSpecifier = tn.IndexOf('[');
					if (rankSpecifier > 0)
						tn = tn.Substring(0, rankSpecifier);
					var numTypeArgs = 0;
					var genericMarkerIndex = tn.IndexOf('`');
					if (genericMarkerIndex > 0)
					{
						numTypeArgs = int.Parse(tn.Substring(genericMarkerIndex + 1));
						tn = tn.Substring(0, genericMarkerIndex);
					}
					_definition = declaringSymbol.FindName(tn, numTypeArgs, true);
					if (_definition == null)
					{
						//	UnityEngine.Debug.LogWarning(tn + " not found in " + result + " " + result.GetHashCode() + "\n" + "while resolving reference to " + reflectedType);
						return null;
					}
					else if (rankSpecifier > 0)
					{
						var elementType = _definition as TypeDefinition;
						if (elementType != null)
						{
							_definition = elementType.MakeArrayType(tn.Length - rankSpecifier - 1);
						}
						else
						{
							_definition = null;
						}
					}
				}
				if (_definition == null)
					_definition = SymbolDefinition.unknownType;
			}
			return _definition;
		}
	}

	public override string ToString()
	{
		return reflectedType.FullName;
	}
}

public class ReflectedMethod : MethodDefinition
{
	public readonly MethodInfo reflectedMethodInfo;

	public ReflectedMethod(MethodInfo methodInfo, SymbolDefinition memberOf)
	{
		modifiers =
			methodInfo.IsPublic ? Modifiers.Public :
			methodInfo.IsFamilyOrAssembly ? Modifiers.Internal | Modifiers.Protected :
			methodInfo.IsAssembly ? Modifiers.Internal :
			methodInfo.IsFamily ? Modifiers.Protected :
			Modifiers.Private;
		if (methodInfo.IsAbstract)
			modifiers |= Modifiers.Abstract;
		if (methodInfo.IsVirtual)
			modifiers |= Modifiers.Virtual;
		if (methodInfo.IsStatic)
			modifiers |= Modifiers.Static;
		if (methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType)
			modifiers = (modifiers & ~Modifiers.Virtual) | Modifiers.Override;
		if (methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false) && IsStatic)
		{
			var parentType = memberOf.parentSymbol as TypeDefinitionBase;
			if (parentType.kind == SymbolKind.Class && parentType.IsStatic && parentType.NumTypeParameters == 0)
			{
				isExtensionMethod = true;
				++parentType.numExtensionMethods;
			}
		}
		accessLevel = AccessLevelFromModifiers(modifiers);

		reflectedMethodInfo = methodInfo;
		var genericMarker = methodInfo.Name.IndexOf('`');
		name = genericMarker < 0 ? methodInfo.Name : methodInfo.Name.Substring(0, genericMarker);
		parentSymbol = memberOf;

		var tp = methodInfo.GetGenericArguments();
		if (tp.Length > 0)
		{
			var numGenericArgs = tp.Length;
		//	Debug.Log(methodInfo.Name + " with " + tp.Length + " generic arguments.");
			typeParameters = new List<TypeParameterDefinition>(tp.Length);
			for (var i = tp.Length - numGenericArgs; i < tp.Length; ++i)
			{
				var tpDef = new TypeParameterDefinition { kind = SymbolKind.TypeParameter, name = tp[i].Name, parentSymbol = this };
				typeParameters.Add(tpDef);
			}
		}

		returnType = ReflectedTypeReference.ForType(methodInfo.ReturnType);

		if (parameters == null)
			parameters = new List<ParameterDefinition>();
		var methodParameters = methodInfo.GetParameters();
		for (var i = 0; i < methodParameters.Length; ++i)
		{
			var p = methodParameters[i];

			var isByRef = p.ParameterType.IsByRef;
			var parameterType = isByRef ? p.ParameterType.GetElementType() : p.ParameterType;
			var parameterToAdd = new ParameterDefinition
			{
			    kind = SymbolKind.Parameter,
				parentSymbol = this,
				name = p.Name,
				type = ReflectedTypeReference.ForType(parameterType),
				modifiers = isByRef ? (p.IsOut ? Modifiers.Out : Modifiers.Ref) : Attribute.IsDefined(p, typeof(ParamArrayAttribute)) ? Modifiers.Params : Modifiers.None,
			};
			if (i == 0 && isExtensionMethod)
				parameterToAdd.modifiers |= Modifiers.This;
			if (p.RawDefaultValue != DBNull.Value)
			{
				//var dv = Attribute.GetCustomAttribute(p, typeof(System.ComponentModel.DefaultValueAttribute));
				parameterToAdd.defaultValue = p.RawDefaultValue == null ? "null" : p.RawDefaultValue.ToString();
			}
			parameters.Add(parameterToAdd);
		}
	}
}

public class ReflectedConstructor : MethodDefinition
{
	//private readonly ConstructorInfo reflectedConstructorInfo;

	public ReflectedConstructor(ConstructorInfo constructorInfo, SymbolDefinition memberOf)
	{
		modifiers =
			constructorInfo.IsPublic ? Modifiers.Public :
			constructorInfo.IsFamilyOrAssembly ? Modifiers.Internal | Modifiers.Protected :
			constructorInfo.IsAssembly ? Modifiers.Internal :
			constructorInfo.IsFamily ? Modifiers.Protected :
			Modifiers.Private;
		if (constructorInfo.IsAbstract)
			modifiers |= Modifiers.Abstract;
		if (constructorInfo.IsStatic)
			modifiers |= Modifiers.Static;
		accessLevel = AccessLevelFromModifiers(modifiers);

		//reflectedConstructorInfo = constructorInfo;
		//var genericMarker = methodInfo.Name.IndexOf('`');
		//name = genericMarker < 0 ? methodInfo.Name : methodInfo.Name.Substring(0, genericMarker);
		name = ".ctor";
		kind = SymbolKind.Constructor;
		parentSymbol = memberOf;

		returnType = new SymbolReference(memberOf);

		if (parameters == null)
			parameters = new List<ParameterDefinition>();
		foreach (var p in constructorInfo.GetParameters())
		{
			var isByRef = p.ParameterType.IsByRef;
			var parameterType = isByRef ? p.ParameterType.GetElementType() : p.ParameterType;
			var parameterToAdd = new ParameterDefinition
			{
				kind = SymbolKind.Parameter,
				parentSymbol = this,
				name = p.Name,
				type = ReflectedTypeReference.ForType(parameterType),
				modifiers = isByRef ? (p.IsOut ? Modifiers.Out : Modifiers.Ref) : Attribute.IsDefined(p, typeof(ParamArrayAttribute)) ? Modifiers.Params : Modifiers.None,
			};
			if (p.RawDefaultValue != DBNull.Value)
			{
				//var dv = Attribute.GetCustomAttribute(p, typeof(System.ComponentModel.DefaultValueAttribute));
				parameterToAdd.defaultValue = p.RawDefaultValue == null ? "null" : p.RawDefaultValue.ToString();
			}
			parameters.Add(parameterToAdd);
		}
	}
}

public class ReflectedType : TypeDefinition
{
	private readonly Type reflectedType;
	public Type GetReflectedType() { return reflectedType; }

	private bool allPublicMembersReflected;
	private bool allNonPublicMembersReflected;

//	private static Dictionary<Type, ReflectedType> allReflectedTypes;

	public ReflectedType(Type type)
	{
		reflectedType = type;
		modifiers = type.IsNested ?
			(	type.IsNestedPublic ? Modifiers.Public :
				type.IsNestedFamORAssem ? Modifiers.Internal | Modifiers.Protected :
				type.IsNestedAssembly ? Modifiers.Internal :
				type.IsNestedFamily ? Modifiers.Protected :
				Modifiers.Private)
			:
			(	type.IsPublic ? Modifiers.Public :
				!type.IsVisible ? Modifiers.Internal :
				Modifiers.Private );
		if (type.IsAbstract && type.IsSealed)
			modifiers |= Modifiers.Static;
		else if (type.IsAbstract)
			modifiers |= Modifiers.Abstract;
		else if (type.IsSealed)
			modifiers |= Modifiers.Sealed;
		accessLevel = AccessLevelFromModifiers(modifiers);

		var assemblyDefinition = AssemblyDefinition.FromAssembly(type.Assembly);

		var generic = type.Name.IndexOf('`');
		name = generic < 0 ? type.Name : type.Name.Substring(0, generic);
		name = name.Replace("[*]", "[]");
		parentSymbol = string.IsNullOrEmpty(type.Namespace) ? assemblyDefinition.GlobalNamespace : assemblyDefinition.FindNamespace(type.Namespace);
		if (type.IsInterface)
			kind = SymbolKind.Interface;
		else if (type.IsEnum)
			kind = SymbolKind.Enum;
		else if (type.IsValueType)
			kind = SymbolKind.Struct;
		else if (type.IsClass)
		{
			kind = SymbolKind.Class;
			if (type.BaseType == typeof(System.MulticastDelegate))
			{
				kind = SymbolKind.Delegate;
			}
		}
		else
			kind = SymbolKind.None;

//		if (type.IsArray)
//			Debug.LogError("ReflectedType is Array " + name);

		//if (!type.IsGenericTypeDefinition && type.IsGenericType)
		//	UnityEngine.Debug.LogError("Creating ReflectedType instead of ConstructedTypeDefinition from " + type.FullName);

		if (type.IsGenericTypeDefinition)// || type.IsGenericType)
		{
			var gtd = type.GetGenericTypeDefinition() ?? type;
			var tp = gtd.GetGenericArguments();
			var numGenericArgs = tp.Length;
			var declaringType = gtd.DeclaringType;
			if (declaringType != null && declaringType.IsGenericType)
			{
				var parentArgs = declaringType.GetGenericArguments();
				numGenericArgs -= parentArgs.Length;
			}

			if (numGenericArgs > 0)
			{
				typeParameters = new List<TypeParameterDefinition>(numGenericArgs);
				for (var i = tp.Length - numGenericArgs; i < tp.Length; ++i)
				{
					var tpDef = new TypeParameterDefinition { kind = SymbolKind.TypeParameter, name = tp[i].Name, parentSymbol = this };
					typeParameters.Add(tpDef);
				}
			}
		}

		if (type.BaseType != null)
			baseType = ReflectedTypeReference.ForType(type.BaseType ?? typeof(object));

		interfaces = new List<SymbolReference>();
		var implements = type.GetInterfaces();
		for (var i = 0; i < implements.Length; ++i)
			interfaces.Add(ReflectedTypeReference.ForType(implements[i]));
		
		if (IsStatic && NumTypeParameters == 0 && !type.IsNested)
		{
			var attributes = System.Attribute.GetCustomAttributes(type);
			foreach (var attribute in attributes)
			{
				if (attribute is System.Runtime.CompilerServices.ExtensionAttribute)
				{
					ReflectAllMembers(BindingFlags.Public | BindingFlags.NonPublic);
					break;
				}
			}
		}
	}

	private Dictionary<int, SymbolDefinition> importedMembers;
	public SymbolDefinition ImportReflectedMember(MemberInfo info)
	{
		if (info.MemberType == MemberTypes.Method && ((MethodInfo) info).IsPrivate)
			return null;
		if (info.MemberType == MemberTypes.Constructor && ((ConstructorInfo) info).IsPrivate)
			return null;
		if (info.MemberType == MemberTypes.Field && (((FieldInfo) info).IsPrivate || kind == SymbolKind.Enum && info.Name == "value__"))
			return null;
		if (info.MemberType == MemberTypes.NestedType && ((Type)info).IsNestedPrivate)
			return null;
		if (info.MemberType == MemberTypes.Property)
		{
			var p = (PropertyInfo) info;
			var get = p.GetGetMethod(true);
			var set = p.GetSetMethod(true);
			if ((get == null || get.IsPrivate) && (set == null || set.IsPrivate))
				return null;
		}
		if (info.MemberType == MemberTypes.Event)
		{
			var e = (EventInfo) info;
			var add = e.GetAddMethod(true);
			var remove = e.GetRemoveMethod(true);
			if ((add == null || add.IsPrivate) && (remove == null || remove.IsPrivate))
				return null;
		}
		//if (info.Name.IndexOf('.', 1) > 0)
		//{
		//	Debug.Log("m.Name");
		//}
		
		SymbolDefinition imported = null;

		if (importedMembers == null)
			importedMembers = new Dictionary<int, SymbolDefinition>();
		else if (importedMembers.TryGetValue(info.MetadataToken, out imported))
			return imported;

		if (info.MemberType == MemberTypes.NestedType || info.MemberType == MemberTypes.TypeInfo)
		{
			imported = ImportReflectedType(info as Type);
		}
		else if (info.MemberType == MemberTypes.Method)
		{
			imported = ImportReflectedMethod(info as MethodInfo);
		}
		else if (info.MemberType == MemberTypes.Constructor)
		{
			imported = ImportReflectedConstructor(info as ConstructorInfo);
		}
		else
		{
			imported = new ReflectedMember(info, this);
		}
		
		members[imported.name, imported.kind != SymbolKind.MethodGroup ? imported.NumTypeParameters : 0] = imported;
		importedMembers[info.MetadataToken] = imported;
		return imported;
	}

	public override string GetName()
	{
		if (builtInTypes.ContainsValue(this))
			return (from x in builtInTypes where x.Value == this select x.Key).First();
		return base.GetName();
	}

	public override SymbolDefinition TypeOf()
	{
		if (kind != SymbolKind.Delegate)
			return this;
		
		GetParameters();
		return returnType.definition;
	}

	public override List<SymbolDefinition> GetAllIndexers()
	{
		if (!allPublicMembersReflected || !allNonPublicMembersReflected)
			ReflectAllMembers(BindingFlags.Public | BindingFlags.NonPublic);
		
		return base.GetAllIndexers();
	}

	//protected string RankString()
	//{
	//    return reflectedType.IsArray ? '[' + new string(',', reflectedType.GetArrayRank() - 1) + ']' : string.Empty;
	//}
	
	//public override TypeDefinition MakeArrayType(int rank)
	//{
	////	Debug.LogWarning("MakeArrayType " + this + RankString());
	////	if (rank == 1)
	//        return ImportReflectedType(reflectedType.MakeArrayType(rank));
	////	return new ArrayTypeDefinition(this, rank) { kind = kind };
	//}

	private static bool FilterByName(MemberInfo m, object filterCriteria)
	{
		var memberName = (string)filterCriteria;
		return m.Name == memberName || m.Name.Length > memberName.Length && m.Name.StartsWith(memberName, StringComparison.Ordinal) && m.Name[memberName.Length] == '`';
	}

	public override SymbolDefinition FindName(string memberName, int numTypeParameters, bool asTypeOnly)
	{
		memberName = DecodeId(memberName);
		
		SymbolDefinition member = null;
		//if (numTypeParameters > 0)
		//	memberName += "`" + numTypeParameters;
		if (!allPublicMembersReflected || !allNonPublicMembersReflected)
			ReflectAllMembers(BindingFlags.Public | BindingFlags.NonPublic);
		if (!members.TryGetValue(memberName, numTypeParameters, out member))
			return null;

//		if (!members.TryGetValue(memberName, out member) && (!allPublicMembersReflected || !allNonPublicMembersReflected))
//		{
//			var findResult = reflectedType.FindMembers(MemberTypes.All, BindingFlags.Public | BindingFlags.Instance, FilterByName, memberName);
//			if (findResult.Length == 0)
//				findResult = reflectedType.FindMembers(MemberTypes.All, BindingFlags.Public | BindingFlags.Static, FilterByName, memberName);
////			if (findResult.Length == 0)
////			{
////				var baseDefinition = BaseType();
////				if (baseDefinition != null)
////				{
////					var rt = baseDefinition as ReflectedType;
////					if (rt != null)
////						rt.ReflectAllMembers(BindingFlags.Public | BindingFlags.NonPublic);
////					member = baseDefinition.FindName(memberName, numTypeParameters, asTypeOnly);
////					if (asTypeOnly && member != null && !(member is TypeDefinitionBase))
////						return null;
////					return member;
////				}
////			}
////			else
//			if (findResult.Length > 0)
//			{
//				member = ImportReflectedMember(findResult[0]);
//				for (var i = 1; i < findResult.Length; ++i)
//					ImportReflectedMember(findResult[i]);
//			}

////			if (member != null)
////				members[member.ReflectionName] = member;
//		}
		if (asTypeOnly && member != null && !(member is TypeDefinitionBase))
			return null;
		return member;
	}

	public void ReflectAllMembers(BindingFlags flags)
	{
		flags |= BindingFlags.DeclaredOnly;

		var instaceMembers = reflectedType.GetMembers(flags | BindingFlags.Instance);
		foreach (var m in instaceMembers)
			if (m.MemberType != MemberTypes.Method || !((MethodInfo) m).IsSpecialName)
				ImportReflectedMember(m);

		var staticMembers = reflectedType.GetMembers(flags | BindingFlags.Static);
		foreach (var m in staticMembers)
			if (m.MemberType != MemberTypes.Method || !((MethodInfo) m).IsSpecialName)
				ImportReflectedMember(m);

		if ((flags & BindingFlags.Public) == BindingFlags.Public)
			allPublicMembersReflected = true;
		if ((flags & BindingFlags.NonPublic) == BindingFlags.NonPublic)
			allNonPublicMembersReflected = true;
	}

	//public override string GetTooltipText()
	//{
	//	if (kind != SymbolKind.Delegate)
	//		return base.GetTooltipText();

	//	//if (tooltipText == null)
	//	{
	//		tooltipText = base.GetTooltipText();
			
	//		//tooltipText += "\n\nDelegate info\n";
	//		//tooltipText += GetDelegateInfoText();
			
	//		var xmlDocs = GetXmlDocs();
	//		if (!string.IsNullOrEmpty(xmlDocs))
	//		{
	//			tooltipText += "\n\n" + xmlDocs;
	//		}
	//	}

	//	return tooltipText;
	//}

	private ReflectedTypeReference returnType;
	private List<ParameterDefinition> parameters;
	public override List<ParameterDefinition> GetParameters()
	{
		if (kind != SymbolKind.Delegate)
			return null;
		
		if (parameters == null)
		{
			var invoke = reflectedType.GetMethod("Invoke");
			
			returnType = ReflectedTypeReference.ForType(invoke.ReturnType);
			
			parameters = new List<ParameterDefinition>();
			foreach (var p in invoke.GetParameters())
			{
				var isByRef = p.ParameterType.IsByRef;
				var parameterType = isByRef ? p.ParameterType.GetElementType() : p.ParameterType;
				parameters.Add(new ParameterDefinition
				{
					kind = SymbolKind.Parameter,
					parentSymbol = this,
					name = p.Name,
					type = ReflectedTypeReference.ForType(parameterType),
					modifiers = isByRef ? (p.IsOut ? Modifiers.Out : Modifiers.Ref) : Attribute.IsDefined(p, typeof(ParamArrayAttribute)) ? Modifiers.Params : Modifiers.None,
				});
			}
		}
		
		return parameters;
	}

	private string delegateInfoText;
	public override string GetDelegateInfoText()
	{
		if (delegateInfoText == null)
		{
			var parameters = GetParameters();
			var returnType = TypeOf();
			
			delegateInfoText = returnType.GetName() + " " + GetName() + (parameters.Count == 1 ? "( " : "(");
			delegateInfoText += PrintParameters(parameters) + (parameters.Count == 1 ? " )" : ")");
		}

		return delegateInfoText;
	}

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		if (!allPublicMembersReflected)
		{
			if (!allNonPublicMembersReflected)
				ReflectAllMembers(BindingFlags.Public | BindingFlags.NonPublic);
			else
				ReflectAllMembers(BindingFlags.Public);
		}
		else if (!allNonPublicMembersReflected)
		{
			ReflectAllMembers(BindingFlags.NonPublic);
		}

		base.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
	}

	//private Dictionary<BindingFlags, Dictionary<string, SymbolDefinition>> cachedMemberCompletions;
	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		if (!allPublicMembersReflected)
		{
			if (!allNonPublicMembersReflected && ((mask & AccessLevelMask.NonPublic) != 0 || (flags & BindingFlags.NonPublic) != 0))
				ReflectAllMembers(BindingFlags.Public | BindingFlags.NonPublic);
			else
				ReflectAllMembers(BindingFlags.Public);
		}
		else if (!allNonPublicMembersReflected && ((mask & AccessLevelMask.NonPublic) != 0 || (flags & BindingFlags.NonPublic) != 0))
		{
			ReflectAllMembers(BindingFlags.NonPublic);
		}
		
		base.GetMembersCompletionData(data, flags, mask, assembly);

		//if ((mask & AccessLevelMask.Public) != 0)
		//{
		//	if (assembly.InternalsVisibleIn(this.Assembly))
		//		mask |= AccessLevelMask.Internal;
		//	else
		//		mask &= ~AccessLevelMask.Internal;
		//}
		
		//if (cachedMemberCompletions == null)
		//	cachedMemberCompletions = new Dictionary<BindingFlags, Dictionary<string, SymbolDefinition>>();
		//if (!cachedMemberCompletions.ContainsKey(flags))
		//{
		//	var cache = cachedMemberCompletions[flags] = new Dictionary<string, SymbolDefinition>();
		//	base.GetMembersCompletionData(cache, flags, mask, assembly);
		//}

		//var completions = cachedMemberCompletions[flags];
		//foreach (var entry in completions)
		//	if (entry.Value.IsAccessible(mask) && !data.ContainsKey(entry.Key))
		//		data.Add(entry.Key, entry.Value);
	}
}

public class ConstructedInstanceDefinition : InstanceDefinition
{
	public readonly InstanceDefinition genericSymbol;

	public ConstructedInstanceDefinition(InstanceDefinition genericSymbolDefinition)
	{
		genericSymbol = genericSymbolDefinition;
		kind = genericSymbol.kind;
		modifiers = genericSymbol.modifiers;
		accessLevel = genericSymbol.accessLevel;
		name = genericSymbol.name;
	}

	public override SymbolDefinition TypeOf()
	{
		var result = genericSymbol.TypeOf() as TypeDefinitionBase;

		var ctx = parentSymbol as ConstructedTypeDefinition;
		if (ctx != null && result != null)
			result = result.SubstituteTypeParameters(ctx);

		return result;
	}
	
	public override SymbolDefinition GetGenericSymbol()
	{
		return genericSymbol;
	}

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		var symbolType = TypeOf() as TypeDefinitionBase;
		if (symbolType != null)
			symbolType.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
	}

	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		var symbolType = TypeOf();
		if (symbolType != null)
			symbolType.GetMembersCompletionData(data, BindingFlags.Instance, mask, assembly);
	}
}
	
public static class ListExtensions
{
	public static T FirstOrDefault<T>(this List<T> self)
	{
		return self.Count == 0 ? default(T) : self[0];
	}
	
	public static T ElementAtOrDefault<T>(this List<T> self, int index)
	{
		return index >= self.Count ? default(T) : self[index];
	}
}

public class InstanceDefinition : SymbolDefinition
{
	public SymbolReference type;
	private bool _resolvingTypeOf = false;

	public override SymbolDefinition TypeOf()
	{
		if (_resolvingTypeOf)
			return unknownType;
		_resolvingTypeOf = true;
		
		if (type != null && (type.definition == null || !type.definition.IsValid()))
			type = null;

		if (type != null && type.definition.kind == SymbolKind.Error)
			type = null;
		
		if (type == null)
		{
			//type = new SymbolReference();

		//	var parentDefinition = parentScope as SymbolDefinition;
		//	if (parentDefinition.declarations.Count > 0)
			{
				SymbolDeclaration decl = declarations != null ? declarations.FirstOrDefault() : null;
				if (decl != null)
				{
					ParseTree.BaseNode typeNode = null;
					switch (decl.kind)
					{
						case SymbolKind.Parameter:
							if (decl.parseTreeNode.RuleName == "implicitAnonymousFunctionParameter")
							{
								type = TypeOfImplicitParameter(decl);
							}
							else
							{
								typeNode = decl.parseTreeNode.FindChildByName("type");
								type = typeNode != null ? new SymbolReference(typeNode) : null;//"System.Object" };
							}
							break;

						case SymbolKind.Field:
							typeNode = decl.parseTreeNode.parent.parent.parent.FindChildByName("type");
							type = typeNode != null ? new SymbolReference(typeNode) : null;//"System.Object" };
							break;

						case SymbolKind.EnumMember:
							type = new SymbolReference(parentSymbol);
							break;

						case SymbolKind.ConstantField:
						case SymbolKind.LocalConstant:
							//typeNode = decl.parseTreeNode.parent.parent.ChildAt(1);
							//break;
							switch (decl.parseTreeNode.parent.parent.RuleName)
							{
								case "constantDeclaration":
								case "localConstantDeclaration":
									typeNode = decl.parseTreeNode.parent.parent.ChildAt(1);
									break;

								default:
									typeNode = decl.parseTreeNode.parent.parent.parent.FindChildByName("IDENTIFIER");
									break;
							}
							type = typeNode != null ? new SymbolReference(typeNode) : null;
							break;

						case SymbolKind.Property:
						case SymbolKind.Indexer:
							typeNode = decl.parseTreeNode.parent.FindChildByName("type");
							type = typeNode != null ? new SymbolReference(typeNode) : null;
							break;

						case SymbolKind.Event:
							typeNode = decl.parseTreeNode.FindParentByName("eventDeclaration").ChildAt(1);
							type = typeNode != null ? new SymbolReference(typeNode) : null;
							break;
						
						case SymbolKind.Variable:
							if (decl.parseTreeNode != null && decl.parseTreeNode.parent != null && decl.parseTreeNode.parent.parent != null)
								typeNode = decl.parseTreeNode.parent.parent.FindChildByName("localVariableType");
							type = typeNode != null ? new SymbolReference(typeNode) : null;
							break;

						case SymbolKind.ForEachVariable:
							if (decl.parseTreeNode != null)
								typeNode = decl.parseTreeNode.FindChildByName("localVariableType");
							type = typeNode != null ? new SymbolReference(typeNode) : null;
							break;

						case SymbolKind.FromClauseVariable:
							type = null;
							if (decl.parseTreeNode != null)
							{
								typeNode = decl.parseTreeNode.FindChildByName("type");
								type = typeNode != null
									? new SymbolReference(typeNode)
									: new SymbolReference(EnumerableElementType(decl.parseTreeNode.NodeAt(-1)));
							}
							break;

						case SymbolKind.CatchParameter:
							if (decl.parseTreeNode != null)
								typeNode = decl.parseTreeNode.parent.FindChildByName("exceptionClassType");
							type = typeNode != null ? new SymbolReference(typeNode) : null;
							break;

						default:
							Debug.LogError(decl.kind);
							break;
					}
				}
			}
		}

		var result = type != null ? type.definition : unknownType;
		_resolvingTypeOf = false;
		return result;
	}

	private SymbolReference TypeOfImplicitParameter(SymbolDeclaration declaration)
	{
		int index = 0;
		var node = declaration.parseTreeNode;
		if (node.parent.RuleName == "implicitAnonymousFunctionParameterList")
		{
			index = node.childIndex / 2;
			node = node.parent;
		}
		node = node.parent; // anonymousFunctionSignature
		node = node.parent; // lambdaExpression
		node = node.parent; // nonAssignmentExpression
		node = node.parent; // elementInitializer or expression
		if (node.RuleName == "expression" && node.parent.RuleName == "argumentValue")
		{
			node = node.parent; // argumentValue
			if (node.childIndex == 0)
			{
				node = node.parent; // argument
				var argumentIndex = node.childIndex / 2;

				node = node.parent; // argumentList
				node = node.parent; // arguments
				node = node.parent; // constructorInitializer or attribute or primaryExpressionPart or objectCreationExpression
				if (node.RuleName == "primaryExpressionPart")
				{
					ParseTree.Leaf methodId = null;
					node = node.parent.NodeAt(node.childIndex - 1); // primaryExpressionStart or primaryExpressionPart
					if (node.RuleName == "primaryExpressionStart")
					{
						methodId = node.LeafAt(0);
					}
					else // node.RuleName == "primaryExpressionPart"
					{
						node = node.NodeAt(0);
						if (node.RuleName == "accessIdentifier")
						{
							methodId = node.LeafAt(1);
						}
					}
					if (methodId != null && methodId.token.tokenKind == SyntaxToken.Kind.Identifier)
					{
						if (methodId.resolvedSymbol == null || methodId.resolvedSymbol.kind == SymbolKind.MethodGroup)
							FGResolver.ResolveNode(node);
						
						var method = methodId.resolvedSymbol as MethodDefinition;
						var constructedSymbol = methodId.resolvedSymbol as ConstructedSymbolReference;
						if (method != null)
						{
							if (method.IsExtensionMethod)
							{
								var nodeLeft = methodId.parent;
								if (nodeLeft != null && nodeLeft.RuleName == "accessIdentifier")
								{
									nodeLeft = nodeLeft.FindPreviousNode() as ParseTree.Node;
									if (nodeLeft != null)
									{
										if (nodeLeft.RuleName == "primaryExpressionPart" || nodeLeft.RuleName == "primaryExpressionStart")
										{
											var symbolLeft = FGResolver.GetResolvedSymbol(nodeLeft);
											if (symbolLeft != null && symbolLeft.kind != SymbolKind.Error && !(symbolLeft is TypeDefinitionBase))
												++argumentIndex;
										}
										else
										{
											++argumentIndex;
										}
									}
								}
							}

							if (argumentIndex < method.parameters.Count)
							{
								var parameter = method.parameters[argumentIndex];
								var parameterType = parameter.TypeOf();
								if (parameterType.kind == SymbolKind.Delegate)
								{
									parameterType = parameterType.SubstituteTypeParameters(method);
									var delegateParameters = parameterType.GetParameters();
									if (delegateParameters != null && index < delegateParameters.Count)
									{
										var type = delegateParameters[index].TypeOf();
										type = type.SubstituteTypeParameters(parameterType);
										//type = type.SubstituteTypeParameters(method);
										return new SymbolReference(type);
									}
								}
							}
						}
						else if (constructedSymbol != null && constructedSymbol.kind == SymbolKind.Method)
						{
							var genericMethod = constructedSymbol.referencedSymbol;
							var parameters = genericMethod.GetParameters();
							if (parameters != null && argumentIndex < parameters.Count)
							{
								var parameter = parameters[argumentIndex];
								var parameterType = parameter.TypeOf();
								if (parameterType.kind == SymbolKind.Delegate)
								{
									parameterType = parameterType.SubstituteTypeParameters(constructedSymbol);
									var delegateParameters = parameterType.GetParameters();
									if (delegateParameters != null && index < delegateParameters.Count)
									{
										var type = delegateParameters[index].TypeOf();
										type = type.SubstituteTypeParameters(parameterType);
										//type = type.SubstituteTypeParameters(constructedSymbol);
										return new SymbolReference(type);
									}
								}
							}
						}
					}
				}
			}
		}
		return null;
	}

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		if (asTypeOnly)
		{
			leaf.resolvedSymbol = null;
			return;
		}

		TypeOf();
		if (type == null || type.definition == null || type.definition == unknownType || type.definition == unknownSymbol)
		{
			leaf.resolvedSymbol = null;
			return;
		}
		type.definition.ResolveMember(leaf, context, numTypeArgs, false);
	}

	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		var instanceType = TypeOf();
		if (instanceType != null)
			instanceType.GetMembersCompletionData(data, BindingFlags.Instance, mask, assembly);
	}

	//public override bool IsGeneric
	//{
	//	get
	//	{
	//		return TypeOf().IsGeneric;
	//	}
	//}
}

public class IndexerDefinition : InstanceDefinition
{
	public List<ParameterDefinition> parameters;

	public SymbolDefinition AddParameter(SymbolDeclaration symbol)
	{
		var symbolName = symbol.Name;
		var parameter = (ParameterDefinition) Create(symbol);
		parameter.type = new SymbolReference(symbol.parseTreeNode.FindChildByName("type"));
		parameter.parentSymbol = this;
		if (!string.IsNullOrEmpty(symbolName))
		{
			if (parameters == null)
				parameters = new List<ParameterDefinition>();
			parameters.Add(parameter);
		}
		return parameter;
	}

	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.Parameter)
		{
			SymbolDefinition definition = AddParameter(symbol);
			symbol.definition = definition;
			return definition;
		}

		return base.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.Parameter && parameters != null)
			parameters.Remove((ParameterDefinition) symbol.definition);
		else
			base.RemoveDeclaration(symbol);
	}

	public override List<ParameterDefinition> GetParameters()
	{
		return parameters ?? _emptyParameterList;
	}
	
	public override SymbolDefinition FindName(string memberName, int numTypeParameters, bool asTypeOnly)
	{
		memberName = DecodeId(memberName);
		
		if (!asTypeOnly && parameters != null)
		{
			var definition = parameters.Find(x => x.name == memberName);
			if (definition != null)
				return definition;
		}
		return base.FindName(memberName, numTypeParameters, asTypeOnly);
	}

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		if (!asTypeOnly && parameters != null)
		{
			var leafText = DecodeId(leaf.token.text);
			var definition = parameters.Find(x => x.name == leafText);
			if (definition != null)
			{
				leaf.resolvedSymbol = definition;
				return;
			}
		}
		base.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
	}
	
	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		if (parameters != null)
		{
			for (var i = parameters.Count; i --> 0; )
			{
				var p = parameters[i];
				if (!data.ContainsKey(p.name))
					data.Add(p.name, p);
			}
		}
	}
}

public class ThisReference : InstanceDefinition
{
	public ThisReference(SymbolReference type)
	{
		this.type = type;
		kind = SymbolKind.Instance;
	}

	public override string GetTooltipText()
	{
		return type.definition.GetTooltipText();
	}
}

public class ValueParameter : ParameterDefinition {}

public class NullLiteral : InstanceDefinition
{
	public readonly NullTypeDefinition nullTypeDefinition = new NullTypeDefinition();

	public override SymbolDefinition TypeOf()
	{
		return nullTypeDefinition;
	}

	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
	}
}

public class NullTypeDefinition : TypeDefinitionBase
{
	public override bool CanConvertTo(TypeDefinitionBase otherType)
	{
		return otherType.kind == SymbolKind.Class || otherType.kind == SymbolKind.Interface || otherType.kind == SymbolKind.Delegate;
	}

	public override TypeDefinitionBase ConvertTo(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return null;

		if (otherType is TypeParameterDefinition)
			return this;

		if (otherType.kind == SymbolKind.Class || otherType.kind == SymbolKind.Interface || otherType.kind == SymbolKind.Delegate)
			return otherType;

		return null;
	}
}

public class ParameterDefinition : InstanceDefinition
{
	public bool IsThisParameter { get { return modifiers == Modifiers.This; } }

	public bool IsRef { get { return modifiers == Modifiers.Ref; } }
	public bool IsOut { get { return modifiers == Modifiers.Out; } }
	public bool IsParametersArray { get { return modifiers == Modifiers.Params; } }

	public bool IsOptional { get { return defaultValue != null || IsParametersArray; } }
	public string defaultValue;
}

public abstract class TypeDefinitionBase : SymbolDefinition
{
	protected SymbolDefinition thisReferenceCache;
	
	public int numExtensionMethods;

	protected bool convertingToBase; // Prevents infinite recursion

	public override Type GetRuntimeType()
	{
		if (Assembly == null || Assembly.assembly == null)
			return null;
		
		if (parentSymbol is TypeDefinitionBase)
		{
			Type parentType = parentSymbol.GetRuntimeType();
			if (parentType == null)
				return null;
			
			var result = parentType.GetNestedType(ReflectionName, BindingFlags.NonPublic | BindingFlags.Public);
			return result;
		}
		
		return Assembly.assembly.GetType(FullReflectionName);
	}
	
	public override SymbolDefinition TypeOf()
	{
		return this;
	}
	
	public override TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	{
		return this;
	}
	
	public virtual void InvalidateBaseType() {}
	
	public virtual List<SymbolReference> Interfaces()
	{
		return _emptyInterfaceList;
	}

	public virtual TypeDefinitionBase BaseType()
	{
		return this == builtInTypes_object ? null : builtInTypes_object;
	}

	protected virtual string RankString()
	{
		return string.Empty;
	}

	protected MethodDefinition defaultConstructor;
	public virtual MethodDefinition GetDefaultConstructor()
	{
		if (defaultConstructor == null)
		{
			defaultConstructor = new MethodDefinition
			{
				kind = SymbolKind.Constructor,
				parentSymbol = this,
				name = ".ctor",
				accessLevel = accessLevel,
				modifiers = modifiers & (Modifiers.Public | Modifiers.Internal | Modifiers.Protected),
			};
		}
		return defaultConstructor;
	}

	private Dictionary<int, ArrayTypeDefinition> createdArrayTypes;
	public ArrayTypeDefinition MakeArrayType(int arrayRank)
	{
		ArrayTypeDefinition arrayType;
		if (createdArrayTypes == null)
			createdArrayTypes = new Dictionary<int, ArrayTypeDefinition>();
		if (!createdArrayTypes.TryGetValue(arrayRank, out arrayType))
			createdArrayTypes[arrayRank] = arrayType = new ArrayTypeDefinition(this, arrayRank);
		return arrayType;
	}

	private TypeDefinition createdNullableType;
	public TypeDefinition MakeNullableType()
	{
		if (createdNullableType == null)
		{
			createdNullableType = builtInTypes_Nullable.ConstructType(new []{ new SymbolReference(this) });
		}
		return createdNullableType;
	}

	public SymbolDefinition GetThisInstance()
	{
		if (thisReferenceCache == null)
		{
			if (IsStatic)
				return thisReferenceCache = unknownType;
			thisReferenceCache = new ThisReference(new SymbolReference(this));
		}
		return thisReferenceCache;
	}
	
	private bool resolvingInBase = false;
	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		base.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);

		if (!resolvingInBase && leaf.resolvedSymbol == null)
		{
			resolvingInBase = true;
			
			var baseType = BaseType();
			var interfaces = Interfaces();
			
			if (!asTypeOnly && interfaces != null && kind == SymbolKind.Interface)
			{
				foreach (var i in interfaces)
				{
					i.definition.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
					if (leaf.resolvedSymbol != null)
					{
						resolvingInBase = false;
						return;
					}
				}
			}

			if (baseType != null && baseType != this)
			{
				baseType.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
			}
			
			resolvingInBase = false;
		}
	}
	
	public virtual bool DerivesFrom(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return false;
		
		return DerivesFromRef(ref otherType);
	}

	public virtual bool DerivesFromRef(ref TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return false;
		
		var otherTypeAsConstructed = otherType as ConstructedTypeDefinition;
		if (otherTypeAsConstructed != null)
			otherType = otherTypeAsConstructed.genericTypeDefinition;

		if (this == otherType)
			return true;

		if (BaseType() != null)
			return BaseType().DerivesFromRef(ref otherType);

		return false;
	}
	
	protected override SymbolDefinition GetIndexer(TypeDefinitionBase[] argumentTypes)
	{
		var indexers = GetAllIndexers();
		
		// TODO: Resolve overloads
		
		return indexers != null ? indexers[0] : null;
	}

	public virtual List<SymbolDefinition> GetAllIndexers()
	{
		List<SymbolDefinition> indexers = null;
		foreach (var m in members)
			if (m.kind == SymbolKind.Indexer)
			{
				if (indexers == null)
					indexers = new List<SymbolDefinition>();
				indexers.Add(m);
			}
		return indexers;
	}

	//public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	//{
	//	base.GetCompletionData(data, false, assembly);
	//	var baseType = BaseType();
	//	var interfaces = Interfaces();
	//	foreach (var i in interfaces)
	//		i.definition.GetMembersCompletionData(data, fromInstance ? 0 : BindingFlags.Static, AccessLevelMask.Any & ~AccessLevelMask.Private, assembly);
	//	if (baseType != null)
	//		baseType.GetMembersCompletionData(data, fromInstance ? 0 : BindingFlags.Static, AccessLevelMask.Any & ~AccessLevelMask.Private, assembly);
	//}
	
	private bool completionsFromBase = false;
	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		base.GetMembersCompletionData(data, flags, mask, assembly);
		
		if (completionsFromBase)
			return;
		completionsFromBase = true;
		
		var baseType = BaseType();
		var interfaces = Interfaces();
		if (flags != BindingFlags.Static && kind == SymbolKind.Interface)
			foreach (var i in interfaces)
				i.definition.GetMembersCompletionData(data, flags, mask & ~AccessLevelMask.Private, assembly);
		if (baseType != null && (kind != SymbolKind.Enum || flags != BindingFlags.Static) &&
			(baseType.kind != SymbolKind.Interface || kind == SymbolKind.Interface))
		{
			baseType.GetMembersCompletionData(data, flags, mask & ~AccessLevelMask.Private, assembly);
		}
		
		completionsFromBase = false;
	}
	
	internal virtual TypeDefinitionBase BindTypeArgument(TypeDefinitionBase typeArgument, TypeDefinitionBase argumentType)
	{
		return null;
	}
	//	if (!argumentType.CanConvertTo(this))
	//		return null;

	//	if ((kind == SymbolKind.Interface) == (argumentType.kind == SymbolKind.Interface))
	//	{
	//		var genericSymbol = argumentType.GetGenericSymbol();
	//		if (genericSymbol == this)
	//		{
	//			var result = BindTypeArgument(typeArgument, argumentType);
	//			if (result != null)
	//				return result;
	//			else
	//				Debug.Log(argumentType.GetTooltipText());
	//		}
	//		return BindTypeArgument(typeArgument, argumentType.BaseType());
	//	}

	//	var interfaces = argumentType.Interfaces();
	//	for (var i = 0; i < interfaces.Count; ++i)
	//	{
	//		var interfaceType = interfaces[i].definition as TypeDefinitionBase;
	//		if (interfaceType != null && interfaceType.kind != SymbolKind.Error)
	//		{
	//			var result = BindTypeArgument(typeArgument, interfaceType);
	//			if (result != null)
	//				return result;
	//		}
	//	}

	//	return null;
	//}

	public virtual bool CanConvertTo(TypeDefinitionBase otherType)
	{
		return ConvertTo(otherType) != null;
	}

	public virtual TypeDefinitionBase ConvertTo(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return null;

		if (otherType == this)
			return this;

		if (otherType is TypeParameterDefinition)
			return this;

		if (otherType == builtInTypes_object)
			return otherType;

		if (this == builtInTypes_int && (
			otherType == builtInTypes_long ||
			otherType == builtInTypes_float ||
			otherType == builtInTypes_double))
			return otherType;
		if (this == builtInTypes_uint && (
			otherType == builtInTypes_long ||
			otherType == builtInTypes_ulong ||
			otherType == builtInTypes_float ||
			otherType == builtInTypes_double))
			return otherType;
		if (this == builtInTypes_byte && (
			otherType == builtInTypes_short ||
			otherType == builtInTypes_ushort ||
			otherType == builtInTypes_int ||
			otherType == builtInTypes_uint ||
			otherType == builtInTypes_long ||
			otherType == builtInTypes_ulong ||
			otherType == builtInTypes_float ||
			otherType == builtInTypes_double))
			return otherType;
		if (this == builtInTypes_sbyte && (
			otherType == builtInTypes_short ||
			otherType == builtInTypes_int ||
			otherType == builtInTypes_long ||
			otherType == builtInTypes_float ||
			otherType == builtInTypes_double))
			return otherType;
		if (this == builtInTypes_short && (
			otherType == builtInTypes_int ||
			otherType == builtInTypes_long ||
			otherType == builtInTypes_float ||
			otherType == builtInTypes_double))
			return otherType;
		if (this == builtInTypes_ushort && (
			otherType == builtInTypes_int ||
			otherType == builtInTypes_uint ||
			otherType == builtInTypes_long ||
			otherType == builtInTypes_ulong ||
			otherType == builtInTypes_float ||
			otherType == builtInTypes_double))
			return otherType;
		if ((this == builtInTypes_long || this == builtInTypes_ulong) &&
			(otherType == builtInTypes_float || otherType == builtInTypes_double))
			return otherType;
		if (this == builtInTypes_float &&
			otherType == builtInTypes_double)
			return otherType;
		if (this == builtInTypes_char &&
			otherType == builtInTypes_string)
			return otherType;

		if (DerivesFromRef(ref otherType))
			return otherType;

		return null;
	}
}

//TODO: Finish this
public class LambdaExpressionDefinition : TypeDefinitionBase
{
	public override bool CanConvertTo(TypeDefinitionBase otherType)
	{
		if (otherType.kind == SymbolKind.Delegate)
			return true;
		return base.CanConvertTo(otherType);
	}

	public override TypeDefinitionBase ConvertTo(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return null;

		if (otherType.kind == SymbolKind.Delegate)
		{
			return otherType;
		}
		return base.ConvertTo(otherType);
	}
}

public class DelegateTypeDefinition : TypeDefinition
{
	public SymbolReference returnType;
	public List<ParameterDefinition> parameters;

	public override TypeDefinitionBase BaseType()
	{
		if (baseType == null)
			baseType = ReflectedTypeReference.ForType(typeof(MulticastDelegate));
		return baseType.definition as TypeDefinitionBase;
	}
	
	public override List<SymbolReference> Interfaces()
	{
		if (interfaces == null)
			interfaces = BaseType().Interfaces();
		return interfaces;
	}

	public override SymbolDefinition TypeOf()
	{
		return returnType != null && returnType.definition.IsValid() ? returnType.definition : unknownType;
	}

	public SymbolDefinition AddParameter(SymbolDeclaration symbol)
	{
		var symbolName = symbol.Name;
		var parameter = (ParameterDefinition) Create(symbol);
		parameter.type = new SymbolReference(symbol.parseTreeNode.FindChildByName("type"));
		parameter.parentSymbol = this;
		if (!string.IsNullOrEmpty(symbolName))
		{
			if (parameters == null)
				parameters = new List<ParameterDefinition>();
			parameters.Add(parameter);
			
			var nameNode = symbol.NameNode();
			if (nameNode != null)
				nameNode.SetDeclaredSymbol(parameter);
		}
		return parameter;
	}

	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.Parameter)
		{
			SymbolDefinition definition = AddParameter(symbol);
			//	if (!members.TryGetValue(symbolName, out definition) || definition is ReflectedMember || definition is ReflectedType)
			//		definition = AddMember(symbol);

			symbol.definition = definition;
			return definition;
		}

		return base.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.Parameter && parameters != null)
			parameters.Remove((ParameterDefinition) symbol.definition);
		else
			base.RemoveDeclaration(symbol);
	}

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		if (!asTypeOnly && parameters != null)
		{
			var leafText = DecodeId(leaf.token.text);
			var definition = parameters.Find(x => x.name == leafText);
			if (definition != null)
			{
				leaf.resolvedSymbol = definition;
				return;
			}
		}
		base.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
	}

	public override List<ParameterDefinition> GetParameters()
	{
		return parameters ?? _emptyParameterList;
	}

	//public override List<TypeParameterDefinition> GetTypeParameters()
	//{
	//	return null;// typeParameters ?? new List<TypeParameterDefinition>();
	//}

	private string delegateInfoText;
	public override string GetDelegateInfoText()
	{
		if (delegateInfoText == null)
		{
			delegateInfoText = returnType.definition.GetName() + " " + GetName() + (parameters != null && parameters.Count == 1 ? "( " : "(");
			delegateInfoText += PrintParameters(parameters) + (parameters != null && parameters.Count == 1 ? " )" : ")");
		}
		return delegateInfoText;
	}
}

public class TypeParameterDefinition : TypeDefinitionBase
{
	public SymbolReference baseTypeConstraint;
	public List<SymbolReference> interfacesConstraint;
	public bool classConstraint;
	public bool structConstraint;
	public bool newConstraint;

	public override string GetTooltipText()
	{
		//if (tooltipText == null)
		{
			tooltipText = name + " in " + parentSymbol.GetName();
			if (baseTypeConstraint != null)
				tooltipText += " where " + name + " : " + BaseType().GetName();
		}
		return tooltipText;
	}

	public override string GetName()
	{
		//var definingType = parentSymbol as TypeDefinition;
		//if (definingType != null && definingType.tempTypeArguments != null)
		//{
		//    var index = definingType.typeParameters.IndexOf(this);
		//    return definingType.tempTypeArguments[index].definition.GetName();
		//}
		return name;
	}

	public override TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	{
		return context.TypeOfTypeParameter(this);
	}

	public override TypeDefinitionBase BaseType()
	{
		if (baseTypeConstraint == null)
			return base.BaseType();
		return baseTypeConstraint.definition as TypeDefinitionBase ?? base.BaseType();
	}

	internal override TypeDefinitionBase BindTypeArgument(TypeDefinitionBase typeArgument, TypeDefinitionBase argumentType)
	{
		if (this == typeArgument)
			return argumentType;

		return null;
	}
}

public class ConstructedTypeDefinition : TypeDefinition
{
	public readonly TypeDefinition genericTypeDefinition;
	public readonly SymbolReference[] typeArguments;

	public ConstructedTypeDefinition(TypeDefinition definition, SymbolReference[] arguments)
	{
		name = definition.name;
		kind = definition.kind;
		parentSymbol = definition.parentSymbol;
		genericTypeDefinition = definition;

		if (definition.typeParameters != null && arguments != null)
		{
			typeParameters = definition.typeParameters;
			typeArguments = new SymbolReference[typeParameters.Count];
			for (var i = 0; i < typeArguments.Length && i < arguments.Length; ++i)
				typeArguments[i] = arguments[i];
		}
	}

	public override ConstructedTypeDefinition ConstructType(SymbolReference[] typeArgs)
	{
		var result = genericTypeDefinition.ConstructType(typeArgs);
		result.parentSymbol = parentSymbol;
		return result;
	}
	
	public override SymbolDefinition TypeOf()
	{
		if (kind != SymbolKind.Delegate)
			return base.TypeOf();
		
		var result = genericTypeDefinition.TypeOf() as TypeDefinitionBase;
		result = result.SubstituteTypeParameters(this);
		return result;
	}
	
	public override SymbolDefinition GetGenericSymbol()
	{
		return genericTypeDefinition;
	}

	public override TypeDefinitionBase TypeOfTypeParameter(TypeParameterDefinition tp)
	{
		if (typeParameters != null)
		{
			var index = typeParameters.IndexOf(tp);
			if (index >= 0)
			{
				if (typeArguments[index] == null)
					return unknownType;
				else
					return typeArguments[index].definition as TypeDefinitionBase ?? tp;
			}
		}
		return base.TypeOfTypeParameter(tp);
	}

	public override TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	{
		var target = this;
		var parentType = parentSymbol as TypeDefinitionBase;
		if (parentType != null)
		{
			parentType = parentType.SubstituteTypeParameters(context);
			var constructedParent = parentType as ConstructedTypeDefinition;
			if (constructedParent != null)
				target = constructedParent.GetConstructedMember(this.genericTypeDefinition) as ConstructedTypeDefinition;
		}

		if (typeArguments == null)
			return target;

		var constructNew = false;
		var newArguments = new SymbolReference[typeArguments.Length];
		for (var i = 0; i < newArguments.Length; ++i)
		{
			newArguments[i] = typeArguments[i];
			var original = typeArguments[i] != null ? typeArguments[i].definition as TypeDefinitionBase : null;
			if (original == null)
				continue;
			var substitute = original.SubstituteTypeParameters(target);
			substitute = substitute.SubstituteTypeParameters(context);
			if (substitute != original)
			{
				newArguments[i] = new SymbolReference(substitute);
				constructNew = true;
			}
		}
		if (!constructNew)
			return target;
		return ConstructType(newArguments);
	}
	
	//internal TypeDefinitionBase BindTypeParameter(TypeDefinitionBase parameterType, TypeParameterDefinition typeParameter)
	//{
	//	var parameterGenericType = parameterType.GetGenericSymbol();
	//	if (genericTypeDefinition != parameterGenericType)
	//		return base.BindTypeParameter(parameterType, typeParameter);
	//	TypeOfTypeParameter
	//	TypeDefinitionBase result = null;
	//	for (var i = 0; i < typeArguments.Length; ++i)
	//	{
	//		var typeArgumentType = typeArguments[i].definition as TypeDefinitionBase;
	//		if (typeArgumentType != null)
	//		{
	//			var boundType = typeArgumentType.BindTypeParameter(parameterType, typeParameter);
	//		}
	//	}
	//}

	internal override TypeDefinitionBase BindTypeArgument(TypeDefinitionBase typeArgument, TypeDefinitionBase argumentType)
	{
		var convertedArgument = argumentType.ConvertTo(this);

		//TypeDefinitionBase convertedArgument = this;
		//if (!argumentType.DerivesFromRef(ref convertedArgument))
		//	return base.BindTypeArgument(typeArgument, argumentType);
			
		var argumentAsConstructedType = convertedArgument as ConstructedTypeDefinition;
		if (argumentAsConstructedType != null && GetGenericSymbol() == argumentAsConstructedType.GetGenericSymbol())
		{
			TypeDefinitionBase inferedType = null;
			for (int i = 0; i < NumTypeParameters; ++i)
			{
				var fromConstructedType = argumentAsConstructedType.typeArguments[i].definition as TypeDefinitionBase;
				if (fromConstructedType != null)
				{
					var bindTarget = typeArguments[i].definition as TypeDefinitionBase;
					var boundTypeArgument = bindTarget.BindTypeArgument(typeArgument, fromConstructedType);
					if (boundTypeArgument != null)
					{
						if (inferedType == null || inferedType.CanConvertTo(boundTypeArgument))
							inferedType = boundTypeArgument;
						else if (!boundTypeArgument.CanConvertTo(inferedType))
							return null;
					}
				}
			}
			
			if (inferedType != null)
				return inferedType;
		}
		return base.BindTypeArgument(typeArgument, argumentType);
	}

	public override List<SymbolReference> Interfaces()
	{
		if (interfaces == null)
			BaseType();
		return interfaces;
	}

	public override TypeDefinitionBase BaseType()
	{
		if (baseType != null && !baseType.definition.IsValid() ||
			interfaces != null && interfaces.Exists(x => !x.definition.IsValid()))
		{
			baseType = null;
			interfaces = null;
		}

		if (interfaces == null)
		{
			var baseTypeDef = genericTypeDefinition.BaseType();
			baseType = baseTypeDef != null ? new SymbolReference(baseTypeDef.SubstituteTypeParameters(this)) : null;

			interfaces = new List<SymbolReference>(genericTypeDefinition.Interfaces());
			for (var i = 0; i < interfaces.Count; ++i)
				interfaces[i] = new SymbolReference(((TypeDefinitionBase) interfaces[i].definition).SubstituteTypeParameters(this));
		}
		return baseType != null ? baseType.definition as TypeDefinitionBase : base.BaseType();
	}

	public override List<ParameterDefinition> GetParameters()
	{
		return genericTypeDefinition.GetParameters();
	}

	public override bool CanConvertTo(TypeDefinitionBase otherType)
	{
		if (genericTypeDefinition == otherType)
			return true;

		if (DerivesFrom(otherType))
			return true;

		return false;
	}

	public override TypeDefinitionBase ConvertTo(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return null;

		if (otherType is TypeParameterDefinition)
			return this;

		if (genericTypeDefinition == otherType)
			return this;

		var otherGenericType = otherType.GetGenericSymbol() as TypeDefinitionBase;
		if (genericTypeDefinition == otherGenericType)
		{
			var otherConstructedType = otherType as ConstructedTypeDefinition;
			var otherTypeTypeArgs = otherConstructedType.typeArguments;

			var convertedTypeArgs = new List<SymbolReference>(typeArguments.Length);
			for (var i = typeArguments.Length; i -- > 0;)
			{
				var typeArgument = typeArguments[i].definition as TypeDefinitionBase;
				if (typeArgument == null)
					typeArgument = otherTypeTypeArgs[i].definition as TypeDefinitionBase;
				else
					typeArgument = typeArgument.ConvertTo(otherTypeTypeArgs[i].definition as TypeDefinitionBase);
				if (typeArgument == null)
					break;
				
				var typeReference = new SymbolReference(typeArgument);
				convertedTypeArgs.Add(typeReference);
			}

			if (convertedTypeArgs.Count == typeArguments.Length)
			{
				var convertedType = genericTypeDefinition.ConstructType(convertedTypeArgs.ToArray());
				return convertedType;
			}
		}

		if (convertingToBase)
			return null;
		convertingToBase = true;
		
		var baseTypeDefinition = BaseType();

		if (otherType.kind == SymbolKind.Interface)
		{
			for (int i = 0; i < interfaces.Count; i++)
			{
				var interfaceTpe = interfaces[i];
				var convertedToInterface = ((TypeDefinitionBase) interfaceTpe.definition).ConvertTo(otherType);
				if (convertedToInterface != null)
				{
					convertingToBase = false;
					return convertedToInterface;
				}
			}
		}

		if (baseTypeDefinition != null)
		{
			var converted = baseTypeDefinition.ConvertTo(otherType);
			if (converted != null)
			{
				convertingToBase = false;
				return converted;
			}
		}
		
		convertingToBase = false;		
		
		return null;
	}

	public override bool DerivesFromRef(ref TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return false;
		
		if (genericTypeDefinition == otherType)
		{
			otherType = this;
			return true;
		}

		var baseType = BaseType();

		if (otherType.kind == SymbolKind.Interface)
		{
			foreach (var i in interfaces)
				if (((TypeDefinitionBase) i.definition).DerivesFromRef(ref otherType))
				{
					otherType = otherType.SubstituteTypeParameters(this);
					return true;
				}
		}

		if (baseType != null && baseType.DerivesFromRef(ref otherType))
		{
			otherType = otherType.SubstituteTypeParameters(this);
			return true;
		}

		return false;
	}

	public override bool DerivesFrom(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return false;
		
		return genericTypeDefinition.DerivesFrom(otherType);
	}

	public override string GetName()
	{
		if (typeArguments == null || typeArguments.Length == 0)
			return name;

		var sb = new StringBuilder();
		sb.Append(name);
		var comma = "<";
		for (var i = 0; i < typeArguments.Length; ++i)
		{
			sb.Append(comma);
			if (typeArguments[i] != null)
				sb.Append(typeArguments[i].definition.GetName());
			comma = ", ";
		}
		sb.Append('>');
		return sb.ToString();
	}
	
	//public override string GetDelegateInfoText()
	//{
	//	var result = genericTypeDefinition.GetTooltipText();
	//	return result;
	//}

//	public override string GetTooltipText()
//	{
//		return base.GetTooltipText();

////		if (tooltipText != null)
////			return tooltipText;

//		if (parentSymbol != null && !string.IsNullOrEmpty(parentSymbol.GetName()))
//			tooltipText = kind.ToString().ToLowerInvariant() + " " + parentSymbol.GetName() + ".";// + name;
//		else
//			tooltipText = kind.ToString().ToLowerInvariant() + " ";// +name;

//		tooltipText += GetName();
//		//tooltipText += "<" + (typeArguments[0] != null ? typeArguments[0].definition : genericTypeDefinition.typeParameters[0]).GetName();
//		//for (var i = 1; i < typeArguments.Length; ++i)
//		//    tooltipText += ", " + (typeArguments[i] != null ? typeArguments[i].definition : genericTypeDefinition.typeParameters[i]).GetName();
//		//tooltipText += '>';

//		var xmlDocs = GetXmlDocs();
//		if (!string.IsNullOrEmpty(xmlDocs))
//		{
//		    tooltipText += "\n\n" + xmlDocs;
//		}

//		return tooltipText;
//	}

	public override SymbolDefinition FindName(string memberName, int numTypeParameters, bool asTypeOnly)
	{
		memberName = DecodeId(memberName);
		
		return genericTypeDefinition.FindName(memberName, numTypeParameters, asTypeOnly);
	}

	public Dictionary<SymbolDefinition, SymbolDefinition> constructedMembers;

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		genericTypeDefinition.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
		
		var genericMember = leaf.resolvedSymbol;
		if (genericMember == null)// || genericMember is MethodGroupDefinition)// !genericMember.IsGeneric)
			return;

		SymbolDefinition constructed;
		if (constructedMembers != null && constructedMembers.TryGetValue(genericMember, out constructed))
			leaf.resolvedSymbol = constructed;
		else
			leaf.resolvedSymbol = GetConstructedMember(genericMember);

		if (asTypeOnly && !(leaf.resolvedSymbol is TypeDefinitionBase))
			leaf.resolvedSymbol = null;
	}

	public SymbolDefinition GetConstructedMember(SymbolDefinition member)
	{
		var parent = member.parentSymbol;
		if (parent is MethodGroupDefinition)
			parent = parent.parentSymbol;

		if (genericTypeDefinition != parent)
		{
		//	UnityEngine.Debug.Log(member.GetTooltipText() + " is not member of " + genericTypeDefinition.GetTooltipText());
			return member;
		}

		//if (!member.IsGeneric)
		//    return member;

		SymbolDefinition constructed;
		if (constructedMembers == null)
			constructedMembers = new Dictionary<SymbolDefinition, SymbolDefinition>();
		else if (constructedMembers.TryGetValue(member, out constructed))
			return constructed;

		constructed = ConstructMember(member);
		constructedMembers[member] = constructed;
		return constructed;
	}

	private SymbolDefinition ConstructMember(SymbolDefinition member)
	{
		SymbolDefinition symbol;
		if (member is InstanceDefinition)
		{
			symbol = new ConstructedInstanceDefinition(member as InstanceDefinition);
		}
		if (member is TypeDefinition)
		{
			symbol = (member as TypeDefinition).ConstructType(null);// new ConstructedTypeDefinition(member as TypeDefinition, null);
		}
		else
		{
			symbol = new ConstructedSymbolReference(member);
		}
		symbol.parentSymbol = this;
		return symbol;
	}

	public override bool IsSameType(TypeDefinitionBase type)
	{
		if (type == this)
			return true;
		
		var constructedType = type as ConstructedTypeDefinition;
		if (constructedType == null)
			return false;
		
		if (genericTypeDefinition != constructedType.genericTypeDefinition)
			return false;
		
		for (var i = 0; i < typeArguments.Length; ++i)
			if (!typeArguments[i].definition.IsSameType(constructedType.typeArguments[i].definition as TypeDefinitionBase))
				return false;
		
		return true;
	}

	protected override SymbolDefinition GetIndexer(TypeDefinitionBase[] argumentTypes)
	{
		var indexers = GetAllIndexers();

		// TODO: Resolve overloads

		return indexers != null ? indexers[indexers.Count - 1] : null;
	}

	public override List<SymbolDefinition> GetAllIndexers()
	{
		List<SymbolDefinition> indexers = genericTypeDefinition.GetAllIndexers();
		if (indexers != null)
		{
			for (var i = 0; i < indexers.Count; ++i)
			{
				var member = indexers[i];
				member = GetConstructedMember(member);
				indexers[i] = member;
			}
		}
		return indexers;
	}

	//public override bool IsGeneric
	//{
	//	get
	//	{
	//		return false;
	//	}
	//}

	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		var dataFromDefinition = new Dictionary<string,SymbolDefinition>();
		genericTypeDefinition.GetMembersCompletionData(dataFromDefinition, flags, mask, assembly);
		foreach (var entry in dataFromDefinition)
		{
			if (!data.ContainsKey(entry.Key))
			{
				var member = GetConstructedMember(entry.Value);
				data.Add(entry.Key, member);
			}
		}

	//	base.GetMembersCompletionData(data, flags, mask, assembly);

		// TODO: Is this really needed?
	//	if (BaseType() != null && (kind != SymbolKind.Enum || flags != BindingFlags.Static))
	//		BaseType().GetMembersCompletionData(data, flags, mask & ~AccessLevelMask.Private, assembly);
	}
}

public class ConstructedSymbolReference : SymbolDefinition
{
	public readonly SymbolDefinition referencedSymbol;

	public ConstructedSymbolReference(SymbolDefinition referencedSymbolDefinition)
	{
		referencedSymbol = referencedSymbolDefinition;
		kind = referencedSymbol.kind;
		modifiers = referencedSymbol.modifiers;
		accessLevel = referencedSymbol.accessLevel;
		name = referencedSymbol.name;
	}

	public override TypeDefinitionBase TypeOfTypeParameter(TypeParameterDefinition tp)
	{
		var fromReferencedSymbol = referencedSymbol.TypeOfTypeParameter(tp);
		var asTypeParameter = fromReferencedSymbol as TypeParameterDefinition;
		if (asTypeParameter != null)
			return base.TypeOfTypeParameter(tp);
		else
			return fromReferencedSymbol;
	}

	public override TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	{
		return base.SubstituteTypeParameters(context);
	}

	public override SymbolDefinition TypeOf()
	{
		var result = referencedSymbol.TypeOf() as TypeDefinitionBase;
		
		var ctx = parentSymbol as ConstructedTypeDefinition;
		if (ctx != null && result != null)
			result = result.SubstituteTypeParameters(ctx); 

		return result;
	}
	
	public override SymbolDefinition GetGenericSymbol()
	{
		return referencedSymbol.GetGenericSymbol();
	}

	public override List<ParameterDefinition> GetParameters()
	{
		return referencedSymbol.GetParameters();
	}

	public override List<TypeParameterDefinition> GetTypeParameters()
	{
		return referencedSymbol.GetTypeParameters();
	}

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		if (asTypeOnly)
			return;

		var symbolType = TypeOf() as TypeDefinitionBase;
		if (symbolType != null)
			symbolType.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
	}

	public override SymbolDefinition ResolveMethodOverloads(ParseTree.Node argumentListNode, SymbolReference[] typeArgs, Scope scope)
	{
		if (kind != SymbolKind.MethodGroup)
			return null;
		var genericMethod = ((MethodGroupDefinition) referencedSymbol).ResolveMethodOverloads(argumentListNode, typeArgs, scope);
		if (genericMethod == null || genericMethod.kind != SymbolKind.Method)
			return null;
		return ((ConstructedTypeDefinition) parentSymbol).GetConstructedMember(genericMethod);
	}
	
	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		var symbolType = TypeOf();
		if (symbolType != null)
			symbolType.GetMembersCompletionData(data, BindingFlags.Instance, mask, assembly);
	}
}

public class ArrayTypeDefinition : TypeDefinition
{
	public readonly TypeDefinitionBase elementType;
	public readonly int rank;

	private List<SymbolReference> arrayGenericInterfaces;

	public ArrayTypeDefinition(TypeDefinitionBase elementType, int rank)
	{
		kind = SymbolKind.Class;
		this.elementType = elementType;
		this.rank = rank;
		name = elementType.GetName() + RankString();
	}

	public override TypeDefinitionBase BaseType()
	{
		if (arrayGenericInterfaces == null && rank == 1)
			Interfaces();
		return builtInTypes_Array;
	}

	public override List<SymbolReference> Interfaces()
	{
		if (arrayGenericInterfaces == null && rank == 1)
		{
			arrayGenericInterfaces = new List<SymbolReference> {
				ReflectedTypeReference.ForType(typeof(IEnumerable<>)),
				ReflectedTypeReference.ForType(typeof(IList<>)),
				ReflectedTypeReference.ForType(typeof(ICollection<>)),
			};

			var typeArguments = new []{ new SymbolReference(elementType) };
			for (var i = 0; i < arrayGenericInterfaces.Count; ++i)
			{
				var genericInterface = arrayGenericInterfaces[i].definition as TypeDefinition;
				genericInterface = genericInterface.ConstructType(typeArguments);
				arrayGenericInterfaces[i] = new SymbolReference(genericInterface);
			}
		}
		interfaces = arrayGenericInterfaces ?? base.Interfaces();
		return interfaces;
	}

	public override TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	{
		var constructedElement = elementType.SubstituteTypeParameters(context);
		if (constructedElement != elementType)
			return constructedElement.MakeArrayType(rank);

		return base.SubstituteTypeParameters(context);
	}

	protected override string RankString()
	{
		return '[' + new string(',', rank - 1) + ']';
	}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters, bool asTypeOnly)
	{
		symbolName = DecodeId(symbolName);
		
		var result = base.FindName(symbolName, numTypeParameters, asTypeOnly);
//		if (result == null && BaseType() != null)
//		{
//			//	Debug.Log("Symbol lookup '" + symbolName +"' in base " + baseType.definition);
//			result = BaseType().FindName(symbolName, numTypeParameters, asTypeOnly);
//		}
		return result;
	}

	public override string GetTooltipText()
	{
//		if (tooltipText != null)
//			return tooltipText;

		if (elementType == null)
			return "array of unknown type";

		if (parentSymbol != null && !string.IsNullOrEmpty(parentSymbol.GetName()))
			tooltipText = parentSymbol.GetName() + "." + elementType.GetName() + RankString();
		tooltipText = elementType.GetName() + RankString();

		var xmlDocs = GetXmlDocs();
		if (!string.IsNullOrEmpty(xmlDocs))
		{
			tooltipText += "\n\n" + xmlDocs;
		}

		return tooltipText;
	}

	public override bool CanConvertTo(TypeDefinitionBase otherType)
	{
		var asArrayType = otherType as ArrayTypeDefinition;
		if (asArrayType != null)
		{
			if (rank != asArrayType.rank)
				return false;
			return elementType.CanConvertTo(asArrayType.elementType);
		}

		if (rank == 1 && otherType.kind == SymbolKind.Interface)
		{
			var genericInterfaces = Interfaces();
			for (var i = 0; i < genericInterfaces.Count; ++i)
			{
				var type = genericInterfaces[i].definition as TypeDefinitionBase;
				if (type != null && type.CanConvertTo(otherType))
					return true;
			}
		}

		return base.CanConvertTo(otherType);
	}

	public override TypeDefinitionBase ConvertTo(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return null;

		if (otherType is TypeParameterDefinition)
			return this;

		var asArrayType = otherType as ArrayTypeDefinition;
		if (asArrayType != null)
		{
			if (rank != asArrayType.rank)
				return null;

			var convertedElementType = elementType.ConvertTo(asArrayType.elementType);
			if (convertedElementType == null)
				return null;

			if (convertedElementType == elementType)
				return this;

			return convertedElementType.MakeArrayType(rank);
		}

		if (rank == 1 && otherType.kind == SymbolKind.Interface)
		{
			var genericInterfaces = Interfaces();
			for (var i = 0; i < genericInterfaces.Count; ++i)
			{
				var interfaceType = genericInterfaces[i].definition as TypeDefinitionBase;
				var constructedInterface = interfaceType.ConvertTo(otherType);
				if (constructedInterface != null)
					return constructedInterface;
			}
		}

		return base.ConvertTo(otherType);
	}

	internal override TypeDefinitionBase BindTypeArgument(TypeDefinitionBase typeArgument, TypeDefinitionBase argumentType)
	{
		var argumentAsArray = argumentType as ArrayTypeDefinition;
		if (argumentAsArray != null && argumentAsArray.rank == rank)
		{
			var boundElementType = elementType.BindTypeArgument(typeArgument, argumentAsArray.elementType);
			if (boundElementType != null)
				return boundElementType;
		}
		return base.BindTypeArgument(typeArgument, argumentType);
	}
}

public class TypeDefinition : TypeDefinitionBase
{
	protected SymbolReference baseType;
	protected List<SymbolReference> interfaces;
	
	public List<TypeParameterDefinition> typeParameters;
	//public SymbolReference[] tempTypeArguments;

	private Dictionary<string, ConstructedTypeDefinition> constructedTypes;
	public virtual ConstructedTypeDefinition ConstructType(SymbolReference[] typeArgs)
	{
		var delimiter = string.Empty;
		var sb = new StringBuilder();
		if (typeArgs != null)
		{
			foreach (var arg in typeArgs)
			{
				sb.Append(delimiter);
				sb.Append(arg.ToString());
				delimiter = ", ";
			}
		}
		var sig = sb.ToString();

		if (constructedTypes == null)
			constructedTypes = new Dictionary<string, ConstructedTypeDefinition>();

		ConstructedTypeDefinition result;
//		if (constructedTypes.TryGetValue(sig, out result))
//		{
//			if (result.IsValid())
//			{
//				return result;
//			}
//		}

		result = new ConstructedTypeDefinition(this, typeArgs);
		constructedTypes[sig] = result;
		return result;
	}

	public override SymbolDefinition TypeOf()
	{
		return this;
	}
	
	public override void InvalidateBaseType()
	{
		baseType = null;
		interfaces = null;
		++ParseTree.resolverVersion;
		if (ParseTree.resolverVersion == 0)
			++ParseTree.resolverVersion;
	}

	public override List<SymbolReference> Interfaces()
	{
		if (interfaces == null)
			BaseType();
		return interfaces;
	}
	
	private bool resolvingBaseType = false;
	public override TypeDefinitionBase BaseType()
	{
		if (resolvingBaseType)
			return null;
		resolvingBaseType = true;
		
		if (baseType != null && (baseType.definition == null || !baseType.definition.IsValid()) ||
			interfaces != null && interfaces.Exists(x => x.definition != null && !x.definition.IsValid()))
		{
			baseType = null;
			interfaces = null;
		}

		if (baseType == null && interfaces == null)
		{
			interfaces = new List<SymbolReference>();
			
			ParseTree.Node baseNode = null;
			ParseTree.Node interfaceListNode = null;
			SymbolDeclaration decl = null;
			if (declarations != null)
			{
				foreach (var d in declarations)
				{
					if (d != null)
					{
						baseNode = (ParseTree.Node) d.parseTreeNode.FindChildByName(
							d.kind == SymbolKind.Class ? "classBase" :
							d.kind == SymbolKind.Struct ? "structInterfaces" :
							"interfaceBase");
						interfaceListNode = baseNode != null ? baseNode.NodeAt(1) : null;
						
						if (baseNode != null)
						{
							decl = d;
							break;
						}
					}
				}
			}
			
			if (decl != null)
			{
				switch (decl.kind)
				{
					case SymbolKind.Class:
						if (interfaceListNode != null)
						{
							baseType = new SymbolReference(interfaceListNode.ChildAt(0));
							if (baseType.definition.kind == SymbolKind.Interface)
							{
								interfaces.Add(baseType);
								baseType = this != builtInTypes_object ? ReflectedTypeReference.ForType(typeof(object)) : null;
							}
	
							for (var i = 2; i < interfaceListNode.numValidNodes; i += 2)
								interfaces.Add(new SymbolReference(interfaceListNode.ChildAt(i)));
						}
						else
						{
							baseType = this != builtInTypes_object ? ReflectedTypeReference.ForType(typeof(object)) : null;
						}
						break;
	
					case SymbolKind.Struct:
					case SymbolKind.Interface:
						baseType = decl.kind == SymbolKind.Struct ?
							ReflectedTypeReference.ForType(typeof(ValueType)) :
							ReflectedTypeReference.ForType(typeof(object));
						if (interfaceListNode != null)
						{
							for (var i = 0; i < interfaceListNode.numValidNodes; i += 2)
								interfaces.Add(new SymbolReference(interfaceListNode.ChildAt(i)));
						}
						break;
	
					case SymbolKind.Enum:
						baseType = ReflectedTypeReference.ForType(typeof(Enum));
						break;
	
					case SymbolKind.Delegate:
						baseType = ReflectedTypeReference.ForType(typeof(MulticastDelegate));
						break;
				}
			}
			//Debug.Log("BaseType() of " + this + " is " + (baseType != null ? baseType.definition.ToString() : "null"));
		}
		
		var result = baseType != null ? baseType.definition as TypeDefinitionBase : base.BaseType();
		if (result == this)
		{
			baseType = new SymbolReference(circularBaseType);
			result = circularBaseType;
		}
		resolvingBaseType = false;
		return result;
	}

	public override TypeDefinitionBase ConvertTo(TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return null;

		if (otherType is TypeParameterDefinition)
			return this;

		var otherTypeAsConstructed = otherType as ConstructedTypeDefinition;
		if (otherTypeAsConstructed != null)
			otherType = otherTypeAsConstructed.genericTypeDefinition;

		if (this == otherType)
			return this;

		if (convertingToBase)
			return null;
		convertingToBase = true;

		var baseTypeDefinition = BaseType();

		if (interfaces != null && otherType.kind == SymbolKind.Interface)
		{
			for (var i = 0; i < interfaces.Count; ++i)
			{
				var interfaceDefinition = interfaces[i].definition as TypeDefinitionBase;
				if (interfaceDefinition != null)
				{
					var convertedInterface = interfaceDefinition.ConvertTo(otherType);
					if (convertedInterface != null)
					{
						convertingToBase = false;
						return convertedInterface;
					}
				}
			}
		}

		if (baseTypeDefinition != null)
		{
			var convertedBase = baseTypeDefinition.ConvertTo(otherType);
			convertingToBase = false;
			return convertedBase;
		}

		convertingToBase = false;
		return null;
	}
	
	private bool checkingDerivesFromBase;
	public override bool DerivesFromRef(ref TypeDefinitionBase otherType)
	{
		if (otherType == null)
			return false;
		
		var otherTypeAsConstructed = otherType as ConstructedTypeDefinition;
		if (otherTypeAsConstructed != null)
			otherType = otherTypeAsConstructed.genericTypeDefinition;

		if (this == otherType)
			return true;

		if (interfaces == null)
			BaseType();
		
		if (checkingDerivesFromBase)
			return false;
		checkingDerivesFromBase = true;
		
		if (interfaces != null)
			for (var i = 0; i < interfaces.Count; ++i)
			{
				var typeDefinition = interfaces[i].definition as TypeDefinitionBase;
				if (typeDefinition != null && typeDefinition.DerivesFromRef(ref otherType))
				{
					checkingDerivesFromBase = false;
					return true;
				}
			}

		if (BaseType() != null)
		{
			var result = BaseType().DerivesFromRef(ref otherType);
			checkingDerivesFromBase = false;
			return result;
		}
		
		checkingDerivesFromBase = false;
		return false;
	}

	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind != SymbolKind.TypeParameter)
			return base.AddDeclaration(symbol);

		var symbolName = symbol.ReflectionName;// symbol.Name;
		if (typeParameters == null)
			typeParameters = new List<TypeParameterDefinition>();
		var definition = typeParameters.Find(x => x.name == symbolName);
		if (definition == null)
		{
			definition = (TypeParameterDefinition) Create(symbol);
			definition.parentSymbol = this;
			typeParameters.Add(definition);
		}

		symbol.definition = definition;

		var nameNode = symbol.NameNode();
		if (nameNode != null)
		{
			var leaf = nameNode as ParseTree.Leaf;
			if (leaf != null)
				leaf.SetDeclaredSymbol(definition);
			else
			{
				// TODO: Remove this block?
				var lastLeaf = ((ParseTree.Node) nameNode).GetLastLeaf();
				if (lastLeaf != null)
				{
					if (lastLeaf.parent.RuleName == "typeParameterList")
						lastLeaf = lastLeaf.parent.parent.LeafAt(0);
					lastLeaf.SetDeclaredSymbol(definition);
				}
			}
		}
		
		//// this.ReflectionName has changed
		//parentSymbol.members.Remove(this);
		//parentSymbol.members[ReflectionName] = this;

		return definition;
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.TypeParameter && typeParameters != null)
		{
			if (typeParameters.Remove(symbol.definition as TypeParameterDefinition))
			{
				//// this.ReflectionName has changed
				//parentSymbol.members.Remove(this);
				//parentSymbol.members[ReflectionName] = this;
			}
		}

		base.RemoveDeclaration (symbol);
	}

	public override SymbolDefinition FindName(string memberName, int numTypeParameters, bool asTypeOnly)
	{
		memberName = DecodeId(memberName);
		
		if (numTypeParameters == 0 && typeParameters != null)
		{
			for (var i = typeParameters.Count; i --> 0; )
				if (typeParameters[i].name == memberName)
					return typeParameters[i];
		}
		
		var member = base.FindName(memberName, numTypeParameters, asTypeOnly);
		return member;
	}

	public override List<TypeParameterDefinition> GetTypeParameters()
	{
		return typeParameters;
	}

	public override string GetTooltipText()
	{
		if (kind == SymbolKind.Delegate)
			return base.GetTooltipText();

	//	if (tooltipText != null)
	//		return tooltipText;

		var parentSD = parentSymbol;
		if (parentSD != null && !string.IsNullOrEmpty(parentSD.GetName()))
			tooltipText = kind.ToString().ToLowerInvariant() + " " + parentSD.GetName() + "." + name;
		else
			tooltipText = kind.ToString().ToLowerInvariant() + " " + name;

		if (typeParameters != null)
		{
			tooltipText += "<" + TypeOfTypeParameter(typeParameters[0]).GetName();
			for (var i = 1; i < typeParameters.Count; ++i)
				tooltipText += ", " + TypeOfTypeParameter(typeParameters[i]).GetName();
			tooltipText += '>';
		}

		var xmlDocs = GetXmlDocs();
		if (!string.IsNullOrEmpty(xmlDocs))
		{
		    tooltipText += "\n\n" + xmlDocs;
		}

		return tooltipText;
	}

	public override TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	{
		if (typeParameters == null)
			return base.SubstituteTypeParameters(context);
		
		var constructType = false;
		var typeArguments = new SymbolReference[typeParameters.Count];
		for (var i = 0; i < typeArguments.Length; ++i)
		{
			typeArguments[i] = new SymbolReference(typeParameters[i]);
			var original = typeParameters[i];
			if (original == null)
				continue;
			var substitute = original.SubstituteTypeParameters(context);
			if (substitute != original)
			{
				typeArguments[i] = new SymbolReference(substitute);
				constructType = true;
			}
		}
		if (!constructType)
			return this;
		return ConstructType(typeArguments);
	}

	internal override TypeDefinitionBase BindTypeArgument(TypeDefinitionBase typeArgument, TypeDefinitionBase argumentType)
	{
		if (NumTypeParameters == 0)
			return base.BindTypeArgument(typeArgument, argumentType);
		
		TypeDefinitionBase convertedArgument = this;
		if (!argumentType.DerivesFromRef(ref convertedArgument))
			return base.BindTypeArgument(typeArgument, argumentType);
		
		var argumentAsConstructedType = convertedArgument as ConstructedTypeDefinition;
		if (argumentAsConstructedType != null && GetGenericSymbol() == argumentAsConstructedType.GetGenericSymbol())
		{
			TypeDefinitionBase inferedType = null;
			for (int i = 0; i < NumTypeParameters; ++i)
			{
				var fromConstructedType = argumentAsConstructedType.typeArguments[i].definition as TypeDefinitionBase;
				if (fromConstructedType != null)
				{
					var boundTypeArgument = typeParameters[i].BindTypeArgument(typeArgument, fromConstructedType);
					if (boundTypeArgument != null)
					{
						if (inferedType == null || inferedType.CanConvertTo(boundTypeArgument))
							inferedType = boundTypeArgument;
						else if (!boundTypeArgument.CanConvertTo(inferedType))
							return null;
					}
				}
			}
			
			if (inferedType != null)
				return inferedType;
		}
		return base.BindTypeArgument(typeArgument, argumentType);
	}
	
	//public override bool IsGeneric
	//{
	//	get
	//	{
	//		return typeParameters != null;
	//	}
	//}
}

public class MethodGroupDefinition : SymbolDefinition
{
	public static readonly MethodDefinition ambiguousMethodOverload = new MethodDefinition { kind = SymbolKind.Error, name = "ambiguous method overload" };
	public static readonly MethodDefinition unresolvedMethodOverload = new MethodDefinition { kind = SymbolKind.Error, name = "unresolved method overload" };

	public HashSet<MethodDefinition> methods = new HashSet<MethodDefinition>();
//	public int numTypeParameters;

	public virtual void AddMethod(MethodDefinition method)
	{
		methods.RemoveWhere((MethodDefinition x) => !x.IsValid());
		if (method.declarations != null)
		{
			var d = method.declarations[0];
			methods.RemoveWhere((MethodDefinition x) => x.declarations != null && x.declarations.Contains(d));
		}
		methods.Add(method);
		method.parentSymbol = this;
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		//Debug.Log("removing " + symbol.Name + " - " + (symbol.parseTreeNode.ChildAt(0) ?? symbol.parseTreeNode).Print());
		methods.RemoveWhere((MethodDefinition x) => x.declarations.Contains(symbol));
	}

	public SymbolDefinition ResolveParameterName(ParseTree.Leaf leaf)
	{
		foreach (var m in methods)
		{
			var p = m.GetParameters();
			var leafText = DecodeId(leaf.token.text);
			var x = p.Find(pd => pd.name == leafText);
			if (x != null)
				return leaf.resolvedSymbol = x;
		}
		return unknownSymbol;
	}
	
	public static int ProcessArgumentListNode(ParseTree.Node argumentListNode, out Modifiers[] modifiers, out List<TypeDefinitionBase> argumentTypes, TypeDefinitionBase extendedType)
	{
		var numArguments = argumentListNode == null ? 0 : (argumentListNode.numValidNodes + 1) / 2;
		var thisOffest = 0;
		if (extendedType != null)
		{
			thisOffest = 1;
			++numArguments;
		}
		
		modifiers = new Modifiers[numArguments];
		argumentTypes = new List<TypeDefinitionBase>();
		var resolvedArguments = new SymbolDefinition[numArguments];
		
		if (extendedType != null)
		{
			argumentTypes.Add(extendedType);
		}
		
		for (var i = thisOffest; i < numArguments; ++i)
		{
			var argumentNode = argumentListNode.NodeAt((i - thisOffest) * 2);
			if (argumentNode != null)
			{
				var argumentValueNode = argumentNode.FindChildByName("argumentValue") as ParseTree.Node;
				if (argumentValueNode != null)
				{
					resolvedArguments[i] = ResolveNode(argumentValueNode);
					if (resolvedArguments[i] != null)
						argumentTypes.Add(resolvedArguments[i].TypeOf() as TypeDefinitionBase ?? unknownType);
					else
						argumentTypes.Add(unknownType);
					
					var modifierLeaf = argumentValueNode.LeafAt(0);
					if (modifierLeaf != null)
					{
						if (modifierLeaf.IsLit("ref"))
							modifiers[i] = Modifiers.Ref;
						else if (modifierLeaf.IsLit("out"))
							modifiers[i] = Modifiers.Out;
					}
					
					continue;
				}
			}
			
			numArguments = i;
			break;
		}
		
		return numArguments;
	}

	public override SymbolDefinition ResolveMethodOverloads(ParseTree.Node argumentListNode, SymbolReference[] typeArgs, Scope scope)
	{
		Modifiers[] modifiers;
		List<TypeDefinitionBase> argumentTypes;
		
		ProcessArgumentListNode(argumentListNode, out modifiers, out argumentTypes, null);

		var resolved = ResolveMethodOverloads(argumentTypes, modifiers, scope);

		//if (resolved != null && resolved.kind != SymbolKind.Error)
		//{
		//	for (var i = 0; i < numArguments; ++i)
		//	{
		//		if (resolvedArguments[i] != null && resolvedArguments[i].kind == SymbolKind.LambdaExpression)
		//		{
		//			var inferedLambda = resolvedArguments[i];
		//		}
		//	}
		//}

		return resolved;
	}
	
	public List<MethodDefinition> CollectExtensionCandidates(List<TypeDefinitionBase> argumentTypes, Scope scope)
	{
		var accessLevelMask = AccessLevelMask.Public;
		var parentType = parentSymbol as TypeDefinitionBase ?? parentSymbol.parentSymbol as TypeDefinitionBase;
		var contextType = scope.EnclosingType();
		if (contextType != null)
		{
			if (parentType.Assembly != null && parentType.Assembly.InternalsVisibleIn(contextType.Assembly))
				accessLevelMask |= AccessLevelMask.Internal;
			
			if (contextType == parentType || parentType.IsSameOrParentOf(contextType))
				accessLevelMask |= AccessLevelMask.Public | AccessLevelMask.Protected | AccessLevelMask.Private;
			else if (contextType.DerivesFrom(parentType))
				accessLevelMask |= AccessLevelMask.Public | AccessLevelMask.Protected;
		}
		
		//var candidates = new HashSet<MethodDefinition>();
		var namespaceScope = scope.EnclosingNamespaceScope();
		while (namespaceScope != null)
		{
			//foreach (MethodDefinition method in namespaceScope.EnumerateExtensionMethods(name))
			//	if (method.IsAccessible(accessLevelMask) &&
			//		(argumentTypes == null || method.CanCallWithNArguments(argumentTypes.Count)))
			//			candidates.Add(method);
			namespaceScope = namespaceScope.EnclosingNamespaceScope();
		}
		
		return null;
	}

	public List<MethodDefinition> CollectCandidates(List<TypeDefinitionBase> argumentTypes, Modifiers[] modifiers, Scope scope)
	{
		var accessLevelMask = AccessLevelMask.Public;
		var parentType = parentSymbol as TypeDefinitionBase ?? parentSymbol.parentSymbol as TypeDefinitionBase;
		var contextType = scope.EnclosingType();
		if (contextType != null)
		{
			if (parentType.Assembly != null && parentType.Assembly.InternalsVisibleIn(contextType.Assembly))
				accessLevelMask |= AccessLevelMask.Internal;

			if (contextType == parentType || parentType.IsSameOrParentOf(contextType))
				accessLevelMask |= AccessLevelMask.Public | AccessLevelMask.Protected | AccessLevelMask.Private;
			else if (contextType.DerivesFrom(parentType))
				accessLevelMask |= AccessLevelMask.Public | AccessLevelMask.Protected;
		}

		var candidates = new List<MethodDefinition>();
		foreach (var method in methods)
			if (!method.IsOverride && method.IsAccessible(accessLevelMask) &&
				(argumentTypes == null || method.CanCallWith(modifiers, false)))
					candidates.Add(method);

		var thisAsConstructedMG = this as ConstructedMethodGroupDefinition;
		for (var i = candidates.Count; i-- > 0; )
		{
			var candidate = candidates[i];
			if (thisAsConstructedMG == null)
			{
				if (candidate.NumTypeParameters == 0 || argumentTypes == null)
					continue;

				candidate = InferMethodTypeArguments(candidate, argumentTypes);
				if (candidate == null)
					candidates.RemoveAt(i);
				else
					candidates[i] = candidate;
			}
			else
			{
				candidate = candidate.ConstructMethod(thisAsConstructedMG.typeArguments);
			}
		}

		if (candidates.Count != 0)
			return candidates;
		
		var baseType = (TypeDefinitionBase) parentSymbol;
		while ((baseType = baseType.BaseType()) != null)
		{
			var baseSymbol = baseType.FindName(name, 0, false) as MethodGroupDefinition;
			if (baseSymbol != null)
				return baseSymbol.CollectCandidates(argumentTypes, modifiers, scope);
		}
		return null;
	}

	private static List<int> GenerateRangeList(int to)
	{
		var list = new List<int>();
		for (var i = 0; i < to; ++i)
			list.Add(i);
		return list;
	}
	
	public static MethodDefinition InferMethodTypeArguments(MethodDefinition method, List<TypeDefinitionBase> argumentTypes)
	{
		var numTypeParameters = method.NumTypeParameters;
		List<TypeDefinitionBase> typeArgs = new List<TypeDefinitionBase>();
		foreach (var item in method.typeParameters)
			typeArgs.Add(item);
		
		//var typeArgsUpper = new TypeDefinitionBase[numTypeParameters];
		//var typeArgsLower = new TypeDefinitionBase[numTypeParameters];
		
		var parameters = method.GetParameters();
		var numParameters = Math.Min(parameters.Count, argumentTypes.Count);
		
		var openTypeArguments = GenerateRangeList(numTypeParameters);
		
		var stayInLoop = true;
		while (stayInLoop)
		{
			stayInLoop = false;
			for (var i = openTypeArguments.Count; i --> 0; )
			{
				var typeArgIndex = openTypeArguments[i];
				var typeArgument = typeArgs[typeArgIndex];

				for (var j = numParameters; j --> 0; )
				{
					var argumentType = argumentTypes[j];
					if (argumentType == null)
						continue;
					
					var parameter = parameters[j]; //TODO: Consider expanded params parameter and all arguments
					var parameterType = parameter.TypeOf() as TypeDefinitionBase;
					//TODO: Have to substitute already infered types in the parameter type
					
					if (parameterType != null && parameterType.IsValid())
					{
						var boundType = parameterType.BindTypeArgument(typeArgument, argumentType);
						if (boundType != null && boundType != typeArgument)
						{
							typeArgs[typeArgIndex] = boundType;
							openTypeArguments.RemoveAt(i);
							stayInLoop = openTypeArguments.Count > 0;
							
							//TODO: Should actually use the lower and upper bounds
							break;
						}
					}
				}
			}
		}
		
		var typeArgRefs = new SymbolReference[numTypeParameters];
		for (var i = 0; i < numTypeParameters; ++i)
			typeArgRefs[i] = new SymbolReference(typeArgs[i] ?? builtInTypes_object);
		method = method.ConstructMethod(typeArgRefs);
		return method;
	}

	public virtual MethodDefinition ResolveMethodOverloads(List<TypeDefinitionBase> argumentTypes, Modifiers[] modifiers, Scope scope)
	{
		var candidates = CollectCandidates(argumentTypes, modifiers, scope);
		if (candidates == null)
			return unresolvedMethodOverload;

	//	if (candidates.Count == 1)
	//		return candidates[0];
		
		return ResolveMethodOverloads(argumentTypes.Count, argumentTypes, modifiers, candidates);
	}
	
	public static MethodDefinition ResolveMethodOverloads(int numArguments, List<TypeDefinitionBase> argumentTypes, Modifiers[] modifiers, List<MethodDefinition> candidates)
	{
		// find best match
		MethodDefinition bestMatch = null;
		var bestExactMatches = -1;
		foreach (var method in candidates)
		{
			var parameters = method.GetParameters();
			var expandParams = true;

		tryNotExpanding:

			var exactMatches = 0;
			ParameterDefinition paramsArray = null;
			for (var i = 0; i < UnityEngine.Mathf.Min(numArguments, parameters.Count); ++i)
			{
				if (argumentTypes[i] == null)
				{
					exactMatches = -1;
					break;
				}
				
				if (expandParams && paramsArray == null && parameters[i].IsParametersArray)
					paramsArray = parameters[i];
					
				TypeDefinitionBase parameterType = null;
				if (paramsArray != null)
				{
					var arrayType = paramsArray.TypeOf() as ArrayTypeDefinition;
					if (arrayType != null)
						parameterType = arrayType.elementType;
				}
				else
				{
					if (i >= parameters.Count)
					{
						exactMatches = -1;
						break;
					}
					parameterType = parameters[i].TypeOf() as TypeDefinitionBase;
				}
				parameterType = parameterType.SubstituteTypeParameters(method);

				if (argumentTypes[i].IsSameType(parameterType))
				{
					++exactMatches;
					continue;
				}
				if (!argumentTypes[i].CanConvertTo(parameterType))
				{
					exactMatches = -1;
					break;
				}
			}
			if (exactMatches < 0)
			{
				if (paramsArray == null)
					continue;
				
				expandParams = false;
				paramsArray = null;
				goto tryNotExpanding;
			}
			if (exactMatches > bestExactMatches)
			{
				bestExactMatches = exactMatches;
				bestMatch = method;
			}
			else if (exactMatches == bestExactMatches)
			{
				if (method.NumTypeParameters == 0 && bestMatch.NumTypeParameters > 0)
				{
					bestMatch = method;
				}
			}
		}

		if (bestMatch != null)
			return bestMatch;
		if (candidates.Count <= 1)
			return unresolvedMethodOverload;
		return ambiguousMethodOverload;// candidates[0];
	}

	public override bool IsAccessible(AccessLevelMask accessLevelMask)
	{
		foreach (var method in methods)
			if (method.IsAccessible(accessLevelMask))
				return true;
		return false;
	}

	private Dictionary<string, ConstructedMethodGroupDefinition> constructedMethodGroups;
	public ConstructedMethodGroupDefinition ConstructMethodGroup(SymbolReference[] typeArgs)
	{
		var delimiter = string.Empty;
		var sb = new StringBuilder();
		if (typeArgs != null)
		{
			foreach (var arg in typeArgs)
			{
				sb.Append(delimiter);
				sb.Append(arg.ToString());
				delimiter = ", ";
			}
		}
		var sig = sb.ToString();

		if (constructedMethodGroups == null)
			constructedMethodGroups = new Dictionary<string, ConstructedMethodGroupDefinition>();

		ConstructedMethodGroupDefinition result;
		if (constructedMethodGroups.TryGetValue(sig, out result))
		{
			if (result.IsValid() && result.typeArguments != null && result.methods.Count == methods.Count)
			{
				if (result.typeArguments.All(x => x.definition != null && x.definition.kind != SymbolKind.Error && x.definition.IsValid()))
				{
					result.methods.RemoveWhere(x =>
						!x.IsValid() ||
						!methods.Contains(((ConstructedMethodDefinition)x).genericMethodDefinition));

					if (methods.Count == result.methods.Count)
						return result;
				}
			}
		}

		if (result != null)
		{
			foreach (var method in result.methods)
				method.parentSymbol = null;
			result.parentSymbol = null;
		}

		result = new ConstructedMethodGroupDefinition(this, typeArgs);
		constructedMethodGroups[sig] = result;
		return result;
	}
}

public class ConstructedMethodGroupDefinition : MethodGroupDefinition
{
	public readonly MethodGroupDefinition genericMethodGroupDefinition;
	public readonly SymbolReference[] typeArguments;

	public override SymbolDefinition GetGenericSymbol()
	{
		return genericMethodGroupDefinition;
	}

	public ConstructedMethodGroupDefinition(MethodGroupDefinition definition, SymbolReference[] arguments)
	{
		name = definition.name;
		kind = definition.kind;
		parentSymbol = definition.parentSymbol;
		genericMethodGroupDefinition = definition;
		modifiers = definition.modifiers;
		//numTypeParameters = definition.numTypeParameters;
		
		typeArguments = arguments;
		//if (arguments != null)
		//{
		//	typeArguments = new SymbolReference[arguments.Length];
		//	for (var i = 0; i < typeArguments.Length; ++i)
		//		typeArguments[i] = new SymbolReference(arguments[i].definition);
		//}
	}

	public override MethodDefinition ResolveMethodOverloads(List<TypeDefinitionBase> argumentTypes, Modifiers[] modifiers, Scope scope)
	{
		if (methods.Count == 0)
		{
			foreach (var m in genericMethodGroupDefinition.methods)
			{
				var constructedMethod = m.ConstructMethod(typeArguments);
				if (constructedMethod != null)
					methods.Add(constructedMethod);
			}
		}
		return base.ResolveMethodOverloads(argumentTypes, modifiers, scope);
	}
	
	public override void AddMethod(MethodDefinition method)
	{
		Debug.LogError("AddMethod on ConstructedMethodGroupDefinition: " + method);
	}
}

public class ConstructedMethodDefinition : MethodDefinition
{
	public readonly MethodDefinition genericMethodDefinition;
	public readonly SymbolReference[] typeArguments;
	
	public override bool IsExtensionMethod {
		get { return genericMethodDefinition.IsExtensionMethod; }
	}

	public override SymbolDefinition GetGenericSymbol()
	{
		return genericMethodDefinition;
	}

	public ConstructedMethodDefinition(MethodDefinition definition, SymbolReference[] arguments)
	{
		name = definition.name;
		kind = definition.kind;
		parentSymbol = definition.parentSymbol;
		genericMethodDefinition = definition;
		parameters = genericMethodDefinition.parameters;
		modifiers = genericMethodDefinition.modifiers;

		if (definition.typeParameters != null && arguments != null)
		{
			typeParameters = definition.typeParameters;
			typeArguments = new SymbolReference[typeParameters.Count];
			for (var i = 0; i < typeArguments.Length; ++i)
				typeArguments[i] = i < arguments.Length ? arguments[i] : new SymbolReference(unknownType);
		}
	}

	public override TypeDefinitionBase TypeOfTypeParameter(TypeParameterDefinition tp)
	{
		if (typeParameters != null)
		{
			var index = typeParameters.IndexOf(tp);
			if (index >= 0)
				return typeArguments[index].definition as TypeDefinitionBase ?? tp;
		}
		return base.TypeOfTypeParameter(tp);
	}

	public override TypeDefinitionBase ReturnType()
	{
 		var result = genericMethodDefinition.ReturnType();
		result = result.SubstituteTypeParameters(this);
		return result;
	}

	public override string GetName()
	{
		var typeParameters = GetTypeParameters();
		if (typeParameters == null || typeParameters.Count == 0)
			return name;

		var sb = new StringBuilder();
		sb.Append(name);
		sb.Append('<');
		sb.Append(TypeOfTypeParameter(typeParameters[0]).GetName());
		for (var i = 1; i < typeParameters.Count; ++i)
		{
			sb.Append(", ");
			sb.Append(TypeOfTypeParameter(typeParameters[i]).GetName());
		}
		sb.Append('>');
		return sb.ToString();
	}
}

public abstract class InvokeableSymbolDefinition : SymbolDefinition
{
	public abstract TypeDefinitionBase ReturnType();

	protected SymbolReference returnType;
	public List<ParameterDefinition> parameters;
	public List<TypeParameterDefinition> typeParameters;
	
	public SymbolDefinition AddTypeParameter(SymbolDeclaration symbol)
	{
		var symbolName = symbol.Name;
		if (typeParameters == null)
			typeParameters = new List<TypeParameterDefinition>();
		var definition = typeParameters.Find(x => x.name == symbolName);
		if (definition == null)
		{
			definition = (TypeParameterDefinition)Create(symbol);
			definition.parentSymbol = this;
			typeParameters.Add(definition);
		}
		
		symbol.definition = definition;
		
		var nameNode = symbol.NameNode();
		if (nameNode != null)
		{
			var leaf = nameNode as ParseTree.Leaf;
			if (leaf != null)
				leaf.SetDeclaredSymbol(definition);
			else
			{
				var lastLeaf = ((ParseTree.Node)nameNode).GetLastLeaf();
				if (lastLeaf != null)
				{
					if (lastLeaf.parent.RuleName == "typeParameterList")
						lastLeaf = lastLeaf.parent.parent.LeafAt(0);
					lastLeaf.SetDeclaredSymbol(definition);
				}
			}
		}
		
		return definition;
	}
	
	public bool CanCallWith(Modifiers[] modifiers, bool asExtensionMethod)
	{
		var numArguments = modifiers.Length;

		var minArgs = asExtensionMethod ? 1 : 0;
		var maxArgs = minArgs;
		if (parameters != null)
		{
			for (var i = 0; i < parameters.Count; ++i)
			{
				var param = parameters[i];

				if (i < numArguments)
				{
					var passedWithOut = modifiers[i] == Modifiers.Out;
					var passedWithRef = modifiers[i] == Modifiers.Ref;
					if (param.IsOut != passedWithOut || param.IsRef != passedWithRef)
						return false;
				}

				if (!asExtensionMethod || !param.IsThisParameter)
				{
					if (param.IsParametersArray)
						maxArgs = 100000;
					else if (!param.IsOptional)
						++minArgs;
					++maxArgs;
				}
			}
		}
		return !(numArguments < minArgs || numArguments > maxArgs);
	}

	public override SymbolDefinition TypeOf()
	{
		return ReturnType();
	}
	
	public override List<ParameterDefinition> GetParameters()
	{
		return parameters ?? _emptyParameterList;
	}

	public override List<TypeParameterDefinition> GetTypeParameters()
	{
		return typeParameters;
	}

	public SymbolDefinition AddParameter(SymbolDeclaration symbol)
	{
		var symbolName = symbol.Name;
		var parameter = (ParameterDefinition) Create(symbol);
		parameter.type = new SymbolReference(symbol.parseTreeNode.FindChildByName("type"));
		parameter.parentSymbol = this;
		var lastNode = symbol.parseTreeNode.NodeAt(-1);
		if (lastNode != null && lastNode.RuleName == "defaultArgument")
		{
			var defaultValueNode = lastNode.NodeAt(1);
			if (defaultValueNode != null)
				parameter.defaultValue = defaultValueNode.Print();
		}
		if (!string.IsNullOrEmpty(symbolName))
		{
			if (parameters == null)
				parameters = new List<ParameterDefinition>();
			parameters.Add(parameter);
		}
		return parameter;
	}
	
	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.Parameter)
		{
			SymbolDefinition definition = AddParameter(symbol);
			//	if (!members.TryGetValue(symbolName, out definition) || definition is ReflectedMember || definition is ReflectedType)
			//		definition = AddMember(symbol);

			symbol.definition = definition;
			return definition;
		}
		else if (symbol.kind == SymbolKind.TypeParameter)
		{
			SymbolDefinition definition = AddTypeParameter(symbol);
			return definition;
		}

		return base.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.Parameter && parameters != null)
			parameters.Remove((ParameterDefinition) symbol.definition);
		else if (symbol.kind == SymbolKind.TypeParameter && typeParameters != null)
			typeParameters.Remove((TypeParameterDefinition) symbol.definition);
		else
			base.RemoveDeclaration(symbol);
	}

	public override SymbolDefinition FindName(string memberName, int numTypeParameters, bool asTypeOnly)
	{
		memberName = DecodeId(memberName);

		if (!asTypeOnly && numTypeParameters == 0 && parameters != null)
		{
			var definition = parameters.Find(x => x.name == memberName);
			if (definition != null)
				return definition;
		}
		else
		{
			if (typeParameters != null)
			{
				var definition = typeParameters.Find(x => x.name == memberName);
				if (definition != null)
					return definition;
			}
		}
		return base.FindName(memberName, numTypeParameters, asTypeOnly);
	}

	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		if (asTypeOnly)
			return;

		if (numTypeArgs == 0)
		{
			var leafText = DecodeId(leaf.token.text);

			if (parameters != null)
			{
				for (var i = parameters.Count; i --> 0; )
				{
					if (parameters[i].name == leafText)
					{
						leaf.resolvedSymbol = parameters[i];
						return;
					}
				}
			}
			
			if (typeParameters != null)
			{
				for (var i = typeParameters.Count; i --> 0; )
				{
					if (typeParameters[i].name == leafText)
					{
						leaf.resolvedSymbol = typeParameters[i];
						return;
					}
				}
			}
		}
		
		base.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
	}

	//public override string GetTooltipText()
	//{
	//    if (tooltipText != null)
	//        return tooltipText;

	//    var parentSD = parentSymbol;
	//    if (parentSD != null && !string.IsNullOrEmpty(parentSD.GetName()))
	//        tooltipText = kind.ToString().ToLowerInvariant() + " " + parentSD.GetName() + "." + name;
	//    else
	//        tooltipText = kind.ToString().ToLowerInvariant() + " " + name;

	//    var typeOf = TypeOf();
	//    var typeName = "";
	//    if (typeOf != null && kind != SymbolKind.Constructor && kind != SymbolKind.Destructor)
	//    {
	//        //var tp = typeOf as TypeParameterDefinition;
	//        //if (tp != null)
	//        //    typeOf = TypeOfTypeParameter(tp);
	//        var ctx = parentSymbol as ConstructedTypeDefinition;
	//        if (ctx != null)
	//            typeOf = ((TypeDefinitionBase) typeOf).SubstituteTypeParameters(ctx);
	//        typeName = typeOf.GetName() + " ";

	//        if (typeOf.kind != SymbolKind.TypeParameter)
	//            for (var parentType = typeOf.parentSymbol as TypeDefinitionBase; parentType != null; parentType = parentType.parentSymbol as TypeDefinitionBase)
	//                typeName = parentType.GetName() + '.' + typeName;
	//    }

	//    var parentText = string.Empty;
	//    var parent = parentSymbol is MethodGroupDefinition ? parentSymbol.parentSymbol : parentSymbol;
	//    if ((parent is TypeDefinitionBase && parent.kind != SymbolKind.Delegate && kind != SymbolKind.TypeParameter)
	//        || parent is NamespaceDefinition
	//        )//|| kind == SymbolKind.Accessor)
	//    {
	//        var parentName = parent.GetName();
	//        if (kind == SymbolKind.Constructor)
	//        {
	//            var typeParent = parent.parentSymbol as TypeDefinitionBase;
	//            parentName = typeParent != null ? typeParent.GetName() : null;
	//        }
	//        if (!string.IsNullOrEmpty(parentName))
	//            parentText = parentName + ".";
	//    }

	//    var nameText = name;

	//    List<ParameterDefinition> parameters = GetParameters();
	//    var parametersText = string.Empty;
	//    string parametersEnd = null;

	//    if (kind == SymbolKind.Method)
	//    {
	//        nameText += '(';
	//        //parameters = ((MethodDefinition) this).parameters;
	//        parametersEnd = ")";
	//    }
	//    else if (kind == SymbolKind.Constructor)
	//    {
	//        nameText = parent.name + '(';
	//        //parameters = ((MethodDefinition) this).parameters;
	//        parametersEnd = ")";
	//    }
	//    else if (kind == SymbolKind.Destructor)
	//    {
	//        nameText = "~" + parent.name + "()";
	//    }
	//    else if (kind == SymbolKind.Indexer)
	//    {
	//        nameText = "this[";
	//        //parameters = ((IndexerDefinition) this).parameters;
	//        parametersEnd = "]";
	//    }
	//    else if (kind == SymbolKind.Delegate)
	//    {
	//        nameText += '(';
	//        //parameters = ((DelegateTypeDefinition) this).parameters;
	//        parametersEnd = ")";
	//    }

	//    if (parameters != null)
	//    {
	//        parametersText = PrintParameters(parameters);
	//    }

	//    tooltipText = kindText + typeName + parentText + nameText + parametersText + parametersEnd;

	//    if (typeOf != null && typeOf.kind == SymbolKind.Delegate)
	//    {
	//        tooltipText += "\n\nDelegate info\n";
	//        tooltipText += typeOf.GetDelegateInfoText();
	//    }

	//    return tooltipText;
	//}

	//public override bool IsGeneric
	//{
	//	get
	//	{
	//		if (ReturnType().IsGeneric)
	//			return true;
	//		var numParams = parameters == null ? 0 : parameters.Count;
	//		for (var i = 0; i < numParams; ++i)
	//			if (parameters[i].TypeOf().IsGeneric)
	//				return true;
	//		return false;
	//	}
	//}
}

public class MethodDefinition : InvokeableSymbolDefinition
{
	protected bool isExtensionMethod;
	public override bool IsExtensionMethod {
		get { return isExtensionMethod; }
	}

	public MethodDefinition()
	{
		kind = SymbolKind.Method;
	}
	
	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		var result = base.AddDeclaration(symbol);
		
		if (IsStatic && result.kind == SymbolKind.Parameter && result.modifiers == Modifiers.This &&
			symbol.parseTreeNode != null && symbol.parseTreeNode.parent != null && symbol.parseTreeNode.parent.childIndex == 0)
		{
			var parentType = (parentSymbol.kind == SymbolKind.MethodGroup ? parentSymbol.parentSymbol : parentSymbol) as TypeDefinitionBase;
			if (parentType.kind == SymbolKind.Class && parentType.IsStatic && parentType.NumTypeParameters == 0)
			{
				var namespaceDefinition = parentType.parentSymbol;
				if (namespaceDefinition is NamespaceDefinition)
				{
					isExtensionMethod = true;
					++parentType.numExtensionMethods;
				}
			}
		}
		
		return result;
	}
	
	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (IsExtensionMethod && symbol.kind == SymbolKind.Parameter && symbol.definition.modifiers == Modifiers.This &&
			(symbol.parseTreeNode == null || symbol.parseTreeNode.parent == null || symbol.parseTreeNode.parent.childIndex == 0))
		{
			isExtensionMethod = false;
			
			var parentType = (parentSymbol.kind == SymbolKind.MethodGroup ? parentSymbol.parentSymbol : parentSymbol) as TypeDefinitionBase;
			
			var namespaceDefinition = parentType.parentSymbol;
			if (namespaceDefinition is NamespaceDefinition)
				--parentType.numExtensionMethods;
		}
		base.RemoveDeclaration(symbol);
	}

	public override TypeDefinitionBase ReturnType()
	{
		if (returnType == null)
		{
			if (kind == SymbolKind.Constructor)
				return parentSymbol as TypeDefinitionBase ?? unknownType;

			if (declarations != null)
			{
				ParseTree.BaseNode refNode = null;
				switch (declarations[0].parseTreeNode.RuleName)
				{
					case "methodDeclaration":
					case "interfaceMethodDeclaration":
						refNode = declarations[0].parseTreeNode.FindPreviousNode();
						break;
					default:
						refNode = declarations[0].parseTreeNode.parent.parent.ChildAt(declarations[0].parseTreeNode.parent.childIndex - 1);
						break;
				}
				if (refNode == null)
					Debug.LogError("Could not find method return type from node: " + declarations[0].parseTreeNode);
				returnType = refNode != null ? new SymbolReference(refNode) : null;
			}
		}
		
		return returnType == null ? unknownType : returnType.definition as TypeDefinitionBase ?? unknownType;
	}
	
	//public override TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	//{
	//	if (typeParameters == null)
	//		return base.SubstituteTypeParameters(context);
		
	//	var constructType = false;
	//	var typeArguments = new SymbolReference[typeParameters.Count];
	//	for (var i = 0; i < typeArguments.Length; ++i)
	//	{
	//		typeArguments[i] = new SymbolReference(typeParameters[i]);
	//		var original = typeParameters[i];
	//		if (original == null)
	//			continue;
	//		var substitute = original.SubstituteTypeParameters(context);
	//		if (substitute != original)
	//		{
	//			typeArguments[i] = new SymbolReference(substitute);
	//			constructType = true;
	//		}
	//	}
	//	if (!constructType)
	//		return this;
	//	return ConstructType(typeArguments);
	//}
	

	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		foreach (var parameter in GetParameters())
		{
			var parameterName = parameter.GetName();
			if (!data.ContainsKey(parameterName))
				data.Add(parameterName, parameter);
		}
		if ((flags & (BindingFlags.Instance | BindingFlags.Static)) != BindingFlags.Instance)
		{
			if (typeParameters != null)
				foreach (var parameter in typeParameters)
				{
					var parameterName = parameter.name;
					if (!data.ContainsKey(parameterName))
						data.Add(parameterName, parameter);
				}
		}
	//	ReturnType().GetMembersCompletionData(data, flags, mask, assembly);
	}
	
	private Dictionary<int, ConstructedMethodDefinition> constructedMethods;
	public ConstructedMethodDefinition ConstructMethod(SymbolReference[] typeArgs)
	{
		var numTypeParams = typeParameters != null ? typeParameters.Count : 0;
		var numTypeArgs = typeArgs != null ? typeArgs.Length : 0;

		var hash = 0;
		if (typeArgs != null)
		{
			unchecked // ignore overflow
			{
				hash = (int)2166136261;
				for (var i = 0; i < numTypeParams; ++i)
					hash = hash * 16777619 ^ (i < numTypeArgs ? typeArgs[i].definition : unknownType).GetHashCode();
			}
		}
		
		if (constructedMethods == null)
			constructedMethods = new Dictionary<int, ConstructedMethodDefinition>();
		
		ConstructedMethodDefinition result;
		if (constructedMethods.TryGetValue(hash, out result))
		{
			if (result.IsValid() && result.typeArguments != null)
			{
				var validCachedMethod = true;
				var resultTypeArgs = result.typeArguments;
				for (var i = 0; i < numTypeParams; ++i)
				{
					var definition = resultTypeArgs[i].definition;
					var typeArg = i < numTypeArgs ? typeArgs[i].definition : unknownType;
					if (definition == null || !definition.IsValid() || definition != typeArg)
					{
						validCachedMethod = false;
						break;
					}
				}
				if (validCachedMethod)
					return result;
			}
		}
			
		result = new ConstructedMethodDefinition(this, typeArgs);
		constructedMethods[hash] = result;
		return result;
	}

	public bool IsOverride
	{
		get { return (modifiers & Modifiers.Override) != 0; }
	}

	public bool IsVirtual
	{
		get { return (modifiers & Modifiers.Virtual) != 0; }
	}
}

public class NamespaceDefinition : SymbolDefinition
{
	//public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	//{
	//	Debug.Log("Adding " + symbol + " to namespace " + name);
	//	return base.AddDeclaration(symbol);
	//}
	
	//public virtual void RemoveDeclaration(SymbolDeclaration symbol)
	//{
	//	Debug.Log("Removing " + symbol + " from namespace " + name);
	//	base.RemoveDeclaration(symbol);
	//}
	
	//public override SymbolDefinition FindName(string memberName)
	//{
	//    var result = base.FindName(memberName);
	//    if (result == null)
	//    {
	//        UnityEngine.Debug.Log(memberName + " not found in " + GetTooltipText());
	//    }
	//    return result;
	//}
	
	public void CollectExtensionMethods(
		string id,
		SymbolReference[] typeArgs,
		TypeDefinitionBase extendedType,
		HashSet<MethodDefinition> extensionsMethods,
		Scope context)
	{
		var numTypeArguments = typeArgs == null ? -1 : typeArgs.Length;
		
		var contextAssembly = context.GetAssembly();
		
		for (var i = members.Count; i --> 0; )
		{
			var typeDefinition = members[i];
			if (typeDefinition.kind != SymbolKind.Class || !typeDefinition.IsValid() || (typeDefinition as TypeDefinitionBase).numExtensionMethods == 0 || !typeDefinition.IsStatic || typeDefinition.NumTypeParameters > 0)
				continue;
			
			var accessLevelMask = AccessLevelMask.Public;
			if (typeDefinition.Assembly != null && typeDefinition.Assembly.InternalsVisibleIn(contextAssembly))
				accessLevelMask |= AccessLevelMask.Internal;
			
			if (!typeDefinition.IsAccessible(accessLevelMask))
				continue;
			
			//if (contextType == parentType || parentType.IsSameOrParentOf(contextType))
			//	accessLevelMask |= AccessLevelMask.Public | AccessLevelMask.Protected | AccessLevelMask.Private;
			//else if (contextType.DerivesFrom(parentType))
			//	accessLevelMask |= AccessLevelMask.Public | AccessLevelMask.Protected;
			
			SymbolDefinition member;
			if (typeDefinition.members.TryGetValue(id, numTypeArguments, out member))
			{
				if (member.kind == SymbolKind.MethodGroup)
				{
					var methodGroup = member as MethodGroupDefinition;
					if (methodGroup != null)
					{
						foreach (var method in methodGroup.methods)
						{
							if (method.IsExtensionMethod && method.IsAccessible(accessLevelMask))
							{
								var extendsType = method.parameters[0].TypeOf() as TypeDefinitionBase;
								if (extendedType.CanConvertTo(extendsType))
								{
									if (numTypeArguments > 0)
									{
										var constructedMethod = method.ConstructMethod(typeArgs);
										extensionsMethods.Add(constructedMethod);
									}
									else
									{
										extensionsMethods.Add(method);
									}
								}
							}
						}
					}
					else
					{
						Debug.LogError("Expected a method group: " + member.GetTooltipText());
					}
				}
			}
		}
	}

	private bool resolvingMember = false;
	public override void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		if (resolvingMember)
			return;
		resolvingMember = true;
		
		leaf.resolvedSymbol = null;
		//if (declarations != null)
		//{
		//	foreach (var declaration in declarations)
		//	{
		//		declaration.scope.Resolve(leaf, numTypeArgs);
		//		if (leaf.resolvedSymbol != null)
		//		{
		//			resolvingMember = false;
		//			return;
		//		}
		//	}
		//}

		base.ResolveMember(leaf, context, numTypeArgs, asTypeOnly);
		
		resolvingMember = false;
		
		if (leaf.resolvedSymbol == null)
		{
			if (context != null)
			{
				var assemblyDefinition = context.GetAssembly();
				//while (namespaceScope.parentScope != null)
				//	namespaceScope = (NamespaceScope) namespaceScope.parentScope;
				//var assemblyDefinition = ((CompilationUnitScope) namespaceScope).assembly;
				assemblyDefinition.ResolveInReferencedAssemblies(leaf, this, numTypeArgs);
			}
		}
	}

	public override void ResolveAttributeMember(ParseTree.Leaf leaf, Scope context)
	{
		if (resolvingMember)
			return;
		resolvingMember = true;

		leaf.resolvedSymbol = null;
		//if (declarations != null)
		//{
		//	foreach (var declaration in declarations)
		//	{
		//		declaration.scope.ResolveAttribute(leaf);
		//		if (leaf.resolvedSymbol != null)
		//			return;
		//	}
		//}

		base.ResolveAttributeMember(leaf, context);

		resolvingMember = false;

		if (leaf.resolvedSymbol == null)
		{
			var namespaceScope = context as NamespaceScope;
			if (namespaceScope != null)
			{
				var assemblyDefinition = namespaceScope.GetAssembly();
				//while (namespaceScope.parentScope != null)
				//	namespaceScope = (NamespaceScope) namespaceScope.parentScope;
				//var assemblyDefinition = ((CompilationUnitScope) namespaceScope).assembly;
				assemblyDefinition.ResolveAttributeInReferencedAssemblies(leaf, this);
			}
		}
	}
	
	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		GetMembersCompletionData(data, fromInstance ? 0 : BindingFlags.Static, AccessLevelMask.Any, assembly);
	}

	public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		base.GetMembersCompletionData(data, flags, mask, assembly);

		var assemblyDefinition = assembly ?? parentSymbol;
		while (assemblyDefinition != null && !(assemblyDefinition is AssemblyDefinition))
			assemblyDefinition = assemblyDefinition.parentSymbol;
		((AssemblyDefinition) assemblyDefinition).GetMembersCompletionDataFromReferencedAssemblies(data, this);
	}

	public void GetTypesOnlyCompletionData(Dictionary<string, SymbolDefinition> data, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		if ((mask & AccessLevelMask.Public) != 0)
		{
			if (assembly.InternalsVisibleIn(this.Assembly))
				mask |= AccessLevelMask.Internal;
			else
				mask &= ~AccessLevelMask.Internal;
		}
		
		foreach (var m in members)
		{
			if (m.kind == SymbolKind.Namespace)
				continue;
			
			if (m.kind != SymbolKind.MethodGroup)
			{
				if (m.IsAccessible(mask) && !data.ContainsKey(m.ReflectionName))
				{
					data.Add(m.ReflectionName, m);
				}
			}
		}
		
		var assemblyDefinition = Assembly;
		if (assemblyDefinition != null)
			assemblyDefinition.GetTypesOnlyCompletionDataFromReferencedAssemblies(data, this);
	}

	//public override bool IsPublic
	//{
	//	get
	//	{
	//		return true;
	//	}
	//}

	public override TypeDefinitionBase TypeOfTypeParameter(TypeParameterDefinition tp)
	{
		return tp;
	}

	public override string GetTooltipText()
	{
		return name == string.Empty ? "global namespace" : base.GetTooltipText();
	}

	public void GetExtensionMethodsCompletionData(TypeDefinitionBase targetType, Dictionary<string, SymbolDefinition> data, AccessLevelMask accessLevelMask)
	{
//	Debug.Log("Extensions for " + targetType.GetTooltipText());
 		foreach (var t in members)
		{
	 		if (t.kind == SymbolKind.Class && t.IsStatic && t.NumTypeParameters == 0 &&
		 		(t as TypeDefinitionBase).numExtensionMethods > 0 && t.IsAccessible(accessLevelMask))
			{
				var classMembers = t.members;
				foreach (var cm in classMembers)
				{
					if (cm.kind == SymbolKind.MethodGroup)
					{
						var mg = cm as MethodGroupDefinition;
						if (mg == null)
							continue;
						if (data.ContainsKey(mg.name))
							continue;
						foreach (var m in mg.methods)
						{
							if (m.kind != SymbolKind.Method)
								continue;
							if (!m.IsExtensionMethod)
								continue;
							if (!m.IsAccessible(accessLevelMask))
								continue;
							
							var parameters = m.GetParameters();
							if (parameters == null || parameters.Count == 0)
								continue;
							if (!targetType.CanConvertTo(parameters[0].TypeOf() as TypeDefinitionBase))
								continue;
							
							data.Add(m.name, m);
							break;
						}
					}
					//else if (cm.kind == SymbolKind.Method)
					//{
					//	var m = cm as MethodDefinition;
					//	if (m == null)
					//		continue;
					//	if (!m.IsExtensionMethod)
					//		continue;
					//	//Debug.Log(m.GetTooltipText() + " in " + m.NamespaceOfExtensionMethod);
					//}
				}
			}
		}
	}
}

public class CompilationUnitDefinition : NamespaceDefinition
{
	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		return base.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		base.RemoveDeclaration(symbol);
	}
}

public class SymbolDeclarationScope : Scope
{
	public SymbolDeclaration declaration;
	
	public SymbolDeclarationScope(ParseTree.Node node) : base(node) {}

	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
	//	if (symbol.kind == SymbolKind.Method)// || symbol.kind == SymbolKind.LambdaExpression)
	//	{
	//		declaration = symbol;
	//		return parentScope.AddDeclaration(symbol);
	//	}
		if (symbol.scope == null)
			symbol.scope = this;
		if (declaration == null)
		{
			Debug.LogWarning("Missing declaration in SymbolDeclarationScope! Can't add " + symbol + "\nfor node: " + parseTreeNode);
			return null;
		}
		return declaration.definition.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if ((symbol.kind == SymbolKind.Method /*|| symbol.kind == SymbolKind.LambdaExpression*/) && declaration == symbol)
		{
			declaration = null;
			parentScope.RemoveDeclaration(symbol);
		}
		else if (declaration != null && declaration.definition != null)
		{
			declaration.definition.RemoveDeclaration(symbol);
		}
	}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		throw new NotImplementedException();
	}

	//public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, bool includePrivate, AssemblyDefinition assembly)
	//{
	//	throw new InvalidOperationException();
	//}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		if (declaration != null && declaration.definition != null)
		{
			declaration.definition.ResolveMember(leaf, this, numTypeArgs, asTypeOnly);

			if (numTypeArgs == 0 && leaf.resolvedSymbol == null)
			{
				var typeParams = declaration.definition.GetTypeParameters();
				if (typeParams != null)
				{
					var id = SymbolDefinition.DecodeId(leaf.token.text);
					for (int i = typeParams.Count; i --> 0; )
					{
						if (typeParams[i].GetName() == id)
						{
							leaf.resolvedSymbol = typeParams[i];
							break;
						}
					}
				}
			}
		}

		if (leaf.resolvedSymbol == null)
			base.Resolve(leaf, numTypeArgs, asTypeOnly);
	}

	public override void ResolveAttribute(ParseTree.Leaf leaf)
	{
		if (declaration != null)
			declaration.definition.ResolveAttributeMember(leaf, this);

		if (leaf.resolvedSymbol == null)
			base.ResolveAttribute(leaf);
	}

	public override TypeDefinition EnclosingType()
	{
		if (declaration != null)
		{
			switch (declaration.kind)
			{
				case SymbolKind.Class:
				case SymbolKind.Struct:
				case SymbolKind.Interface:
					return (TypeDefinition) declaration.definition;
			}
		}
		return parentScope != null ? parentScope.EnclosingType() : null;
	}

	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		if (declaration != null && declaration.definition != null)
		{
			var typeParameters = declaration.definition.GetTypeParameters();
			if (typeParameters != null)
			{
				for (var i = typeParameters.Count; i --> 0; )
				{
					var tp = typeParameters[i];
					if (!data.ContainsKey(tp.name))
						data.Add(tp.name, tp);
				}
			}
		}
		base.GetCompletionData(data, fromInstance, assembly);
	}
}

public class TypeBaseScope : Scope
{
	public TypeDefinitionBase definition;
	
	public TypeBaseScope(ParseTree.Node node) : base(node) {}

	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		//Debug.Log("Adding base types list: " + symbol);
		//if (definition != null)
		//    definition.baseType = new SymbolReference { identifier = symbol.Name };
		//Debug.Log("baseType: " + definition.baseType.definition);
		return null;
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
	}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		return parentScope.FindName(symbolName, numTypeParameters);
	}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		if (parentScope != null)
			parentScope.Resolve(leaf, numTypeArgs, asTypeOnly);
	}

	//public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, bool includePrivate, AssemblyDefinition assembly)
	//{
	//	parentScope.GetMembersCompletionData(data, flags, includePrivate, assembly);
	////	definition.GetMembersCompletionData(data, flags, includePrivate, assembly);
	//}
}

public class BodyScope : LocalScope
{
	public SymbolDefinition definition;
	
	public BodyScope(ParseTree.Node node) : base(node) {}
	
	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		if (definition == null)
			return null;
		
		symbol.scope = this;
	//	Debug.Log("Adding declaration " + symbol + " to " + definition);

		switch (symbol.kind)
		{
		case SymbolKind.ConstantField:
		case SymbolKind.LocalConstant:
			if (!(definition is TypeDefinitionBase))
				return base.AddDeclaration(symbol);
			break;
		case SymbolKind.Variable:
		case SymbolKind.ForEachVariable:
		case SymbolKind.FromClauseVariable:
			return base.AddDeclaration(symbol);
		}

		return definition.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		switch (symbol.kind)
		{
		case SymbolKind.LocalConstant:
		case SymbolKind.Variable:
		case SymbolKind.ForEachVariable:
		case SymbolKind.FromClauseVariable:
			base.RemoveDeclaration(symbol);
			return;
		}

		if (definition != null)
			definition.RemoveDeclaration(symbol);
		base.RemoveDeclaration(symbol);
	}

	//public virtual SymbolDefinition ImportReflectedType(Type type)
	//{
	//    throw new InvalidOperationException();
	//}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		return definition.FindName(symbolName, numTypeParameters, false);
	}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		leaf.resolvedSymbol = null;

		if (definition != null)
		{
			definition.ResolveMember(leaf, this, numTypeArgs, asTypeOnly);
			
			if (leaf.resolvedSymbol != null)
				return;
						
			if (numTypeArgs == 0 && leaf.resolvedSymbol == null)
			{
				var typeParams = definition.GetTypeParameters();
				if (typeParams != null)
				{
					var id = SymbolDefinition.DecodeId(leaf.token.text);
					for (var i = typeParams.Count; i --> 0; )
					{
						if (typeParams[i].GetName() == id)
						{
							leaf.resolvedSymbol = typeParams[i];
							return;
						}
					}
				}
			}
		}

		base.Resolve(leaf, numTypeArgs, asTypeOnly);
	}

	public override void ResolveAttribute(ParseTree.Leaf leaf)
	{
		leaf.resolvedSymbol = null;
		if (definition != null)
			definition.ResolveAttributeMember(leaf, this);

		if (leaf.resolvedSymbol == null)
			base.ResolveAttribute(leaf);
	}

	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		if (definition != null)
			definition.GetCompletionData(data, fromInstance, assembly);
		
		Scope scope = this;
		while (fromInstance && scope != null)
		{
			var asBodyScope = scope as BodyScope;
			if (asBodyScope != null)
			{
				var symbol = asBodyScope.definition;
				if (symbol != null && symbol.kind != SymbolKind.LambdaExpression)
				{
					if (!symbol.IsInstanceMember)
						fromInstance = false;
					break;
				}
			}
			scope = scope.parentScope;
		}
		base.GetCompletionData(data, fromInstance, assembly);
	}

	//public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, bool includePrivate, AssemblyDefinition assembly)
	//{
	//	definition.GetMembersCompletionData(data, flags, includePrivate ? AccessLevelMask.Any : AccessLevelMask.Public, assembly);
	//}
}

public struct TypeAlias
{
	public string name;
	public SymbolReference type;
	public SymbolDeclaration declaration;
}

public class NamespaceScope : Scope
{
	public NamespaceDeclaration declaration;
	public NamespaceDefinition definition;

	public List<SymbolDeclaration> typeDeclarations;

	public NamespaceScope(ParseTree.Node node) : base(node) {}
	
	public override IEnumerable<NamespaceDefinition> VisibleNamespacesInScope()
	{
		yield return definition;

		foreach (var nsRef in declaration.importedNamespaces)
		{
			var ns = nsRef.definition as NamespaceDefinition;
			if (ns != null)
				yield return ns;
		}

		if (parentScope != null)
			foreach (var ns in parentScope.VisibleNamespacesInScope())
				yield return ns;
	}

	//public override SymbolDefinition AddDeclaration(SymbolKind symbolKind, ParseTree.Node definitionNode)
	//{
	//    SymbolDefinition result;

	//    if (symbolKind != SymbolKind.Namespace)
	//    {
	//        result = base.AddDeclaration(symbolKind, definitionNode);
	//    }
	//    else
	//    {
	//        var symbol = new NamespaceDeclaration { kind = symbolKind, parseTreeNode = definitionNode };
	//        result = AddDeclaration(symbol);
	//    }

	//    result.parentSymbol = definition;
	//    return result;
	//}

	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		if (definition == null)
			return null;

		symbol.scope = this;
		
		if (symbol.kind == SymbolKind.Class ||
		    symbol.kind == SymbolKind.Struct ||
		    symbol.kind == SymbolKind.Interface ||
		    symbol.kind == SymbolKind.Enum ||
		    symbol.kind == SymbolKind.Delegate)
		{
			if (typeDeclarations == null)
				typeDeclarations = new List<SymbolDeclaration>();
			typeDeclarations.Add(symbol);
			//symbol.modifiers = (symbol.modifiers & Modifiers.Public) != 0 ? Modifiers.Public : Modifiers.Internal;
		}

		if (symbol.kind == SymbolKind.ImportedNamespace)
		{
			declaration.importedNamespaces.Add(new SymbolReference(symbol.parseTreeNode.ChildAt(0)));
			return null;
		}
		else if (symbol.kind == SymbolKind.TypeAlias)
		{
			declaration.typeAliases.Add(new TypeAlias{
				name = symbol.parseTreeNode.ChildAt(0).Print(),
				type = new SymbolReference(symbol.parseTreeNode.ChildAt(2)),
				declaration = symbol
			});
			return null;
		}

		return definition.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (typeDeclarations != null)
			typeDeclarations.Remove(symbol);

		if (symbol.kind == SymbolKind.ImportedNamespace)
		{
			var node = symbol.parseTreeNode;
			declaration.importedNamespaces.RemoveAll(x => x.Node.parent == node);
			return;
		}
		else if (symbol.kind == SymbolKind.TypeAlias)
		{
			var index = declaration.typeAliases.FindIndex(x => x.declaration == symbol);
			if (index >= 0)
				declaration.typeAliases.RemoveAt(index);
			return;
		}

		if (definition != null)
			definition.RemoveDeclaration(symbol);
	}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		leaf.resolvedSymbol = null;
		
		var id = SymbolDefinition.DecodeId(leaf.token.text);
		
		for (int i = declaration.typeAliases.Count; i --> 0; )
		{
			if (declaration.typeAliases[i].name == id)
			{
				if (declaration.typeAliases[i].type != null)
				{
					leaf.resolvedSymbol = declaration.typeAliases[i].type.definition;
					return;
				}
				else
				{
					break;
				}
			}
		}
		
		if (leaf.resolvedSymbol == null)
		{
			for (var i = declaration.importedNamespaces.Count; i --> 0; )
			{
				var nsRef = declaration.importedNamespaces[i];
				if (nsRef.IsBefore(leaf) && nsRef.definition != null)
				{
					nsRef.definition.ResolveMember(leaf, this, numTypeArgs, true);
					if (leaf.resolvedSymbol != null)
					{
						if (leaf.resolvedSymbol.kind == SymbolKind.Namespace)
							leaf.resolvedSymbol = null;
						else
							break;
					}
				}
			}
		}
		
		var parentScopeDef = parentScope != null ? ((NamespaceScope) parentScope).definition : null;
		for (var nsDef = definition;
			leaf.resolvedSymbol == null && nsDef != null && nsDef != parentScopeDef;
			nsDef = nsDef.parentSymbol as NamespaceDefinition)
		{
			nsDef.ResolveMember(leaf, this, numTypeArgs, true);
		}

		if (leaf.resolvedSymbol == null && parentScope != null)
			parentScope.Resolve(leaf, numTypeArgs, true);
	}

	public override void ResolveAttribute(ParseTree.Leaf leaf)
	{
		leaf.resolvedSymbol = null;

		var id = SymbolDefinition.DecodeId(leaf.token.text);
		
		for (int i = declaration.typeAliases.Count; i --> 0; )
		{
			if (declaration.typeAliases[i].name == id)
			{
				if (declaration.typeAliases[i].type != null)
				{
					leaf.resolvedSymbol = declaration.typeAliases[i].type.definition;
					return;
				}
				else
				{
					break;
				}
			}
		}
		
		var parentScopeDef = parentScope != null ? ((NamespaceScope) parentScope).definition : null;
		for (var nsDef = definition;
			leaf.resolvedSymbol == null && nsDef != null && nsDef != parentScopeDef;
			nsDef = nsDef.parentSymbol as NamespaceDefinition)
		{
			nsDef.ResolveAttributeMember(leaf, this);
		}
		
		if (leaf.resolvedSymbol == null)
		{
			foreach (var nsRef in declaration.importedNamespaces)
			{
				if (nsRef.IsBefore(leaf) && nsRef.definition != null)
				{
					nsRef.definition.ResolveAttributeMember(leaf, this);
					if (leaf.resolvedSymbol != null)
						break;
				}
			}
		}

		if (leaf.resolvedSymbol == null && parentScope != null)
			parentScope.ResolveAttribute(leaf);
	}
	
	public override SymbolDefinition ResolveAsExtensionMethod(ParseTree.Leaf invokedLeaf, SymbolDefinition invokedSymbol, TypeDefinitionBase memberOf, ParseTree.Node argumentListNode, SymbolReference[] typeArgs, Scope context)
	{
		if (invokedLeaf == null && (invokedSymbol == null || invokedSymbol.kind == SymbolKind.Error))
			return null;
		
		var id = invokedSymbol != null && invokedSymbol.kind != SymbolKind.Error ? invokedSymbol.name : invokedLeaf != null ? SymbolDefinition.DecodeId(invokedLeaf.token.text) : "";
		
		int numArguments = 1;
		Modifiers[] modifiers = null;
		List<TypeDefinitionBase> argumentTypes = null;
		
		MethodDefinition firstAccessibleMethod = null;
		
		var thisAssembly = GetAssembly();
		
		var extensionsMethods = new HashSet<MethodDefinition>();
		
		thisAssembly.CollectExtensionMethods(definition, id, typeArgs, memberOf, extensionsMethods, context);
		if (extensionsMethods.Count > 0)
		{
			firstAccessibleMethod = extensionsMethods.First();
			
			if (argumentTypes == null)
				numArguments = MethodGroupDefinition.ProcessArgumentListNode(argumentListNode, out modifiers, out argumentTypes, memberOf);
			
			var candidates = new List<MethodDefinition>(extensionsMethods.Count);
			foreach (var method in extensionsMethods)
				if (argumentTypes == null || method.CanCallWith(modifiers, true))
					candidates.Add(method);

			if (typeArgs == null)
			{
				for (var i = candidates.Count; i-- > 0; )
				{
					var candidate = candidates[i];
					if (candidate.NumTypeParameters == 0 || argumentTypes == null)
						continue;

					candidate = MethodGroupDefinition.InferMethodTypeArguments(candidate, argumentTypes);
					if (candidate == null)
						candidates.RemoveAt(i);
					else
						candidates[i] = candidate;
				}
			}
			
			var resolved = MethodGroupDefinition.ResolveMethodOverloads(numArguments, argumentTypes, modifiers, candidates);
			if (resolved != null && resolved.kind != SymbolKind.Error)
				return resolved;
		}
		
		extensionsMethods.Clear();
		
		var importedNamespaces = declaration.importedNamespaces;
		for (var i = importedNamespaces.Count; i --> 0; )
		{
			var nsDef = importedNamespaces[i].definition as NamespaceDefinition;
			if (nsDef != null)
				thisAssembly.CollectExtensionMethods(nsDef, id, typeArgs, memberOf, extensionsMethods, context);
		}
		if (extensionsMethods.Count > 0)
		{
			if (firstAccessibleMethod == null)
				firstAccessibleMethod = extensionsMethods.First();
			
			if (argumentTypes == null)
				numArguments = MethodGroupDefinition.ProcessArgumentListNode(argumentListNode, out modifiers, out argumentTypes, memberOf);
			
			var candidates = new List<MethodDefinition>(extensionsMethods.Count);
			foreach (var method in extensionsMethods)
				if (argumentTypes == null || method.CanCallWith(modifiers, true))
					candidates.Add(method);

			if (typeArgs == null)
			{
				for (var i = candidates.Count; i-- > 0; )
				{
					var candidate = candidates[i];
					if (candidate.NumTypeParameters == 0 || argumentTypes == null)
						continue;

					candidate = MethodGroupDefinition.InferMethodTypeArguments(candidate, argumentTypes);
					if (candidate == null)
						candidates.RemoveAt(i);
					else
						candidates[i] = candidate;
				}
			}
			
			var resolved = MethodGroupDefinition.ResolveMethodOverloads(numArguments, argumentTypes, modifiers, candidates);
			if (resolved != null && resolved.kind != SymbolKind.Error)
				return resolved;
		}
		
		if (parentScope != null)
		{
			var resolved = parentScope.ResolveAsExtensionMethod(invokedLeaf, invokedSymbol, memberOf, argumentListNode, typeArgs, context);
			if (resolved != null)
				return resolved;
		}
		
		if (firstAccessibleMethod != null)
		{
			invokedLeaf.resolvedSymbol = firstAccessibleMethod;
			invokedLeaf.semanticError = MethodGroupDefinition.unresolvedMethodOverload.name;
		}
		return null;
	}
	
	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		return definition.FindName(symbolName, numTypeParameters, true);
	}

	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		assembly = GetAssembly();

		definition.GetMembersCompletionData(data, BindingFlags.NonPublic, AccessLevelMask.Any, assembly);
		
		foreach (var ta in declaration.typeAliases)
			if (!data.ContainsKey(ta.name))
				data.Add(ta.name, ta.type.definition);
		
		foreach (var i in declaration.importedNamespaces)
		{
			var nsDef = i.definition as NamespaceDefinition;
			if (nsDef != null)
				nsDef.GetTypesOnlyCompletionData(data, AccessLevelMask.Any, assembly);
		}
		
		var parentScopeDef = parentScope != null ? ((NamespaceScope) parentScope).definition : null;
		for (var nsDef = definition.parentSymbol;
			nsDef != null && nsDef != parentScopeDef;
			nsDef = nsDef.parentSymbol as NamespaceDefinition)
		{
			nsDef.GetCompletionData(data, fromInstance, assembly);
		}
		
		base.GetCompletionData(data, false, assembly);
	}

	public override TypeDefinition EnclosingType()
	{
		return null;
	}

	//public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, bool includePrivate, AssemblyDefinition assembly)
	//{
	//	definition.GetMembersCompletionData(data, flags, includePrivate ? AccessLevelMask.Any : AccessLevelMask.Public, assembly);
	//}

	public override void GetExtensionMethodsCompletionData(TypeDefinitionBase forType, Dictionary<string, SymbolDefinition> data)
	{
//	Debug.Log("Extensions for " + forType.GetTooltipText());
		var assembly = this.GetAssembly();
		
		assembly.GetExtensionMethodsCompletionData(forType, definition, data);
		foreach (var nsRef in declaration.importedNamespaces)
		{
			var ns = nsRef.definition as NamespaceDefinition;
			if (ns != null)
				assembly.GetExtensionMethodsCompletionData(forType, ns, data);
		}
		
 		if (parentScope != null)
	 		parentScope.GetExtensionMethodsCompletionData(forType, data);
	}
}

public class AttributesScope : Scope
{
	public AttributesScope(ParseTree.Node node) : base(node) {}
	
	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		Debug.LogException(new InvalidOperationException());
		return null;
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		Debug.LogException(new InvalidOperationException());
	}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		var result = parentScope.FindName(symbolName, numTypeParameters);
		return result;
	}

	//public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, bool includePrivate, AssemblyDefinition assembly)
	//{
	//	throw new NotImplementedException();
	//}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		leaf.resolvedSymbol = null;
		base.Resolve(leaf, numTypeArgs, asTypeOnly);

		if (leaf.resolvedSymbol == null || leaf.resolvedSymbol == SymbolDefinition.unknownSymbol)
		{
			if (leaf.parent.RuleName == "typeOrGeneric" && leaf.parent.parent.parent.parent.RuleName == "attribute" &&
				leaf.parent.childIndex == leaf.parent.parent.numValidNodes - 1)
			{
				var old = leaf.token.text;
				leaf.token.text += "Attribute";
				leaf.resolvedSymbol = null;
				base.Resolve(leaf, numTypeArgs, true);
				leaf.token.text = old;
			}
		}

		//if (leaf.resolvedSymbol == SymbolDefinition.unknownSymbol)
		//	Debug.LogError(leaf);
	}
}

public class MemberInitializerScope : Scope
{
	public MemberInitializerScope(ParseTree.Node node) : base(node)	{}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		leaf.resolvedSymbol = null;
		if (numTypeArgs == 0 && !asTypeOnly)
		{
			ParseTree.BaseNode target = null;
	
			if (leaf.childIndex == 0 && leaf.parent != null && leaf.parent.parent == parseTreeNode)
			{
				var node = parseTreeNode // memberInitializerList
					.parent // objectInitializer
					.parent // objectOrCollectionInitializer
					.parent;
				if (node.RuleName == "objectCreationExpression")
				{
					target = node.parent.NodeAt(1); // nonArrayType in a primaryExpression node
				}
				else // node is a memberInitializer node
				{
					target = node.LeafAt(0); // IDENTIFIER in a memberInitializer node
				}
	
				if (target != null)
				{
					var targetSymbol = target.resolvedSymbol;
					if (targetSymbol != null)
						targetSymbol = targetSymbol.TypeOf();
					else
						targetSymbol = SymbolDefinition.ResolveNode(target, parentScope);
	
					if (targetSymbol != null)
						targetSymbol.ResolveMember(leaf, parentScope, 0, false);
					return;
				}
			}
		}
		
		base.Resolve(leaf, numTypeArgs, asTypeOnly);
	}

	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		var baseNode = completionNode;

		if (baseNode.parent != null && (baseNode.parent == parseTreeNode || baseNode.childIndex == 0 && baseNode.parent.parent == parseTreeNode))
		{
			SymbolDefinition target = null;
			ParseTree.BaseNode targetNode = null;

			var node = parseTreeNode // memberInitializerList
				.parent // objectInitializer
				.parent // objectOrCollectionInitializer
				.parent;
			if (node.RuleName == "objectCreationExpression")
			{
				targetNode = node.parent;
				target = SymbolDefinition.ResolveNode(targetNode); // nonArrayType in a primaryExpression node
				var targetAsType = target as TypeDefinitionBase;
				if (targetAsType != null)
					target = targetAsType.GetThisInstance();
			}
			else // parent is a memberInitializer node
			{
				targetNode = node.parent.LeafAt(0);
				target = SymbolDefinition.ResolveNode(node.parent.LeafAt(0)); // IDENTIFIER in a memberInitializer node
			}

			if (target != null)
			{
				HashSet<SymbolDefinition> completions = new HashSet<SymbolDefinition>();
				FGResolver.GetCompletions(IdentifierCompletionsType.Member, targetNode, completions, completionAssetPath);
				foreach (var symbol in completions)
					data.Add(symbol.name, symbol);
			}
		}
		else
		{
			base.GetCompletionData(data, fromInstance, assembly);
		}
	}

	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		return parentScope.AddDeclaration(symbol);
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		parentScope.RemoveDeclaration(symbol);
	}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		throw new InvalidOperationException("Calling FindName on MemberInitializerScope is not allowed!");
	}
}

public class LocalScope : Scope
{
	protected List<SymbolDefinition> localSymbols;

	public LocalScope(ParseTree.Node node) : base(node) {}
	
	public override SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		symbol.scope = this;
		if (localSymbols == null)
			localSymbols = new List<SymbolDefinition>();

		//var name = symbol.Name;

	//	Debug.Log("Adding localSymbol " + name);
		var definition = SymbolDefinition.Create(symbol);
	//	var oldDefinition = (from ls in localSymbols where ls.Value.declarations[0].parseTreeNode.parent == symbol.parseTreeNode.parent select ls.Key).FirstOrDefault();
	//	if (oldDefinition != null)
	//		Debug.LogWarning(oldDefinition);
		localSymbols.Add(definition);

		return definition;
	}

	public override void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (localSymbols != null)
		{
	//		Debug.Log("Removing localSymbol " + symbol.Name);
			localSymbols.RemoveAll((SymbolDefinition x) => {
				if (x.declarations == null)
					return false;
				if (!x.declarations.Remove(symbol))
					return false;
				return x.declarations.Count == 0;
			});
		}
		symbol.definition = null;
	}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		leaf.resolvedSymbol = null;

		if (!asTypeOnly && localSymbols != null)
		{
			var id = SymbolDefinition.DecodeId(leaf.token.text);
			for (var i = localSymbols.Count; i --> 0; )
			{
				if (localSymbols[i].name == id)
				{
					leaf.resolvedSymbol = localSymbols[i];
					return;
				}
			}
		}

		base.Resolve(leaf, numTypeArgs, asTypeOnly);
	}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		symbolName = SymbolDefinition.DecodeId(symbolName);
		
		if (numTypeParameters == 0 && localSymbols != null)
		{
			for (var i = localSymbols.Count; i --> 0; )
				if (localSymbols[i].name == symbolName)
					return localSymbols[i];
		}
		return null;
	}

	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		if (localSymbols != null)
		{
			foreach (var ls in localSymbols)
			{
				SymbolDeclaration declaration = ls.declarations.FirstOrDefault();
				ParseTree.Node declarationNode = declaration != null ? declaration.parseTreeNode : null;
				if (declarationNode == null)
					continue;
				var firstLeaf = declarationNode.GetFirstLeaf();
				if (firstLeaf != null &&
					(firstLeaf.line > completionAtLine ||
					firstLeaf.line == completionAtLine && firstLeaf.tokenIndex >= completionAtTokenIndex))
						continue;
				if (!data.ContainsKey(ls.name))
					data.Add(ls.name, ls);
			}
		}
		base.GetCompletionData(data, fromInstance, assembly);
	}
	
	//public override void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, bool includePrivate, AssemblyDefinition assembly)
	//{
	//	throw new InvalidOperationException();
	//}
}

public class AttributeArgumentsScope : LocalScope
{
	public AttributeArgumentsScope(ParseTree.Node node) : base(node) {}
	
	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		var attributeTypeLeaf = parseTreeNode.parent.parent.NodeAt(0).GetLastLeaf();
		if (attributeTypeLeaf != null)
		{
			var attributeType = attributeTypeLeaf.resolvedSymbol as TypeDefinitionBase;
			if (attributeType != null)
			{
				var tempData = new Dictionary<string, SymbolDefinition>();
				attributeType.GetMembersCompletionData(tempData, BindingFlags.Instance, AccessLevelMask.Public | AccessLevelMask.Internal, assembly);
				foreach (var kv in tempData)
				{
					var symbolKind = kv.Value.kind;
					if (symbolKind == SymbolKind.Field || symbolKind == SymbolKind.Property)
						if (!data.ContainsKey(kv.Key))
							data[kv.Key] = kv.Value;
				}
			}
		}
		base.GetCompletionData(data, fromInstance, assembly);
	}
}

public class AccessorBodyScope : BodyScope
{
	private ValueParameter _value;
	private ValueParameter Value {
		get {
			if (_value == null || !_value.IsValid())
			{
				/*var valueType =*/ definition.parentSymbol.TypeOf();
				_value = new ValueParameter
				{
					name = "value",
					kind = SymbolKind.Parameter,
					parentSymbol = definition,
					type = ((InstanceDefinition) definition.parentSymbol).type,
				};
			}
			return _value;
		}
	}
	
	public AccessorBodyScope(ParseTree.Node node) : base(node) {}

	public override SymbolDefinition FindName(string symbolName, int numTypeParameters)
	{
		if (numTypeParameters == 0 && symbolName == "value" && definition.name != "get")
		{
			return Value;
		}

		return base.FindName(symbolName, numTypeParameters);
	}

	public override void Resolve(ParseTree.Leaf leaf, int numTypeArgs, bool asTypeOnly)
	{
		if (!asTypeOnly && numTypeArgs == 0 && leaf.token.text == "value" && definition.name != "get")
		{
			leaf.resolvedSymbol = Value;
			return;
		}

		base.Resolve(leaf, numTypeArgs, asTypeOnly);
	}

	public override void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		if (definition.name != "get")
			data["value"] = Value;
		definition.parentSymbol.GetCompletionData(data, fromInstance, assembly);
		base.GetCompletionData(data, fromInstance, assembly);
	}
}

public class SymbolDefinition
{
	public static readonly SymbolDefinition resolvedChildren = new SymbolDefinition { kind = SymbolKind.None };
	public static readonly SymbolDefinition nullLiteral = new NullLiteral { kind = SymbolKind.Null };
	public static readonly SymbolDefinition contextualKeyword = new SymbolDefinition { kind = SymbolKind.Null };
	public static readonly TypeDefinition unknownType = new TypeDefinition { name = "unknown type", kind = SymbolKind.Error };
	public static readonly TypeDefinition circularBaseType = new TypeDefinition { name = "circular base type", kind = SymbolKind.Error };
	public static readonly SymbolDefinition unknownSymbol = new SymbolDefinition { name = "unknown symbol", kind = SymbolKind.Error };
	public static readonly SymbolDefinition thisInStaticMember = new SymbolDefinition { name = "cannot use 'this' in static member", kind = SymbolKind.Error };
	public static readonly SymbolDefinition baseInStaticMember = new SymbolDefinition { name = "cannot use 'base' in static member", kind = SymbolKind.Error };

	protected static readonly List<ParameterDefinition> _emptyParameterList = new List<ParameterDefinition>();
	protected static readonly List<SymbolReference> _emptyInterfaceList = new List<SymbolReference>();
	
	public SymbolKind kind;
	public string name;
	public UnityEngine.Texture2D cachedIcon;

	public SymbolDefinition parentSymbol;

	public Modifiers modifiers;
	public AccessLevel accessLevel;

	/// <summary>
	/// Zero, one, or more declarations defining this symbol
	/// </summary>
	/// <remarks>Check for null!!!</remarks>
	public List<SymbolDeclaration> declarations;

	public class SymbolList : List<SymbolDefinition>
	{
		public bool TryGetValue(string name, int numTypeParameters, out SymbolDefinition value)
		{
			for (var index = Count; index --> 0; )
			{
				var x = base[index];
				if (x.name == name &&
					(numTypeParameters < 0 ||
						x.kind == SymbolKind.MethodGroup ||
						x.NumTypeParameters == numTypeParameters))
				{
					value = x;
					return true;
				}
			}
			
			value = null;
			return false;
		}

		public bool Remove(string name, int numTypeParameters)
		{
			for (var index = Count; index --> 0; )
			{
				var x = base[index];
				if (x.name == name && (x.kind == SymbolKind.MethodGroup || x.NumTypeParameters == numTypeParameters))
				{
					RemoveAt(index);
					return true;
				}
			}
			return false;
		}
		
		public bool Contains(string name, int numTypeParameters)
		{
			SymbolDefinition value;
			return TryGetValue(name, numTypeParameters, out value);
		}

		public SymbolDefinition this[string name, int numTypeParameters]
		{
			get
			{
				SymbolDefinition value;
				if (!TryGetValue(name, numTypeParameters, out value))
					throw new KeyNotFoundException(name);
				return value;
			}

			set
			{
				//if (name.Contains('`') || name.Contains('['))
				//	throw new InvalidOperationException("Can't add name " + name);
				
				var index = this.FindIndex(x => x.name == name
					&& (x.kind == SymbolKind.MethodGroup || x.NumTypeParameters == numTypeParameters));
				while (index >= 0)
				{
					var old = base[index];
					if (old.declarations == null ||
						old.declarations.Count == 0 ||
						old.declarations.All(x => !x.IsValid()))
					{
						RemoveAt(index);
					}
					else
					{
						++index;
					}
					
					if (index < Count)
						index = this.FindIndex(index, x => x.name == name
							&& (x.kind == SymbolKind.MethodGroup || x.NumTypeParameters == numTypeParameters));
					else
						break;
				}
				Add(value);
			}
		}
	}
	public SymbolList members = new SymbolList();

	public static AccessLevel AccessLevelFromModifiers(Modifiers modifiers)
	{
		if ((modifiers & Modifiers.Public) != 0)
			return AccessLevel.Public;
		if ((modifiers & Modifiers.Protected) != 0)
		{
			if ((modifiers & Modifiers.Internal) != 0)
				return AccessLevel.ProtectedOrInternal;
			return AccessLevel.Protected;
		}
		if ((modifiers & Modifiers.Internal) != 0)
			return AccessLevel.Internal;
		if ((modifiers & Modifiers.Private) != 0)
			return AccessLevel.Private;
		return AccessLevel.None;
	}
	
	public static string DecodeId(string name)
	{
		if (!string.IsNullOrEmpty(name) && name[0] == '@')
			return name.Substring(1);
		return name;
	}

	public bool IsValid()
	{
		if (declarations == null)
		{
			if (this is ReflectedType || this is ReflectedMethod || this is ReflectedConstructor || this is ReflectedMember)
			{
				return Assembly != null;
			}

			return true; // kind != SymbolKind.Error;
		}
		
		for (var i = declarations.Count; i --> 0; )
		{
			var declaration = declarations[i];
			if (!declaration.IsValid())
			{
				declarations.RemoveAt(i);
				if (declaration.scope != null)
				{
					declaration.scope.RemoveDeclaration(declaration);
					++ParseTree.resolverVersion;
					if (ParseTree.resolverVersion == 0)
						++ParseTree.resolverVersion;
				}
			}
		}

		return declarations.Count > 0;
	}
	
	public SymbolDefinition Rebind()
	{
		if (kind == SymbolKind.Namespace)
			return Assembly.FindNamespace(FullName);
		
		if (parentSymbol == null)
			return this;
		
		var newParent = parentSymbol.Rebind();
		if (newParent == null)
			return null;
		
		var tp = GetTypeParameters();
		var numTypeParams = tp != null ? tp.Count : 0;
		var symbolIsType = this is TypeDefinitionBase;
		SymbolDefinition newSymbol = newParent.FindName(name, numTypeParams, symbolIsType);
		if (newSymbol == null)
		{
			if (newParent.kind == SymbolKind.MethodGroup)
			{
				var mg = newParent as MethodGroupDefinition;
				if (mg == null)
				{
					var generic = newParent.GetGenericSymbol();
					if (generic != null)
						mg = generic as MethodGroupDefinition;
				}
				if (mg != null)
				{
					var signature = GetGenericSymbol().PrintParameters(GetParameters(), true);
					foreach (var m in mg.methods)
					{
						var sig = m.PrintParameters(m.GetParameters(), true);
						if (sig == signature)
						{
							newSymbol = m;
							break;
						}
					}
				}
			}
#if SI3_WARNINGS
			if (newSymbol == null)
			{
				Debug.LogWarning(GetTooltipText() + " not found in " + newParent.GetTooltipText());
				return null;
			}
#endif
		}
		return newSymbol;
	}
	
	public virtual Type GetRuntimeType()
	{
		if (parentSymbol == null)
			return null;
		return parentSymbol.GetRuntimeType();
	}

	public static SymbolDefinition Create(SymbolDeclaration declaration)
	{
		var symbolName = declaration.Name;
		if (symbolName != null)
			symbolName = DecodeId(symbolName);
		
		var definition = Create(declaration.kind, symbolName);
		declaration.definition = definition;

		if (declaration.parseTreeNode != null)
		{
			definition.modifiers = declaration.modifiers;
			definition.accessLevel = AccessLevelFromModifiers(declaration.modifiers);

			if (definition.declarations == null)
				definition.declarations = new List<SymbolDeclaration>();
			definition.declarations.Add(declaration);
		}

		var nameNode = declaration.NameNode();
		if (nameNode is ParseTree.Leaf)
			nameNode.SetDeclaredSymbol(definition);

		return definition;
	}

	public static SymbolDefinition Create(SymbolKind kind, string name)
	{
		SymbolDefinition definition;

		switch (kind)
		{
			case SymbolKind.LambdaExpression:
				definition = new LambdaExpressionDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Parameter:
				definition = new ParameterDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.ForEachVariable:
			case SymbolKind.FromClauseVariable:
			case SymbolKind.Variable:
			case SymbolKind.Field:
			case SymbolKind.ConstantField:
			case SymbolKind.LocalConstant:
			case SymbolKind.Property:
			case SymbolKind.Event:
			case SymbolKind.CatchParameter:
			case SymbolKind.EnumMember:
				definition = new InstanceDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Indexer:
				definition = new IndexerDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Struct:
			case SymbolKind.Class:
			case SymbolKind.Enum:
			case SymbolKind.Interface:
				definition = new TypeDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Delegate:
				definition = new DelegateTypeDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Namespace:
				definition = new NamespaceDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Method:
				definition = new MethodDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Constructor:
				definition = new MethodDefinition
				{
				    name = ".ctor",
				};
				break;

			case SymbolKind.MethodGroup:
				definition = new MethodGroupDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.TypeParameter:
				definition = new TypeParameterDefinition
				{
					name = name,
				};
				break;

			case SymbolKind.Accessor:
				definition = new SymbolDefinition
				{
					name = name,
				};
				break;

			default:
				definition = new SymbolDefinition
				{
					name = name,
				};
				break;
		}

		definition.kind = kind;

		return definition;
	}

	public virtual string GetName()
	{
		var typeParameters = GetTypeParameters();
		if (typeParameters == null || typeParameters.Count == 0)
			return name;

		var sb = new StringBuilder();
		sb.Append(name);
		sb.Append('<');
		sb.Append(typeParameters[0].GetName());
		for (var i = 1; i < typeParameters.Count; ++i)
		{
			sb.Append(", ");
			sb.Append(typeParameters[i].GetName());
		}
		sb.Append('>');
		return sb.ToString();
	}

	public string ReflectionName
	{
		get {
			var tp = GetTypeParameters();
			return tp != null && tp.Count > 0 ? name + "`" + tp.Count : name;
		}
	}

	public virtual SymbolDefinition TypeOf()
	{
		return this;
	}
	
	public virtual SymbolDefinition GetGenericSymbol()
	{
		return this;
	}
	
	public virtual TypeDefinitionBase SubstituteTypeParameters(SymbolDefinition context)
	{
		Debug.Log("Not a type! Can't substitute type of: " + GetTooltipText());
		return null;
	}

	public static Dictionary<Type, ReflectedType> reflectedTypes = new Dictionary<Type, ReflectedType>();

	public TypeDefinitionBase ImportReflectedType(Type type)
	{
		ReflectedType reflectedType;
		if (reflectedTypes.TryGetValue(type, out reflectedType))
			return reflectedType;

		if (type.IsArray)
		{
			var elementType = ImportReflectedType(type.GetElementType());
			var arrayType = elementType.MakeArrayType(type.GetArrayRank());
			return arrayType;
		}

		if ((type.IsGenericType || type.ContainsGenericParameters) && !type.IsGenericTypeDefinition)
		{
			var arguments = type.GetGenericArguments();
			var numGenericArgs = arguments.Length;
			var declaringType = type.DeclaringType;
			if (declaringType != null && declaringType.IsGenericType)
			{
				var parentArgs = declaringType.GetGenericArguments();
				numGenericArgs -= parentArgs.Length;
			}

			var argumentRefs = new List<ReflectedTypeReference>(numGenericArgs);
			for (var i = arguments.Length - numGenericArgs; i < arguments.Length; ++i)
				argumentRefs.Add(ReflectedTypeReference.ForType(arguments[i]));

			var typeDefinitionRef = ReflectedTypeReference.ForType(type.GetGenericTypeDefinition());
			var typeDefinition = typeDefinitionRef.definition as TypeDefinition;
			var constructedType = typeDefinition.ConstructType(argumentRefs.ToArray());
			return constructedType;
		}

		if (type.IsGenericParameter)
		{
			UnityEngine.Debug.LogError("Importing reflected generic type parameter " + type.FullName);
		}

		reflectedTypes[type] = reflectedType = new ReflectedType(type);
		members[reflectedType.name, reflectedType.NumTypeParameters] = reflectedType;
		reflectedType.parentSymbol = this;
		return reflectedType;
	}

	public SymbolDefinition ImportReflectedMethod(MethodInfo info)
	{
		var importedReflectionName = info.Name;
		//var numTypeParameters = info.GetGenericArguments().Length;
		//if (info.IsGenericMethodDefinition)
		//	importedReflectionName += "`" + numTypeParameters;
		//else
		//	numTypeParameters = 0;
		SymbolDefinition methodGroup;
		if (!members.TryGetValue(importedReflectionName, 0, out methodGroup))
		{
			methodGroup = Create(SymbolKind.MethodGroup, importedReflectionName);
			methodGroup.parentSymbol = this;
			//(methodGroup as MethodGroupDefinition).numTypeParameters = numTypeParameters;
			members[importedReflectionName, 0] = methodGroup;
		}
		var imported = new ReflectedMethod(info, methodGroup);
		((MethodGroupDefinition) methodGroup).AddMethod(imported);
		return methodGroup;
	}

	public SymbolDefinition ImportReflectedConstructor(ConstructorInfo info)
	{
		var imported = new ReflectedConstructor(info, this);
		members[".ctor", 0] = imported;
		return imported;
	}

	public void AddMember(SymbolDefinition symbol)
	{
		symbol.parentSymbol = this;
		if (!string.IsNullOrEmpty(symbol.name))
		{
			var declaration = symbol.declarations != null && symbol.declarations.Count == 1 ? symbol.declarations[0] : null;
			if (declaration != null && declaration.numTypeParameters > 0)
				members[declaration.Name, declaration.numTypeParameters] = symbol;
			else
				members[symbol.name, symbol.NumTypeParameters] = symbol;
		}
	}

	public SymbolDefinition AddMember(SymbolDeclaration symbol)
	{
		var member = Create(symbol);
		var symbolName = member.name;
		if (member.kind == SymbolKind.Method)
		{
			SymbolDefinition methodGroup = null;
			if (!members.TryGetValue(symbolName, 0, out methodGroup) || !(methodGroup is MethodGroupDefinition))
			{
				methodGroup = AddMember(new SymbolDeclaration(symbolName)
				{
					kind = SymbolKind.MethodGroup,
					modifiers = symbol.modifiers,
					parseTreeNode = symbol.parseTreeNode,
					scope = symbol.scope,
				//	numTypeParameters = symbol.numTypeParameters,
				});
			}
			var asMethodGroup = methodGroup as MethodGroupDefinition;
			if (asMethodGroup != null)
			{
				asMethodGroup.AddMethod((MethodDefinition) member);
			//	member = methodGroup;
			}
			//else
			//	UnityEngine.Debug.LogError(methodGroup);
		}
		else
		{
			if (member.kind == SymbolKind.Delegate)
			{
				var memberAsDelegate = (DelegateTypeDefinition) member;
				memberAsDelegate.returnType = new SymbolReference(symbol.parseTreeNode.ChildAt(1));
			}
			//else if (member.kind == SymbolKind.MethodGroup)
			//{
			//	((MethodGroupDefinition) member).numTypeParameters = symbol.numTypeParameters;
			//}

			AddMember(member);
		}
		
		if (member.IsPartial)
		{
			if (member is TypeDefinitionBase)
				FGTextBufferManager.FindOtherTypeDeclarationParts(symbol);
		}

		return member;
	}

	public virtual SymbolDefinition AddDeclaration(SymbolDeclaration symbol)
	{
		var parentNamespace = this as NamespaceDefinition;

		SymbolDefinition definition;
		if (parentNamespace != null && symbol is NamespaceDeclaration)
		{
			var qnNode = symbol.parseTreeNode.NodeAt(1);
			if (qnNode == null)
				return null;

			for (var i = 0; i < qnNode.numValidNodes - 2; i += 2)
			{
				var ns = qnNode.ChildAt(i).Print();
				var childNS = parentNamespace.FindName(ns, 0, false);
				if (childNS == null)
				{
					childNS = new NamespaceDefinition {
						kind = SymbolKind.Namespace,
						name = ns,
						accessLevel = AccessLevel.Public,
						modifiers = Modifiers.Public,
					};
					parentNamespace.AddMember(childNS);
				}
				parentNamespace = childNS as NamespaceDefinition;
				if (parentNamespace == null)
					break;
			}
		}

		var addToSymbol = parentNamespace ?? this;
		if (!addToSymbol.members.TryGetValue(symbol.Name, symbol.kind == SymbolKind.Method ? 0 : symbol.numTypeParameters, out definition) ||
			symbol.kind == SymbolKind.Method && definition is MethodGroupDefinition ||
			definition is ReflectedMember || definition is ReflectedType ||
			definition is ReflectedMethod || definition is ReflectedConstructor ||
			!definition.IsValid())
		{
			if (definition != null &&
				(definition is ReflectedMember || definition is ReflectedType ||
					definition is ReflectedMethod || definition is ReflectedConstructor)
				&& definition != symbol.definition)
			{
				definition.Invalidate();
			}
			definition = addToSymbol.AddMember(symbol);
		}
		else
		{
			if (definition.kind == SymbolKind.Namespace && symbol.kind == SymbolKind.Namespace)
			{
				if (definition.declarations == null)
					definition.declarations = new List<SymbolDeclaration>();
				definition.declarations.Add(symbol);
			}
			else if (symbol.IsPartial && definition.declarations != null && definition.declarations.Count > 0)
			{
				var definitionAsType = definition as TypeDefinitionBase;
				if (definitionAsType != null)
				{
					definitionAsType.InvalidateBaseType();
				}
				definition.declarations.Add(symbol);
			}
			else
			{
				definition = addToSymbol.AddMember(symbol);
			}
		}

		symbol.definition = definition;

		var nameNode = symbol.NameNode();
		if (nameNode != null)
		{
			var leaf = nameNode as ParseTree.Leaf;
			if (leaf == null)
			{
				var node = (ParseTree.Node) nameNode;
				if (node.RuleName == "memberName")
				{
					node = node.NodeAt(0); // qid
					if (node != null)
					{
						node = node.NodeAt(-1); // the last child node, qidPart or qidStart
						if (node != null)
						{
							if (node.RuleName == "qidStart")
							{
								if (node.numValidNodes < 3)
									leaf = node.LeafAt(0);
								else
									leaf = node.LeafAt(2);
							}
							else // node is qidPart
							{
								node = node.NodeAt(0); // accessIdentifier
								if (node != null)
									leaf = node.LeafAt(1);
							}
						}
					}
				}
			}
			if (leaf != null)
				leaf.SetDeclaredSymbol(definition);
		}

		return definition;
	}

	private void Invalidate()
	{
		parentSymbol = null;
		if (members != null)
		{
			foreach (var member in members)
				member.Invalidate();
		}
	}

	public virtual void RemoveDeclaration(SymbolDeclaration symbol)
	{
		if (symbol.kind == SymbolKind.Method)
		{
			// TODO: There's no need to RemoveAll - there can only be one
			members.RemoveAll((SymbolDefinition x) => {
				if (x.declarations == null)
					return false;
				if (x.kind == SymbolKind.MethodGroup)
				{
					var mg = x as MethodGroupDefinition;
					mg.RemoveDeclaration(symbol);
					if (mg.methods.Count == 0)
					{
						mg.declarations.Clear();
						mg.parentSymbol = null;
						return true;
					}
				}
				return false;
			});
		}
		else
		{
			var index = members.FindIndex(x => {
				if (x.declarations == null)
					return false;
				if (x.kind == SymbolKind.MethodGroup)
					return false;
				return x.declarations.Contains(symbol);
			});
			if (index >= 0)
			{
				var member = members[index];
				member.declarations.Remove(symbol);
				if (member.declarations.Count == 0)
				{
					members.RemoveAt(index);
				}
				else
				{
					var firstDeclarationKind = member.declarations[0].kind;
					if (member.kind != firstDeclarationKind)
					{
						if ((firstDeclarationKind == SymbolKind.Class ||
							firstDeclarationKind == SymbolKind.Struct ||
							firstDeclarationKind == SymbolKind.Interface) &&
							member.IsPartial && member is TypeDefinitionBase)
						{
							member.kind = firstDeclarationKind;
						}
					}
				}
			}
		}
	}

	public override string ToString()
	{
		return kind + " " + name;
	}
	
	public virtual string CompletionDisplayString(string styledName)
	{
		return styledName;
	}
	
	public virtual string GetDelegateInfoText() { return GetTooltipText(); }

	public string PrintParameters(List<ParameterDefinition> parameters, bool singleLine = false)
	{
		if (parameters == null || tooltipAsExtensionMethod && parameters.Count == 1)
			return "";

		var parametersText = "";
		var comma = !singleLine && parameters.Count > (tooltipAsExtensionMethod ? 2 : 1) ? "\n    " : "";
		var nextComma = !singleLine && parameters.Count > (tooltipAsExtensionMethod ? 2 : 1) ? ",\n    " : ", ";
		for (var i = (tooltipAsExtensionMethod ? 1 : 0); i < parameters.Count; ++i)
		{
			var param = parameters[i];
			
			if (param == null)
				continue;
			var typeOfP = param.TypeOf() as TypeDefinitionBase;
			if (typeOfP == null)
				continue;

			//var ctx = (kind == SymbolKind.Delegate ? this : parentSymbol) as ConstructedTypeDefinition;
			//if (ctx != null)
			//	typeOfP = typeOfP.SubstituteTypeParameters(ctx);
			//if (kind == SymbolKind.Method || kind == SymbolKind.MethodGroup)
				typeOfP = typeOfP.SubstituteTypeParameters(this);

			if (typeOfP == null)
				continue;
			parametersText += comma;
			if (param.IsThisParameter)
				parametersText += "this ";
			else if (param.IsRef)
				parametersText += "ref ";
			else if (param.IsOut)
				parametersText += "out ";
			else if (param.IsParametersArray)
				parametersText += "params ";
			parametersText += typeOfP.GetName() + ' ' + param.name;
			if (param.defaultValue != null)
				parametersText += " = " + param.defaultValue;
			comma = nextComma;
		}
		if (!singleLine && parameters.Count > 1)
			parametersText += '\n';
		return parametersText;
	}
	
	public virtual bool IsExtensionMethod {
		get { return false; }
	}

	protected string tooltipText;
	private bool tooltipAsExtensionMethod;
	
	public string GetTooltipTextAsExtensionMethod()
	{
		string result = "";
		try
		{
			tooltipAsExtensionMethod = true;
			result = GetTooltipText();
		}
		finally
		{
			tooltipAsExtensionMethod = false;
		}
		return result;
	}

	public virtual string GetTooltipText()
	{
		if (kind == SymbolKind.Null)
			return null;

//		if (tooltipText != null)
//			return tooltipText;

		if (kind == SymbolKind.Error)
			return name;

		var kindText = string.Empty;
		switch (kind)
		{
			case SymbolKind.Namespace: return tooltipText = "namespace " + FullName;
			case SymbolKind.Constructor: kindText = "(constructor) "; break;
			case SymbolKind.Destructor: kindText = "(destructor) "; break;
			case SymbolKind.ConstantField:
			case SymbolKind.LocalConstant: kindText = "(constant) "; break;
			case SymbolKind.Property: kindText = "(property) "; break;
			case SymbolKind.Event: kindText = "(event) "; break;
			case SymbolKind.Variable:
			case SymbolKind.ForEachVariable:
			case SymbolKind.FromClauseVariable:
			case SymbolKind.CatchParameter: kindText = "(local variable) "; break;
			case SymbolKind.Parameter: kindText = "(parameter) "; break;
			case SymbolKind.Delegate: kindText = "delegate "; break;
			case SymbolKind.MethodGroup: kindText = "(method group) "; break;
			case SymbolKind.Accessor: kindText = "(accessor) "; break;
			case SymbolKind.Label: return tooltipText = "(label) " + name;
			case SymbolKind.Method: kindText = IsExtensionMethod ? "(extension) " : ""; break;
		}

		var typeOf = kind == SymbolKind.Accessor || kind == SymbolKind.MethodGroup ? null : TypeOf();
		var typeName = string.Empty;
		if (typeOf != null && kind != SymbolKind.Namespace && kind != SymbolKind.Constructor && kind != SymbolKind.Destructor)
		{
			var ctx = (typeOf.kind == SymbolKind.Delegate ? typeOf : parentSymbol) as ConstructedTypeDefinition;
			if (ctx != null)
				typeOf = ((TypeDefinitionBase) typeOf).SubstituteTypeParameters(ctx);
			typeName = typeOf.GetName() + " ";

			if (typeOf.kind != SymbolKind.TypeParameter)
				for (var parentType = typeOf.parentSymbol as TypeDefinitionBase; parentType != null; parentType = parentType.parentSymbol as TypeDefinitionBase)
					typeName = parentType.GetName() + '.' + typeName;
		}

		var parameters = GetParameters();
		
		var parentText = string.Empty;
		var parent = parentSymbol is MethodGroupDefinition ? parentSymbol.parentSymbol : parentSymbol;
		if ((parent is TypeDefinitionBase &&
				parent.kind != SymbolKind.Delegate && kind != SymbolKind.TypeParameter && parent.kind != SymbolKind.LambdaExpression)
			|| parent is NamespaceDefinition)
		{
			var parentName = parent.GetName();
			if (kind == SymbolKind.Constructor)
			{
				var typeParent = parent.parentSymbol as TypeDefinitionBase;
				parentName = typeParent != null ? typeParent.GetName() : null;
			}
			else if (kind == SymbolKind.Method && tooltipAsExtensionMethod)
			{
				var typeOfThisParameter = parameters[0].TypeOf();
				if (typeOfThisParameter != null)
					typeOfThisParameter = typeOfThisParameter.SubstituteTypeParameters(this);
				parentName = typeOfThisParameter != null ? typeOfThisParameter.GetName() : null;
			}
			if (!string.IsNullOrEmpty(parentName))
				parentText = parentName + ".";
		}

		var nameText = GetName();

		var parametersText = string.Empty;
		string parametersEnd = null;
		
		if (kind == SymbolKind.Method)
		{
			nameText += (parameters.Count == (tooltipAsExtensionMethod ? 2 : 1) ? "( " : "(");
			parametersEnd = (parameters.Count == (tooltipAsExtensionMethod ? 2 : 1) ? " )" : ")");
		}
		else if (kind == SymbolKind.Constructor)
		{
			nameText = parent.name + '(';
			parametersEnd = ")";
		}
		else if (kind == SymbolKind.Destructor)
		{
			nameText = "~" + parent.name + "()";
		}
		else if (kind == SymbolKind.Indexer)
		{
			nameText = (parameters.Count == 1 ? "this[ " : "this[");
			parametersEnd = (parameters.Count == 1 ? " ]" : "]");
		}
		else if (kind == SymbolKind.Delegate)
		{
			nameText += (parameters.Count == 1 ? "( " : "(");
			parametersEnd = (parameters.Count == 1 ? " )" : ")");
		}

		if (parameters != null)
		{
			parametersText = PrintParameters(parameters);
		}

		tooltipText = kindText + typeName + parentText + nameText + parametersText + parametersEnd;
		
		tooltipText += DebugValue();

		if (typeOf != null && typeOf.kind == SymbolKind.Delegate)
		{
			tooltipText += "\n\nDelegate info\n";
			tooltipText += typeOf.GetDelegateInfoText();
		}

		var xmlDocs = GetXmlDocs();
		if (!string.IsNullOrEmpty(xmlDocs))
		{
		    tooltipText += "\n\n" + xmlDocs;
		}

		return tooltipText;
	}
	
	protected string DebugValue()
	{
		var typeOf = TypeOf() as TypeDefinitionBase;
		if (//kind == SymbolKind.ConstantField || kind == SymbolKind.LocalConstant ||
			(kind == SymbolKind.Field || kind == SymbolKind.Property))
		{
			if (!(parentSymbol is TypeDefinitionBase))
				return "";
			
			var runtimeType = parentSymbol.GetRuntimeType();
			if (runtimeType == null)
				return "";
			
			if (runtimeType.ContainsGenericParameters)
				return "";
			
			object value;
			
			if (!IsStatic)
			{
				var isScriptableObject = typeof(UnityEngine.ScriptableObject).IsAssignableFrom(runtimeType);
				var isComponent = typeof(UnityEngine.Component).IsAssignableFrom(runtimeType);
				if (isScriptableObject || isComponent)
				{
					const BindingFlags instanceMember = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					UnityEngine.Object[] allInstances = null;
					string result = "";
					if (isComponent)
					{
						allInstances = UnityEditor.Selection.GetFiltered(runtimeType, UnityEditor.SelectionMode.ExcludePrefab);
						if (allInstances.Length > 0)
						{
							result = "\n    in " + allInstances.Length + " selected scene objects";
						}
						else
						{
							allInstances = UnityEngine.Object.FindObjectsOfType(runtimeType);
							if (allInstances.Length > 0)
								result = "\n    in " + allInstances.Length + " active scene objects";
						}
					}
					if (allInstances == null || allInstances.Length == 0)
					{
						allInstances = UnityEngine.Resources.FindObjectsOfTypeAll(runtimeType);
						result = "\n    in " + allInstances.Length + " instances";
					}
					
					var fieldInfo = kind == SymbolKind.Field ? runtimeType.GetField(name, instanceMember) : null;
					var propertyInfo = kind == SymbolKind.Property ? runtimeType.GetProperty(name, instanceMember) : null;
					if (fieldInfo == null && propertyInfo == null)
						return result;
					try
					{
						for (var i = 0; i < Math.Min(allInstances.Length, 10); ++i)
						{
							value = fieldInfo != null
								? fieldInfo.GetValue(allInstances[i])
								: propertyInfo.GetValue(allInstances[i], null);
							result += DebugPrintValue(typeOf, value, "\n    " + (
								allInstances[i].name == ""
								? allInstances[i].ToString()
								: "\"" + allInstances[i].name + "\" (" + allInstances[i].GetHashCode() + ")") + ": ");
						}
#if SI3_WARNINGS
					} catch (Exception e) {
						Debug.LogException(e);
					}
#else
					} catch {}
#endif
					return result;
				}
				return "";
			}
			
			const BindingFlags staticMember = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			if (kind == SymbolKind.Field)
			{
				var fieldInfo = runtimeType.GetField(name, staticMember);
				if (fieldInfo == null)
					return "";
				try {
					value = fieldInfo.GetValue(null);
				} catch {
					return "";
				}
			}
			else if (kind == SymbolKind.Property)
			{
				var propertyInfo = runtimeType.GetProperty(name, staticMember);
				if (propertyInfo == null)
					return "";
				try {
					value = propertyInfo.GetValue(null, null);
				} catch {
					return "";
				}
			}
			else
			{
				return "";
			}
			return DebugPrintValue(typeOf, value, "\n    = ");
		}
		return "";
	}
	
	protected string DebugPrintValue(TypeDefinitionBase typeOf, object value, string header)
	{
		if (value == null)
			return header + "null;";
		
		if (typeOf == builtInTypes_bool)
			return header + ((bool) value ? "true;" : "false;");
		if (typeOf == builtInTypes_int ||
			typeOf == builtInTypes_short ||
			typeOf == builtInTypes_sbyte)
			return header + value + ";";
		if (typeOf == builtInTypes_uint ||
			typeOf == builtInTypes_ushort ||
			typeOf == builtInTypes_byte)
			return header + value + "u;";
		if (typeOf == builtInTypes_long)
			return header + value + "L;";
		if (typeOf == builtInTypes_ulong)
			return header + value + "UL;";
		if (typeOf == builtInTypes_float)
			return header + value + "f;";
		if (typeOf == builtInTypes_char)
			return header + "'" + value + "';";
		if (typeOf == builtInTypes_string)
		{
			string s = "";
			try {
				s = value as string;
			} catch {}
			if (s.Length > 100)
				s = s.Substring(0, 100) + "...";
			var nl = s.IndexOfAny(new []{'\r', '\n'});
			if (nl >= 0)
				s = s.Substring(0, nl) + "...";
			return header + "\"" + s + "\";";
		}
		
		var asEnumerable = value as System.Collections.IEnumerable;
		if (asEnumerable != null)
		{
			var asArray = value as Array;
			if (asArray != null)
				return header + "{ Length = " + asArray.Length + " }";
			var asCollection = value as System.Collections.ICollection;
			if (asCollection != null)
				return header + "{ Count = " + asCollection.Count + " }";
			var countProperty = value.GetType().GetProperty("Count");
			if (countProperty != null)
			{
				var count = countProperty.GetValue(value, null);
				return header + "{ Count = " + count + " }";
			}
		}
		
		var str = value.ToString();
		if (str.Length > 100)
			str = str.Substring(0, 100) + "...";
		var newLine = str.IndexOfAny(new []{'\r', '\n'});
		if (newLine >= 0)
			str = str.Substring(0, newLine) + "...";
		return header + "{ " + str + " }";
	}

	public virtual List<ParameterDefinition> GetParameters()
	{
		return null;
	}

	public virtual List<TypeParameterDefinition> GetTypeParameters()
	{
		return null;
	}

	protected string GetXmlDocs()
	{
#if UNITY_WEBPLAYER && !UNITY_5_0
		return null;
#else
		string result = null;
		
		var unityName = UnityHelpName;
		if (unityName != null)
		{
			if (UnitySymbols.summaries.TryGetValue(unityName, out result))
				return result;
			//Debug.Log(unityName);
			return null;
		}
		
		return result;
#endif
		
	    //var xml = new System.Xml.XmlDocument();
	    //xml.Load(UnityEngine.Application.dataPath + "/FlipbookGames/ScriptInspector2/Editor/EditorResources/XmlDocs/UnityEngine.xml");
	    //var summary = xml.SelectSingleNode("/doc/members/member[@name = 'T:" + FullName + "']/summary");
	    //if (summary != null)
	    //    return summary.InnerText;
	    //return null;
	}

	public string UnityHelpName
	{
		get
		{
			if (kind == SymbolKind.TypeParameter)
				return null;
			
			var result = FullName;
			if (result == null)
				return null;
			if (result.StartsWith("UnityEngine.", StringComparison.Ordinal))
				result = result.Substring("UnityEngine.".Length);
			else if (result.StartsWith("UnityEditor.", StringComparison.Ordinal))
				result = result.Substring("UnityEditor.".Length);
			else
				return null;
			
			if (kind == SymbolKind.Indexer)
				result = result.Substring(0, result.LastIndexOf('.') + 1) + "Index_operator";
			else if (kind == SymbolKind.Constructor)
				result = result.Substring(0, result.LastIndexOf('.')) + "-ctor";
			else if ((kind == SymbolKind.Field || kind == SymbolKind.Property) && parentSymbol.kind != SymbolKind.Enum)
				result = result.Substring(0, result.LastIndexOf('.')) + "-" + name;
			
			return result;
		}
	}
	
	protected int IndexOfTypeParameter(TypeParameterDefinition tp)
	{
		var typeParams = GetTypeParameters();
		var index = typeParams != null ? typeParams.IndexOf(tp) : -1;
		if (index < 0)
			return parentSymbol != null ? parentSymbol.IndexOfTypeParameter(tp) : -1;
		for (var parent = parentSymbol; parent != null; parent = parent.parentSymbol)
		{
			typeParams = parent.GetTypeParameters();
			if (typeParams != null)
				index += typeParams.Count;
		}
		return index;
	}
	
	public string XmlDocsName
	{
		get
		{
			var sb = new StringBuilder();
			switch (kind)
			{
				case SymbolKind.Namespace:
					sb.Append("N:");
					sb.Append(FullName);
					break;
				case SymbolKind.Class:
				case SymbolKind.Struct:
				case SymbolKind.Interface:
				case SymbolKind.Enum:
				case SymbolKind.Delegate:
					sb.Append("T:");
					sb.Append(FullReflectionName);
					break;
				case SymbolKind.Field:
				case SymbolKind.ConstantField:
					sb.Append("F:");
					sb.Append(FullReflectionName);
					break;
				case SymbolKind.Property:
					sb.Append("P:");
					sb.Append(FullReflectionName);
					break;
				case SymbolKind.Indexer:
					sb.Append("P:");
					sb.Append(parentSymbol.FullReflectionName);
					sb.Append(".Item");
					break;
				case SymbolKind.Method:
				case SymbolKind.Operator:
					sb.Append("M:");
					sb.Append(FullReflectionName);
					break;
				case SymbolKind.Constructor:
					sb.Append("M:");
					sb.Append(parentSymbol.FullReflectionName);
					sb.Append(".#ctor");
					break;
				case SymbolKind.Destructor:
					sb.Append("M:");
					sb.Append(parentSymbol.FullReflectionName);
					sb.Append(".Finalize");
					break;
				case SymbolKind.Event:
					sb.Append("E:");
					sb.Append(FullReflectionName);
					break;
				default:
					return null;
			}
			var parameters = GetParameters();
			if (kind != SymbolKind.Delegate && parameters != null && parameters.Count > 0)
			{
				sb.Append("(");
				for (var i = 0; i < parameters.Count; ++i)
				{
					var p = parameters[i];
					if (i > 0)
						sb.Append(",");
					var t = p.TypeOf();
					if (t.kind == SymbolKind.TypeParameter)
					{
						sb.Append('`');
						var tp = t as TypeParameterDefinition;
						var tpIndex = tp.parentSymbol.IndexOfTypeParameter(tp);
						sb.Append(tpIndex);
					}
					else
					{
						sb.Append(t.FullReflectionName);
					}
					var a = t as ArrayTypeDefinition;
					if (a != null)
					{
						if (a.rank == 1)
						{
							sb.Append("[]");
						}
						else
						{
							sb.Append("[0:");
							for (var j = 1; j < a.rank; ++j)
								sb.Append(",0:");
							sb.Append("]");
						}
					}
					else if (p.IsRef || p.IsOut)
						sb.Append("@");
					if (p.IsOptional)
						sb.Append("!");
				}
				sb.Append(")");
			}
			return sb.ToString();
		}
	}
	
	public string RelativeName(Scope context)
	{
		if (context == null)
			return FullName;
		
		foreach (var kv in builtInTypes)
			if (kv.Value == this)
				return kv.Key;
		
		var thisPath = new List<SymbolDefinition>();
		for (var parent = this; parent != null; parent = parent.parentSymbol)
		{
			if (parent is MethodGroupDefinition)
				parent = parent.parentSymbol;
			if (!string.IsNullOrEmpty(parent.name))
				thisPath.Add(parent);
		}
		
		var contextPath = new List<SymbolDefinition>();
		var contextScope = context;
		while (contextScope != null)
		{
			var asNamespaceScope = contextScope as NamespaceScope;
			if (asNamespaceScope != null)
			{
				if (!string.IsNullOrEmpty(asNamespaceScope.definition.name))
					contextPath.Add(asNamespaceScope.definition);
			}
			else
			{
				var asBodyScope = contextScope as BodyScope;
				if (asBodyScope != null)
				{
					var scopeDefinition = asBodyScope.definition;
					switch (scopeDefinition.kind)
					{
					case SymbolKind.Class:
					case SymbolKind.Struct:
					case SymbolKind.Interface:
						contextPath.Add(scopeDefinition);
						break;
					}
				}
			}
			
			contextScope = contextScope.parentScope;
		}
		
		while (contextPath.Count > 0 && thisPath.Count > 0 && contextPath[contextPath.Count - 1] == thisPath[thisPath.Count - 1])
		{
			contextPath.RemoveAt(contextPath.Count - 1);
			thisPath.RemoveAt(thisPath.Count - 1);
		}
		
		if (thisPath.Count <= 1)
			return name;
		
		NamespaceDefinition thisNamespace = null;
		var index = thisPath.Count;
		while (index --> 0)
		{
			var namespaceDefinition = thisPath[index] as NamespaceDefinition;
			if (namespaceDefinition == null)
				break;
			thisNamespace = namespaceDefinition;
		}
		if (index >= 0 && thisNamespace != null && thisNamespace.parentSymbol != null)
		{
			++index;
			var thisNamespaceName = thisNamespace.FullName;
			
			var contextNamespaceScope = context.EnclosingNamespaceScope();
			while (contextNamespaceScope != null)
			{
				var importedNamespaces = contextNamespaceScope.declaration.importedNamespaces;
				for (var i = importedNamespaces.Count; i --> 0; )
				{
					if (importedNamespaces[i].definition.FullName == thisNamespaceName)
					{
						thisPath.RemoveRange(index, thisPath.Count - index);
						goto namespaceIsImported;
					}
				}
				contextNamespaceScope = contextNamespaceScope.parentScope as NamespaceScope;
			}
		}
		
	namespaceIsImported:
		
		var sb = new StringBuilder();
		for (var i = thisPath.Count; i --> 0; )
		{
			sb.Append(thisPath[i].name);
			var asConstructedType = thisPath[i] as ConstructedTypeDefinition;
			if (asConstructedType != null)
			{
				var typeArguments = asConstructedType.typeArguments;
				if (typeArguments != null && typeArguments.Length > 0)
				{
					var comma = "<";
					for (var j = 0; j < typeArguments.Length; ++j)
					{
						sb.Append(comma);
						if (typeArguments[j] != null)
							sb.Append(typeArguments[j].definition.RelativeName(context));
						comma = ", ";
					}
					sb.Append('>');
				}
			}
			if (i > 0)
				sb.Append('.');
		}
		return sb.ToString();
	}

	public string FullName
	{
		get
		{
			if (parentSymbol != null)
			{
				var parentFullName = (parentSymbol is MethodGroupDefinition)
					? (parentSymbol.parentSymbol ?? unknownSymbol).FullName
					: parentSymbol.FullName;
				if (string.IsNullOrEmpty(name))
					return parentFullName;
				if (string.IsNullOrEmpty(parentFullName))
					return name;
				return parentFullName + '.' + name;
			}
			return name;
		}
	}

	public string FullReflectionName
	{
		get
		{
			if (parentSymbol != null)
			{
				var parentFullName = (parentSymbol is MethodGroupDefinition)
					? (parentSymbol.parentSymbol ?? unknownSymbol).FullReflectionName
					: parentSymbol.FullReflectionName;
				if (string.IsNullOrEmpty(ReflectionName))
					return parentFullName;
				if (string.IsNullOrEmpty(parentFullName))
					return ReflectionName;
				return parentFullName + '.' + ReflectionName;
			}
			return ReflectionName;
		}
	}

	public string Dump()
	{
		var sb = new StringBuilder();
		Dump(sb, string.Empty);
		return sb.ToString();
	}

	protected virtual void Dump(StringBuilder sb, string indent)
	{
		sb.AppendLine(indent + kind + " " + name + " (" + GetType() + ")");

		foreach (var member in members)
			member.Dump(sb, indent + "  ");
	}

	public virtual void ResolveMember(ParseTree.Leaf leaf, Scope context, int numTypeArgs, bool asTypeOnly)
	{
		leaf.resolvedSymbol = null;

		var id = DecodeId(leaf.token.text);

		SymbolDefinition definition;
		if (!members.TryGetValue(id, numTypeArgs, out definition))
		{
			return;
			//if (numTypeArgs > 0)
			//	members.TryGetValue(id, out definition);
		}
		if (definition != null && definition.kind != SymbolKind.Namespace && !(definition is TypeDefinitionBase))
		{
			if (asTypeOnly)
				return;
			if (leaf.parent != null && leaf.parent.RuleName == "typeOrGeneric")
				leaf.semanticError = "Type expected";
		}

		leaf.resolvedSymbol = definition;
	}

	public virtual void ResolveAttributeMember(ParseTree.Leaf leaf, Scope context)
	{
		leaf.resolvedSymbol = null;

		var id = leaf.token.text;
		leaf.resolvedSymbol = FindName(id, 0, true) ?? FindName(id + "Attribute", 0, true);
	}
	
	public virtual SymbolDefinition ResolveMethodOverloads(ParseTree.Node argumentListNode, SymbolReference[] typeArgs, Scope scope)
	{
		throw new InvalidOperationException();
	}
	
	public static Dictionary<string, TypeDefinitionBase> builtInTypes;
	
	public static TypeDefinition builtInTypes_int;
	public static TypeDefinition builtInTypes_uint;
	public static TypeDefinition builtInTypes_byte;
	public static TypeDefinition builtInTypes_sbyte;
	public static TypeDefinition builtInTypes_short;
	public static TypeDefinition builtInTypes_ushort;
	public static TypeDefinition builtInTypes_long;
	public static TypeDefinition builtInTypes_ulong;
	public static TypeDefinition builtInTypes_float;
	public static TypeDefinition builtInTypes_double;
	public static TypeDefinition builtInTypes_decimal;
	public static TypeDefinition builtInTypes_char;
	public static TypeDefinition builtInTypes_string;
	public static TypeDefinition builtInTypes_bool;
	public static TypeDefinition builtInTypes_object;
	public static TypeDefinition builtInTypes_void;
	
	public static TypeDefinition builtInTypes_Array;
	public static TypeDefinition builtInTypes_Nullable;
	public static TypeDefinition builtInTypes_IEnumerable;
	public static TypeDefinition builtInTypes_IEnumerable_1;
	public static TypeDefinition builtInTypes_Exception;

	//public static HashSet<string> missingResolveNodePaths = new HashSet<string>();
	
	public static SymbolDefinition ResolveNodeAsConstructor(ParseTree.BaseNode oceNode, Scope scope, SymbolDefinition asMemberOf)
	{
		if (asMemberOf == null)
			return null;

		var node = oceNode as ParseTree.Node;
		if (node == null || node.numValidNodes == 0)
			return null;

		var node1 = node.RuleName == "arguments" ? node : node.NodeAt(0);
		if (node1 == null)
			return null;

		var constructor = asMemberOf.FindName(".ctor", 0, false);
		if (constructor == null || constructor.parentSymbol != asMemberOf)
			constructor = ((TypeDefinitionBase) asMemberOf).GetDefaultConstructor();
		if (constructor is MethodGroupDefinition)
		{
			if (node1.RuleName == "arguments")
				constructor = ResolveNode(node1, scope, constructor);
		}
		else if (node1.RuleName == "arguments")
		{
			for (var i = 1; i < node1.numValidNodes - 1; ++i)
				ResolveNode(node1.ChildAt(i), scope, constructor);
		}

		if (node.RuleName != "arguments" && node.numValidNodes == 2)
			ResolveNode(node.ChildAt(1));
		
		return constructor;
	}

	public static SymbolDefinition EnumerableElementType(ParseTree.Node node)
	{
		var enumerableExpr = ResolveNode(node);
		if (enumerableExpr != null)
		{
			var arrayType = enumerableExpr.TypeOf() as ArrayTypeDefinition;
			if (arrayType != null)
			{
				if (arrayType.rank > 0 && arrayType.elementType != null)
					return arrayType.elementType;
			}
			else
			{
				var enumerableType = enumerableExpr.TypeOf() as TypeDefinitionBase;
				if (enumerableType != null)
				{
					TypeDefinitionBase iEnumerableGenericTypeDef = builtInTypes_IEnumerable_1;
					if (enumerableType.DerivesFromRef(ref iEnumerableGenericTypeDef))
					{
						var asGenericEnumerable = iEnumerableGenericTypeDef as ConstructedTypeDefinition;
						if (asGenericEnumerable != null)
							return asGenericEnumerable.typeArguments[0].definition;
					}

					var iEnumerableTypeDef = builtInTypes_IEnumerable;
					if (enumerableType.DerivesFrom(iEnumerableTypeDef))
						return builtInTypes_object;
				}
			}
		}
		return unknownType;
	}
	
	private static SymbolDefinition ResolveArgumentsNode(ParseTree.Node argumentsNode, Scope scope, ParseTree.Leaf invokedLeaf, SymbolDefinition invokedSymbol, SymbolDefinition memberOf)
	{
		SymbolDefinition result = null;
		
		invokedSymbol = invokedSymbol ?? invokedLeaf.resolvedSymbol;
		
		var argumentListNode = argumentsNode != null && argumentsNode.numValidNodes >= 2 ? argumentsNode.NodeAt(1) : null;
		if (argumentListNode != null)
			ResolveNode(argumentListNode, scope);

		SymbolReference[] typeArgs = null;
		if (invokedLeaf != null)
		{
			var accessIdentifierNode = invokedLeaf.parent;
			if (accessIdentifierNode != null && accessIdentifierNode.RuleName == "accessIdentifier")
			{
				var typeArgumentListNode = accessIdentifierNode.NodeAt(2);
				if (typeArgumentListNode != null)
				{
					var numTypeArguments = typeArgumentListNode.numValidNodes / 2;
					typeArgs = new SymbolReference[numTypeArguments];
					for (int i = 0; i < numTypeArguments; ++i)
						typeArgs[i] = new SymbolReference(typeArgumentListNode.ChildAt(1 + 2 * i));
				}
			}
		}

		if (invokedSymbol.kind == SymbolKind.MethodGroup)
		{
			result = invokedSymbol.ResolveMethodOverloads(argumentListNode, typeArgs, scope);
			var targetType = invokedSymbol;
			while (targetType != null && !(targetType is TypeDefinitionBase))
				targetType = targetType.parentSymbol;
			while (result == MethodGroupDefinition.unresolvedMethodOverload && targetType != null)
			{
				targetType = (targetType as TypeDefinitionBase).BaseType();
				if (targetType != null)
				{
					var inBase = targetType.FindName(invokedSymbol.name, 0, false);
					if (inBase != null && inBase.kind == SymbolKind.MethodGroup)
						result = inBase.ResolveMethodOverloads(argumentListNode, typeArgs, scope);
				}
			}
			
			if (result != null && result.kind == SymbolKind.Method && !(result is MethodDefinition))
				result = result as ConstructedSymbolReference;
			
			if (result != null && result.kind != SymbolKind.Error)
			{
				var prevNode = argumentsNode != null ? argumentsNode.parent.FindPreviousNode() as ParseTree.Node : null;
				var idLeaf = prevNode != null ? prevNode.LeafAt(0) ?? prevNode.NodeAt(0).LeafAt(1) : invokedLeaf;
				if (result.kind == SymbolKind.Error)
				{
					idLeaf.resolvedSymbol = invokedSymbol as MethodGroupDefinition;
					idLeaf.semanticError = result.name;
				}
				else if (idLeaf.resolvedSymbol != result)
				{
					idLeaf.resolvedSymbol = result;
					idLeaf.semanticError = null;
				}
				
				return result;
			}
		}
		
		if (memberOf != null && !(memberOf is TypeDefinitionBase) &&
			(invokedSymbol.kind == SymbolKind.MethodGroup || invokedSymbol.kind == SymbolKind.Error))
		{
			var memberOfType = memberOf.TypeOf() as TypeDefinitionBase ?? scope.EnclosingType();
			result = scope.ResolveAsExtensionMethod(invokedLeaf, invokedSymbol, memberOfType, argumentListNode, typeArgs, scope);
			//Debug.Log("ResolveAsExtensionMethod: " + (invokedLeaf != null ? invokedLeaf.token.text : invokedSymbol.ToString()) + " on " + memberOf.ToString());
			
			if (result != null && result.kind == SymbolKind.Method && !(result is MethodDefinition))
				result = result as ConstructedSymbolReference;
			
			if (result != null)
			{
				if (result.kind == SymbolKind.Error)
				{
					invokedLeaf.resolvedSymbol = result;
					invokedLeaf.semanticError = result.name;
				}
				else if (invokedLeaf.resolvedSymbol != result)
				{
					invokedLeaf.resolvedSymbol = result;
					invokedLeaf.semanticError = null;
				}
			}
		}
		else if (invokedSymbol.kind != SymbolKind.Method && invokedSymbol.kind != SymbolKind.Error)
		{
			var typeOf = invokedSymbol.TypeOf() as TypeDefinitionBase;
			if (typeOf == null || typeOf.kind == SymbolKind.Error)
				return unknownType;
			
			var returnType = invokedSymbol.kind == SymbolKind.Delegate ? typeOf :
				typeOf.kind == SymbolKind.Delegate ? typeOf.TypeOf() as TypeDefinitionBase : null;
			if (returnType != null)
				return returnType.GetThisInstance();
			
					//Debug.Log(">> " + asMemberOf.GetTooltipText());
					//Debug.Log(node);
//					if (asMemberOf.kind != SymbolKind.Event)
			var firstLeaf = argumentListNode != null ? argumentListNode.LeafAt(0) : null;
			if (firstLeaf != null)
				firstLeaf.semanticError = "Cannot invoke symbol";
		}
		
		return result;
	}
	
	public static SymbolDefinition ResolveNode(ParseTree.BaseNode baseNode, Scope scope = null, SymbolDefinition asMemberOf = null, int numTypeArguments = 0, bool asTypeOnly = false)
	{
		var node = baseNode as ParseTree.Node;

		if (scope == null)
		{
			var scanNode = node;
			while (scanNode != null && scanNode.parent != null)
			{
				var ruleName = scanNode.RuleName;
				if (ruleName == "type" && scanNode.childIndex == 2)
				{
					var nextNode = scanNode.parent.NodeAt(3);
					if (nextNode != null &&
						(nextNode.RuleName == "methodDeclaration" || nextNode.RuleName == "interfaceMethodDeclaration"))
					{
						scope = nextNode.scope;
						break;
					}
				}
				
				if (ruleName != "type"
					&& ruleName != "typeName"
					&& ruleName != "namespaceOrTypeName"
					&& ruleName != "typeOrGeneric"
					&& ruleName != "typeArgumentList")
				{
					break;
				}
				scanNode = scanNode.parent;
			}
		}

		if (scope == null)
		{
			var scopeNode = CsGrammar.EnclosingSemanticNode(baseNode, SemanticFlags.ScopesMask);
			while (scopeNode != null && scopeNode.scope == null && scopeNode.parent != null)
				scopeNode = CsGrammar.EnclosingSemanticNode(scopeNode.parent, SemanticFlags.ScopesMask);
			if (scopeNode != null)
				scope = scopeNode.scope;
		}

		var leaf = baseNode as ParseTree.Leaf;
		if (leaf != null)
		{
			if ((leaf.resolvedSymbol == null || leaf.semanticError != null ||
				leaf.resolvedSymbol.kind == SymbolKind.Method ||
				!leaf.resolvedSymbol.IsValid()) && leaf.token != null)
			{
				leaf.resolvedSymbol = null;
				leaf.semanticError = null;

				switch (leaf.token.tokenKind)
				{
					case SyntaxToken.Kind.Identifier:
						if (asMemberOf != null)
						{
							asMemberOf.ResolveMember(leaf, scope, numTypeArguments, asTypeOnly);
							if (asTypeOnly && leaf.resolvedSymbol == null)
							{
								asMemberOf.ResolveMember(leaf, scope, numTypeArguments, false);
								if (leaf.resolvedSymbol != null && leaf.resolvedSymbol.kind != SymbolKind.Error)
								{
									leaf.semanticError = "Type expected!";
								}
							}
							//	UnityEngine.Debug.LogWarning("Could not resolve member '" + leaf + "' of " + asMemberOf + "[" + asMemberOf.GetType() + "], line " + (1+leaf.line));
						}
						else if (scope != null)
						{
							if (leaf.token.text == "global")
							{
								var nextLeaf = leaf.FindNextLeaf();
								if (nextLeaf != null && nextLeaf.IsLit("::"))
								{
									var assembly = scope.GetAssembly();
									if (assembly != null)
									{
										leaf.resolvedSymbol = scope.GetAssembly().GlobalNamespace;
//										nextLeaf = nextLeaf.FindNextLeaf();
//										if (nextLeaf != null && nextLeaf.token.tokenKind == SyntaxToken.Kind.Identifier)
//										{
//											nextLeaf.resolvedSymbol = assembly.FindNamespace(nextLeaf.token.text);
//										}
										return leaf.resolvedSymbol;
									}
								}
							}
							scope.Resolve(leaf, numTypeArguments, asTypeOnly);
							if (asTypeOnly && leaf.resolvedSymbol == null)
							{
								scope.Resolve(leaf, numTypeArguments, false);
								if (leaf.resolvedSymbol != null && leaf.resolvedSymbol.kind != SymbolKind.Error)
								{
									leaf.semanticError = "Type expected!";
								}
							}
						}
						if (leaf.resolvedSymbol == null)
						{
							if (asMemberOf != null)
								asMemberOf.ResolveMember(leaf, scope, -1, asTypeOnly);
							else if (scope != null)
								scope.Resolve(leaf, -1, asTypeOnly);
						}
						if (leaf.resolvedSymbol != null &&
							leaf.resolvedSymbol.NumTypeParameters != numTypeArguments &&
							leaf.resolvedSymbol.kind != SymbolKind.Error)
						{
							if (leaf.resolvedSymbol is TypeDefinitionBase)
							{
								leaf.semanticError = string.Format("Type '{0}' does not take {1} type argument{2}",
									leaf.resolvedSymbol.GetName(), numTypeArguments, numTypeArguments == 1 ? "" : "s");
							}
							else if (numTypeArguments > 0 &&
								(leaf.resolvedSymbol.kind == SymbolKind.Method))// || leaf.resolvedSymbol.kind == SymbolKind.MethodGroup))
							{
								leaf.semanticError = string.Format("Method '{0}' does not take {1} type argument{2}",
									leaf.token.text, numTypeArguments, numTypeArguments == 1 ? "" : "s");
							}
						}
						break;

					case SyntaxToken.Kind.Keyword:
						if (leaf.token.text == "this" || leaf.token.text == "base")
						{
							var scopeNode = CsGrammar.EnclosingScopeNode(leaf.parent,
								SemanticFlags.MethodBodyScope,
								SemanticFlags.AccessorBodyScope);//,
								//SemanticFlags.LambdaExpressionBodyScope,
								//SemanticFlags.AnonymousMethodBodyScope);
							if (scopeNode == null)
							{
								if (leaf.childIndex == 1 && leaf.parent.RuleName == "constructorInitializer")
								{
									var bodyScope = scope.parentScope.parentScope as BodyScope;
									if (bodyScope == null)
										break;
									
									asMemberOf = bodyScope.definition;
									if (asMemberOf.kind != SymbolKind.Class && asMemberOf.kind != SymbolKind.Struct)
										break;
									
									if (leaf.token.text == "base")
									{
										if (asMemberOf.kind == SymbolKind.Struct)
											break; // CS0522: Struct constructors cannot call base constructors
										
										asMemberOf = ((TypeDefinitionBase) asMemberOf).BaseType();
									}
										
									leaf.resolvedSymbol = ResolveNodeAsConstructor(leaf.parent.NodeAt(2), scope, asMemberOf);
								}
								break;
							}

							var memberScope = scopeNode.scope as BodyScope;
							if (memberScope != null && memberScope.definition.IsStatic)
							{
								if (leaf.token.text == "base")
									leaf.resolvedSymbol = baseInStaticMember;
								else
									leaf.resolvedSymbol = thisInStaticMember;
								break;
							}

							scopeNode = CsGrammar.EnclosingScopeNode(scopeNode, SemanticFlags.TypeDeclarationScope);
							if (scopeNode == null)
							{
								leaf.resolvedSymbol = unknownSymbol;
								break;
							}

							var thisType = ((SymbolDeclarationScope) scopeNode.scope).declaration.definition as TypeDefinitionBase;
							if (thisType != null && leaf.token.text == "base")
								thisType = thisType.BaseType();
							if (thisType != null && (thisType.kind == SymbolKind.Struct || thisType.kind == SymbolKind.Class))
								leaf.resolvedSymbol = thisType.GetThisInstance();
							else
								leaf.resolvedSymbol = unknownSymbol;
							break;
						}
						else
						{
							TypeDefinitionBase type;
							if (builtInTypes.TryGetValue(leaf.token.text, out type))
								leaf.resolvedSymbol = type;
						}
						break;

					case SyntaxToken.Kind.CharLiteral:
						leaf.resolvedSymbol = builtInTypes_char.GetThisInstance();
						break;

					case SyntaxToken.Kind.IntegerLiteral:
						var endsWith = leaf.token.text[leaf.token.text.Length - 1];
						var unsignedDecimal = endsWith == 'u' || endsWith == 'U';
						var longDecimal = endsWith == 'l' || endsWith == 'L';
						if (unsignedDecimal)
						{
							endsWith = leaf.token.text[leaf.token.text.Length - 2];
							longDecimal = endsWith == 'l' || endsWith == 'L';
						}
						else if (longDecimal)
						{
							endsWith = leaf.token.text[leaf.token.text.Length - 2];
							unsignedDecimal = endsWith == 'u' || endsWith == 'U';
						}
						leaf.resolvedSymbol =
							longDecimal ? (unsignedDecimal ? builtInTypes_ulong.GetThisInstance() : builtInTypes_long.GetThisInstance())
							: unsignedDecimal ? builtInTypes_uint.GetThisInstance() : builtInTypes_int.GetThisInstance();
						break;

					case SyntaxToken.Kind.RealLiteral:
						endsWith = leaf.token.text[leaf.token.text.Length - 1];
						leaf.resolvedSymbol =
							endsWith == 'f' || endsWith == 'F' ? builtInTypes_float.GetThisInstance() :
							endsWith == 'm' || endsWith == 'M' ? builtInTypes_decimal.GetThisInstance() :
							builtInTypes_double.GetThisInstance();
						break;

					case SyntaxToken.Kind.StringLiteral:
					case SyntaxToken.Kind.VerbatimStringBegin:
					case SyntaxToken.Kind.VerbatimStringLiteral:
						leaf.resolvedSymbol = builtInTypes_string.GetThisInstance();
						break;

					case SyntaxToken.Kind.BuiltInLiteral:
						leaf.resolvedSymbol = leaf.token.text == "null" ? nullLiteral : builtInTypes_bool.GetThisInstance();
						break;
					
					case SyntaxToken.Kind.Missing:
						return null;
					
					case SyntaxToken.Kind.ContextualKeyword:
						return null;
					
					case SyntaxToken.Kind.Punctuator:
						return null;

					default:
						Debug.LogWarning(leaf.ToString());
						return null;
				}

				if (leaf.resolvedSymbol == null)
					leaf.resolvedSymbol = unknownSymbol;
				if (leaf.semanticError == null && leaf.resolvedSymbol.kind == SymbolKind.Error)
					leaf.semanticError = leaf.resolvedSymbol.name;
			}
			return leaf.resolvedSymbol;
		}

		if (node == null || node.numValidNodes == 0 || node.missing)
			return null;

		int rank;
		SymbolDefinition part = null, dummy = null; // used as non-null return value for explicitly resolving child nodes

//		Debug.Log("Resolving node: " + node);
		switch (node.RuleName)
		{
			case "localVariableType":
				if (node.numValidNodes == 1)
					return ResolveNode(node.ChildAt(0), scope, asMemberOf);
				break;

			case "GET":
			case "SET":
			case "ADD":
			case "REMOVE":
				SymbolDeclaration declaration = null;
				for (var tempNode = node; declaration == null && tempNode != null; tempNode = tempNode.parent)
					declaration = tempNode.declaration;
				if (declaration == null)
					return node.ChildAt(0).resolvedSymbol = unknownSymbol;
				return node.ChildAt(0).resolvedSymbol = declaration.definition;

			case "YIELD":
			case "FROM":
			case "SELECT":
			case "WHERE":
			case "GROUP":
			case "INTO":
			case "ORDERBY":
			case "JOIN":
			case "LET":
			case "ON":
			case "EQUALS":
			case "BY":
			case "ASCENDING":
			case "DESCENDING":
			case "ATTRIBUTETARGET":
				node.ChildAt(0).resolvedSymbol = contextualKeyword;
				return contextualKeyword;

			case "memberName":
				declaration = null;
				while (declaration == null && node != null)
				{
					declaration = node.declaration;
					node = node.parent;
				}
				if (declaration == null)
					return unknownSymbol;
				return declaration.definition;

			case "VAR":
				ParseTree.Node varDeclsNode = null;
				if (node.parent.parent.RuleName == "foreachStatement" && node.parent.parent.numValidNodes >= 6)
				{
					varDeclsNode = node.parent.parent.NodeAt(5);
					if (varDeclsNode != null && varDeclsNode.numValidNodes == 1)
					{
						var elementType = EnumerableElementType(varDeclsNode);
						node.ChildAt(0).resolvedSymbol = elementType;
					}
				}
				else if (node.parent.parent.numValidNodes >= 2)
				{
					varDeclsNode = node.parent.parent.NodeAt(1);
					if (varDeclsNode != null && varDeclsNode.numValidNodes == 1)
					{
						var declNode = varDeclsNode.NodeAt(0);
						if (declNode != null && declNode.numValidNodes == 3)
						{
							var initExpr = ResolveNode(declNode.ChildAt(2));
							var varLeaf = node.ChildAt(0);
							varLeaf.semanticError = null;
							if (initExpr != null && initExpr.kind != SymbolKind.Error)
								varLeaf.resolvedSymbol = initExpr.TypeOf();
							else
								varLeaf.resolvedSymbol = unknownType;
						}
						else
							node.ChildAt(0).resolvedSymbol = unknownType;
					}
				}
				else
					node.ChildAt(0).resolvedSymbol = unknownType;
				return node.ChildAt(0).resolvedSymbol;

			case "type": case "type2":
				var resolvedTypeNode = ResolveNode(node.ChildAt(0), scope, asMemberOf, numTypeArguments, true);
				var typeNodeType = resolvedTypeNode as TypeDefinitionBase;
				if (typeNodeType != null)
				{
					if (node.numValidNodes > 1)
					{
						var nullableShorthand = node.LeafAt(1);
						if (nullableShorthand != null && nullableShorthand.token.text == "?")
						{
							typeNodeType = typeNodeType.MakeNullableType();
						}

						var rankNode = node.NodeAt(-1);
						if (rankNode != null)
						{
							typeNodeType = typeNodeType.MakeArrayType(rankNode.numValidNodes - 1);
						}
					}
					return typeNodeType;
				}
				else if (resolvedTypeNode != null && resolvedTypeNode.kind != SymbolKind.Error)
				{
					var firstLeaf = node.LeafAt(0) ?? node.NodeAt(0).GetFirstLeaf();
					if (firstLeaf != null)
						firstLeaf.semanticError = "Type expected";
				}
				break;

			case "attribute":
				var attributeTypeName = ResolveNode(node.ChildAt(0), scope);
				//if (attributeTypeName == null || attributeTypeName == unknownSymbol || attributeTypeName == unknownType)
				//{
				//    var lastLeaf = ((ParseTree.Node) node.nodes[0]).GetLastLeaf();
				//    var oldText = lastLeaf.token.text;
				//    lastLeaf.token.text += "Attribute";
				//    lastLeaf.resolvedSymbol = null;
				//    attributeTypeName = ResolveNode(node.nodes[0], scope);
				//    lastLeaf.token.text = oldText;
				//}
				if (node.numValidNodes == 2)
					ResolveNode(node.ChildAt(1), null);
				return attributeTypeName;

			case "integralType":
			case "simpleType":
			case "numericType":
			case "floatingPointType":
			case "predefinedType":
			case "typeName":
			case "exceptionClassType":
				var resolvedType = ResolveNode(node.ChildAt(0), scope, asMemberOf, numTypeArguments, true);
				if (resolvedType != null && resolvedType.kind != SymbolKind.Error && !(resolvedType is TypeDefinitionBase))
					node.GetFirstLeaf().semanticError = "Type expected";
				return resolvedType;
			
			case "globalNamespace":
				return ResolveNode(node.ChildAt(0), scope, null, 0);

			case "nonArrayType":
				var nonArrayTypeSymbol = ResolveNode(node.ChildAt(0), scope, asMemberOf, 0, true);
				var nonArrayType = nonArrayTypeSymbol as TypeDefinitionBase;
				if (nonArrayType != null && node.numValidNodes == 2)
					return nonArrayType.MakeNullableType();
				return nonArrayType;

			//case "typeParameterList":
			//    return null;

			case "typeParameter":
				return ResolveNode(node.ChildAt(0), scope, asMemberOf, 0, true) as TypeDefinitionBase;

			case "typeVariableName":
				//asMemberOf = ((SymbolDeclarationScope) scope).declaration.definition;
				return ResolveNode(node.ChildAt(0), scope) as TypeParameterDefinition;

			case "typeOrGeneric":
				if (asMemberOf == null && node.childIndex > 0)
					asMemberOf = ResolveNode(node.parent.ChildAt(node.childIndex - 2), scope, null, 0, true);
				if (node.numValidNodes >= 2)
				{
					var typeArgsListNode = node.NodeAt(1);
					if (typeArgsListNode != null && typeArgsListNode.numValidNodes > 0)
					{
						bool isUnboundType = typeArgsListNode.RuleName == "unboundTypeRank";
						var numTypeArgs = isUnboundType ? typeArgsListNode.numValidNodes - 1 : typeArgsListNode.numValidNodes / 2;
						var typeDefinition = ResolveNode(node.ChildAt(0), scope, asMemberOf, numTypeArgs, true) as TypeDefinition;
						if (typeDefinition == null)
							return node.ChildAt(0).resolvedSymbol;

						if (!isUnboundType)
						{
							var typeArgs = new SymbolReference[numTypeArgs];
							for (var i = 0; i < numTypeArgs; ++i)
								typeArgs[i] = new SymbolReference(typeArgsListNode.ChildAt(1 + 2 * i));
							if (typeDefinition.typeParameters != null)
							{
								var constructedType = typeDefinition.ConstructType(typeArgs);
								node.ChildAt(0).resolvedSymbol = constructedType;
								return constructedType;
							}
						}

						return typeDefinition;
					}
				}
				else if (scope is AttributesScope && node.parent.parent.parent.RuleName == "attribute")
				{
					var lastLeaf = node.LeafAt(0);
					if (asMemberOf != null)
						asMemberOf.ResolveAttributeMember(lastLeaf, scope);
					else
						scope.ResolveAttribute(lastLeaf);

					if (lastLeaf.resolvedSymbol == null)
						lastLeaf.resolvedSymbol = unknownSymbol;
					return lastLeaf.resolvedSymbol;
				}
				return ResolveNode(node.ChildAt(0), scope, asMemberOf, 0, true);

			case "namespaceName":
				var resolvedSymbol = ResolveNode(node.ChildAt(0), scope, asMemberOf, 0, true);
				if (resolvedSymbol != null && resolvedSymbol.kind != SymbolKind.Error && !(resolvedSymbol is NamespaceDefinition))
					node.ChildAt(0).semanticError = "Namespace name expected";
				return resolvedSymbol;

			case "namespaceOrTypeName":
				part = ResolveNode(node.ChildAt(0), scope, null, node.numValidNodes == 1 ? numTypeArguments : 0, true);
				for (var i = 2; i < node.numValidNodes; i += 2)
					part = ResolveNode(node.ChildAt(i), scope, part, i == node.numValidNodes - 1 ? numTypeArguments : 0, true);
				return part;

			case "usingAliasDirective":
				return ResolveNode(node.ChildAt(0), scope);

			case "qualifiedIdentifier":
				part = ResolveNode(node.ChildAt(0), scope) as NamespaceDefinition;
				for (var i = 2; part != null && i < node.numValidNodes; i += 2)
				{
					part = ResolveNode(node.ChildAt(i), scope, part);
					var idNode = node.NodeAt(i);
					if (idNode != null && idNode.numValidNodes == 1)
						idNode.ChildAt(0).resolvedSymbol = part;
				}
				return part;

			case "destructorDeclarator":
				return builtInTypes_void;

			case "memberInitializer":
				ResolveNode(node.ChildAt(0), scope);
				if (node.numValidNodes == 3)
					ResolveNode(node.ChildAt(2), scope);
				return null;

			case "primaryExpression":
				var invokeTarget = part;
				ParseTree.Leaf invokeTargetLeaf = null;
				for (var i = 0; i < node.numValidNodes; ++i)
				{
					var child = node.ChildAt(i);
					var childAsLeaf = child as ParseTree.Leaf;
					if (childAsLeaf != null && childAsLeaf.missing)
						return part;

					var methodNameNode = child as ParseTree.Node;
					SymbolDefinition nextPart = null;

					if (i == 0 && childAsLeaf != null && childAsLeaf.token.text == "new")
					{
						methodNameNode = node.NodeAt(1);
						if (methodNameNode != null && methodNameNode.numValidNodes > 0)
						{
							var nonArrayTypeNode = methodNameNode.RuleName == "nonArrayType" ? methodNameNode : null;
							if (nonArrayTypeNode != null)
							{
								asMemberOf = ResolveNode(nonArrayTypeNode, scope);
								var node3 = node.NodeAt(2);
								if (node3 != null && node3.RuleName == "objectCreationExpression")
								{
									i += 2;
									nextPart = ResolveNodeAsConstructor(node3, scope, asMemberOf);
									if (nextPart != null && nextPart.kind == SymbolKind.Constructor)
									{
										var asMemberOfAsConstructedType = asMemberOf as ConstructedTypeDefinition;
										if (asMemberOfAsConstructedType != null)
											nextPart = asMemberOfAsConstructedType.GetConstructedMember(nextPart);
									}
								}
								else if (node3 != null) // && node3.RuleName == "arrayCreationExpression")
								{
									i += 2;
									nextPart = ResolveNode(node.ChildAt(i), scope, asMemberOf);
								}
							}
							else // methodNameNode is implicitArrayCreationExpression, or anonymousObjectCreationExpression
								nextPart = ResolveNode(methodNameNode, scope);
						}
					}
					else
					{
						// child is primaryExpressionStart, primaryExpressionPart, or anonymousMethodExpression
						
						var primaryExpressionPartNode = i != 0 ? child as ParseTree.Node : null;
						var argumentsNode = primaryExpressionPartNode != null ? primaryExpressionPartNode.NodeAt(0) : null;
						if (argumentsNode != null && argumentsNode.RuleName == "arguments")
						{
							//firstMatchHack = null;
							nextPart = ResolveArgumentsNode(argumentsNode, scope, invokeTargetLeaf, part, asMemberOf);
							//if (nextPart == null)
							//	nextPart = firstMatchHack;
						}
						else
						{
							nextPart = ResolveNode(child, scope, part);
						}
					}
					
					asMemberOf = part;
					
					if (nextPart != null && nextPart.kind != SymbolKind.Error)
					{
						SymbolDefinition method = nextPart.kind == SymbolKind.Method || nextPart.kind == SymbolKind.Constructor ? nextPart : null;
						if (nextPart.kind == SymbolKind.MethodGroup)
						{
							if (methodNameNode.numValidNodes == 2 && !(nextPart is ConstructedMethodGroupDefinition))
							{
								nextPart = ResolveNode(methodNameNode.NodeAt(1), scope, nextPart);
							}
						}
						//if (part.kind == SymbolKind.MethodGroup && ++i < node.numValidNodes)
						//{
						//	methodNameNode = node.NodeAt(i - 1);
						//	child = node.ChildAt(i);
						//	part = ResolveNode(child, scope, part);
						//	if (part != null)
						//		method = part.kind == SymbolKind.Method ? part : null;
						//}
						
						if (method != null)
						{
	//						var asMemberOfConstructedType = asMemberOf as ConstructedTypeDefinition;
	//						if (asMemberOfConstructedType != null)
	//							part = asMemberOfConstructedType.GetConstructedMember(method);
	
							if (methodNameNode != null)
							{
	//							if (methodNameNode.RuleName == "nonArrayType")
	//							{
	//								methodNameNode = methodNameNode.NodeAt(0);
	//							}
	
								if (methodNameNode.RuleName == "primaryExpressionStart")
								{
									var methodNameLeaf = methodNameNode.LeafAt(methodNameNode.numValidNodes < 3 ? 0 : 2);
									if (methodNameLeaf != null)
										methodNameLeaf.resolvedSymbol = nextPart;
								}
								else if (methodNameNode.RuleName == "primaryExpressionPart")
								{
									var accessIdentifierNode = methodNameNode.NodeAt(0);
									if (accessIdentifierNode != null && accessIdentifierNode.RuleName == "accessIdentifier")
									{
										var methodNameLeaf = accessIdentifierNode.LeafAt(1);
										if (methodNameLeaf != null)
											methodNameLeaf.resolvedSymbol = nextPart;
									}
								}
	//							else if (methodNameNode.RuleName == "nonArrayType")
	//							{
	//								var nameNode = methodNameNode.ChildAt(0);
	//								while (nameNode is ParseTree.Node)
	//								{
	//									var nameNodeAsNode = nameNode as ParseTree.Node;
	//									if (nameNodeAsNode.RuleName == "namespaceOrTypeName")
	//										nameNode = nameNodeAsNode.ChildAt(-1);
	//									else
	//										nameNode = nameNodeAsNode.ChildAt(0);
	//								}
	//								nameNode.resolvedSymbol = method;
	//							}
							}
							else
							{
								node.ChildAt(i).resolvedSymbol = method;
							}
						}
					}
					
					var childNode = child as ParseTree.Node;
					if (childNode != null)
						childNode = childNode.NodeAt(0);
					if (childNode != null)// && childNode.RuleName == "accessIdentifier")
					{
						if (nextPart != null && invokeTarget != null && !(invokeTarget is TypeDefinitionBase) &&
							(nextPart is TypeDefinitionBase || nextPart.IsStatic))
						{
							switch (invokeTarget.kind)
							{
							case SymbolKind.ConstantField:
							case SymbolKind.Field:
							case SymbolKind.Property:
							case SymbolKind.Indexer:
							case SymbolKind.Event:
							case SymbolKind.LocalConstant:
							case SymbolKind.Variable:
							case SymbolKind.ForEachVariable:
							case SymbolKind.FromClauseVariable:
							case SymbolKind.Parameter:
							case SymbolKind.CatchParameter:
							case SymbolKind.Instance:
								var parentType = nextPart.parentSymbol;
								while (parentType != null && !(parentType is TypeDefinitionBase))
									parentType = parentType.parentSymbol;
								if (parentType != null && parentType.NumTypeParameters == 0 &&
									invokeTarget.NumTypeParameters == 0 && invokeTarget.name == parentType.name)
								{
									invokeTargetLeaf.resolvedSymbol = parentType;
									//Debug.Log(invokeTarget.GetTooltipText() + " ## " + (nextPart != null ? nextPart.GetTooltipText() : "null"));
								}
								break;
							}
						}
					}
					part = nextPart;
					if (part == null)// || part.kind == SymbolKind.Error)
						break;

					if (part.kind == SymbolKind.Method)
					{
						var currentNode = child as ParseTree.Node;
						if (currentNode != null)
							currentNode = currentNode.RuleName == "primaryExpressionPart" ? currentNode.NodeAt(0) : null;
						if (currentNode == null || currentNode.RuleName != "arguments")
							part = part.parentSymbol;
					}

					if (part.kind == SymbolKind.Method)
					{
						var returnType = (part = part.TypeOf()) as TypeDefinitionBase;
						if (returnType != null)
							part = returnType.GetThisInstance();
					}
					else if (part.kind == SymbolKind.Constructor)
					{
						part = ((TypeDefinitionBase) part.parentSymbol).GetThisInstance();
					}
					
					if (part == null)// || part.kind == SymbolKind.Error)
						break;
					
					if (part.kind != SymbolKind.MethodGroup)
					{
						invokeTarget = part;
					}
					
					var partNode = child as ParseTree.Node;
					if (partNode != null)
					{
						if (partNode.RuleName == "primaryExpressionPart")
						{
							var accessIdentifierNode = partNode.NodeAt(0);
							if (accessIdentifierNode != null && accessIdentifierNode.RuleName == "accessIdentifier")
							{
								invokeTargetLeaf = accessIdentifierNode.LeafAt(1);
							}
							else
							{
								invokeTargetLeaf = null;
							}
						}
						else if (partNode.RuleName == "primaryExpressionStart")
						{
							var identifierLeaf = partNode.LeafAt(0);
							if (identifierLeaf != null && identifierLeaf.token.tokenKind == SyntaxToken.Kind.Identifier)
								invokeTargetLeaf = partNode.LeafAt(partNode.numValidNodes == 3 ? 2 : 0);
						}
					}
				}
				return part ?? unknownSymbol;

			case "primaryExpressionStart":
				if (node.numValidNodes == 1)
					return ResolveNode(node.ChildAt(0), scope, null);
				if (node.numValidNodes == 2)
				{
					var typeArgsNode = node.NodeAt(1);
					if (typeArgsNode != null && typeArgsNode.RuleName == "typeArgumentList")
						numTypeArguments = typeArgsNode.numValidNodes / 2;
					asMemberOf = ResolveNode(node.ChildAt(0), scope, null, numTypeArguments);
					return ResolveNode(typeArgsNode, scope, asMemberOf);
					//return ResolveNode(node.ChildAt(0), scope, null, numTypeArguments);
				}
				if (node.numValidNodes == 3)
				{
					part = ResolveNode(node.ChildAt(0), scope, null);
					return ResolveNode(node.ChildAt(2), scope, part);
				}
				break;

			case "primaryExpressionPart":
				if (asMemberOf == null)
				{
					asMemberOf = ResolveNode(node.FindPreviousNode(), scope);
					if (asMemberOf != null && asMemberOf.kind == SymbolKind.Method)
						asMemberOf = asMemberOf.TypeOf();
				}
				if (asMemberOf != null)
					return ResolveNode(node.ChildAt(0), scope, asMemberOf);
				break;

			case "brackets":
				if (asMemberOf == null)
					asMemberOf = ResolveNode(node.FindPreviousNode(), scope);
				if (asMemberOf != null)
				{
				//	Debug.LogWarning("Resolving brackets on " + asMemberOf.GetTooltipText());
					var arrayType = asMemberOf.TypeOf() as ArrayTypeDefinition;
					if (arrayType != null && arrayType.elementType != null)
					{
					//	UnityEngine.Debug.Log("    elementType " + arrayType.elementType.TypeOf());
						return arrayType.elementType.GetThisInstance();
					}
					if (node.numValidNodes == 3)
					{
						var expressionListNode = node.NodeAt(1);
						if (expressionListNode != null && expressionListNode.numValidNodes >= 1)
						{
							var argumentTypes = new TypeDefinitionBase[(expressionListNode.numValidNodes + 1) / 2];
							for (var i = 0; i < argumentTypes.Length; ++i)
							{
								var expression = ResolveNode(expressionListNode.ChildAt(i*2), scope);
								if (expression == null)
									goto default;
								argumentTypes[i] = expression.TypeOf() as TypeDefinitionBase;
							}
							var typeOf = asMemberOf.TypeOf() as TypeDefinitionBase;
							var indexer = typeOf == null ? null : typeOf.GetIndexer(argumentTypes);
							if (indexer != null)
							{
								typeOf = indexer.TypeOf() as TypeDefinitionBase;
								return typeOf == null ? null : typeOf.GetThisInstance();
							}
							else
							{
								return unknownSymbol;
							}
						}
					}
				}
				break;

			case "accessIdentifier":
				if (asMemberOf == null)
				{
					asMemberOf = ResolveNode(node.FindPreviousNode(), scope);
					if (asMemberOf != null && asMemberOf.kind == SymbolKind.Method)
						asMemberOf = asMemberOf.TypeOf();
				}
				if (node.numValidNodes == 2)
				{
					var node1 = node.ChildAt(1);
					if (!node1.missing)
						return ResolveNode(node.ChildAt(1), scope, asMemberOf);
				}
				else if (node.numValidNodes == 3)
				{
					var typeArgsNode = node.NodeAt(2);
					if (typeArgsNode != null && typeArgsNode.RuleName == "typeArgumentList")
						numTypeArguments = typeArgsNode.numValidNodes / 2;
					asMemberOf = ResolveNode(node.ChildAt(1), scope, asMemberOf, numTypeArguments);
					return ResolveNode(typeArgsNode, scope, asMemberOf);
				}
				return asMemberOf;

			case "typeArgumentList":
				if (asMemberOf == null)
				{
					//Debug.Log("asMemberOf is null / resolving " + node);
					asMemberOf = ResolveNode(node.FindPreviousNode(), scope);
				}
				numTypeArguments = node.numValidNodes / 2;
				var genericMethodGroup = asMemberOf as MethodGroupDefinition;
				if (genericMethodGroup != null)
				{
					var typeArgs = new SymbolReference[numTypeArguments];
					for (var i = 0; i < numTypeArguments; ++i)
						typeArgs[i] = new SymbolReference(node.ChildAt(2 * i + 1));
					return genericMethodGroup.ConstructMethodGroup(typeArgs);
				}
				else
				{
					var genericType = asMemberOf as TypeDefinition;
					if (genericType != null)
					{
						var typeArgs = new SymbolReference[numTypeArguments];
						for (var i = 0; i < numTypeArguments; ++i)
							typeArgs[i] = new SymbolReference(node.ChildAt(2 * i + 1));
						var constructedType = genericType.ConstructType(typeArgs);
						if (constructedType != null)
						{
							var prevNode = node.FindPreviousNode() as ParseTree.Leaf;
							if (prevNode != null)
								prevNode.resolvedSymbol = constructedType;
							return constructedType;
						}
					}
				}
				return asMemberOf;
			
			case "attributeArguments":
				if (asMemberOf == null)
				{
					var prevNode = node.FindPreviousNode();
					asMemberOf = ResolveNode(prevNode, scope);
				}
				var attributeArgumentListNode = node.numValidNodes >= 2 ? node.NodeAt(1) : null;
				if (attributeArgumentListNode != null)
					ResolveNode(attributeArgumentListNode, scope, asMemberOf);
				return resolvedChildren;
			
			case "arguments":
				if (asMemberOf == null)
				{
					var prevNode = node.FindPreviousNode();
					asMemberOf = ResolveNode(prevNode, scope);
					if (asMemberOf == null)
					{
						return null;
					}
				}
//				if (node.numValidNodes < 2)
//					return null;

				var argumentListNode = node.numValidNodes >= 2 ? node.NodeAt(1) : null;
				if (argumentListNode != null)
					ResolveNode(argumentListNode, scope);
				
				if (node.parent.RuleName == "attribute" || node.parent.RuleName == "constructorInitializer")
					return unknownSymbol;

				var methodGroup = asMemberOf as MethodGroupDefinition;
				if (methodGroup != null)
				{
					asMemberOf = methodGroup.ResolveMethodOverloads(argumentListNode, null, scope);
					SymbolDefinition method = asMemberOf as MethodDefinition;
					if (method == null && asMemberOf != null && asMemberOf.kind == SymbolKind.Method)
						method = asMemberOf as ConstructedSymbolReference;
					if (method != null)
					{
						var prevNode = node.FindPreviousNode() as ParseTree.Node;
						var idLeaf = prevNode.LeafAt(0) ?? prevNode.NodeAt(0).LeafAt(1);
						if (method.kind == SymbolKind.Error)
						{
							idLeaf.resolvedSymbol = methodGroup;
							idLeaf.semanticError = method.name;
						}
						else if (idLeaf.resolvedSymbol != method)
						{
							idLeaf.resolvedSymbol = method;
						//	ResolveNode(argumentListNode);
						}
						
						return method;
						//var returnType = method.TypeOf() as TypeDefinitionBase;
						//return returnType != null ? returnType.GetThisInstance() : null;
					}
				}
				else if (asMemberOf.kind == SymbolKind.MethodGroup)
				{
					var constructedMethodGroup = asMemberOf as ConstructedSymbolReference;
					if (constructedMethodGroup != null)
						asMemberOf = constructedMethodGroup.ResolveMethodOverloads(argumentListNode, null, scope);
					SymbolDefinition method = asMemberOf as MethodDefinition;
					if (method == null && asMemberOf != null && asMemberOf.kind == SymbolKind.Method)
						method = asMemberOf as ConstructedSymbolReference;
					if (method != null)
					{
						var prevNode = node.FindPreviousNode() as ParseTree.Node;
						var idLeaf = prevNode.LeafAt(0) ?? prevNode.NodeAt(0).LeafAt(1);
						if (method.kind == SymbolKind.Error)
						{
							idLeaf.resolvedSymbol = methodGroup;
							idLeaf.semanticError = method.name;
						}
						else if (idLeaf.resolvedSymbol != method)
						{
							idLeaf.resolvedSymbol = method;
						//	ResolveNode(argumentListNode);
						}
						
						return method;
						//var returnType = method.TypeOf() as TypeDefinitionBase;
						//return returnType != null ? returnType.GetThisInstance() : null;
					}
				}
				else if (asMemberOf.kind != SymbolKind.Method && asMemberOf.kind != SymbolKind.Error)
				{
					var typeOf = asMemberOf.TypeOf() as TypeDefinitionBase;
					if (typeOf == null || typeOf.kind == SymbolKind.Error)
						return unknownType;

					var returnType = asMemberOf.kind == SymbolKind.Delegate ? typeOf :
						typeOf.kind == SymbolKind.Delegate ? typeOf.TypeOf() as TypeDefinitionBase : null;
					if (returnType != null)
						return returnType.GetThisInstance();
					
					//Debug.Log(">> " + asMemberOf.GetTooltipText());
					//Debug.Log(node);
//					if (asMemberOf.kind != SymbolKind.Event)
					node.LeafAt(0).semanticError = "Cannot invoke symbol";
				}
				
				return asMemberOf;

			case "argument":
				if (node.numValidNodes >= 1)
				{
					if (node.numValidNodes == 1)
						return ResolveNode(node.ChildAt(0), scope);
					else
						ResolveNode(node.ChildAt(0), scope);
				}
				if (node.numValidNodes == 3)
				{
					return ResolveNode(node.ChildAt(2), scope);
				}
				return resolvedChildren;

			case "attributeArgument":
				if (node.numValidNodes >= 1)
				{
					if (node.numValidNodes == 1)
						return ResolveNode(node.ChildAt(0), scope);
					else
						ResolveNode(node.ChildAt(0), scope, asMemberOf);
				}
				if (node.numValidNodes == 3)
				{
					return ResolveNode(node.ChildAt(2), scope);
				}
				return resolvedChildren;
			
			case "argumentList":
				for (var i = 0; i < node.numValidNodes; i += 2)
					dummy = ResolveNode(node.ChildAt(i), scope);
				return dummy;

			case "attributeArgumentList":
				for (var i = 0; i < node.numValidNodes; i += 2)
					dummy = ResolveNode(node.ChildAt(i), scope, asMemberOf);
				return resolvedChildren;

			case "argumentValue":
				return ResolveNode(node.ChildAt(-1), scope);

			case "argumentName":
				//return ResolveNode(node.ChildAt(0), asMemberOf: asMemberOf);
				                                       // arguments
				                                // argumentList
				                         // argument
				var parameterNameLeaf = node.LeafAt(0);
				if (parameterNameLeaf == null)
					return unknownSymbol;
				var arguments = node.parent.parent.parent;
				var invokableNode = arguments.FindPreviousNode();
				var invokableSymbol = ResolveNode(invokableNode);
				if (invokableSymbol.kind != SymbolKind.MethodGroup)
					invokableSymbol = invokableSymbol.parentSymbol;
				methodGroup = invokableSymbol as MethodGroupDefinition;
				if (methodGroup == null)
					return parameterNameLeaf.resolvedSymbol = unknownSymbol;
				return methodGroup.ResolveParameterName(parameterNameLeaf);
			
			case "attributeMemberName":
				var asType = asMemberOf as TypeDefinitionBase;
				if (asType != null)
					return ResolveNode(node.ChildAt(0), scope, asType.GetThisInstance());
				return unknownSymbol;

			case "castExpression":
				if (node.numValidNodes == 4)
				{
					var target = ResolveNode(node.ChildAt(3), scope);
					if (target is TypeDefinitionBase || target != null && target.kind == SymbolKind.Namespace)
					{
						ResolveNode(node.ChildAt(1), scope);
						return target;
					}
				}
				var castType = ResolveNode(node.ChildAt(1), scope) as TypeDefinitionBase;
					if (castType != null)
						return castType.GetThisInstance();
				break;

			case "typeofExpression":
				if (node.numValidNodes >= 3)
					ResolveNode(node.ChildAt(2), scope);
				return ((TypeDefinitionBase) ReflectedTypeReference.ForType(typeof(Type)).definition).GetThisInstance();
				//var tempAssemblyDefinition = AssemblyDefinition.FromAssembly(typeof(System.Type).Assembly);
				//return tempAssemblyDefinition.FindNamespace("System").FindName("Type");

			case "defaultValueExpression":
				if (node.numValidNodes >= 3)
				{
					var typeNode = ResolveNode(node.ChildAt(2), scope) as TypeDefinitionBase;
					if (typeNode != null)
						return typeNode.GetThisInstance();
				}
				break;

			case "sizeofExpression":
				if (node.numValidNodes >= 3)
					ResolveNode(node.ChildAt(2), scope);
				return builtInTypes_int.GetThisInstance();

			case "checkedExpression":
			case "uncheckedExpression":
				if (node.numValidNodes >= 3)
					return ResolveNode(node.ChildAt(2), scope);
				return unknownSymbol;

			case "assignment":
				if (node.numValidNodes >= 3)
					ResolveNode(node.ChildAt(2), scope);
				return ResolveNode(node.ChildAt(0), scope);
			
			case "localVariableInitializer":
			case "variableReference":
			case "expression":
			case "constantExpression":
			case "nonAssignmentExpression":
				return ResolveNode(node.ChildAt(0), scope);

			case "parenExpression":
				return ResolveNode(node.ChildAt(1), scope);

			case "nullCoalescingExpression":
				for (var i = 2; i < node.numValidNodes; i += 2)
					ResolveNode(node.ChildAt(i), scope); // HACK
				return ResolveNode(node.ChildAt(0), scope); // HACK

			case "conditionalExpression":
				if (node.numValidNodes >= 3)
				{
					ResolveNode(node.ChildAt(0), scope);
					var typeRight = nullLiteral;
					if (node.numValidNodes == 5)
						typeRight = ResolveNode(node.ChildAt(4), scope);
					var typeLeft = ResolveNode(node.ChildAt(2), scope); // HACK
					return typeLeft != nullLiteral ? typeLeft : typeRight;
				}
				else
					return ResolveNode(node.ChildAt(0), scope, asMemberOf);

			case "unaryExpression":
				if (node.numValidNodes == 1)
					return ResolveNode(node.ChildAt(0), scope, null);
				if (node.ChildAt(0) is ParseTree.Node)
					return ResolveNode(node.ChildAt(0), scope, null);
				return ResolveNode(node.ChildAt(1), scope, null);

			case "inclusiveOrExpression":
			case "exclusiveOrExpression":
			case "andExpression":
			case "shiftExpression":
			case "additiveExpression":
			case "multiplicativeExpression":
				for (var i = 2; i < node.numValidNodes; i += 2)
					ResolveNode(node.ChildAt(i), scope);
				return ResolveNode(node.ChildAt(0), scope); // HACK

			case "arrayCreationExpression":
				if (asMemberOf == null)
					asMemberOf = ResolveNode(node.FindPreviousNode());
				var resultType = asMemberOf as TypeDefinitionBase;
				if (resultType == null)
					return unknownType.MakeArrayType(1);

				var rankSpecifiersNode = node.FindChildByName("rankSpecifiers") as ParseTree.Node;
				if (rankSpecifiersNode == null || rankSpecifiersNode.childIndex > 0)
				{
					var expressionListNode = node.NodeAt(1);
					if (expressionListNode != null && expressionListNode.RuleName == "expressionList")
						resultType = resultType.MakeArrayType(1 + expressionListNode.numValidNodes / 2);
				}
				if (rankSpecifiersNode != null && rankSpecifiersNode.numValidNodes != 0)
				{
					for (var i = 1; i < rankSpecifiersNode.numValidNodes; i += 2)
					{
						rank = 1;
						while (i < rankSpecifiersNode.numValidNodes && rankSpecifiersNode.ChildAt(i).IsLit(","))
						{
							++rank;
							++i;
						}
						resultType = resultType.MakeArrayType(rank);
					}
				}

				var initializerNode = node.NodeAt(-1);
				if (initializerNode != null && initializerNode.RuleName == "arrayInitializer")
					ResolveNode(initializerNode);

				return (resultType ?? unknownType).GetThisInstance();

			case "implicitArrayCreationExpression":
				resultType = null;

				var rankSpecifierNode = node.NodeAt(0);
				rank = rankSpecifierNode != null && rankSpecifierNode.numValidNodes > 0 ? rankSpecifierNode.numValidNodes - 1 : 1;

				initializerNode = node.NodeAt(1);
				var elements = initializerNode != null ? ResolveNode(initializerNode) : null;
				if (elements != null)
					resultType = (elements.TypeOf() as TypeDefinitionBase ?? unknownType).MakeArrayType(rank);

				return (resultType ?? unknownType).GetThisInstance();

			case "arrayInitializer":
				if (node.numValidNodes >= 2)
					if (!node.ChildAt(1).IsLit("}"))
						return ResolveNode(node.ChildAt(1), scope);
				return unknownType;

			case "variableInitializerList":
				TypeDefinitionBase commonType = null;
				for (var i = 0; i < node.numValidNodes; i += 2)
				{
					var type = (ResolveNode(node.ChildAt(i), scope) ?? unknownSymbol).TypeOf() as TypeDefinitionBase;
					if (type != null)
					{
						if (commonType == null)
						{
							commonType = type;
						}
						else
						{
							// HACK!!!
							if (commonType.DerivesFrom(type))
								commonType = type;
						}
					}
				}
				return commonType;

			case "variableInitializer":
				return ResolveNode(node.ChildAt(0), scope);

			case "conditionalOrExpression":
				if (node.numValidNodes == 1)
				{
					node = node.NodeAt(0);
					goto case "conditionalAndExpression";
				}
				for (var i = 0; i < node.numValidNodes; i += 2)
					ResolveNode(node.ChildAt(i), scope);
				return builtInTypes_bool;

			case "conditionalAndExpression":
				if (node.numValidNodes == 1)
				{
					node = node.NodeAt(0);
					goto case "inclusiveOrExpression";
				}
				for (var i = 0; i < node.numValidNodes; i += 2)
					ResolveNode(node.ChildAt(i), scope);
				return builtInTypes_bool;

			case "equalityExpression":
				if (node.numValidNodes == 1)
				{
					node = node.NodeAt(0);
					goto case "relationalExpression";
				}
				for (var i = 0; i < node.numValidNodes; i += 2 )
					ResolveNode(node.ChildAt(i), scope);
				return builtInTypes_bool;

			case "relationalExpression":
				if (node.numValidNodes == 1)
				{
					node = node.NodeAt(0);
					goto case "shiftExpression";
				}
				part = ResolveNode(node.ChildAt(0), scope);
				for (var i = 2; i < node.numValidNodes; i += 2)
				{
					if (node.ChildAt(i - 1).IsLit("as"))
					{
						part = ResolveNode(node.ChildAt(i), scope);
						if (part is TypeDefinitionBase)
							part = (part as TypeDefinitionBase).GetThisInstance();
					}
					else
					{
						ResolveNode(node.ChildAt(i), scope);
						part = builtInTypes_bool.GetThisInstance();
					}
				}
				return part;

			case "booleanExpression":
				ResolveNode(node.ChildAt(0), scope);
				return builtInTypes_bool;

			case "lambdaExpression":
				ResolveNode(node.ChildAt(0), scope);
			//	if (node.numValidNodes == 3)
			//		ResolveNode(node.ChildAt(2), scope);
				var nodeScope = node.scope as SymbolDeclarationScope;
				if (nodeScope != null && nodeScope.declaration != null)
					return nodeScope.declaration.definition;
				return unknownSymbol;

			case "lambdaExpressionBody":
				var expressionNode = node.NodeAt(0);
				if (expressionNode != null)
					return ResolveNode(expressionNode);
				return null;

			case "objectCreationExpression":
				var objectType = (ResolveNode(node.FindPreviousNode(), scope) ?? unknownType).TypeOf() as TypeDefinitionBase;
				return objectType != null ? objectType.GetThisInstance() : null;

			case "queryExpression":
				var queryBodyNode = node.NodeAt(1);
				if (queryBodyNode != null)
				{
					var selectClauseNode = queryBodyNode.FindChildByName("selectClause") as ParseTree.Node;
					if (selectClauseNode != null)
					{
						var selectExpressionNode = selectClauseNode.NodeAt(1);
						if (selectExpressionNode != null)
						{
							var element = ResolveNode(selectExpressionNode);
							if (element != null)
							{
								var elementType = element.TypeOf() as TypeDefinitionBase;
								if (elementType != null)
								{
									var genericType = builtInTypes_IEnumerable_1.ConstructType(new[]{ new SymbolReference(elementType) });
									return genericType.GetThisInstance();
								}
							}
						}
					}
				}
				return unknownSymbol;
				
			case "classMemberDeclaration":
				return null;

			case "implicitAnonymousFunctionParameterList":
			case "implicitAnonymousFunctionParameter":
			case "explicitAnonymousFunctionSignature":
			case "explicitAnonymousFunctionParameterList":
			case "explicitAnonymousFunctionParameter":
			case "anonymousFunctionSignature":
			case "qid":
			case "qidStart":
			case "qidPart":
			case "typeParameterList":
			case "constructorInitializer":
			case "interfaceMemberDeclaration":
			case "collectionInitializer":
			case "elementInitializerList":
			case "elementInitializer":
			case "methodHeader":
				return null;

			default:
		//		if (missingResolveNodePaths.Add(node.RuleName))
		//			UnityEngine.Debug.Log("TODO: Add ResolveNode path for " + node.RuleName);
				return null;
		}

	//	Debug.Log("TODO: Canceled ResolveNode for " + node.RuleName);
		return null;
	}

	protected virtual SymbolDefinition GetIndexer(TypeDefinitionBase[] argumentTypes)
	{
		return null;
	}

	public virtual SymbolDefinition FindName(string memberName, int numTypeParameters, bool asTypeOnly)
	{
		memberName = DecodeId(memberName);
		
		SymbolDefinition definition;
		if (!members.TryGetValue(memberName, numTypeParameters, out definition))
		{
			var marker = memberName.IndexOf('`');
			if (marker > 0)
			{
				Debug.LogError("FindName!!! " + memberName);
				members.TryGetValue(memberName.Substring(0, marker), numTypeParameters, out definition);
			}
		}
		if (asTypeOnly && definition != null && definition.kind != SymbolKind.Namespace && !(definition is TypeDefinitionBase))
			return null;
		return definition;
	}

	public virtual void GetCompletionData(Dictionary<string, SymbolDefinition> data, bool fromInstance, AssemblyDefinition assembly)
	{
		var tp = GetTypeParameters();
		if (tp != null)
		{
			for (var i = 0; i < tp.Count; ++i)
			{
				TypeParameterDefinition p = tp[i];
				if (!data.ContainsKey(p.name))
					data.Add(p.name, p);
			}
		}

		GetMembersCompletionData(data, fromInstance ? 0 : BindingFlags.Static, AccessLevelMask.Any, assembly);
	//	base.GetCompletionData(data, assembly);
	}

	public virtual void GetMembersCompletionData(Dictionary<string, SymbolDefinition> data, BindingFlags flags, AccessLevelMask mask, AssemblyDefinition assembly)
	{
		if ((mask & AccessLevelMask.Public) != 0)
		{
			if (assembly.InternalsVisibleIn(this.Assembly))
				mask |= AccessLevelMask.Internal;
			else
				mask &= ~AccessLevelMask.Internal;
		}
		
		flags = flags & (BindingFlags.Static | BindingFlags.Instance);
		bool onlyStatic = flags == BindingFlags.Static;
		bool onlyInstance = flags == BindingFlags.Instance;

		foreach (var m in members)
		{
			if (m.kind == SymbolKind.Namespace)
			{
				if (!data.ContainsKey(m.ReflectionName))
					data.Add(m.ReflectionName, m);
			}
			else if (m.kind != SymbolKind.MethodGroup)
			{
				if ((onlyStatic ? !m.IsInstanceMember : onlyInstance ? m.IsInstanceMember : true)
					&& m.IsAccessible(mask)
					&& m.kind != SymbolKind.Constructor && m.kind != SymbolKind.Destructor && m.kind != SymbolKind.Indexer
					&& !data.ContainsKey(m.ReflectionName))
				{
					data.Add(m.ReflectionName, m);
				}
			}
			else
			{
				var methodGroup = m as MethodGroupDefinition;
				foreach (var method in methodGroup.methods)
					if ((onlyStatic ? method.IsStatic : onlyInstance ? !method.IsStatic : true)
						&& method.IsAccessible(mask)
						&& method.kind != SymbolKind.Constructor && method.kind != SymbolKind.Destructor && method.kind != SymbolKind.Indexer
						&& !data.ContainsKey(m.ReflectionName))
					{
						data.Add(m.ReflectionName, method);
					}
			}
		}
	}
	
	public bool IsInstanceMember
	{
		get
		{
			return !IsStatic && kind != SymbolKind.ConstantField && !(this is TypeDefinitionBase);
		}
	}
	
	public bool IsSealed
	{
		get
		{
			return (modifiers & Modifiers.Sealed) != 0;
		}
	}

	public virtual bool IsStatic
	{
		get
		{
			return (modifiers & Modifiers.Static) != 0;
		}
		set
		{
			if (value)
				modifiers |= Modifiers.Static;
			else
				modifiers &= ~Modifiers.Static;
		}
	}

	public bool IsPublic
	{
		get
		{
			return (modifiers & Modifiers.Public) != 0 ||
				(kind == SymbolKind.Namespace) ||
				parentSymbol != null && (
					parentSymbol.parentSymbol != null
					&& (kind == SymbolKind.Method || kind == SymbolKind.Indexer)
					&& (parentSymbol.parentSymbol.kind == SymbolKind.Interface)
					||
					(kind == SymbolKind.Property || kind == SymbolKind.Event)
					&& (parentSymbol.kind == SymbolKind.Interface)
				);
		}
		set
		{
			if (value)
				modifiers |= Modifiers.Public;
			else
				modifiers &= ~Modifiers.Public;
		}
	}

	public bool IsInternal
	{
		get
		{
			return (modifiers & Modifiers.Internal) != 0 ||
				kind != SymbolKind.Namespace && (modifiers & Modifiers.Public) == 0 && parentSymbol != null && parentSymbol.kind == SymbolKind.Namespace;
		}
		set
		{
			if (value)
				modifiers |= Modifiers.Internal;
			else
				modifiers &= ~Modifiers.Internal;
		}
	}

	public bool IsProtected
	{
		get
		{
			return (modifiers & Modifiers.Protected) != 0;
		}
		set
		{
			if (value)
				modifiers |= Modifiers.Protected;
			else
				modifiers &= ~Modifiers.Protected;
		}
	}
	
	public bool IsPrivate
	{
		get
		{
			return (modifiers & (Modifiers.Protected | Modifiers.Internal | Modifiers.Public)) == 0;
		}
	}
	
	public bool IsAbstract
	{
		get
		{
			return (modifiers & Modifiers.Abstract) != 0;
		}
		set
		{
			if (value)
				modifiers |= Modifiers.Abstract;
			else
				modifiers &= ~Modifiers.Abstract;
		}
	}
	
	public bool IsPartial
	{
		get { return (modifiers & Modifiers.Partial) != 0; }
	}
	
	//public virtual bool IsGeneric
	//{
	//	get
	//	{
	//		return false;
	//	}
	//}

	public AssemblyDefinition Assembly
	{
		get
		{
			var assembly = this;
			while (assembly != null)
			{
				var result = assembly as AssemblyDefinition;
				if (result != null)
					return result;
				assembly = assembly.parentSymbol;
			}
			return null;
		}
	}

	public virtual bool IsSameType(TypeDefinitionBase type)
	{
		return type == this;
	}

	public bool IsSameOrParentOf(TypeDefinitionBase type)
	{
		var constructedType = this as ConstructedTypeDefinition;
		var thisType = constructedType != null ? constructedType.genericTypeDefinition : this;
		while (type != null)
		{
			if (type == thisType)
				return true;
			constructedType = type as ConstructedTypeDefinition;
			type = (constructedType != null ? constructedType.genericTypeDefinition : type).parentSymbol as TypeDefinitionBase;
		}
		return false;
	}

	public virtual TypeDefinitionBase TypeOfTypeParameter(TypeParameterDefinition tp)
	{
		if (parentSymbol != null)
			return parentSymbol.TypeOfTypeParameter(tp);
		return tp;
	}

	public virtual bool IsAccessible(AccessLevelMask accessLevelMask)
	{
		if (accessLevelMask == AccessLevelMask.None)
			return false;
		if (IsPublic)
			return true;
		if (IsProtected && (accessLevelMask & AccessLevelMask.Protected) != 0)
			return true;
		if (IsInternal && (accessLevelMask & AccessLevelMask.Internal) != 0)
			return true;

		return (accessLevelMask & AccessLevelMask.Private) != 0;
	}

	public int NumTypeParameters {
		get {
			var typeParameters = GetTypeParameters();
			return typeParameters != null ? typeParameters.Count : 0;
		}
	}
}

static class DictExtensions
{
	public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
	{
		return "{" + string.Join(",", dictionary.Select(kv => kv.Key.ToString() + "=" + kv.Value.ToString()).ToArray()) + "}";
	}
}

public class SymbolDeclaration //: IVisitableTreeNode<SymbolDeclaration, SymbolDeclaration>
{
	public SymbolDefinition definition;
	public Scope scope;

	public SymbolKind kind;

	public ParseTree.Node parseTreeNode;
	public Modifiers modifiers;
	public int numTypeParameters;

	protected string name;

	//public SymbolDeclaration parentDeclaration;
	//public List<SymbolDeclaration> nestedDeclarations = new List<SymbolDeclaration>();

	public SymbolDeclaration() {}

	public SymbolDeclaration(string name)
	{
		this.name = name;
	}

	public bool IsValid()
	{
		var node = parseTreeNode;
		if (node != null)
		{
			while (node.parent != null)
				node = node.parent;
			if (node.RuleName == "compilationUnit")
				return true;
		}

		if (scope != null)
		{
			scope.RemoveDeclaration(this);
			++ParseTree.resolverVersion;
			if (ParseTree.resolverVersion == 0)
				++ParseTree.resolverVersion;
		}
		else if (definition != null)
		{
			Debug.Log("Scope is null for declaration " + name + ". Removing " + definition);
			if (definition.parentSymbol != null)
				definition.parentSymbol.RemoveDeclaration(this);
		}
		scope = null;
		return false;
	}
	
	public bool IsPartial
	{
		get { return (modifiers & Modifiers.Partial) != 0; }
	}

	public ParseTree.BaseNode NameNode()
	{
		if (parseTreeNode == null || parseTreeNode.numValidNodes == 0)
			return null;

		ParseTree.BaseNode nameNode = null;
		switch (parseTreeNode.RuleName)
		{
			case "namespaceDeclaration":
				nameNode = parseTreeNode.ChildAt(1);
				var nameNodeAsNode = nameNode as ParseTree.Node;
				if (nameNodeAsNode != null && nameNodeAsNode.numValidNodes != 0)
					nameNode = nameNodeAsNode.ChildAt(-1) ?? nameNode;
				break;

			case "usingAliasDirective":
				nameNode = parseTreeNode.ChildAt(0);
				break;

			case "interfaceDeclaration":
			case "structDeclaration":
			case "classDeclaration":
			case "enumDeclaration":
				nameNode = parseTreeNode.ChildAt(1);
				break;

			case "delegateDeclaration":
				nameNode = parseTreeNode.ChildAt(2);
				break;

			case "eventDeclarator":
			case "eventWithAccessorsDeclaration":
			case "propertyDeclaration":
			case "interfacePropertyDeclaration":
			case "variableDeclarator":
			case "localVariableDeclarator":
			case "constantDeclarator":
			case "interfaceMethodDeclaration":
			case "catchExceptionIdentifier":
				nameNode = parseTreeNode.ChildAt(0);
				break;

			case "methodDeclaration":
			case "constructorDeclaration":
				var methodHeaderNode = parseTreeNode.NodeAt(0);
				if (methodHeaderNode != null && methodHeaderNode.numValidNodes > 0)
					nameNode = methodHeaderNode.ChildAt(0);
				break;

			case "methodHeader":
			case "constructorDeclarator":
				nameNode = parseTreeNode.ChildAt(0);
				break;

			case "destructorDeclarator":
				nameNode = parseTreeNode.FindChildByName("IDENTIFIER");
				break;

			case "fixedParameter":
			case "operatorParameter":
			case "parameterArray":
			case "explicitAnonymousFunctionParameter":
				nameNode = parseTreeNode.FindChildByName("NAME");
				break;

			case "implicitAnonymousFunctionParameter":
				nameNode = parseTreeNode.ChildAt(0);
				break;

			case "typeParameter":
				nameNode = parseTreeNode.ChildAt(0);
				break;

			case "enumMemberDeclaration":
				if (parseTreeNode.ChildAt(0) is ParseTree.Node)
					nameNode = parseTreeNode.ChildAt(1);
				else
					nameNode = parseTreeNode.ChildAt(0);
				break;

			case "statementList":
				return null;

			case "lambdaExpression":
			case "anonymousMethodExpression":
				return parseTreeNode;

			case "interfaceTypeList":
				nameNode = parseTreeNode.ChildAt(0);
				break;

			case "foreachStatement":
			case "fromClause":
				nameNode = parseTreeNode.FindChildByName("NAME");
				break;

			case "getAccessorDeclaration":
			case "interfaceGetAccessorDeclaration":
			case "setAccessorDeclaration":
			case "interfaceSetAccessorDeclaration":
			case "addAccessorDeclaration":
			case "removeAccessorDeclaration":
				nameNode = parseTreeNode.FindChildByName("IDENTIFIER");
				break;

			case "indexerDeclaration":
			case "interfaceIndexerDeclaration":
			case "labeledStatement":
				return parseTreeNode.ChildAt(0);

			case "conversionOperatorDeclarator":
			case "operatorDeclarator":
		case "usingNamespaceDirective":
				return null;

			default:
				Debug.LogWarning("Don't know how to extract symbol name from: " + parseTreeNode);
				return null;
		}
		return nameNode;
	}

	public string Name
	{
		get
		{
			if (name != null)
				return name;

			if (definition != null)
				return name = definition.name;

			if (kind == SymbolKind.Constructor)
				return name = ".ctor";
			if (kind == SymbolKind.Indexer)
				return name = "Item";
			if (kind == SymbolKind.LambdaExpression)
			{
				var cuNode = parseTreeNode;
				while (cuNode != null && !(cuNode.scope is CompilationUnitScope))
					cuNode = cuNode.parent;
				name = cuNode != null ? cuNode.scope.CreateAnonymousName() : scope.CreateAnonymousName();
				return name;
			}
			if (kind == SymbolKind.Accessor)
			{
				switch (parseTreeNode.RuleName)
				{
					case "getAccessorDeclaration":
					case "interfaceGetAccessorDeclaration":
						return "get";
					case "setAccessorDeclaration":
					case "interfaceSetAccessorDeclaration":
						return "set";
					case "addAccessorDeclaration":
						return "add";
					case "removeAccessorDeclaration":
						return "remove";
				}
			}

			var nameNode = NameNode();
			var asNode = nameNode as ParseTree.Node;
			if (asNode != null && asNode.numValidNodes != 0 && asNode.RuleName == "memberName")
			{
				asNode = asNode.NodeAt(0);
				if (asNode != null && asNode.numValidNodes != 0 && asNode.RuleName == "qid")
				{
					asNode = asNode.NodeAt(-1);
					if (asNode != null && asNode.numValidNodes != 0)
					{
						if (asNode.RuleName == "qidStart")
						{
							nameNode = asNode.ChildAt(0);
						}
						else
						{
							asNode = asNode.NodeAt(0);
							if (asNode != null && asNode.numValidNodes != 0)
							{
								nameNode = asNode.ChildAt(1);
							}
						}
					}
				}
			}
			var asLeaf = nameNode as ParseTree.Leaf;
			if (asLeaf != null && asLeaf.token != null && asLeaf.token.tokenKind != SyntaxToken.Kind.Identifier)
				nameNode = null;
			name = nameNode != null ? nameNode.Print() : "UNKNOWN";
			return name;
		}
	}
	
	public string ReflectionName {
		get {
			if (numTypeParameters == 0)
				return Name;
			return Name + '`' + numTypeParameters;
		}
	}

	//public bool Accept(IHierarchicalVisitor<SymbolDeclaration, SymbolDeclaration> visitor)
	//{
	//    if (nestedDeclarations.Count == 0)
	//        return visitor.Visit(this);
		
	//    if (visitor.VisitEnter(this))
	//    {
	//        foreach (var nested in nestedDeclarations)
	//            if (!nested.Accept(visitor))
	//                break;
	//    }
	//    return visitor.VisitLeave(this);
	//}

	public override string ToString()
	{
		var sb = new StringBuilder();
		Dump(sb, string.Empty);
		return sb.ToString();
	}

	protected virtual void Dump(StringBuilder sb, string indent)
	{
		sb.AppendLine(indent + kind + " " + ReflectionName + " (" + GetType() + ")");
		
		//foreach (var nested in nestedDeclarations)
		//    nested.Dump(sb, indent + "  ");
	}

	public bool HasAllModifiers(Modifiers mods)
	{
		return (modifiers & mods) == mods;
	}

	public bool HasAnyModifierOf(Modifiers mods)
	{
		return (modifiers & mods) != 0;
	}
}

public class NamespaceDeclaration : SymbolDeclaration
{
	public List<SymbolReference> importedNamespaces = new List<SymbolReference>();
	public List<TypeAlias> typeAliases = new List<TypeAlias>();

	public NamespaceDeclaration(string nsName)
		: base(nsName)
	{}

	public NamespaceDeclaration() {}

	public void ImportNamespace(string namespaceToImport, ParseTree.BaseNode declaringNode)
	{
		throw new NotImplementedException ();
	}

	protected override void Dump(StringBuilder sb, string indent)
	{
		base.Dump(sb, indent);

		sb.AppendLine(indent + "Imports:");
		var indent2 = indent + "  ";
		foreach (var ns in importedNamespaces)
			sb.AppendLine(indent2 + ns);

		sb.AppendLine("  Aliases:");
		foreach (var ta in typeAliases)
			sb.AppendLine(indent2 + ta.name);
	}
}

public class CompilationUnitScope : NamespaceScope
{
	public string path;

	public AssemblyDefinition assembly;

	private int numAnonymousSymbols;
	
	public CompilationUnitScope() : base(null) {}

	public override string CreateAnonymousName()
	{
		return ".Anonymous_" + numAnonymousSymbols++;
	}
}

public class AssemblyDefinition : SymbolDefinition
{
	public enum UnityAssembly
	{
		None,
		DllFirstPass,
		CSharpFirstPass,
		UnityScriptFirstPass,
		BooFirstPass,
		DllEditorFirstPass,
		CSharpEditorFirstPass,
		UnityScriptEditorFirstPass,
		BooEditorFirstPass,
		Dll,
		CSharp,
		UnityScript,
		Boo,
		DllEditor,
		CSharpEditor,
		UnityScriptEditor,
		BooEditor,

		Last = BooEditor
	}

	public readonly Assembly assembly;
	public readonly UnityAssembly assemblyId;

	private AssemblyDefinition[] _referencedAssemblies;
	public AssemblyDefinition[] referencedAssemblies
	{
		get {
			if (_referencedAssemblies == null)
			{
				var raSet = new HashSet<AssemblyDefinition>();
				if (assembly != null)
				{
					foreach (var ra in assembly.GetReferencedAssemblies())
					{
						var assemblyDefinition = FromName(ra.Name);
						if (assemblyDefinition != null)
							raSet.Add(assemblyDefinition);
					}
				}
				
				var isEditorAssembly = false;
				var isFirstPassAssembly = false;
				switch (assemblyId)
				{
				case UnityAssembly.CSharpFirstPass:
				case UnityAssembly.UnityScriptFirstPass:
				case UnityAssembly.BooFirstPass:
					isFirstPassAssembly = true;
					break;
					
				case UnityAssembly.CSharpEditorFirstPass:
				case UnityAssembly.UnityScriptEditorFirstPass:
				case UnityAssembly.BooEditorFirstPass:
					isFirstPassAssembly = true;
					isEditorAssembly = true;
					break;
					
				case UnityAssembly.CSharpEditor:
				case UnityAssembly.UnityScriptEditor:
				case UnityAssembly.BooEditor:
					isEditorAssembly = true;
					break;
				}
				
				var stdAssemblies = isEditorAssembly ? editorReferencedAssemblies : standardReferencedAssemblies;
				
				raSet.UnionWith(
					from a in stdAssemblies
					select FromName(a.GetName().Name)
				);
				
				if (isEditorAssembly || !isFirstPassAssembly)
				{
					raSet.Add(FromId(UnityAssembly.CSharpFirstPass));
					raSet.Add(FromId(UnityAssembly.UnityScriptFirstPass));
					raSet.Add(FromId(UnityAssembly.BooFirstPass));
				}
				if (isEditorAssembly && !isFirstPassAssembly)
				{
					raSet.Add(FromId(UnityAssembly.CSharp));
					raSet.Add(FromId(UnityAssembly.UnityScript));
					raSet.Add(FromId(UnityAssembly.Boo));
					raSet.Add(FromId(UnityAssembly.CSharpEditorFirstPass));
					raSet.Add(FromId(UnityAssembly.UnityScriptEditorFirstPass));
					raSet.Add(FromId(UnityAssembly.BooEditorFirstPass));
				}
				
				raSet.Remove(null);
				
				_referencedAssemblies = new AssemblyDefinition[raSet.Count];
				raSet.CopyTo(_referencedAssemblies);
			}
			return _referencedAssemblies;
		}
	}

	public Dictionary<string, CompilationUnitScope> compilationUnits;

	private static readonly Dictionary<Assembly, AssemblyDefinition> allAssemblies = new Dictionary<Assembly, AssemblyDefinition>();
	public static AssemblyDefinition FromAssembly(Assembly assembly)
	{
		AssemblyDefinition definition;
		if (!allAssemblies.TryGetValue(assembly, out definition))
		{
			definition = new AssemblyDefinition(assembly);
			allAssemblies[assembly] = definition;
		}
		return definition;
	}

	private static readonly string[] unityAssemblyNames = new[]
	{
		null,
		"assembly-csharp-firstpass",
		"assembly-unityscript-firstpass",
		"assembly-boo-firstpass",
		null,
		"assembly-csharp-editor-firstpass",
		"assembly-unityscript-editor-firstpass",
		"assembly-boo-editor-firstpass",
		null,
		"assembly-csharp",
		"assembly-unityscript",
		"assembly-boo",
		null,
		"assembly-csharp-editor",
		"assembly-unityscript-editor",
		"assembly-boo-editor"
	};
	
	public static bool IsScriptAssemblyName(string name)
	{
		return Array.IndexOf<string>(unityAssemblyNames, name.ToLowerInvariant()) >= 0;
	}
	
	private static Assembly[] standardReferencedAssemblies;
	private static Assembly[] editorReferencedAssemblies;
	
	private static Assembly[] _domainAssemblies;
	private static Assembly[] domainAssemblies {
		get {
			if (_domainAssemblies == null || _domainAssemblies.Length != AppDomain.CurrentDomain.GetAssemblies().Length)
			{
				var standardRefs = new List<Assembly>();
				var editorRefs = new List<Assembly>();
				var assetsPath = UnityEngine.Application.dataPath.ToLowerInvariant();
				
				_domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in _domainAssemblies)
				{
					if (assembly is System.Reflection.Emit.AssemblyBuilder)
						continue;

					var path = assembly.Location.Replace('\\', '/').ToLowerInvariant();
					if (path.StartsWith(assetsPath, StringComparison.Ordinal))
					{
						path = path.Substring(assetsPath.Length - "assets".Length);
						var id = AssemblyIdFromAssetPath(path);
						if (id == UnityAssembly.Dll || id == UnityAssembly.DllFirstPass)
							standardRefs.Add(assembly);
						else if (id == UnityAssembly.DllEditor || id == UnityAssembly.DllEditorFirstPass)
							editorRefs.Add(assembly);
					}
					else if (path.EndsWith("/unityengine.dll", StringComparison.Ordinal)
						|| path.EndsWith("/unityeditor.dll", StringComparison.Ordinal)
						|| path.EndsWith("/system.dll", StringComparison.Ordinal)
						|| path.EndsWith("/system.core.dll", StringComparison.Ordinal)
						|| path.EndsWith("/system.xml.linq.dll", StringComparison.Ordinal)
						|| path.EndsWith("/system.xml.dll", StringComparison.Ordinal))
					{
						standardRefs.Add(assembly);
					}
					else if (path.EndsWith("/unityeditor.graphs.dll", StringComparison.Ordinal))
					{
						editorRefs.Add(assembly);
					}
				}
				standardReferencedAssemblies = standardRefs.ToArray();
				editorRefs.AddRange(standardRefs);
				editorReferencedAssemblies = editorRefs.ToArray();
			}
			return _domainAssemblies;
		}
	}
	
	private static AssemblyDefinition FromName(string assemblyName)
	{
		assemblyName = assemblyName.ToLower();
		for (var i = domainAssemblies.Length; i-- > 0; )
		{
			var assembly = domainAssemblies[i];
			if (assembly is System.Reflection.Emit.AssemblyBuilder)
				continue;
			if (assembly.GetName().Name.ToLower() == assemblyName)
				return FromAssembly(assembly);
		}
		return null;
	}

	private static readonly AssemblyDefinition[] unityAssemblies = new AssemblyDefinition[(int) UnityAssembly.Last - 1];
	public static AssemblyDefinition FromId(UnityAssembly assemblyId)
	{
		if (assemblyId == UnityAssembly.None)
			return null;
		
		var index = ((int) assemblyId) - 1;
		if (unityAssemblies[index] == null)
		{
			var assemblyName = unityAssemblyNames[index];
			unityAssemblies[index] = FromName(assemblyName) ?? new AssemblyDefinition(assemblyId);
		}
		return unityAssemblies[index];
	}
	
	public static UnityAssembly AssemblyIdFromAssetPath(string pathName)
	{
		var ext = (System.IO.Path.GetExtension(pathName) ?? string.Empty).ToLower();
		var isCSharp = ext == ".cs";
		var isUnityScript = ext == ".js";
		var isBoo = ext == ".boo";
		var isDll = ext == ".dll";
		if (!isCSharp && !isUnityScript && !isBoo && !isDll)
			return UnityAssembly.None;

		var path = (System.IO.Path.GetDirectoryName(pathName) ?? string.Empty).ToLowerInvariant() + "/";

		var isIgnoredScript = path.StartsWith("assets/webplayertemplates/", StringComparison.Ordinal);
		if (isIgnoredScript)
			return UnityAssembly.None;
		
		var isPlugins = path.StartsWith("assets/plugins/", StringComparison.Ordinal);
		var isStandardAssets = path.StartsWith("assets/standard assets/", StringComparison.Ordinal) ||
			path.StartsWith("assets/pro standard assets/", StringComparison.Ordinal);
		var isEditor = !isPlugins && path.Contains("/editor/") || path.StartsWith("assets/plugins/editor/", StringComparison.Ordinal);
		var isFirstPass = isPlugins || isStandardAssets;

		UnityAssembly assemblyId;
		if (isFirstPass && isEditor)
			assemblyId = isCSharp ? UnityAssembly.CSharpEditorFirstPass : isBoo ? UnityAssembly.BooEditorFirstPass : isUnityScript ? UnityAssembly.UnityScriptEditorFirstPass : UnityAssembly.DllEditorFirstPass;
		else if (isEditor)
			assemblyId = isCSharp ? UnityAssembly.CSharpEditor : isBoo ? UnityAssembly.BooEditor : isUnityScript ? UnityAssembly.UnityScriptEditor : UnityAssembly.DllEditor;
		else if (isFirstPass)
			assemblyId = isCSharp ? UnityAssembly.CSharpFirstPass : isBoo ? UnityAssembly.BooFirstPass : isUnityScript ? UnityAssembly.UnityScriptFirstPass : UnityAssembly.DllFirstPass;
		else
			assemblyId = isCSharp ? UnityAssembly.CSharp : isBoo ? UnityAssembly.Boo : isUnityScript ? UnityAssembly.UnityScript : UnityAssembly.Dll;
		
		return assemblyId;
	}
	
	public static AssemblyDefinition FromAssetPath(string pathName)
	{
		return FromId(AssemblyIdFromAssetPath(pathName));
	}

	private AssemblyDefinition(UnityAssembly id)
	{
		assemblyId = id;
	}

	private AssemblyDefinition(Assembly assembly)
	{
		this.assembly = assembly;

		switch (assembly.GetName().Name.ToLower())
		{
			case "assembly-csharp-firstpass":
				assemblyId = UnityAssembly.CSharpFirstPass;
				break;
			case "assembly-unityscript-firstpass":
				assemblyId = UnityAssembly.UnityScriptFirstPass;
				break;
			case "assembly-boo-firstpass":
				assemblyId = UnityAssembly.BooFirstPass;
				break;
			case "assembly-csharp-editor-firstpass":
				assemblyId = UnityAssembly.CSharpEditorFirstPass;
				break;
			case "assembly-unityscript-editor-firstpass":
				assemblyId = UnityAssembly.UnityScriptEditorFirstPass;
				break;
			case "assembly-boo-editor-firstpass":
				assemblyId = UnityAssembly.BooEditorFirstPass;
				break;
			case "assembly-csharp":
				assemblyId = UnityAssembly.CSharp;
				break;
			case "assembly-unityscript":
				assemblyId = UnityAssembly.UnityScript;
				break;
			case "assembly-boo":
				assemblyId = UnityAssembly.Boo;
				break;
			case "assembly-csharp-editor":
				assemblyId = UnityAssembly.CSharpEditor;
				break;
			case "assembly-unityscript-editor":
				assemblyId = UnityAssembly.UnityScriptEditor;
				break;
			case "assembly-boo-editor":
				assemblyId = UnityAssembly.BooEditor;
				break;
			default:
				assemblyId = UnityAssembly.None;
				break;
		}
	}

	public string AssemblyName
	{
		get
		{
			return assembly.GetName().Name;
		}
	}
	
	public bool InternalsVisibleIn(AssemblyDefinition referencedAssembly)
	{
		if (referencedAssembly == this)
			return true;
			
		//TODO: Check are internals visible

		return false;
	}

	public static CompilationUnitScope GetCompilationUnitScope(string assetPath, bool forceCreateNew = false)
	{
		if (assetPath == null)
			return null;
		
		assetPath = assetPath.ToLower();

		var assembly = FromAssetPath(assetPath);
		if (assembly == null)
			return null;

		if (assembly.compilationUnits == null)
			assembly.compilationUnits = new Dictionary<string, CompilationUnitScope>();

		CompilationUnitScope scope;
		if (!assembly.compilationUnits.TryGetValue(assetPath, out scope) || forceCreateNew)
		{
			if (forceCreateNew)
			{
				if (scope != null && scope.typeDeclarations != null)
				{
					var newResolverVersion = false;
					var scopeTypes = scope.typeDeclarations;
					for (var i = scopeTypes.Count; i --> 0; )
					{
						var typeDeclaration = scopeTypes[i];
						scope.RemoveDeclaration(typeDeclaration);
						newResolverVersion = true;
					}
					if (newResolverVersion)
					{
						++ParseTree.resolverVersion;
						if (ParseTree.resolverVersion == 0)
							++ParseTree.resolverVersion;
					}
				}
				assembly.compilationUnits.Remove(assetPath);
			}

			scope = new CompilationUnitScope
			{
				assembly = assembly,
				path = assetPath,
			};
			assembly.compilationUnits[assetPath] = scope;

			//var cuDefinition = new CompilationUnitDefinition
			//{
			//    kind = SymbolKind.None,
			//    parentSymbol = assembly,
			//};

			scope.declaration = new NamespaceDeclaration
			{
				kind = SymbolKind.Namespace,
				definition = assembly.GlobalNamespace,
			};
			scope.definition = assembly.GlobalNamespace;
		}
		return scope;
	}

	private NamespaceDefinition _globalNamespace;
	public NamespaceDefinition GlobalNamespace
	{
		get { return _globalNamespace ?? InitializeGlobalNamespace(); }
		set { _globalNamespace = value; }
	}

	private NamespaceDefinition InitializeGlobalNamespace()
	{
	//	var timer = new Stopwatch();
	//	timer.Start();

		_globalNamespace = new NamespaceDefinition { name = "", kind = SymbolKind.Namespace, parentSymbol = this };

		if (assembly != null)
		{
			var types = assemblyId != UnityAssembly.None ? assembly.GetTypes() : assembly.GetExportedTypes();
			foreach (var t in types)
			{
				if (t.IsNested)
					continue;
	
				SymbolDefinition current = _globalNamespace;
	
				if (!string.IsNullOrEmpty(t.Namespace))
				{
					var ns = t.Namespace.Split('.');
					for (var i = 0; i < ns.Length; ++i)
					{
						var nsName = ns[i];
						var definition = current.FindName(nsName, 0, true);
						if (definition != null)
						{
							current = definition;
						}
						else
						{
							var nsd = new NamespaceDefinition
							{
								kind = SymbolKind.Namespace,
								name = nsName,
								parentSymbol = current,
								accessLevel = AccessLevel.Public,
								modifiers = Modifiers.Public,
							};
							current.AddMember(nsd);
							current = nsd;
						}
					}
				}
	
				current.ImportReflectedType(t);
			}
		}

		//	timer.Stop();
		//	UnityEngine.Debug.Log(timer.ElapsedMilliseconds + " ms\n" + string.Join(", ", _globalNamespace.members.Keys.ToArray()));
		//	Debug.Log(_globalNamespace.Dump());

		if (builtInTypes == null)
		{
			builtInTypes = new Dictionary<string, TypeDefinitionBase>
		    {
			    { "int", builtInTypes_int = DefineBuiltInType(typeof(int)) },
			    { "uint", builtInTypes_uint = DefineBuiltInType(typeof(uint)) },
			    { "byte", builtInTypes_byte = DefineBuiltInType(typeof(byte)) },
			    { "sbyte", builtInTypes_sbyte = DefineBuiltInType(typeof(sbyte)) },
			    { "short", builtInTypes_short = DefineBuiltInType(typeof(short)) },
			    { "ushort", builtInTypes_ushort = DefineBuiltInType(typeof(ushort)) },
			    { "long", builtInTypes_long = DefineBuiltInType(typeof(long)) },
			    { "ulong", builtInTypes_ulong = DefineBuiltInType(typeof(ulong)) },
			    { "float", builtInTypes_float = DefineBuiltInType(typeof(float)) },
			    { "double", builtInTypes_double = DefineBuiltInType(typeof(float)) },
			    { "decimal", builtInTypes_decimal = DefineBuiltInType(typeof(decimal)) },
			    { "char", builtInTypes_char = DefineBuiltInType(typeof(char)) },
			    { "string", builtInTypes_string = DefineBuiltInType(typeof(string)) },
			    { "bool", builtInTypes_bool = DefineBuiltInType(typeof(bool)) },
			    { "object", builtInTypes_object = DefineBuiltInType(typeof(object)) },
			    { "void", builtInTypes_void = DefineBuiltInType(typeof(void)) },
		    };
			
			builtInTypes_Array = DefineBuiltInType(typeof(System.Array));
			builtInTypes_Nullable = DefineBuiltInType(typeof(System.Nullable<>));
			builtInTypes_IEnumerable = DefineBuiltInType(typeof(System.Collections.IEnumerable));
			builtInTypes_IEnumerable_1 = DefineBuiltInType(typeof(System.Collections.Generic.IEnumerable<>));
			builtInTypes_Exception = DefineBuiltInType(typeof(System.Exception));
		}

		return _globalNamespace;
	}

	public static TypeDefinition DefineBuiltInType(Type type)
	{
		var assembly = FromAssembly(type.Assembly);
		var @namespace = assembly.FindNamespace(type.Namespace);
		var name = type.Name;
		var index = name.IndexOf('`');
		if (index > 0)
			name = name.Substring(0, index);
		var definition = @namespace.FindName(name, type.GetGenericArguments().Length, true);
		return definition as TypeDefinition;
	}

	public SymbolDefinition FindNamespace(string namespaceName)
	{
		SymbolDefinition result = GlobalNamespace;
		if (string.IsNullOrEmpty(namespaceName))
			return result;
		var start = 0;
		while (start < namespaceName.Length)
		{
			var dotPos = namespaceName.IndexOf('.', start);
			var ns = dotPos == -1 ? namespaceName.Substring(start) : namespaceName.Substring(start, dotPos - start);
			result = result.FindName(ns, 0, true) as NamespaceDefinition;
			if (result == null)
				return unknownSymbol;
			start = dotPos == -1 ? int.MaxValue : dotPos + 1;
		}
		return result ?? unknownSymbol;
	}
	
	public NamespaceDefinition FindSameNamespace(NamespaceDefinition namespaceDefinition)
	{
		if (string.IsNullOrEmpty(namespaceDefinition.name))
			return GlobalNamespace;
		var parent = FindSameNamespace(namespaceDefinition.parentSymbol as NamespaceDefinition);
		if (parent == null)
			return null;
		return parent.FindName(namespaceDefinition.name, 0, true) as NamespaceDefinition;
	}

	public void ResolveInReferencedAssemblies(ParseTree.Leaf leaf, NamespaceDefinition namespaceDefinition, int numTypeArgs)
	{
		foreach (var ra in referencedAssemblies)
		{
			var nsDef = ra.FindSameNamespace(namespaceDefinition);
			if (nsDef != null)
			{
				var leafText = DecodeId(leaf.token.text);
				leaf.resolvedSymbol = nsDef.FindName(leafText, numTypeArgs, true);
				if (leaf.resolvedSymbol != null)
					return;
			}
		}
	}

	public void ResolveAttributeInReferencedAssemblies(ParseTree.Leaf leaf, NamespaceDefinition namespaceDefinition)
	{
		foreach (var ra in referencedAssemblies)
		{
			var nsDef = ra.FindSameNamespace(namespaceDefinition);
			if (nsDef != null)
			{
				var leafText = DecodeId(leaf.token.text);
				leaf.resolvedSymbol = nsDef.FindName(leafText, 0, true);
				if (leaf.resolvedSymbol != null)
					return;

				leaf.resolvedSymbol = nsDef.FindName(leafText + "Attribute", 0, true);
				if (leaf.resolvedSymbol != null)
					return;
			}
		}
	}

	private static bool dontReEnter = false;

	public void GetMembersCompletionDataFromReferencedAssemblies(Dictionary<string, SymbolDefinition> data, NamespaceDefinition namespaceDefinition)
	{
		if (dontReEnter)
			return;

		foreach (var ra in referencedAssemblies)
		{ 
			var nsDef = ra.FindSameNamespace(namespaceDefinition);	
			if (nsDef != null)
			{
				dontReEnter = true;
				var accessLevelMask = ra.InternalsVisibleIn(this) ? AccessLevelMask.Public | AccessLevelMask.Internal : AccessLevelMask.Public;
				nsDef.GetMembersCompletionData(data, 0, accessLevelMask, this);
				dontReEnter = false;
			}
		}
	}

	public void GetTypesOnlyCompletionDataFromReferencedAssemblies(Dictionary<string, SymbolDefinition> data, NamespaceDefinition namespaceDefinition)
	{
		if (dontReEnter)
			return;

		foreach (var ra in referencedAssemblies)
		{ 
			var nsDef = ra.FindSameNamespace(namespaceDefinition);
			if (nsDef != null)
			{
				dontReEnter = true;
				var accessLevelMask = ra.InternalsVisibleIn(this) ? AccessLevelMask.Public | AccessLevelMask.Internal : AccessLevelMask.Public;
				nsDef.GetTypesOnlyCompletionData(data, accessLevelMask, this);
				dontReEnter = false;
			}
		}
	}
	
	public void CollectExtensionMethods(
		NamespaceDefinition namespaceDefinition,
		string id,
		SymbolReference[] typeArgs,
		TypeDefinitionBase extendedType,
		HashSet<MethodDefinition> extensionsMethods,
		Scope context)
	{
		namespaceDefinition.CollectExtensionMethods(id, typeArgs, extendedType, extensionsMethods, context);
		
		foreach (var ra in referencedAssemblies)
		{
			var nsDef = ra.FindSameNamespace(namespaceDefinition);	
			if (nsDef != null)
				nsDef.CollectExtensionMethods(id, typeArgs, extendedType, extensionsMethods, context);
		}
	}
	
	public void GetExtensionMethodsCompletionData(TypeDefinitionBase targetType, NamespaceDefinition namespaceDefinition, Dictionary<string, SymbolDefinition> data)
	{
		namespaceDefinition.GetExtensionMethodsCompletionData(targetType, data, AccessLevelMask.Public | AccessLevelMask.Internal);

		foreach (var ra in referencedAssemblies)
		{
			var nsDef = ra.FindSameNamespace(namespaceDefinition);	
			if (nsDef != null)
				nsDef.GetExtensionMethodsCompletionData(targetType, data, AccessLevelMask.Public | (ra.InternalsVisibleIn(this) ? AccessLevelMask.Internal : 0));
		}
	}
	
	public IEnumerable<TypeDefinitionBase> EnumAssignableTypesFor(TypeDefinitionBase type)
	{
		yield return type;
		//foreach (var derived in Assembly.EnumDerivedTypes(this))
		//	yield return derived;
	}
}

public static class FGResolver
{
	public static void GetCompletions(IdentifierCompletionsType completionTypes, ParseTree.BaseNode parseTreeNode, HashSet<SymbolDefinition> completionSymbols, string assetPath)
	{
#if false
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		GetCompletions_Profiled(completionTypes, parseTreeNode, completionSymbols, assetPath);
		stopwatch.Stop();
		Debug.Log("GetCompletions: " + stopwatch.ElapsedMilliseconds + "ms");
	}
	
	public static void GetCompletions_Profiled(IdentifierCompletionsType completionTypes, ParseTree.BaseNode parseTreeNode, HashSet<SymbolDefinition> completionSymbols, string assetPath)
	{
#endif
		try
		{
			var d = new Dictionary<string, SymbolDefinition>();
			var assemblyDefinition = AssemblyDefinition.FromAssetPath(assetPath);
			
			if ((completionTypes & IdentifierCompletionsType.MemberName) != 0)
			{
				ParseTree.BaseNode targetNode = null;

				var node = parseTreeNode.parent; // memberInitializerList or objectInitializer or objectCreationExpression
				if (node.RuleName != "objectOrCollectionInitializer")
				{
					if (node.RuleName != "objectInitializer")
					{
						if (node.RuleName == "memberInitializerList")
							node = node.parent; // objectInitializer
					}
					node = node.parent; // objectOrCollectionInitializer
				}
				node = node.parent;
				if (node.RuleName == "objectCreationExpression")
				{
					targetNode = node.parent;
				}
				else // node is memberInitializer
				{
					targetNode = node.LeafAt(0);
				}
				
				var targetDef = targetNode != null ? SymbolDefinition.ResolveNode(targetNode) : null;
				if (targetDef != null)
				{
					GetMemberCompletions(targetDef, parseTreeNode, assemblyDefinition, d, false);

					var filteredData = new Dictionary<string, SymbolDefinition>();
					foreach (var kv in d)
					{
						var symbol = kv.Value;
						if (symbol.kind == SymbolKind.Field && (symbol.modifiers & Modifiers.ReadOnly) == 0 ||
							symbol.kind == SymbolKind.Property && symbol.FindName("set", 0, false) != null)
						{
							filteredData[kv.Key] = symbol;
						}
					}
					d = filteredData;
				}
				
				var targetType = targetDef != null ? targetDef.TypeOf() as TypeDefinitionBase : null;
				if (targetType == null || !targetType.DerivesFrom(SymbolDefinition.builtInTypes_IEnumerable))
				{
					completionSymbols.Clear();
					completionSymbols.UnionWith(d.Values);
					return;
				}
			}

			if ((completionTypes & IdentifierCompletionsType.Member) != 0)
			{
				var target = parseTreeNode.FindPreviousNode();
				if (target != null)
				{
					var targetAsNode = target as ParseTree.Node;
					if (targetAsNode != null && targetAsNode.RuleName == "primaryExpressionPart")
					{
						var node0 = targetAsNode.NodeAt(0);
						if (node0 != null && node0.RuleName == "arguments")
						{
							target = target.FindPreviousNode();
							targetAsNode = target as ParseTree.Node;
						}
					}
					//Debug.Log(targetAsNode ?? target.parent);
					ResolveNode(targetAsNode ?? target.parent);
					var targetDef = GetResolvedSymbol(targetAsNode ?? target.parent);

					GetMemberCompletions(targetDef, parseTreeNode, assemblyDefinition, d, true);
				}
			}
			else if (parseTreeNode == null)
			{
#if SI3_WARNINGS
				Debug.LogWarning(completionTypes);
#endif
			}
			else
			{
				Scope.completionNode = parseTreeNode;
				Scope.completionAssetPath = assetPath;

				if (parseTreeNode.IsLit("=>"))
				{
					parseTreeNode = parseTreeNode.parent.NodeAt(parseTreeNode.childIndex + 1) ?? parseTreeNode;
				}
				if (parseTreeNode.IsLit("]") && parseTreeNode.parent.RuleName == "attributes")
				{
					parseTreeNode = parseTreeNode.parent.parent.NodeAt(parseTreeNode.parent.childIndex + 1);
				}

				var enclosingScopeNode = parseTreeNode as ParseTree.Node ?? parseTreeNode.parent;
				if (enclosingScopeNode != null && (enclosingScopeNode.scope is SymbolDeclarationScope) &&
					(parseTreeNode.IsLit(";") || parseTreeNode.IsLit("}")) &&
					enclosingScopeNode.GetLastLeaf() == parseTreeNode)
				{
					enclosingScopeNode = enclosingScopeNode.parent;
				}
				while (enclosingScopeNode != null && enclosingScopeNode.scope == null)
					enclosingScopeNode = enclosingScopeNode.parent;
				if (enclosingScopeNode != null)
				{
					var lastLeaf = parseTreeNode as ParseTree.Leaf ??
						((ParseTree.Node) parseTreeNode).GetLastLeaf() ??
						((ParseTree.Node) parseTreeNode).FindPreviousLeaf();
					Scope.completionAtLine = lastLeaf != null ? lastLeaf.line : 0;
					Scope.completionAtTokenIndex = lastLeaf != null ? lastLeaf.tokenIndex : 0;
					
					enclosingScopeNode.scope.GetCompletionData(d, true, assemblyDefinition);
				}
			}
	
			completionSymbols.UnionWith(d.Values);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	public static SymbolDefinition GetResolvedSymbol(ParseTree.BaseNode baseNode)
	{
#if false
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = GetResolvedSymbol_Internal(baseNode);
		stopwatch.Stop();
		Debug.Log("GetResolvedSymbol: " + stopwatch.ElapsedMilliseconds + "ms");
		return result;
	}
	
	public static SymbolDefinition GetResolvedSymbol_Internal(ParseTree.BaseNode baseNode)
	{
#endif
		var leaf = baseNode as ParseTree.Leaf;
		if (leaf != null)
		{
			if (leaf.resolvedSymbol == null && leaf.parent != null)
				ResolveNode(leaf.parent);
			return leaf.resolvedSymbol;
		}

		var node = baseNode as ParseTree.Node;
		if (node == null || node.numValidNodes == 0)
			return null;

		switch (node.RuleName)
		{
			case "primaryExpressionStart":
				if (node.numValidNodes < 3)
					return GetResolvedSymbol(node.ChildAt(0));
				leaf = node.LeafAt(2);
				return leaf != null ? leaf.resolvedSymbol : null;
			case "primaryExpressionPart":
				return GetResolvedSymbol(node.NodeAt(0));
			case "arguments":
				return GetResolvedSymbol(node.FindPreviousNode() as ParseTree.Node);
			case "objectCreationExpression":
				var newType = GetResolvedSymbol(node.FindPreviousNode() as ParseTree.Node);
				if (newType == null || newType.kind == SymbolKind.Error)
					newType = SymbolDefinition.builtInTypes_object;
				var typeOfNewType = (TypeDefinitionBase) newType.TypeOf();
				return typeOfNewType.GetThisInstance();
			case "arrayCreationExpression":
				var elementType = GetResolvedSymbol(node.FindPreviousNode() as ParseTree.Node);
				var arrayInstance = SymbolDefinition.ResolveNode(node, null, elementType);
				return arrayInstance ?? SymbolDefinition.builtInTypes_Array.GetThisInstance();
			case "nonArrayType":
				var typeNameType = GetResolvedSymbol(node.NodeAt(0)) as TypeDefinitionBase;
				if (typeNameType == null || typeNameType.kind == SymbolKind.Error)
					typeNameType = SymbolDefinition.builtInTypes_object;
				return node.numValidNodes == 1 ? typeNameType : typeNameType.MakeNullableType();
			case "typeName":
				return GetResolvedSymbol(node.NodeAt(0));
			case "namespaceOrTypeName":
				return GetResolvedSymbol(node.NodeAt(node.numValidNodes & ~1));
			case "accessIdentifier":
				leaf = node.numValidNodes < 2 ? null : node.LeafAt(1);
				if (leaf != null && leaf.resolvedSymbol == null)
					FGResolver.ResolveNode(node);
				return leaf != null ? leaf.resolvedSymbol : null;
			case "predefinedType":
			case "typeOrGeneric":
				return node.LeafAt(0).resolvedSymbol;
			case "typeofExpression":
				return ((TypeDefinitionBase) ReflectedTypeReference.ForType(typeof(Type)).definition).GetThisInstance();
			case "sizeofExpression":
				return SymbolDefinition.builtInTypes_int.GetThisInstance();
			case "localVariableType":
			case "brackets":
			case "expression":
			case "unaryExpression":
			case "parenExpression":
			case "checkedExpression":
			case "uncheckedExpression":
			case "defaultValueExpression":
			case "relationalExpression":
			case "inclusiveOrExpression":
			case "exclusiveOrExpression":
			case "andExpression":
			case "equalityExpression":
			case "shiftExpression":
			case "primaryExpression":
			case "type":
				return SymbolDefinition.ResolveNode(node, null, null, 0);
			default:
#if SI3_WARNINGS
				Debug.LogWarning(node.RuleName);
#endif
				return SymbolDefinition.ResolveNode(node, null, null, 0);
		}
	}

	private static void GetMemberCompletions(
		SymbolDefinition targetDef,
		ParseTree.BaseNode parseTreeNode,
		AssemblyDefinition assemblyDefinition,
		Dictionary<string, SymbolDefinition> d,
		bool includeExtensionMethods)
	{
		if (targetDef != null)
		{
			//Debug.Log(targetDef.GetTooltipText());
			var typeOf = targetDef.TypeOf();
			//UnityEngine.Debug.Log(typeOf);

			var flags = BindingFlags.Instance | BindingFlags.Static;
			switch (targetDef.kind)
			{
				case SymbolKind.None:
				case SymbolKind.Error:
					break;
				case SymbolKind.Namespace:
				case SymbolKind.Interface:
				case SymbolKind.Struct:
				case SymbolKind.Class:
				case SymbolKind.TypeParameter:
				case SymbolKind.Delegate:
					flags = BindingFlags.Static;
					break;
				case SymbolKind.Enum:
					flags = BindingFlags.Static;
					break;
				case SymbolKind.Field:
				case SymbolKind.ConstantField:
				case SymbolKind.LocalConstant:
				case SymbolKind.Property:
				case SymbolKind.Event:
				case SymbolKind.Indexer:
				case SymbolKind.Method:
				case SymbolKind.MethodGroup:
				case SymbolKind.Constructor:
				case SymbolKind.Destructor:
				case SymbolKind.Operator:
				case SymbolKind.Accessor:
				case SymbolKind.Parameter:
				case SymbolKind.CatchParameter:
				case SymbolKind.Variable:
				case SymbolKind.ForEachVariable:
				case SymbolKind.FromClauseVariable:
				case SymbolKind.EnumMember:
					flags = BindingFlags.Instance;
					break;
				case SymbolKind.BaseTypesList:
					flags = BindingFlags.Static;
					break;
				case SymbolKind.Instance:
					flags = BindingFlags.Instance;
					break;
				case SymbolKind.Null:
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}
			//targetDef.kind = targetDef is TypeDefinitionBase && targetDef.kind != SymbolKind.Enum ? BindingFlags.Static : targetDef is InstanceDefinition ? BindingFlags.Instance : 0;

			TypeDefinitionBase contextType = null;
			for (var n = parseTreeNode as ParseTree.Node ?? parseTreeNode.parent; n != null; n = n.parent)
			{
				var s = n.scope as SymbolDeclarationScope;
				if (s != null)
				{
					contextType = s.declaration.definition as TypeDefinitionBase;
					if (contextType != null)
						break;
				}
			}

			AccessLevelMask mask =
				typeOf == contextType || typeOf.IsSameOrParentOf(contextType) ? AccessLevelMask.Private | AccessLevelMask.Protected | AccessLevelMask.Internal | AccessLevelMask.Public :
				contextType != null && contextType.DerivesFrom(typeOf as TypeDefinitionBase) ? AccessLevelMask.Protected | AccessLevelMask.Internal | AccessLevelMask.Public :
				AccessLevelMask.Internal | AccessLevelMask.Public;

			if (typeOf.Assembly == null || !typeOf.Assembly.InternalsVisibleIn(assemblyDefinition))
				mask &= ~AccessLevelMask.Internal;

			//					var enclosingScopeNode = parseTreeNode as ParseTree.Node ?? parseTreeNode.parent;
			//					while (enclosingScopeNode != null && enclosingScopeNode.scope == null)
			//						enclosingScopeNode = enclosingScopeNode.parent;
			//					var enclosingScope = enclosingScopeNode != null ? enclosingScopeNode.scope : null;

			//UnityEngine.Debug.Log(flags + "\n" + mask);
			typeOf.GetMembersCompletionData(d, flags, mask, assemblyDefinition);

			if (includeExtensionMethods && flags == BindingFlags.Instance &&
				(typeOf.kind == SymbolKind.Class || typeOf.kind == SymbolKind.Struct || typeOf.kind == SymbolKind.Interface || typeOf.kind == SymbolKind.Enum))
			{
				var enclosingScopeNode = parseTreeNode as ParseTree.Node ?? parseTreeNode.parent;
				while (enclosingScopeNode != null && enclosingScopeNode.scope == null)
					enclosingScopeNode = enclosingScopeNode.parent;
				var enclosingScope = enclosingScopeNode != null ? enclosingScopeNode.scope : null;
				
				if (enclosingScope != null)
					enclosingScope.GetExtensionMethodsCompletionData(typeOf as TypeDefinitionBase, d);
			}
		}
	}

	public static ParseTree.Node ResolveNode(ParseTree.Node node)
	{
		if (node == null)
			return null;
		
	//	UnityEngine.Debug.Log(node.RuleName);
		while (node.parent != null)
		{
			switch (node.RuleName)
			{
				//case "primaryExpression":
				case "primaryExpressionStart":
				case "primaryExpressionPart":
				case "objectCreationExpression":
				case "objectOrCollectionInitializer":
				case "typeOrGeneric":
				case "namespaceOrTypeName":
				case "typeName":
				case "nonArrayType":
				//case "attribute":
				case "accessIdentifier":
				case "brackets":
				case "argumentList":
				case "attributeArgumentList":
				case "argumentName":
				case "attributeMemberName":
				case "argument":
				case "attributeArgument":
				case "attributeArguments":
				// case "VAR":
//				case "localVariableType":
//				case "localVariableDeclaration":
				case "arrayCreationExpression":
				case "implicitArrayCreationExpression":
				case "arrayInitializer":
				case "arrayInitializerList":
//				case "qid":
				case "qidStart":
				case "qidPart":
				case "memberInitializer":
//				case "memberName":
			//	case "unaryExpression":
			//	case "modifiers":
				case "globalNamespace":
					node = node.parent;
				//	UnityEngine.Debug.Log("--> " + node.RuleName);
					continue;
			}
			break;
		}
		
		try
		{
			//var numTypeArgs = 0;
			//var parent = node.parent;
			//if (parent != null)
			//{
			//	var nextNode = node.NodeAt(node.childIndex + 1);
			//	if (nextNode != null)
			//	{
			//		if (nextNode.RuleName == "typeArgumentList")
			//			numTypeArgs = (nextNode.numValidNodes + 1) / 2;
			//		else if (nextNode.RuleName == "typeParameterList")
			//			numTypeArgs = (nextNode.numValidNodes + 2) / 3;
			//		else if (nextNode.RuleName == "unboundTypeRank")
			//			numTypeArgs = nextNode.numValidNodes - 1;
			//	}
			//}
			var result = SymbolDefinition.ResolveNode(node, null, null, 0);//numTypeArgs);
			if (result == null)
				ResolveChildren(node);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
			return null;
		}
		
		return node;
	}

	static void ResolveChildren(ParseTree.Node node)
	{
		if (node == null)
			return;
		if (node.numValidNodes != 0)
		{
			for (var i = 0; i < node.numValidNodes; ++i)
			{
				var child = node.ChildAt(i);
				
				var leaf = child as ParseTree.Leaf;
				if (leaf == null ||
				    leaf.token != null &&
				    leaf.token.tokenKind != SyntaxToken.Kind.Punctuator &&
					(leaf.token.tokenKind != SyntaxToken.Kind.Keyword || SymbolDefinition.builtInTypes.ContainsKey(leaf.token.text)))
				{
					if (leaf == null)
					{
						switch (((ParseTree.Node) child).RuleName)
						{
							case "modifiers":
							case "methodBody":
								continue;
						}
					}
					var numTypeArgs = 0;
					//var nextNode = node.NodeAt(i + 1);
					//if (nextNode != null)
					//{
					//	if (nextNode.RuleName == "typeArgumentList")
					//		numTypeArgs = (nextNode.numValidNodes + 1) / 2;
					//	else if (nextNode.RuleName == "typeParameterList")
					//		numTypeArgs = (nextNode.numValidNodes + 2) / 3;
					//	else if (nextNode.RuleName == "unboundTypeRank")
					//		numTypeArgs = nextNode.numValidNodes - 1;
					//}
					if (SymbolDefinition.ResolveNode(child, null, null, numTypeArgs) == null)
					{
						var childAsNode = child as ParseTree.Node;
						if (childAsNode != null)
							ResolveChildren(childAsNode);
					}
				}
			}
		}
	}
}

}
