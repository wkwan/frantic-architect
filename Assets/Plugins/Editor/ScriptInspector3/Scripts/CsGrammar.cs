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

using Debug = UnityEngine.Debug;


namespace ScriptInspector
{

public class CsGrammar : FGGrammar
{
	private static CsGrammar instance;
	public static CsGrammar Instance
	{
		get
		{
			return instance ?? new CsGrammar();
		}
	}

	//private static readonly string[] tokenLiterals;

	public override string GetToken(int n)
	{
		return parser.GetToken(n);
		//return tokenLiterals[n];
	}

	public override int TokenToId(string tokenText)
	{
		int id = parser.TokenToId(tokenText);
		if (id < 0)
			id = tokenIdentifier; // parser.TokenToId("IDENTIFIER");
		//Debug.Log("TokenToId(\"" + tokenText + "\") => " + id);
		return id;
		//return Array.BinarySearch(tokenLiterals, tokenText);
	}

	//private static readonly string[] csPunctsAndOps = {
	//	"{", "}", ";", "#", ".", "(", ")", "[", "]", "++", "--", "->", "+", "-",
	//	"!", "~", "&", "*", "*", "/", "%", "+", "-", "<<", ">>", "<", ">", "<=",
	//	">=", "==", "!=", "&", "^", "|", "&&", "||", "??", "?", ":", "=", "+=",
	//	"-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=>"
	//};

	//private static readonly string[] csKeywords = {
	//	"abstract", "as", "base", "break", "case", "catch", "checked", "class", "const", "continue",
	//	"default", "delegate", "do", "else", "enum", "event", "explicit", "extern", "finally",
	//	"fixed", "for", "foreach", "goto", "if", "implicit", "in", "interface", "internal", "is", "lock",
	//	"namespace", "new", "operator", "out", "override", "params", "private", "protected",
	//	"public", "readonly", "ref", "return", "sealed", "sizeof", "stackalloc", "static", "struct",
	//	"switch", "this", "throw", "try", "typeof", "unchecked", "unsafe", "using", "virtual",
	//	"volatile", "where", "while"
	//};

	//private static readonly string[] csTypes = {
	//	"bool", "byte", "char", "decimal", "double", "float", "int", "long", "object", "sbyte", "short",
	//	"string", "uint", "ulong", "ushort", "void"
	//};

	//private static readonly string[] csLiterals = {
	//	"false", "null", "true",
	//};

	static CsGrammar()
	{
		//var all = new HashSet<string>(csKeywords);
		//all.UnionWith(csPunctsAndOps);
		//all.UnionWith(csTypes);
		//all.UnionWith(csLiterals);

		//tokenLiterals = new string[all.Count];
		//all.CopyTo(tokenLiterals);
		//Array.Sort<string>(tokenLiterals);
	}

	public Rule r_compilationUnit { get; private set; }

	private Parser parser;
	public override Parser GetParser { get { return parser; } }

	//protected enum TokenClass
	//{
	//    EOF,
	//    Keyword,
	//    Identifier,
	//    Integer,
	//    Real,
	//    String,
	//};

	readonly Id EOF, IDENTIFIER, NAME, LITERAL;

	public int tokenIdentifier,
		tokenName,
		tokenLiteral,
		tokenAttribute,
		tokenStatement,
		tokenClassBody,
		tokenStructBody,
		tokenInterfaceBody,
		tokenNamespaceBody,
		tokenBinaryOperator,
		tokenExpectedType,
		tokenMemberInitializer,
		tokenNamedParameter,
		tokenEOF;

	void InitializeTokenCategories()
	{
		tokenIdentifier = TokenToId("IDENTIFIER");
		tokenName = TokenToId("NAME");
		tokenLiteral = TokenToId("LITERAL");
		tokenAttribute = TokenToId(".ATTRIBUTE");
		tokenStatement = TokenToId(".STATEMENT");
		tokenClassBody = TokenToId(".CLASSBODY");
		tokenStructBody = TokenToId(".STRUCTBODY");
		tokenInterfaceBody = TokenToId(".INTERFACEBODY");
		tokenNamespaceBody = TokenToId(".NAMESPACEBODY");
		tokenBinaryOperator = TokenToId(".BINARYOPERATOR");
		tokenExpectedType = TokenToId(".EXPECTEDTYPE");
		tokenMemberInitializer = TokenToId(".MEMBERINITIALIZER");
		tokenNamedParameter = TokenToId(".NAMEDPARAMETER");
		tokenEOF = TokenToId("EOF");
	}

	public CsGrammar()
	{
		instance = this;

		EOF = new Id("EOF");
		IDENTIFIER = new Id("IDENTIFIER");
		LITERAL = new Id("LITERAL");
		NAME = new NameId();

	//	NAME = new Id("IDENTIFIER");
	//	tokenName = TokenToId("NAME");
	//	NAME.lookahead.Add(new TokenSet(tokenName));

		var externAliasDirective = new Id("externAliasDirective");
		var usingDirective = new Id("usingDirective");
		//var globalAttribute = new Id("globalAttribute");
		var namespaceMemberDeclaration = new Id("namespaceMemberDeclaration");
		var namespaceDeclaration = new Id("namespaceDeclaration");
		var qualifiedIdentifier = new Id("qualifiedIdentifier");
		var namespaceBody = new Id("namespaceBody");
		//var typeDeclaration = new Id("typeDeclaration");
		var attributes = new Id("attributes");
		var modifiers = new Id("modifiers");
		var typeParameterList = new Id("typeParameterList");
		var typeParameter = new Id("typeParameter");
		var classBase = new Id("classBase");
		var typeParameterConstraintsClauses = new Id("typeParameterConstraintsClauses");
		var typeParameterConstraintsClause = new Id("typeParameterConstraintsClause");
		var classBody = new Id("classBody");
		var classMemberDeclaration = new Id("classMemberDeclaration");
		var attribute = new Id("attribute");
		var typeName = new Id("typeName");
		var argumentList = new Id("argumentList");
		var attributeArgumentList = new Id("attributeArgumentList");
		var expression = new Id("expression");
		var constantExpression = new Id("constantExpression");
		var primaryExpression = new Id("primaryExpression");
		var arrayCreationExpression = new Id("arrayCreationExpression");
		var implicitArrayCreationExpression = new Id("implicitArrayCreationExpression");
		var constantDeclaration = new Id("constantDeclaration");
		var fieldDeclaration = new Id("fieldDeclaration");
		var methodDeclaration = new Id("methodDeclaration");
		var propertyDeclaration = new Id("propertyDeclaration");
		var eventDeclaration = new Id("eventDeclaration");
		var indexerDeclaration = new Id("indexerDeclaration");
		var operatorDeclaration = new Id("operatorDeclaration");
		var constructorDeclaration = new Id("constructorDeclaration");
		var destructorDeclaration = new Id("destructorDeclaration");
		//var staticConstructorDeclaration = new Id("staticConstructorDeclaration");
		var constantDeclarators = new Id("constantDeclarators");
		var constantDeclarator = new Id("constantDeclarator");
		var type = new Id("type");
		var type2 = new Id("type2");
		var predefinedType = new Id("predefinedType");
		var variableDeclarators = new Id("variableDeclarators");
		var variableDeclarator = new Id("variableDeclarator");
		var arrayInitializer = new Id("arrayInitializer");
		var variableInitializer = new Id("variableInitializer");
		var variableInitializerList = new Id("variableInitializerList");
		var simpleType = new Id("simpleType");
		var exceptionClassType = new Id("exceptionClassType");
		//var arrayType = new Id("arrayType");
		var nonArrayType = new Id("nonArrayType");
		var rankSpecifier = new Id("rankSpecifier");
		var numericType = new Id("numericType");
		var integralType = new Id("integralType");
		var floatingPointType = new Id("floatingPointType");


		r_compilationUnit = new Rule("compilationUnit",
			new Many(externAliasDirective) - new Many(usingDirective)
				//new Many(globalAttribute),
				- new Many(namespaceMemberDeclaration) - EOF
			) { semantics = SemanticFlags.CompilationUnitScope };

		parser = new Parser(r_compilationUnit, this);


		//NAME = new Id("NAME");
		//parser.Add(new Rule("NAME",
		//    IDENTIFIER
		//    ));


		parser.Add(new Rule("externAliasDirective",
			new Seq( "extern", "alias", IDENTIFIER, ";" )
			) { semantics = SemanticFlags.ExternAlias });

		var usingNamespaceDirective = new Id("usingNamespaceDirective");
		var usingAliasDirective = new Id("usingAliasDirective");
		var globalNamespace = new Id("globalNamespace");
		var namespaceName = new Id("namespaceName");
		var namespaceOrTypeName = new Id("namespaceOrTypeName");
		var PARTIAL = new Id("PARTIAL");

		parser.Add(new Rule("usingDirective",
			"using" - (new If(IDENTIFIER - "=", usingAliasDirective) | usingNamespaceDirective) - ";"
			));

		parser.Add(new Rule("PARTIAL",
			new If(s => s.Current.text == "partial", IDENTIFIER) | "partial"
		) { contextualKeyword = true });
		
		parser.Add(new Rule("namespaceMemberDeclaration",
			namespaceDeclaration
			| (attributes - modifiers - new Alt(
				new Seq(
					new Opt(PARTIAL),
					new Id("classDeclaration") | new Id("structDeclaration") | new Id("interfaceDeclaration")),
				new Id("enumDeclaration"),
				new Id("delegateDeclaration"))
			)
			| ".NAMESPACEBODY"
			));

		parser.Add(new Rule("namespaceDeclaration",
			"namespace" - qualifiedIdentifier - namespaceBody - new Opt(";")
			) { semantics = SemanticFlags.NamespaceDeclaration });

		parser.Add(new Rule("qualifiedIdentifier",
			NAME - new Many(new Lit(".") - NAME)
			));

		parser.Add(new Rule("namespaceBody",
			"{" - new Many(externAliasDirective) - new Many(usingDirective) - new Many(namespaceMemberDeclaration) - "}"
			) { semantics = SemanticFlags.NamespaceBodyScope });

			//parser.Add(new Rule("typeDeclaration",
			//   new Alt(
			//       new Seq(
			//           new Opt("partial"),
			//           new Id("classDeclaration") | new Id("structDeclaration") | new Id("interfaceDeclaration")),
			//       new Id("enumDeclaration"),
			//       new Id("delegateDeclaration"))
			//   ));


	//	parser = new Parser(r_compilationUnit, this);


		parser.Add(new Rule("usingNamespaceDirective",
			namespaceName
			) { semantics = SemanticFlags.UsingNamespace });

		parser.Add(new Rule("namespaceName",
			namespaceOrTypeName
			));

		parser.Add(new Rule("usingAliasDirective",
			IDENTIFIER - "=" - namespaceOrTypeName
			) { semantics = SemanticFlags.UsingAlias });

		parser.Add(new Rule("classDeclaration",
			new Seq(
				"class",
				NAME,
				new Opt(typeParameterList),
				new Opt(classBase),
				new Opt(typeParameterConstraintsClauses),
				classBody,
				new Opt(";"))
			) { semantics = SemanticFlags.ClassDeclaration | SemanticFlags.TypeDeclarationScope });

		parser.Add(new Rule("typeParameterList",
			"<" - attributes - typeParameter - new Many("," - attributes - typeParameter) - ">"
			));

		parser.Add(new Rule("typeParameter",
			NAME
			) { semantics = SemanticFlags.TypeParameterDeclaration });

		var interfaceTypeList = new Id("interfaceTypeList");
			
		parser.Add(new Rule("classBase",
			":" - interfaceTypeList
			) { semantics = SemanticFlags.ClassBaseScope });

		parser.Add(new Rule("interfaceTypeList",
			(typeName | "object") - new Many("," - typeName)
			) { semantics = SemanticFlags.BaseListDeclaration });

		parser.Add(new Rule("classBody",
			"{" - new Many(classMemberDeclaration) - "}"
			) { semantics = SemanticFlags.ClassBodyScope });

		var interfaceDeclaration = new Id("interfaceDeclaration");
		var classDeclaration = new Id("classDeclaration");
		var structDeclaration = new Id("structDeclaration");
		var memberName = new Id("memberName");
		var qid = new Id("qid");
		var enumDeclaration = new Id("enumDeclaration");
		//var enumDeclarator = new Id("enumDeclarator");
		var delegateDeclaration = new Id("delegateDeclaration");
		var conversionOperatorDeclaration = new Id("conversionOperatorDeclaration");

		parser.Add(new Rule("memberName",
			qid
			));

		parser.Add(new Rule("classMemberDeclaration",
			new Seq(
				attributes,
				modifiers,
				constantDeclaration
				| "void" - methodDeclaration
				| new If(PARTIAL, PARTIAL - ("void" - methodDeclaration | classDeclaration | structDeclaration | interfaceDeclaration))
				| new If(IDENTIFIER - "(", constructorDeclaration)
				| new Seq(
					type,
					new If(memberName - "(", methodDeclaration)
					| new If(memberName - "{", propertyDeclaration)
					| new If(typeName - "." - "this", typeName - "." - indexerDeclaration)
					| new If(predefinedType - "." - "this", predefinedType - "." - indexerDeclaration)
					| indexerDeclaration
					| fieldDeclaration
					| operatorDeclaration)
				| classDeclaration
				| structDeclaration
				| interfaceDeclaration
				| enumDeclaration
				| delegateDeclaration
				| eventDeclaration
				| conversionOperatorDeclaration)
			| destructorDeclaration
			| ".CLASSBODY"
			) /*{ semantics = SemanticFlags.MemberDeclarationScope }*/);

		var constructorDeclarator = new Id("constructorDeclarator");
		var constructorBody = new Id("constructorBody");
		var constructorInitializer = new Id("constructorInitializer");
		var destructorDeclarator = new Id("destructorDeclarator");
		var destructorBody = new Id("destructorBody");
		var arguments = new Id("arguments");
		var attributeArguments = new Id("attributeArguments");
		var formalParameterList = new Id("formalParameterList");
		var block = new Id("block");
		var statementList = new Id("statementList");

		parser.Add(new Rule("constructorDeclaration",
			constructorDeclarator - constructorBody
			) { semantics = SemanticFlags.MethodDeclarationScope | SemanticFlags.ConstructorDeclarator });

		parser.Add(new Rule("constructorDeclarator",
			IDENTIFIER - "(" - new Opt(formalParameterList) - ")" - new Opt(constructorInitializer)
			));

		parser.Add(new Rule("constructorInitializer",
			":" - (new Lit("base") | "this") - arguments
			) { semantics = SemanticFlags.ConstructorInitializerScope });

		parser.Add(new Rule("constructorBody",
			"{" - statementList - "}"
			| ";"
			) { semantics = SemanticFlags.MethodBodyScope });

		parser.Add(new Rule("destructorDeclaration",
			destructorDeclarator - destructorBody
			));

		parser.Add(new Rule("destructorDeclarator",
			"~" - new Opt("extern") - IDENTIFIER - "(" - ")"
			) { semantics = SemanticFlags.DestructorDeclarator });

		parser.Add(new Rule("destructorBody",
			"{" - statementList - "}"
			| ";"
			) { semantics = SemanticFlags.MethodBodyScope });

		parser.Add(new Rule("constantDeclaration",
			"const" - type - constantDeclarators - ";"
			));

		parser.Add(new Rule("constantDeclarators",
			constantDeclarator - new Many("," - constantDeclarator)
			));

		parser.Add(new Rule("constantDeclarator",
			NAME - "=" -  constantExpression
			) { semantics = SemanticFlags.ConstantDeclarator });

		parser.Add(new Rule("constantExpression",
			expression
			| ".EXPECTEDTYPE"
			));

		var methodHeader = new Id("methodHeader");
		var methodBody = new Id("methodBody");
		var formalParameter = new Id("formalParameter");
		var fixedParameter = new Id("fixedParameter");
		var parameterModifier = new Id("parameterModifier");
		var defaultArgument = new Id("defaultArgument");
		var parameterArray = new Id("parameterArray");
		var statement = new Id("statement");
		var typeVariableName = new Id("typeVariableName");
		var typeParameterConstraintList = new Id("typeParameterConstraintList");
		var secondaryConstraintList = new Id("secondaryConstraintList");
		var secondaryConstraint = new Id("secondaryConstraint");
		var constructorConstraint = new Id("constructorConstraint");
		var WHERE = new Id("WHERE");

		parser.Add(new Rule("methodDeclaration",
			methodHeader - methodBody
			) { semantics = SemanticFlags.MethodDeclarationScope | SemanticFlags.MethodDeclarator });

		parser.Add(new Rule("methodHeader",
			memberName - "(" - new Opt(formalParameterList) - ")" - new Opt(typeParameterConstraintsClauses)
			));

		parser.Add(new Rule("typeParameterConstraintsClauses",
			typeParameterConstraintsClause - new Many(typeParameterConstraintsClause)
			) );//{ semantics = SemanticFlags.TypeParameterConstraintsScope });

		parser.Add(new Rule("WHERE",
			new If(s => s.Current.text == "where", IDENTIFIER) | "where"
			) { contextualKeyword = true });

		parser.Add(new Rule("typeParameterConstraintsClause",
			WHERE - typeVariableName - ":" - typeParameterConstraintList
			) { semantics = SemanticFlags.TypeParameterConstraint });

		parser.Add(new Rule("typeParameterConstraintList",
			(new Lit("class") | "struct") - new Opt("," - secondaryConstraintList)
			| secondaryConstraintList
			));

		parser.Add(new Rule("secondaryConstraintList",
			secondaryConstraint - new Opt("," - new Id("secondaryConstraintList"))
			| constructorConstraint
			));

		parser.Add(new Rule("secondaryConstraint",
			typeName // | typeVariableName
			));

		parser.Add(new Rule("typeVariableName",
			IDENTIFIER
			));

		parser.Add(new Rule("constructorConstraint",
			new Lit("new") - "(" - ")"
			));

//primary_constraint:
//	class_type
//	| 'class'
//	| 'struct' ;

		parser.Add(new Rule("methodBody",
			"{" - statementList - "}"
			| ";"
			) { semantics = SemanticFlags.MethodBodyScope });

		parser.Add(new Rule("block",
			"{" - statementList - "}"
			| ";"
			) { semantics = SemanticFlags.CodeBlockScope });

		parser.Add(new Rule("statementList",
			new Many(new IfNot(new Seq( "default", ":" ), statement))
			));

	//	var declarationStatement = new Id("declarationStatement");
		var labeledStatement = new Id("labeledStatement");
		var embeddedStatement = new Id("embeddedStatement");
		var selectionStatement = new Id("selectionStatement");
		var iterationStatement = new Id("iterationStatement");
		var jumpStatement = new Id("jumpStatement");
		var tryStatement = new Id("tryStatement");
		var lockStatement = new Id("lockStatement");
		var usingStatement = new Id("usingStatement");
		var yieldStatement = new Id("yieldStatement");
		var expressionStatement = new Id("expressionStatement");
		var breakStatement = new Id("breakStatement");
		var continueStatement = new Id("continueStatement");
		var gotoStatement = new Id("gotoStatement");
		var returnStatement = new Id("returnStatement");
		var throwStatement = new Id("throwStatement");
		var checkedStatement = new Id("checkedStatement");
		var uncheckedStatement = new Id("uncheckedStatement");
		var localVariableDeclaration = new Id("localVariableDeclaration");
		var localVariableType = new Id("localVariableType");
		var localVariableDeclarators = new Id("localVariableDeclarators");
		var localVariableDeclarator = new Id("localVariableDeclarator");
		var localVariableInitializer = new Id("localVariableInitializer");
		//var stackallocInitializer = new Id("stackallocInitializer");
		var localConstantDeclaration = new Id("localConstantDeclaration");
		//var unmanagedType = new Id("unmanagedType");
		var resourceAcquisition = new Id("resourceAcquisition");

		var VAR = new Id("VAR");

		parser.Add(new Rule("VAR",
			//new If(s => s.Current.text == "var", IDENTIFIER)
			IDENTIFIER | "var"
			) { contextualKeyword = true });

		parser.Add(new Rule("statement",
			new If((type | "var") - IDENTIFIER - (new Lit(";") | "=" | "[" | ","), localVariableDeclaration - ";")
			| new If(IDENTIFIER - ":", labeledStatement)
			| localConstantDeclaration
			| embeddedStatement
			));

	//	parser.Add(new Rule("declarationStatement",
	//		new Seq( localVariableDeclaration | localConstantDeclaration, ";" )
	//		));

		parser.Add(new Rule("localVariableDeclaration",
			localVariableType - localVariableDeclarators
			));

		parser.Add(new Rule("localVariableType",
			new If(s => s.Current.text == "var", VAR)
			| type
			));

		parser.Add(new Rule("localVariableDeclarators",
			localVariableDeclarator - new Many("," - localVariableDeclarator)
			));

		parser.Add(new Rule("localVariableDeclarator",
			NAME - new Opt("=" - localVariableInitializer)
			) { semantics = SemanticFlags.LocalVariableDeclarator });

		parser.Add(new Rule("localVariableInitializer",
			expression | arrayInitializer //| stackallocInitializer
			| ".EXPECTEDTYPE"
			) { semantics = SemanticFlags.LocalVariableInitializerScope });

		//parser.Add(new Rule("stackallocInitializer",
		//    new Seq( "stackalloc", unmanagedType, "[", expression, "]" )
		//    ));

		parser.Add(new Rule("localConstantDeclaration",
			"const" - type - constantDeclarators - ";"
			));

		//parser.Add(new Rule("unmanagedType",
		//    type
		//    ));

		parser.Add(new Rule("labeledStatement",
			IDENTIFIER - ":" - statement
			) { semantics = SemanticFlags.LabeledStatement });

		var YIELD = new Id("YIELD");
			
		parser.Add(new Rule("YIELD",
				//new If(s => s.Current.text == "yield", IDENTIFIER)
				IDENTIFIER | "yield"
				){ contextualKeyword = true });

		parser.Add(new Rule("embeddedStatement",
			block
			| selectionStatement    // if, switch
			| iterationStatement    // while, do, for, foreach
			| jumpStatement         // break, continue, goto, return, throw
			| tryStatement
			| lockStatement
			| usingStatement
			| new If(
				s => {
					if (s.Current.text != "yield")
						return false;
					var next = s.Lookahead(1).text;
					return next == "return" || next == "break";
				}, yieldStatement)
			| "yield return" | "yield break;" // auto-completion hint only, while the actual code will be handled by the previous line
			| new If(new Lit("checked") - "{", checkedStatement)
			| new If(new Lit("unchecked") - "{", uncheckedStatement)
			| expressionStatement  // expression!
			| ".STATEMENT"
			) { semantics = SemanticFlags.CodeBlockScope });

		parser.Add(new Rule("lockStatement",
			new Seq( "lock", "(", expression, ")", embeddedStatement )
			));

		parser.Add(new Rule("checkedStatement",
			new Seq( "checked", block )
		));
		
		parser.Add(new Rule("uncheckedStatement",
			new Seq( "unchecked", block )
		));
		
		parser.Add(new Rule("usingStatement",
			new Seq( "using", "(", resourceAcquisition, ")", embeddedStatement )
			) { semantics = SemanticFlags.UsingStatementScope });

		parser.Add(new Rule("resourceAcquisition",
			new If(localVariableType - IDENTIFIER, localVariableDeclaration)
			| expression
			));

		parser.Add(new Rule("yieldStatement",
			YIELD -
				(( "return" - expression - ";" )
				| ( new Lit("break") - ";" ))
			));

		var ifStatement = new Id("ifStatement");
		var elseStatement = new Id("elseStatement");
		var switchStatement = new Id("switchStatement");
		var booleanExpression = new Id("booleanExpression");
		var switchBlock = new Id("switchBlock");
		var switchSection = new Id("switchSection");
		var switchLabel = new Id("switchLabel");
		var statementExpression = new Id("statementExpression");

		parser.Add(new Rule("selectionStatement",
			ifStatement | switchStatement
			));

		parser.Add(new Rule("ifStatement",
			new Lit("if") - "(" - booleanExpression - ")" - embeddedStatement - new Opt(elseStatement)
			));

		parser.Add(new Rule("elseStatement",
			"else" - embeddedStatement
			));

		parser.Add(new Rule("switchStatement",
			new Lit("switch") - "(" - expression - ")" - switchBlock
			));

		parser.Add(new Rule("switchBlock",
			"{" - new Many(switchSection) - "}"
			) { semantics = SemanticFlags.SwitchBlockScope });

		parser.Add(new Rule("switchSection",
			new Many(switchLabel) - statement - statementList
			));

		parser.Add(new Rule("switchLabel",
			"case" - constantExpression - ":"
			| new Seq( "default", ":" )
			));

		parser.Add(new Rule("expressionStatement",
			statementExpression - ";"
			));

		parser.Add(new Rule("jumpStatement",
			breakStatement
			| continueStatement
			| gotoStatement
			| returnStatement
			| throwStatement
			));

		parser.Add(new Rule("breakStatement",
			new Seq( "break", ";" )
			));

		parser.Add(new Rule("continueStatement",
			new Seq( "continue", ";" )
			));

		parser.Add(new Rule("gotoStatement",
			"goto" - (
				IDENTIFIER
				| "case" - constantExpression
				| "default")
			- ";"
			));

		parser.Add(new Rule("returnStatement",
			"return" - new Opt(expression) - ";"
			));

		parser.Add(new Rule("throwStatement",
			"throw" - new Opt(expression) - ";"
			));

		var catchClauses = new Id("catchClauses");
		var finallyClause = new Id("finallyClause");
		var specificCatchClauses = new Id("specificCatchClauses");
		var specificCatchClause = new Id("specificCatchClause");
		var catchExceptionIdentifier = new Id("catchExceptionIdentifier");
		var generalCatchClause = new Id("generalCatchClause");

		parser.Add(new Rule("tryStatement",
			"try" - block - (catchClauses - new Opt(finallyClause) | finallyClause)
			));

		parser.Add(new Rule("catchClauses",
			new If(new Seq( "catch", "(" ), specificCatchClauses - new Opt(generalCatchClause))
			| generalCatchClause
			));

		parser.Add(new Rule("specificCatchClauses",
			specificCatchClause - new Many(new If(new Seq( "catch", "(" ), specificCatchClause ))
			));

		parser.Add(new Rule("specificCatchClause",
			new Lit("catch") - "(" - exceptionClassType - new Opt(catchExceptionIdentifier) - ")" - block
			) { semantics = SemanticFlags.SpecificCatchScope });

		parser.Add(new Rule("catchExceptionIdentifier",
			NAME
			) { semantics = SemanticFlags.CatchExceptionParameterDeclaration });

		parser.Add(new Rule("generalCatchClause",
			"catch" - block
			));

		parser.Add(new Rule("finallyClause",
			"finally" - block
			));

		parser.Add(new Rule("formalParameterList",
			formalParameter - new Many("," - formalParameter)
			) { semantics = SemanticFlags.FormalParameterListScope });

		parser.Add(new Rule("formalParameter",
			attributes - (fixedParameter | parameterArray)
		//	| "__arglist"
			));

		parser.Add(new Rule("fixedParameter",
			new Opt(parameterModifier) - type - NAME - new Opt(defaultArgument)
			) { semantics = SemanticFlags.FixedParameterDeclaration });

		parser.Add(new Rule("parameterModifier",
			new Lit("ref") | "out" | "this"
			));

		parser.Add(new Rule("defaultArgument",
			"=" - (expression | ".EXPECTEDTYPE")
			));

		parser.Add(new Rule("parameterArray",
			"params" - type - NAME
			) { semantics = SemanticFlags.ParameterArrayDeclaration });

		var whileStatement = new Id("whileStatement");
		var doStatement = new Id("doStatement");
		var forStatement = new Id("forStatement");
		var foreachStatement = new Id("foreachStatement");
		var forInitializer = new Id("forInitializer");
		//var forCondition = new Id("forCondition");
		var forIterator = new Id("forIterator");
		var statementExpressionList = new Id("statementExpressionList");

		parser.Add(new Rule("iterationStatement",
			whileStatement | doStatement | forStatement | foreachStatement
			));

		parser.Add(new Rule("whileStatement",
			new Seq("while", "(", booleanExpression, ")", embeddedStatement)
			));

		parser.Add(new Rule("doStatement",
			"do" - embeddedStatement - "while" - "(" - booleanExpression - ")" - ";"
			));

		parser.Add(new Rule("forStatement",
			new Seq("for", "(", new Opt(forInitializer), ";", new Opt(booleanExpression), ";",
				new Opt(forIterator), ")", embeddedStatement)
			) { semantics = SemanticFlags.ForStatementScope });

		parser.Add(new Rule("forInitializer",
			new If(localVariableType - IDENTIFIER, localVariableDeclaration)
			| statementExpressionList
			));

		parser.Add(new Rule("forIterator",
			statementExpressionList
			));

		parser.Add(new Rule("foreachStatement",
			new Lit("foreach") - "(" - localVariableType - NAME - "in" - expression - ")" - embeddedStatement
			) { semantics = SemanticFlags.ForStatementScope | SemanticFlags.ForEachVariableDeclaration });

		parser.Add(new Rule("statementExpressionList",
			new Seq( statementExpression, new Many(new Seq( ",", statementExpression )) )
			));

		// TODO: should be assignment, call, increment, decrement, and new object expressions
		parser.Add(new Rule("statementExpression",
			expression
			));

		var accessorDeclarations = new Id("accessorDeclarations");
		var getAccessorDeclaration = new Id("getAccessorDeclaration");
		var setAccessorDeclaration = new Id("setAccessorDeclaration");
		var accessorModifiers = new Id("accessorModifiers");
		var accessorBody = new Id("accessorBody");

		parser.Add(new Rule("indexerDeclaration",
			new Lit("this") - "[" - formalParameterList - "]" - "{" - accessorDeclarations - "}"
			) { semantics = SemanticFlags.IndexerDeclaration | SemanticFlags.AccessorsListScope });

		parser.Add(new Rule("propertyDeclaration",
			memberName - "{" - accessorDeclarations - "}"
			) { semantics = SemanticFlags.PropertyDeclaration | SemanticFlags.AccessorsListScope });

		parser.Add(new Rule("accessorModifiers",
			"internal" - new Opt("protected")
			| "protected" - new Opt("internal")
			| "public"
			| "private"
			));

		var GET = new Id("GET");
		var SET = new Id("SET");

		parser.Add(new Rule("GET",
			IDENTIFIER | "get"
			){ contextualKeyword = true } );

		parser.Add(new Rule("SET",
			IDENTIFIER | "set"
			){ contextualKeyword = true } );

		parser.Add(new Rule("accessorDeclarations",
			attributes - new Opt(accessorModifiers)
				- (new If(s => s.Current.text == "get", getAccessorDeclaration - new Opt(attributes - new Opt(accessorModifiers) - setAccessorDeclaration))
				| setAccessorDeclaration - new Opt(attributes - new Opt(accessorModifiers) - getAccessorDeclaration))
			));

		parser.Add(new Rule("getAccessorDeclaration",
			GET - accessorBody
			) { semantics = SemanticFlags.GetAccessorDeclaration });

		parser.Add(new Rule("setAccessorDeclaration",
			SET - accessorBody
			) { semantics = SemanticFlags.SetAccessorDeclaration });

		parser.Add(new Rule("accessorBody",
			"{" - statementList - "}"
			| ";"
			) { semantics = SemanticFlags.AccessorBodyScope });


		var eventDeclarators = new Id("eventDeclarators");
		var eventDeclarator = new Id("eventDeclarator");
		var eventWithAccessorsDeclaration = new Id("eventWithAccessorsDeclaration");
		var eventAccessorDeclarations = new Id("eventAccessorDeclarations");
		var addAccessorDeclaration = new Id("addAccessorDeclaration");
		var removeAccessorDeclaration = new Id("removeAccessorDeclaration");

		parser.Add(new Rule("eventDeclaration",
			"event" - typeName - (
				new If(memberName - "{",
					eventWithAccessorsDeclaration)
				| eventDeclarators - ";" )
			));

		parser.Add(new Rule("eventWithAccessorsDeclaration",
			memberName - "{" - eventAccessorDeclarations - "}"
			) { semantics = SemanticFlags.EventWithAccessorsDeclaration | SemanticFlags.AccessorsListScope });

		parser.Add(new Rule("eventDeclarators",
			eventDeclarator - new Many("," - eventDeclarator)
			));

		parser.Add(new Rule("eventDeclarator",
			NAME - new Opt("=" - variableInitializer)
			) { semantics = SemanticFlags.EventDeclarator });

		parser.Add(new Rule("eventAccessorDeclarations",
			attributes - (
				new If(s => s.Current.text == "add", addAccessorDeclaration - attributes - removeAccessorDeclaration)
				| removeAccessorDeclaration - attributes - addAccessorDeclaration )
			));

		var ADD = new Id("ADD");
		var REMOVE = new Id("REMOVE");

		parser.Add(new Rule("ADD",
			IDENTIFIER | "add"
			) { contextualKeyword = true });

		parser.Add(new Rule("REMOVE",
			IDENTIFIER | "remove"
			) { contextualKeyword = true });

		parser.Add(new Rule("addAccessorDeclaration",
			ADD - accessorBody
			) {semantics = SemanticFlags.AddAccessorDeclaration });

		parser.Add(new Rule("removeAccessorDeclaration",
			REMOVE - accessorBody
			) {semantics = SemanticFlags.RemoveAccessorDeclaration });


		parser.Add(new Rule("fieldDeclaration",
			variableDeclarators - ";"
			));

		parser.Add(new Rule("variableDeclarators",
			new Seq( variableDeclarator, new Many(new Seq( ",", variableDeclarator )) )
			));

		parser.Add(new Rule("variableDeclarator",
			NAME - new Opt("=" - variableInitializer)
			) { semantics = SemanticFlags.VariableDeclarator });

		parser.Add(new Rule("variableInitializer",
			expression | arrayInitializer
			| ".EXPECTEDTYPE"
			));

		//parser.Add(new Rule("arrayInitializer",
		//    new Seq( "{", new Opt(variableInitializerList), "}" )
		//    ));

		parser.Add(new Rule("modifiers",
			new Many(
				new Lit("new") | "public" | "protected" | "internal" | "private" | "abstract"
				| "sealed" | "static" | "readonly" | "volatile" | "virtual" | "override" | "extern"
				)
			));

		var operatorDeclarator = new Id("operatorDeclarator");
		var operatorBody = new Id("operatorBody");
		var operatorParameter = new Id("operatorParameter");
		var binaryOperatorPart = new Id("binaryOperatorPart");
		var unaryOperatorPart = new Id("unaryOperatorPart");
		var overloadableBinaryOperator = new Id("overloadableBinaryOperator");
		var overloadableUnaryOperator = new Id("overloadableUnaryOperator");
		var conversionOperatorDeclarator = new Id("conversionOperatorDeclarator");

		parser.Add(new Rule("operatorDeclaration",
			new Seq( operatorDeclarator, operatorBody )
			));

		parser.Add(new Rule("operatorDeclarator",
			new Seq( "operator",
			new Seq( new Lit("+") | "-", "(", operatorParameter, binaryOperatorPart | unaryOperatorPart )
				| overloadableUnaryOperator - "(" - operatorParameter - unaryOperatorPart
				| overloadableBinaryOperator - "(" - operatorParameter - binaryOperatorPart)
			) { semantics = SemanticFlags.OperatorDeclarator });

		parser.Add(new Rule("operatorParameter",
			type - NAME
			) { semantics = SemanticFlags.FixedParameterDeclaration });

		parser.Add(new Rule("unaryOperatorPart",
			")"
			));

		parser.Add(new Rule("binaryOperatorPart",
			"," - operatorParameter - ")"
			));

		parser.Add(new Rule("overloadableUnaryOperator",
			/*'+' |  '-' | */ new Lit("!") |  "~" |  "++" | "--" | "true" |  "false"
			));

		parser.Add(new Rule("overloadableBinaryOperator",
			/*'+' | '-' | */
			// >> check needed
			new Lit("*") | "/" | "%" | "&" | "|" | "^" | "<<" | new If(new Seq(">",">"), new Seq(">",">")) | "==" | "!=" | ">" | "<" | ">=" | "<="
			));

		parser.Add(new Rule("conversionOperatorDeclaration",
			conversionOperatorDeclarator - operatorBody
			));

		parser.Add(new Rule("conversionOperatorDeclarator",
			(new Lit("implicit") | "explicit") - "operator" - type - "(" - operatorParameter - ")"
			) { semantics = SemanticFlags.ConversionOperatorDeclarator });

		parser.Add(new Rule("operatorBody",
			"{" - statementList - "}"
			| ";"
			) { semantics = SemanticFlags.MethodBodyScope });

		var typeOrGeneric = new Id("typeOrGeneric");
		var typeArgumentList = new Id("typeArgumentList");
		var unboundTypeRank = new Id("unboundTypeRank");
		var structInterfaces = new Id("structInterfaces");
		var structBody = new Id("structBody");
		var structMemberDeclaration = new Id("structMemberDeclaration");

		parser.Add(new Rule("structDeclaration",
			"struct" - NAME - new Opt(typeParameterList) - new Opt(structInterfaces)
			- new Opt(typeParameterConstraintsClauses) - structBody - new Opt(";")
			) { semantics = SemanticFlags.StructDeclaration | SemanticFlags.TypeDeclarationScope });

		//struct_modifier:
		//    'new' | 'public' | 'protected' | 'internal' | 'private' | 'unsafe' ;

		parser.Add(new Rule("structInterfaces",
			":" - interfaceTypeList
			) { semantics = SemanticFlags.StructInterfacesScope });

		parser.Add(new Rule("structBody",
			"{" - new Many(structMemberDeclaration) - "}"
			) { semantics = SemanticFlags.StructBodyScope });

		parser.Add(new Rule("structMemberDeclaration",
			new Seq(
				attributes,
				modifiers,
				constantDeclaration
				| "void" - methodDeclaration
				| new If(PARTIAL, PARTIAL - ("void" - methodDeclaration | classDeclaration | structDeclaration | interfaceDeclaration))
				| new If(IDENTIFIER - "(", constructorDeclaration)
				| new Seq(
					type,
					new If(memberName - "(", methodDeclaration)
					| new If(memberName - "{", propertyDeclaration)
					| new If(memberName - "." - "this", typeName - "." - indexerDeclaration)
					| new If(predefinedType - "." - "this", predefinedType - "." - indexerDeclaration)
					| indexerDeclaration
					| fieldDeclaration
					| operatorDeclaration )
				| classDeclaration
				| structDeclaration
				| interfaceDeclaration
				| enumDeclaration
				| delegateDeclaration
				| eventDeclaration
				| conversionOperatorDeclaration
				| ".STRUCTBODY" )
			));

		var interfaceBase = new Id("interfaceBase");
		var interfaceBody = new Id("interfaceBody");
		var interfaceMemberDeclaration = new Id("interfaceMemberDeclaration");
		var interfaceMethodDeclaration = new Id("interfaceMethodDeclaration");
		var interfaceEventDeclaration = new Id("interfaceEventDeclaration");
		var interfacePropertyDeclaration = new Id("interfacePropertyDeclaration");
		var interfaceIndexerDeclaration = new Id("interfaceIndexerDeclaration");
		var interfaceAccessorDeclarations = new Id("interfaceAccessorDeclarations");
		var interfaceGetAccessorDeclaration = new Id("interfaceGetAccessorDeclaration");
		var interfaceSetAccessorDeclaration = new Id("interfaceSetAccessorDeclaration");

		parser.Add(new Rule("interfaceDeclaration",
			"interface" - NAME - new Opt(typeParameterList) - new Opt(interfaceBase) -
				new Opt(typeParameterConstraintsClauses) - interfaceBody - new Opt(";")
			) { semantics = SemanticFlags.InterfaceDeclaration | SemanticFlags.TypeDeclarationScope });

		parser.Add(new Rule("interfaceBase",
			":" - interfaceTypeList
			) { semantics = SemanticFlags.InterfaceBaseScope });

		parser.Add(new Rule("interfaceBody",
			"{" - new Many(interfaceMemberDeclaration) - "}"
			) { semantics = SemanticFlags.InterfaceBodyScope });

		parser.Add(new Rule("interfaceMemberDeclaration",
			new Seq( attributes, modifiers,
				"void" - interfaceMethodDeclaration
				| interfaceEventDeclaration
				| new Seq( type,
					new If(memberName - "(", interfaceMethodDeclaration)
					| new If(memberName - "{", interfacePropertyDeclaration)
					| interfaceIndexerDeclaration )
				| ".INTERFACEBODY" )
			));

		parser.Add(new Rule("interfacePropertyDeclaration",
			NAME - "{" - interfaceAccessorDeclarations - "}"
			) { semantics = SemanticFlags.InterfacePropertyDeclaration });

		parser.Add(new Rule("interfaceMethodDeclaration",
			NAME - new Opt(typeParameterList) - "(" - new Opt(formalParameterList) - ")"
				- new Opt(typeParameterConstraintsClauses) - ";"
			) { semantics = SemanticFlags.MethodDeclarationScope | SemanticFlags.InterfaceMethodDeclaration });

		parser.Add(new Rule("interfaceEventDeclaration",
			new Seq( "event", typeName, NAME, ";" )
			) { semantics = SemanticFlags.InterfaceEventDeclaration });

		parser.Add(new Rule("interfaceIndexerDeclaration",
			new Seq( "this", "[", formalParameterList, "]", "{", interfaceAccessorDeclarations, "}" )
			) { semantics = SemanticFlags.InterfaceIndexerDeclaration });

		parser.Add(new Rule("interfaceAccessorDeclarations",
			new Seq( attributes,
				new If(s => s.Current.text == "get", interfaceGetAccessorDeclaration - new Opt(attributes - interfaceSetAccessorDeclaration))
				| interfaceSetAccessorDeclaration - new Opt(attributes - interfaceGetAccessorDeclaration) )
			));

		parser.Add(new Rule("interfaceGetAccessorDeclaration",
			GET - ";"
			) { semantics = SemanticFlags.InterfaceGetAccessorDeclaration });

		parser.Add(new Rule("interfaceSetAccessorDeclaration",
			SET - ";"
			) { semantics = SemanticFlags.InterfaceSetAccessorDeclaration });

		var enumBase = new Id("enumBase");
		var enumBody = new Id("enumBody");
		var enumMemberDeclarations = new Id("enumMemberDeclarations");
		var enumMemberDeclaration = new Id("enumMemberDeclaration");

		parser.Add(new Rule("enumDeclaration",
			"enum" - NAME - new Opt(enumBase) - enumBody - new Opt(";")
			) { semantics = SemanticFlags.EnumDeclaration | SemanticFlags.TypeDeclarationScope });

		parser.Add(new Rule("enumBase",
			new Seq( ":", integralType )
			) { semantics = SemanticFlags.EnumBaseScope } );

		parser.Add(new Rule("enumBody",
			"{" - new Opt(enumMemberDeclarations) - "}"
			) { semantics = SemanticFlags.EnumBodyScope });

		parser.Add(new Rule("enumMemberDeclarations",
			new Seq(
				enumMemberDeclaration,
				new Many(new IfNot(new Seq( ",", "}" ) | "}", "," - enumMemberDeclaration)), new Opt(",") )
			));

		parser.Add(new Rule("enumMemberDeclaration",
			attributes - NAME - new Opt("=" - constantExpression)
			) { semantics = SemanticFlags.EnumMemberDeclaration });

		parser.Add(new Rule("delegateDeclaration",
			"delegate" - ("void" | type) - NAME - new Opt(typeParameterList)
				- "(" - new Opt(formalParameterList) - ")" - new Opt(typeParameterConstraintsClauses) - ";"
			) { semantics = SemanticFlags.DelegateDeclaration | SemanticFlags.TypeDeclarationScope });

		var attributeTargetSpecifier = new Id("attributeTargetSpecifier");
		var ATTRIBUTETARGET = new Id("ATTRIBUTETARGET");
		parser.Add(new Rule("ATTRIBUTETARGET",
			(IDENTIFIER | "event:" | "return:" | "field:" | "method:" | "param:" | "property:" | "type:" | "assembly:" | "module:")
			) { contextualKeyword = true });

		parser.Add(new Rule("attributeTargetSpecifier",
			(new If(s => s.Current.text == "field"
				|| s.Current.text == "method"
				|| s.Current.text == "param"
				|| s.Current.text == "property"
				|| s.Current.text == "type"
				|| s.Current.text == "assembly"
				|| s.Current.text == "module",
					ATTRIBUTETARGET) | "event" | "return") - ":"
			));

		parser.Add(new Rule("attributes",
			new Many(
				"[" - new If(attributeTargetSpecifier, attributeTargetSpecifier) - attribute -
					new Many("," - attribute) - "]"
			)
			) { semantics = SemanticFlags.AttributesScope });

		parser.Add(new Rule("attribute",
			typeName - new Opt(attributeArguments)
			| ".ATTRIBUTE"
			));

		var rankSpecifiers = new Id("rankSpecifiers");

		parser.Add(new Rule("type",
			predefinedType - new Opt("?") - new Opt(rankSpecifiers)
			| typeName - new Opt("?") - new Opt(rankSpecifiers)
			));

		parser.Add(new Rule("type2",
			predefinedType - new Opt(rankSpecifiers)
			| typeName - new Opt(rankSpecifiers)
			));

		//parser.Add(new Rule("typeSpecifier",
		//    predefinedType - new Opt("?") - new Opt(rankSpecifiers)
		//    | typeName - new Opt("?") - new Opt(rankSpecifiers)
		//    ));

		parser.Add(new Rule("exceptionClassType",
			typeName | "object" | "string"
			));

		parser.Add(new Rule("nonArrayType",
			(typeName | simpleType) - new Opt("?")
			| "object"
			| "string"
			));

	//	parser.Add(new Rule("arrayType",
	//		nonArrayType - rankSpecifiers
	//		));

		parser.Add(new Rule("simpleType",
			numericType | "bool"
			));
			
		parser.Add(new Rule("numericType",
			integralType | floatingPointType | "decimal"
			));

		parser.Add(new Rule("integralType",
			new Lit("sbyte") | "byte" | "short" | "ushort" | "int" | "uint" | "long" | "ulong" | "char"
			));

		parser.Add(new Rule("floatingPointType",
			new Lit("float") | "double"
			));

		parser.Add(new Rule("typeName",
			namespaceOrTypeName
			));
		
		parser.Add(new Rule("globalNamespace",
			IDENTIFIER | "global"
			));

		parser.Add(new Rule("namespaceOrTypeName",
			new If(IDENTIFIER - "::", globalNamespace - "::") -
			typeOrGeneric - new Many("." - typeOrGeneric)
			));

		parser.Add(new Rule("typeOrGeneric",
			IDENTIFIER - new If("<" - type, typeArgumentList) - new If("<" - (new Lit(",") | ">"), unboundTypeRank)
//				new If(new Seq(IDENTIFIER, typeArgumentList), new Seq(IDENTIFIER, typeArgumentList))
//				| IDENTIFIER
			));

		parser.Add(new Rule("typeArgumentList",
			"<" - type - new Many("," - type) - ">"
			));

		parser.Add(new Rule("unboundTypeRank",
			"<" - new Many(",") - ">"
			));

		//parser.Add(new Rule("typeArguments",
		//    type - new Many("," - type)
		//    ));

		parser.Add(new Rule("rankSpecifier",
			"[" - new Many(",") - "]"
			));

		var unaryExpression = new Id("unaryExpression");
		var assignmentOperator = new Id("assignmentOperator");
		var assignment = new Id("assignment");
		var nonAssignmentExpression = new Id("nonAssignmentExpression");
		var castExpression = new Id("castExpression");

		parser.Add(new Rule("expression",
			new If(unaryExpression - assignmentOperator, assignment)
			| nonAssignmentExpression
			));

		parser.Add(new Rule("assignment",
			unaryExpression - assignmentOperator - (expression | ".EXPECTEDTYPE")
			));

		var preIncrementExpression = new Id("preIncrementExpression");
		var preDecrementExpression = new Id("preDecrementExpression");

		parser.Add(new Rule("unaryExpression",
			new If("(" - type - ")" - IDENTIFIER, castExpression)
			| new If(castExpression, castExpression)
			| primaryExpression - new Many(new Lit("++") | "--") // TODO: Fix this! Post increment operators should be primaryExpressionPart
			| new Seq( new Lit("+") | "-" | "!", unaryExpression )
			| new Seq( "~", unaryExpression | ".EXPECTEDTYPE" )
			| preIncrementExpression
			| preDecrementExpression
			));

		parser.Add(new Rule("castExpression",
			"(" - type - ")" - unaryExpression
			));

		parser.Add(new Rule("assignmentOperator",
			new Lit("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" | "<<=" | new Seq(">", ">=")
			));

		parser.Add(new Rule("preIncrementExpression",
			"++" - unaryExpression
			));

		parser.Add(new Rule("preDecrementExpression",
			"--" - unaryExpression
			));

		var brackets = new Id("brackets");
		var primaryExpressionStart = new Id("primaryExpressionStart");
		var primaryExpressionPart = new Id("primaryExpressionPart");
		var objectCreationExpression = new Id("objectCreationExpression");
		//var delegateCreationExpression = new Id("delegateCreationExpression");
		var anonymousObjectCreationExpression = new Id("anonymousObjectCreationExpression");
		var sizeofExpression = new Id("sizeofExpression");
		var checkedExpression = new Id("checkedExpression");
		var uncheckedExpression = new Id("uncheckedExpression");
		var defaultValueExpression = new Id("defaultValueExpression");
		var anonymousMethodExpression = new Id("anonymousMethodExpression");

//*
		parser.Add(new Rule("primaryExpression",
			primaryExpressionStart - new Many(primaryExpressionPart)
			| new Seq( "new",
				((nonArrayType | ".EXPECTEDTYPE") - (objectCreationExpression | arrayCreationExpression))
				| implicitArrayCreationExpression
				| anonymousObjectCreationExpression,
				new Many(primaryExpressionPart))
			| anonymousMethodExpression
			));

		var parenExpression = new Id("parenExpression");
		var typeofExpression = new Id("typeofExpression");
		var qidStart = new Id("qidStart");
		var qidPart = new Id("qidPart");
		var accessIdentifier = new Id("accessIdentifier");

		parser.Add(new Rule("typeofExpression",
			new Lit("typeof") - "(" - (type | "void") - ")"
			));
			
		parser.Add(new Rule("predefinedType",
			new Lit("bool") | "byte" | "char" | "decimal" | "double" | "float" | "int" | "long"
			| "object" | "sbyte" | "short" | "string" | "uint" | "ulong" | "ushort"
			));

		parser.Add(new Rule("qid",
			qidStart - new Many(qidPart)
			));

		parser.Add(new Rule("qidStart",
			predefinedType
			| new If(IDENTIFIER - "<", NAME - typeParameterList)
			| IDENTIFIER - new Opt("::" - IDENTIFIER)
			));

		parser.Add(new Rule("qidPart",
			accessIdentifier
			));

		parser.Add(new Rule("primaryExpressionStart",
			predefinedType
			| LITERAL | "true" | "false"
			| new If(IDENTIFIER - typeArgumentList /*- (new Lit("(") | ")" | ":" | ";" | "," | "." | "?" | "==" | "!="*/, IDENTIFIER - typeArgumentList)
			| new If(IDENTIFIER - "::", globalNamespace - "::") - IDENTIFIER
			| parenExpression
			| "this"
			| "base"
			| typeofExpression
			| sizeofExpression
			| checkedExpression
			| uncheckedExpression
			| defaultValueExpression
			));

		//var bracketsOrArguments = new Id("bracketsOrArguments");
		var argument = new Id("argument");
		var attributeArgument = new Id("attributeArgument");
		var expressionList = new Id("expressionList");
		var argumentValue = new Id("argumentValue");
		var argumentName = new Id("argumentName");
		var attributeMemberName = new Id("attributeMemberName");
		var variableReference = new Id("variableReference");

		parser.Add(new Rule("primaryExpressionPart",
			accessIdentifier
			| brackets
			| arguments
			));

		parser.Add(new Rule("accessIdentifier",
			"." - IDENTIFIER - new If(typeArgumentList, typeArgumentList)
			));

		//parser.Add(new Rule("bracketsOrArguments",
		//    brackets | arguments
		//    ));

		parser.Add(new Rule("brackets",
			"[" - new Opt(expressionList) - "]"
			));

		parser.Add(new Rule("expressionList",
			expression - new Many("," - expression)
			));

		parser.Add(new Rule("parenExpression",	
			"(" - expression - ")"
			));

		parser.Add(new Rule("arguments",
			"(" - argumentList - ")"
			));

		parser.Add(new Rule("attributeArguments",
			"(" - attributeArgumentList - ")"
		));
		
		parser.Add(new Rule("argumentList",
			new Opt(argument - new Many("," - argument))
			) { semantics = SemanticFlags.ArgumentListScope });

		parser.Add(new Rule("attributeArgumentList",
			new Opt(attributeArgument - new Many("," - attributeArgument))
			) { semantics = SemanticFlags.AttributeArgumentsScope });

		parser.Add(new Rule("argument",
			new If(IDENTIFIER - ":", argumentName - argumentValue)
			| argumentValue
			));

		parser.Add(new Rule("attributeArgument",
			new If(IDENTIFIER - "=", attributeMemberName - argumentValue)
			| new If(IDENTIFIER - ":", argumentName - argumentValue)
			| argumentValue
			));

		parser.Add(new Rule("argumentName",
			IDENTIFIER - ":"
			));

		parser.Add(new Rule("attributeMemberName",
			IDENTIFIER - "="
			));

		parser.Add(new Rule("argumentValue",
			expression
			| new Seq( new Lit("out") | "ref", variableReference )
			| ".EXPECTEDTYPE"
			));

		parser.Add(new Rule("variableReference",
			expression
			));

		parser.Add(new Rule("rankSpecifiers",
			"[" - new Many(",") - "]" - new Many("[" - new Many(",") - "]")
			));

		//parser.Add(new Rule("delegateCreationExpression",
		//    new Seq( typeName, "(", typeName, ")" )
		//    ));

		var anonymousObjectInitializer = new Id("anonymousObjectInitializer");
		var memberDeclaratorList = new Id("memberDeclaratorList");
		var memberDeclarator = new Id("memberDeclarator");
		var memberAccessExpression = new Id("memberAccessExpression");

		parser.Add(new Rule("anonymousObjectCreationExpression",
			anonymousObjectInitializer
			) { semantics = SemanticFlags.AnonymousObjectCreation });
			
		parser.Add(new Rule("anonymousObjectInitializer",
			"{" - new Opt(memberDeclaratorList) - new Opt(",") - "}"
			));

		parser.Add(new Rule("memberDeclaratorList",
			memberDeclarator - new Many( new If("," - IDENTIFIER, "," - memberDeclarator) )
			));
 
		parser.Add(new Rule("memberDeclarator",
			new If(IDENTIFIER - "=", IDENTIFIER - "=" - expression)
			| memberAccessExpression
			) { semantics = SemanticFlags.MemberDeclarator });

		parser.Add(new Rule("memberAccessExpression",
			expression
			));

		//parser.Add(new Rule("primaryOrArrayCreationExpression",
		//    new If( /*arrayCreationExpression*/ "new" - new Opt(nonArrayType) - "[", arrayCreationExpression )
		//    | primaryExpression
		//    ));

		var objectOrCollectionInitializer = new Id("objectOrCollectionInitializer");
		var objectInitializer = new Id("objectInitializer");
		var collectionInitializer = new Id("collectionInitializer");
		var elementInitializerList = new Id("elementInitializerList");
		var elementInitializer = new Id("elementInitializer");
		var memberInitializerList = new Id("memberInitializerList");
		var memberInitializer = new Id("memberInitializer");

		parser.Add(new Rule("objectCreationExpression",
			arguments - new Opt(objectOrCollectionInitializer)
			| objectOrCollectionInitializer
			));

		parser.Add(new Rule("objectOrCollectionInitializer",
			"{" - ((new If(IDENTIFIER - "=", objectInitializer) | "}" | collectionInitializer))
			));

		parser.Add(new Rule("collectionInitializer",
			elementInitializerList - new Opt(",") - "}"
			));

		parser.Add(new Rule("elementInitializerList",
			elementInitializer - new Many(new IfNot(new Opt(",") - "}", "," - elementInitializer))
			));

		parser.Add(new Rule("elementInitializer",
			nonAssignmentExpression
			| "{" - expressionList - "}"
			| ".EXPECTEDTYPE"
			));

		parser.Add(new Rule("objectInitializer",
			new Opt(memberInitializerList) - new Opt(",") - (new Lit("}") | ".MEMBERINITIALIZER")
			));

		parser.Add(new Rule("memberInitializerList",
			memberInitializer - new Many(new IfNot(new Opt(",") - "}", "," - memberInitializer))
			) { semantics = SemanticFlags.MemberInitializerScope });

		parser.Add(new Rule("memberInitializer",
			(IDENTIFIER | ".MEMBERINITIALIZER") - "=" - (expression | objectOrCollectionInitializer | ".EXPECTEDTYPE")
			));

		//var invocationPart = new Id("invocationPart");
		var explicitAnonymousFunctionParameterList = new Id("explicitAnonymousFunctionParameterList");
		var explicitAnonymousFunctionParameter = new Id("explicitAnonymousFunctionParameter");
		var anonymousFunctionParameterModifier = new Id("anonymousFunctionParameterModifier");
		var explicitAnonymousFunctionSignature = new Id("explicitAnonymousFunctionSignature");

		parser.Add(new Rule("arrayCreationExpression",
			new If( new Seq("[", new Lit(",") | "]"), rankSpecifiers - arrayInitializer )
			|  "[" - expressionList - "]" - new Opt(rankSpecifiers) - new Opt(arrayInitializer)
			));

		parser.Add(new Rule("implicitArrayCreationExpression",
			rankSpecifier - arrayInitializer
			));

		//parser.Add(new Rule("invocationPart",
		//    accessIdentifier | brackets
		//    ));

		parser.Add(new Rule("arrayInitializer",
			"{" - new Opt(variableInitializerList) - "}"
			));

		parser.Add(new Rule("variableInitializerList",
			variableInitializer - new Many(new IfNot(new Seq(",", "}"), "," - new Opt(variableInitializer))) - new Opt(",")
			));

		parser.Add(new Rule("sizeofExpression",
			new Lit("sizeof") - "(" - simpleType - ")"
			));

		parser.Add(new Rule("checkedExpression",
			new Lit("checked") - "(" - expression - ")"
		));
		
		parser.Add(new Rule("uncheckedExpression",
			new Lit("unchecked") - "(" - expression - ")"
		));
		
		parser.Add(new Rule("defaultValueExpression",
			new Seq( "default", "(", type, ")" )
			));

		var anonymousFunctionSignature = new Id("anonymousFunctionSignature");
		var lambdaExpression = new Id("lambdaExpression");
		var queryExpression = new Id("queryExpression");
		var conditionalExpression = new Id("conditionalExpression");
		var lambdaExpressionBody = new Id("lambdaExpressionBody");
		var anonymousMethodBody = new Id("anonymousMethodBody");
		var implicitAnonymousFunctionParameterList = new Id("implicitAnonymousFunctionParameterList");
		var implicitAnonymousFunctionParameter = new Id("implicitAnonymousFunctionParameter");

		var FROM = new Id("FROM"); parser.Add(new Rule("FROM", IDENTIFIER | "from") { contextualKeyword = true });
		var SELECT = new Id("SELECT"); parser.Add(new Rule("SELECT", IDENTIFIER) { contextualKeyword = true });
		var GROUP = new Id("GROUP"); parser.Add(new Rule("GROUP", IDENTIFIER | "group") { contextualKeyword = true });
		var INTO = new Id("INTO"); parser.Add(new Rule("INTO", new If(s => s.Current.text == "into", IDENTIFIER | "into")) { contextualKeyword = true });
		var ORDERBY = new Id("ORDERBY"); parser.Add(new Rule("ORDERBY", new If(s => s.Current.text == "orderby", IDENTIFIER | "orderby")) { contextualKeyword = true });
		var JOIN = new Id("JOIN"); parser.Add(new Rule("JOIN", new If(s => s.Current.text == "join", IDENTIFIER | "join")) { contextualKeyword = true });
		var LET = new Id("LET"); parser.Add(new Rule("LET", new If(s => s.Current.text == "let", IDENTIFIER | "let")) { contextualKeyword = true });
		var ON = new Id("ON"); parser.Add(new Rule("ON", new If(s => s.Current.text == "on", IDENTIFIER | "on")) { contextualKeyword = true });
		var EQUALS = new Id("EQUALS"); parser.Add(new Rule("EQUALS", new If(s => s.Current.text == "equals", IDENTIFIER | "equals")) { contextualKeyword = true });
		var BY = new Id("BY"); parser.Add(new Rule("BY", new If(s => s.Current.text == "by", IDENTIFIER | "by")) { contextualKeyword = true });
		var ASCENDING = new Id("ASCENDING"); parser.Add(new Rule("ASCENDING", IDENTIFIER | "ascending") { contextualKeyword = true });
		var DESCENDING = new Id("DESCENDING"); parser.Add(new Rule("DESCENDING", IDENTIFIER | "descending") { contextualKeyword = true });

		var fromClause = new Id("fromClause");

		parser.Add(new Rule("nonAssignmentExpression",
			new If(anonymousFunctionSignature - "=>", lambdaExpression)
			| new If(IDENTIFIER - IDENTIFIER - "in", queryExpression)
			| new If(IDENTIFIER - type - IDENTIFIER - "in", queryExpression)
			| conditionalExpression
			));

		parser.Add(new Rule("lambdaExpression",
			anonymousFunctionSignature - "=>" - lambdaExpressionBody
			) { semantics = SemanticFlags.LambdaExpressionScope | SemanticFlags.LambdaExpressionDeclaration });

		parser.Add(new Rule("anonymousFunctionSignature",
			new Seq(
				"(",
				new Opt(
					new If(new Seq(IDENTIFIER, new Lit(",") | ")"), implicitAnonymousFunctionParameterList)
					| explicitAnonymousFunctionParameterList),
				")")
			| implicitAnonymousFunctionParameter
			));// { semantics = SemanticFlags.LambdaExpressionDeclaration });

		parser.Add(new Rule("implicitAnonymousFunctionParameterList",
			implicitAnonymousFunctionParameter - new Many("," - implicitAnonymousFunctionParameter)
			));

		parser.Add(new Rule("implicitAnonymousFunctionParameter",
			IDENTIFIER
			) { semantics = SemanticFlags.ImplicitParameterDeclaration });

		parser.Add(new Rule("lambdaExpressionBody",
			expression
			| "{" - statementList - "}"
			) { semantics = SemanticFlags.LambdaExpressionBodyScope });

		parser.Add(new Rule("anonymousMethodExpression",
			"delegate" - new Opt(explicitAnonymousFunctionSignature) - anonymousMethodBody
			) { semantics = SemanticFlags.AnonymousMethodDeclaration | SemanticFlags.AnonymousMethodScope });

		parser.Add(new Rule("anonymousMethodBody",
			"{" - statementList - "}"
			) { semantics = SemanticFlags.AnonymousMethodBodyScope });

		parser.Add(new Rule("explicitAnonymousFunctionSignature",
			"(" - new Opt(explicitAnonymousFunctionParameterList) - ")"
			) { semantics = SemanticFlags.FormalParameterListScope });

		parser.Add(new Rule("explicitAnonymousFunctionParameterList",
			explicitAnonymousFunctionParameter - new Many("," - explicitAnonymousFunctionParameter)
			));

		parser.Add(new Rule("explicitAnonymousFunctionParameter",
			new Opt(anonymousFunctionParameterModifier) - type - NAME
			) { semantics = SemanticFlags.ExplicitParameterDeclaration });

		parser.Add(new Rule("anonymousFunctionParameterModifier",
			new Lit("ref") | "out"
			));

		var queryBody = new Id("queryBody");
		var queryBodyClause = new Id("queryBodyClause");
		var queryContinuation = new Id("queryContinuation");
		var letClause = new Id("letClause");
		var whereClause = new Id("whereClause");
		var joinClause = new Id("joinClause");
		var orderbyClause = new Id("orderbyClause");
		var orderingList = new Id("orderingList");
		var ordering = new Id("ordering");
		var selectClause = new Id("selectClause");
		var groupClause = new Id("groupClause");

		parser.Add(new Rule("queryExpression",
			fromClause - queryBody
			) { semantics = SemanticFlags.QueryExpressionScope });

		parser.Add(new Rule("fromClause",
			FROM - (new If(NAME - "in", NAME) | type - NAME) - "in" - expression
			) { semantics = SemanticFlags.FromClauseVariableDeclaration });

		parser.Add(new Rule("queryBody",
			new Many(
				new If(s => s.Current.text == "from"
					| s.Current.text == "let"
					| s.Current.text == "join"
					| s.Current.text == "orderby"
					| s.Current.text == "where", queryBodyClause)
			) - (new If(s => s.Current.text == "select", selectClause) | "select" | groupClause) - new Opt(queryContinuation)
			) { semantics = SemanticFlags.QueryBodyScope });

		parser.Add(new Rule("queryContinuation",
			INTO - IDENTIFIER - queryBody
			));

		parser.Add(new Rule("queryBodyClause",
			new If(s => s.Current.text == "from", fromClause)
			| new If(s => s.Current.text == "let", letClause)
			| new If(s => s.Current.text == "join", joinClause)
			| new If(s => s.Current.text == "orderby", orderbyClause)
			| whereClause
			));

		parser.Add(new Rule("joinClause",
			JOIN - new Opt(type) - IDENTIFIER - "in" - expression - ON - expression - EQUALS - expression - new Opt(INTO - IDENTIFIER)
			));

		parser.Add(new Rule("letClause",
			LET - IDENTIFIER - "=" - expression
			));

		parser.Add(new Rule("orderbyClause",
			ORDERBY - orderingList
			));

		parser.Add(new Rule("orderingList",
			ordering - new Many("," - ordering)
			));

		parser.Add(new Rule("ordering",
			expression - new Opt(new If(s => s.Current.text == "ascending", ASCENDING) | DESCENDING)
			));

		parser.Add(new Rule("selectClause",
			SELECT - expression
			));

		parser.Add(new Rule("groupClause",
			GROUP- expression - BY - expression
			));

		parser.Add(new Rule("whereClause",
			WHERE - booleanExpression
			));

		parser.Add(new Rule("booleanExpression",
			expression
			));

		var nullCoalescingExpression = new Id("nullCoalescingExpression");
		var conditionalOrExpression = new Id("conditionalOrExpression");
		var conditionalAndExpression = new Id("conditionalAndExpression");
		var inclusiveOrExpression = new Id("inclusiveOrExpression");
		var exclusiveOrExpression = new Id("exclusiveOrExpression");
		var andExpression = new Id("andExpression");
		var equalityExpression = new Id("equalityExpression");
		var relationalExpression = new Id("relationalExpression");
		var shiftExpression = new Id("shiftExpression");
		var additiveExpression = new Id("additiveExpression");
		var multiplicativeExpression = new Id("multiplicativeExpression");

		parser.Add(new Rule("conditionalExpression",
			nullCoalescingExpression - new Opt("?" - expression - ":" - expression)
			) { autoExclude = true });

		parser.Add(new Rule("nullCoalescingExpression",
			conditionalOrExpression - new Many("??" - conditionalOrExpression)
			) { autoExclude = true });

		parser.Add(new Rule("conditionalOrExpression",
			conditionalAndExpression - new Many("||" - conditionalAndExpression)
			) { autoExclude = true });

		parser.Add(new Rule("conditionalAndExpression",
			inclusiveOrExpression - new Many("&&" - inclusiveOrExpression)
			) { autoExclude = true });

		parser.Add(new Rule("inclusiveOrExpression",
			exclusiveOrExpression - new Many("|" - (exclusiveOrExpression | ".EXPECTEDTYPE"))
			) { autoExclude = true });

		parser.Add(new Rule("exclusiveOrExpression",
			andExpression - new Many("^" - andExpression)
			) { autoExclude = true });

		parser.Add(new Rule("andExpression",
			equalityExpression - new Many("&" - (equalityExpression | ".EXPECTEDTYPE"))
			) { autoExclude = true });

		parser.Add(new Rule("equalityExpression",
			relationalExpression - new Many( (new Lit("==") | "!=") - (relationalExpression | ".EXPECTEDTYPE") )
			) { autoExclude = true });

		parser.Add(new Rule("relationalExpression",
			new Seq( shiftExpression, new Many(
				new Seq( new Lit("<") | ">" | "<=" | ">=", (shiftExpression | ".EXPECTEDTYPE") )
				| new Seq( new Lit("is") | "as", new If(type2 - "?" - expression - ":", type2) | type) ))
			) { autoExclude = true });

		parser.Add(new Rule("shiftExpression",
			new Seq( additiveExpression,
				new Many(new If(new Seq(">", ">") | "<<", new Seq( new Seq(">", ">") | "<<", additiveExpression ))))
			) { autoExclude = true });

		parser.Add(new Rule("additiveExpression",
			new Seq( multiplicativeExpression, new Many(new Seq( new Lit("+") | "-", multiplicativeExpression )) )
			) { autoExclude = true });

		parser.Add(new Rule("multiplicativeExpression",
			new Seq( unaryExpression, new Many(new Seq( new Lit("*") | "/" | "%", unaryExpression )) )
			) { autoExclude = true });

		Rule.debug = false;

		parser.InitializeGrammar();
		InitializeTokenCategories();

		//Debug.Log(parser);
	}

	public void Parse(FGTextBuffer.FormatedLine[] lines, string bufferName)
	{
	//	var s = new Stopwatch();
	//	s.Start();

		//FGGramar.GoalDebugger.debug = new StringBuilder();

		var scanner = new Scanner(this, lines, bufferName);
		try
		{
			/*ParseTree parseTree =*/ parser.ParseAll(scanner);
		//	Debug.Log(parseTree);
		}
		catch (Exception e)
		{
			Debug.LogError("Parsing crashed at line: " + scanner.CurrentLine() + ", token " + scanner.CurrentTokenIndex() + " with:\n    " + e + " at " + e.StackTrace);
			Debug.Log("Current token: " + scanner.Current.tokenKind + " '" + scanner.Current.text + "'");
		}

	//	Debug.Log(FGGramar.GoalDebugger.debug.ToString());

	//	s.Stop();
	//	Debug.Log("Parsing " + System.IO.Path.GetFileName(bufferName) + " (" + lines.Length + " lines) took " + s.ElapsedMilliseconds + " ms.");

		//var sb = new StringBuilder();
		//foreach (var node in scanner.numLookaheads.Keys)
		//    sb.AppendLine(node + ": " + scanner.numLookaheads[node] + " times in " + scanner.timeLookaheads[node] + " ticks (" + scanner.timeLookaheads[node] * 1000 / Stopwatch.Frequency + " ms)");
		//Debug.Log(sb);
	}

	public ParseTree ParseAll(Scanner scanner)
	{
		return parser.ParseAll(scanner);
	}

	public bool ParseLine(Scanner scanner, int line)
	{
		var numErrors = scanner.ErrorMessage != null ? 1 : 0;
		while (scanner.CurrentLine() - 1 <= line)
		{
		/*	if (scanner.CurrentGrammarNode == EOF)
			{
			//	Debug.LogError("EOF expected at line " + (line + 1));
				return false;
			}
			else*/ if (!parser.ParseStep(scanner))
				return false;
			
			if (scanner.ErrorMessage != null)
			{
				++numErrors;
			}
			else
			{
				numErrors = 0;
			}
		}
		//if (numErrors > 0)
		//	Debug.Log(numErrors + " error steps on line " + line);
		return numErrors < 10;
	}

	public override IdentifierCompletionsType GetCompletionTypes(ParseTree.BaseNode afterNode)
	{
		var cit = IdentifierCompletionsType.Namespace | IdentifierCompletionsType.TypeName | IdentifierCompletionsType.Value;
		var leaf = afterNode as ParseTree.Leaf;
		if (leaf == null)
			return cit;

		if (leaf.token.text == ".")
			cit = IdentifierCompletionsType.Member | IdentifierCompletionsType.TypeName | IdentifierCompletionsType.Value;
		else if (leaf.token.text == "::")
			cit = IdentifierCompletionsType.Member | IdentifierCompletionsType.TypeName | IdentifierCompletionsType.Namespace;

	//	var debug = "";
	//	var debugConcatenator = "";

		var childIndex = afterNode.childIndex;
		for (var node = afterNode.parent; node != null; childIndex = node.childIndex, node = node.parent)
		{
	//		debug += debugConcatenator + node.RuleName;
	//		debugConcatenator = " <- ";

			switch (node.RuleName)
			{
				case "namespaceName":
					cit &= ~(IdentifierCompletionsType.TypeName | IdentifierCompletionsType.Value);
					break;

				case "exceptionClassType":
					//cit = IdentifierCompletionsType.ExceptionClassType;
					goto breakLoop;

				case "attributes":
				case "attribute":
					cit |= IdentifierCompletionsType.AttributeClassType;
					goto breakLoop;

				case "arguments":
				case "attributeArguments":
					goto breakLoop;

				case "argumentName":
					cit = IdentifierCompletionsType.ArgumentName;
					goto breakLoop;

				case "typeName":
					cit &= ~IdentifierCompletionsType.Value;
					break;

				case "namespaceOrTypeName":
					cit &= ~IdentifierCompletionsType.Value;
					break;

				case "usingAliasDirective":
					break;

				case "statement":
					if (childIndex == 0)
						cit |= IdentifierCompletionsType.Namespace | IdentifierCompletionsType.TypeName | IdentifierCompletionsType.Value;
					break;

				case "accessIdentifier":
					cit &= ~IdentifierCompletionsType.Namespace;
					goto breakLoop;
			}
		}

	breakLoop:

	//	UnityEngine.Debug.Log(debug + ": " + cit);
		return cit;
	}

	public static ParseTree.Node EnclosingSemanticNode(ParseTree.BaseNode node, SemanticFlags flags)
	{
		if (node is ParseTree.Leaf)
			return EnclosingSemanticNode(node.parent, flags);
		return EnclosingSemanticNode((ParseTree.Node) node, flags);
	}

	public static ParseTree.Node EnclosingSemanticNode(ParseTree.Node node, SemanticFlags flags)
	{
		while (node != null)
		{
			var scopeSemantics = node.semantics & flags;
			if (scopeSemantics != SemanticFlags.None)
				return node;
			node = node.parent;
		}
		return null;
	}

	public static ParseTree.Node EnclosingScopeNode(ParseTree.Node node)
	{
		while (node != null)
		{
			var scopeSemantics = node.semantics & SemanticFlags.ScopesMask;
			if (scopeSemantics != SemanticFlags.None)
				return node;
			node = node.parent;
		}
		return null;
	}

	public static ParseTree.Node EnclosingScopeNode(ParseTree.Node node, params SemanticFlags[] scopeTypes)
	{
		while (node != null)
		{
			var scopeSemantics = node.semantics & SemanticFlags.ScopesMask;
			if (scopeSemantics != SemanticFlags.None)
				if (scopeTypes.Contains(scopeSemantics))
					return node;
			node = node.parent;
		}
		return null;
	}

	private static Scope GetNodeScope(ParseTree.Node node, string fileName = null)
	{
		if (node == null)
			return null;
		
		var nodeScopeAsSDS = node.scope as SymbolDeclarationScope;
		if (node.scope == null || nodeScopeAsSDS != null && nodeScopeAsSDS.declaration == null)
		{
			var enclosingScopeNode = EnclosingSemanticNode(node.parent, SemanticFlags.ScopesMask);
			var enclosingScope = GetNodeScope(enclosingScopeNode, fileName);

			var scopeSemantics = node.semantics & SemanticFlags.ScopesMask;
			switch (scopeSemantics)
			{
				case SemanticFlags.CompilationUnitScope:
					var scope = AssemblyDefinition.GetCompilationUnitScope(fileName, true);
					if (scope == null)
						return null;
					scope.declaration.parseTreeNode = node;
					node.scope = scope;
					break;

				case SemanticFlags.NamespaceBodyScope:
					var declaration = GetNodeDeclaration(node.parent);
					node.scope = new NamespaceScope(node)
					{
						declaration = (NamespaceDeclaration) declaration,
						definition = (NamespaceDefinition) declaration.definition,
						parentScope = enclosingScope,
					};
					break;

				case SemanticFlags.ClassBaseScope:
				case SemanticFlags.StructInterfacesScope:
				case SemanticFlags.InterfaceBaseScope:
					declaration = (enclosingScope as SymbolDeclarationScope).declaration;
					node.scope = new TypeBaseScope(node) { parentScope = enclosingScope, definition = declaration != null ? declaration.definition as TypeDefinitionBase : null };
					break;
				case SemanticFlags.EnumBaseScope:
					node.scope = new LocalScope(node) { parentScope = enclosingScope };
					break;
				case SemanticFlags.TypeParameterConstraintsScope:
					break;
				case SemanticFlags.ClassBodyScope:
				case SemanticFlags.StructBodyScope:
				case SemanticFlags.InterfaceBodyScope:
				case SemanticFlags.EnumBodyScope:
					declaration = GetNodeDeclaration(node.parent);
					if (declaration == null)
						node.scope = new LocalScope(node) { parentScope = enclosingScope };
					else
						node.scope = new BodyScope(node) { parentScope = enclosingScope, definition = declaration.definition };
					break;
				case SemanticFlags.AnonymousMethodScope:
				case SemanticFlags.LambdaExpressionScope:
					declaration = GetNodeDeclaration(node);
					node.scope = new SymbolDeclarationScope(node) { parentScope = enclosingScope, declaration = declaration };
					break;
				case SemanticFlags.AnonymousMethodBodyScope:
				case SemanticFlags.LambdaExpressionBodyScope:
					declaration = GetNodeDeclaration(node.parent);
					node.scope = new BodyScope(node) { parentScope = enclosingScope, definition = declaration.definition };
					break;
				case SemanticFlags.MethodBodyScope:
					if ((node.parent.semantics & SemanticFlags.SymbolDeclarationsMask) != SemanticFlags.None)
						declaration = GetNodeDeclaration(node.parent);
					else
						declaration = node.parent.NodeAt(0) == null ? null : GetNodeDeclaration(node.parent.NodeAt(0));
					node.scope = declaration == null ? null :
						new BodyScope(node) { parentScope = enclosingScope, definition = declaration.definition };
					break;
				case SemanticFlags.AccessorBodyScope:
					declaration = GetNodeDeclaration(node.parent);
					node.scope = new AccessorBodyScope(node) { parentScope = enclosingScope,
						definition = declaration != null ? declaration.definition : null };
					break;
				case SemanticFlags.CodeBlockScope:
				case SemanticFlags.QueryExpressionScope:
				case SemanticFlags.QueryBodyScope:
					node.scope = new LocalScope(node) { parentScope = enclosingScope };
					break;
				case SemanticFlags.FormalParameterListScope:
					declaration =
						(node.parent.semantics & SemanticFlags.SymbolDeclarationsMask) != SemanticFlags.None
						? GetNodeDeclaration(node.parent)
						: GetNodeDeclaration(node.parent.parent);
				//	node.scope = new SymbolDeclarationScope { parentScope = enclosingScope, declaration = declaration };
					if (declaration != null)
					{
					//	Debug.Log("Adding parameter declaration scope for " + declaration.definition + " to enclosing scope " + enclosingScope);
						node.scope = new BodyScope(node) { parentScope = enclosingScope, definition = declaration.definition };
					}
					//else
					//    Debug.LogWarning("Declaration not found: " + node.parent);
					break;
				case SemanticFlags.ConstructorInitializerScope:
					break;
				case SemanticFlags.SwitchBlockScope:
				case SemanticFlags.ForStatementScope:
				case SemanticFlags.UsingStatementScope:
					node.scope = new LocalScope(node) { parentScope = enclosingScope };
					break;
				case SemanticFlags.EmbeddedStatementScope:
					break;
				case SemanticFlags.LocalVariableInitializerScope:
					break;
				case SemanticFlags.SpecificCatchScope:
					node.scope = new LocalScope(node) { parentScope = enclosingScope };
					break;
				case SemanticFlags.ArgumentListScope:
					node.scope = new LocalScope(node) { parentScope = enclosingScope };
					break;
				case SemanticFlags.AttributeArgumentsScope:
					node.scope = new AttributeArgumentsScope(node) { parentScope = enclosingScope };
					break;
				case SemanticFlags.MemberInitializerScope:
					node.scope = new MemberInitializerScope(node) { parentScope = enclosingScope };
					break;
				case SemanticFlags.TypeDeclarationScope:
					declaration = GetNodeDeclaration(node);
					node.scope = new SymbolDeclarationScope(node) { parentScope = enclosingScope, declaration = declaration };
					break;
				case SemanticFlags.MemberDeclarationScope:
					break;
				case SemanticFlags.MethodDeclarationScope:
					declaration = GetNodeDeclaration(node);
					node.scope = new SymbolDeclarationScope(node) { parentScope = enclosingScope, declaration = declaration };
					break;
				case SemanticFlags.AccessorsListScope:
					declaration = GetNodeDeclaration(node);
					node.scope = new SymbolDeclarationScope(node) { parentScope = enclosingScope, declaration = declaration };
					break;
				case SemanticFlags.AttributesScope:
					node.scope = new AttributesScope(node) { parentScope = enclosingScope };
					break;
				default:
					throw new ArgumentOutOfRangeException("Unhandled case " + scopeSemantics + ": in switch statement!\nsemantics: " + node.semantics);
			}
			//	Debug.Log("  in " + scopeSemantics);
			if (node.scope == null)
				node.scope = new LocalScope(node) { parentScope = enclosingScope };
		}
		return node.scope;
	}

	private static SymbolDeclaration GetNodeDeclaration(ParseTree.Node node, string fileName = null)
	{
		if (node.declaration == null)
		{
			var declarationSemantics = node.semantics & SemanticFlags.SymbolDeclarationsMask;

			var enclosingScopeNode = EnclosingSemanticNode(node.parent, SemanticFlags.ScopesMask);
			var enclosingScope = GetNodeScope(enclosingScopeNode, fileName);
			
			if (enclosingScope == null)
				return null;

			ParseTree.BaseNode modifiersNode = null;
			ParseTree.BaseNode partialNode = null;
			ParseTree.Node typeParamsNode = null;

			switch (declarationSemantics)
			{
				case SemanticFlags.NamespaceDeclaration:
					node.declaration = new NamespaceDeclaration { parseTreeNode = node, kind = SymbolKind.Namespace };
					break;

				case SemanticFlags.UsingNamespace:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.ImportedNamespace };
					break;
					//var namespaceToImport = node.Print();
					//(enclosingScope as NamespaceScope).declaration.importedNamespaces[namespaceToImport] = new SymbolReference(node.ChildAt(0));
					//return null;

				case SemanticFlags.UsingAlias:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.TypeAlias };
					break;
					//var name = node.ChildAt(0).Print();
					//((NamespaceScope) enclosingScope).declaration.typeAliases[name] = new SymbolReference(node.ChildAt(2));
					//return null;

				case SemanticFlags.ExternAlias:
					break;
				case SemanticFlags.ClassDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					partialNode = node.parent.FindChildByName("PARTIAL");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Class };
					typeParamsNode = node.FindChildByName("typeParameterList") as ParseTree.Node;
				//	Debug.Log(node.declaration + " mods: " + node.declaration.modifiers);
					break;
				case SemanticFlags.TypeParameterDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.TypeParameter };
					break;
				case SemanticFlags.BaseListDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.BaseTypesList };
					break;
				case SemanticFlags.ConstantDeclarator:
					modifiersNode = node.parent.parent.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration {
						parseTreeNode = node,
						kind =
							node.parent.parent.RuleName == "constantDeclaration" ?
							SymbolKind.ConstantField :
							SymbolKind.LocalConstant
					};
					break;
				case SemanticFlags.ConstructorDeclarator:
					modifiersNode = node.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Constructor };
					break;
				case SemanticFlags.DestructorDeclarator:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Destructor };
					break;
				case SemanticFlags.OperatorDeclarator:
				case SemanticFlags.ConversionOperatorDeclarator:
					modifiersNode = node.parent.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Method };
					break;
				case SemanticFlags.MethodDeclarator:
					modifiersNode = node.parent.FindChildByName("modifiers");
					typeParamsNode = node.NodeAt(0).NodeAt(0).NodeAt(0).NodeAt(-1).FindChildByName("typeParameterList") as ParseTree.Node;
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Method };
					break;
				case SemanticFlags.TypeParameterConstraint:
					break;
				case SemanticFlags.LocalVariableDeclarator:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Variable };
					break;
				case SemanticFlags.ForEachVariableDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.ForEachVariable };
					break;
				case SemanticFlags.FromClauseVariableDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.FromClauseVariable };
					break;
				case SemanticFlags.LabeledStatement:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Label };
					break;
				case SemanticFlags.CatchExceptionParameterDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.CatchParameter };
					break;
				case SemanticFlags.FixedParameterDeclaration:
					modifiersNode = node.FindChildByName("parameterModifier");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Parameter };
				//	Debug.Log("Adding parameter declaration to " + enclosingScope);
					break;
				case SemanticFlags.ParameterArrayDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Parameter, modifiers = Modifiers.Params };
					break;
				case SemanticFlags.ImplicitParameterDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Parameter };
					break;
				case SemanticFlags.ExplicitParameterDeclaration:
					modifiersNode = node.FindChildByName("anonymousFunctionParameterModifier");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Parameter };
					break;
				case SemanticFlags.PropertyDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Property };
					break;
				case SemanticFlags.IndexerDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Indexer };
					break;
				case SemanticFlags.GetAccessorDeclaration:
				case SemanticFlags.SetAccessorDeclaration:
					modifiersNode = node.parent.FindChildByName("accessorModifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Accessor };
					break;
				case SemanticFlags.InterfaceGetAccessorDeclaration:
				case SemanticFlags.InterfaceSetAccessorDeclaration:
				case SemanticFlags.AddAccessorDeclaration:
				case SemanticFlags.RemoveAccessorDeclaration:
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Accessor };
					break;
				case SemanticFlags.EventDeclarator:
					modifiersNode = node.parent.parent.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Event };
					break;
				case SemanticFlags.EventWithAccessorsDeclaration:
					modifiersNode = node.parent.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Event };
					break;
				case SemanticFlags.VariableDeclarator:
					modifiersNode = node.parent.parent.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Field };
					break;
				case SemanticFlags.StructDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					partialNode = node.parent.FindChildByName("PARTIAL");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Struct };
					break;
				case SemanticFlags.InterfaceDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					partialNode = node.parent.FindChildByName("PARTIAL");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Interface };
					break;
				case SemanticFlags.InterfaceIndexerDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Property };
					break;
				case SemanticFlags.InterfacePropertyDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Property };
					break;
				case SemanticFlags.InterfaceMethodDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Method };
					break;
				case SemanticFlags.InterfaceEventDeclaration:
					break;
				case SemanticFlags.EnumDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Enum };
					break;
				case SemanticFlags.EnumMemberDeclaration:
					node.declaration = new SymbolDeclaration {
						parseTreeNode = node, kind = SymbolKind.EnumMember,
						modifiers = Modifiers.ReadOnly | Modifiers.Public | Modifiers.Static
					};
					break;
				case SemanticFlags.DelegateDeclaration:
					modifiersNode = node.parent.FindChildByName("modifiers");
					typeParamsNode = node.FindChildByName("typeParameterList") as ParseTree.Node;
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.Delegate };
					break;
				case SemanticFlags.AnonymousObjectCreation:
					break;
				case SemanticFlags.MemberDeclarator:
					break;
				case SemanticFlags.LambdaExpressionDeclaration:
				case SemanticFlags.AnonymousMethodDeclaration:
					enclosingScopeNode = EnclosingScopeNode(node.parent,
						SemanticFlags.CodeBlockScope,
						SemanticFlags.MethodBodyScope,
						SemanticFlags.TypeDeclarationScope);
					enclosingScope = GetNodeScope(enclosingScopeNode, fileName);
					node.declaration = new SymbolDeclaration { parseTreeNode = node, kind = SymbolKind.LambdaExpression };
					break;
				case SemanticFlags.None:
					Debug.LogWarning("declarationSemantics is None on " + node);
					break;
				default:
					throw new ArgumentOutOfRangeException("Unhandled case " + declarationSemantics + " for node " + node);
			}

			if (node.declaration != null)
			{
				if (modifiersNode != null)
					node.declaration.modifiers = ParseModifiers(modifiersNode);
				if (partialNode != null)
					node.declaration.modifiers |= Modifiers.Partial;
				if (typeParamsNode != null)
					node.declaration.numTypeParameters = CountTypeParameters(typeParamsNode);
				if (enclosingScope == null)
					Debug.LogWarning("Symbol declaration " + declarationSemantics + " outside of declaration space!\nenclosingScopeNode: " + (enclosingScopeNode != null ? enclosingScopeNode.RuleName : "null") + "\nnode: " + node);
				else
				{
					var saved = SymbolReference.dontResolveNow;
					SymbolReference.dontResolveNow = true;
					try
					{
						//var symbol =
						enclosingScope.AddDeclaration(node.declaration);
						//if (symbol != null)
						//	Debug.Log("Added declaration " + symbol.ReflectionName);
						//else
						//	Debug.Log("Adding declaration for " + node.declaration.Name + " failed!");
					}
					catch (Exception e)
					{
						Debug.LogException(e);
					}
					finally
					{
						SymbolReference.dontResolveNow = saved;
					}
					++ParseTree.resolverVersion;
					if (ParseTree.resolverVersion == 0)
						++ParseTree.resolverVersion;
				}
			}
		}
		return node.declaration;
	}

	private static int CountTypeParameters(ParseTree.Node typeParamsNode)
	{
		var count = typeParamsNode.numValidNodes > 0 ? 1 : 0;
		for (var i = 1; i < typeParamsNode.numValidNodes; ++i)
			if (typeParamsNode.ChildAt(i).IsLit(","))
				++count;
		return count;
	}

	private static Modifiers ParseModifiers(ParseTree.BaseNode node)
	{
		var root = node as ParseTree.Node;
		if (root == null || root.Nodes == null)
			return Modifiers.None;
		var mods = Modifiers.None;
		foreach (var mod in root.Nodes)
			switch (mod.Print())
			{
				case "public":
					mods |= Modifiers.Public;
					break;
				case "internal":
					mods |= Modifiers.Internal;
					break;
				case "protected":
					mods |= Modifiers.Protected;
					break;
				case "private":
					mods |= Modifiers.Private;
					break;
				case "static":
					mods |= Modifiers.Static;
					break;
				case "new":
					mods |= Modifiers.New;
					break;
				case "sealed":
					mods |= Modifiers.Sealed;
					break;
				case "abstract":
					mods |= Modifiers.Abstract;
					break;
				case "readonly":
					mods |= Modifiers.ReadOnly;
					break;
				case "volatile":
					mods |= Modifiers.Volatile;
					break;
				case "virtual":
					mods |= Modifiers.Virtual;
					break;
				case "override":
					mods |= Modifiers.Override;
					break;
				case "extern":
					mods |= Modifiers.Extern;
					break;
				case "ref":
					mods |= Modifiers.Ref;
					break;
				case "out":
					mods |= Modifiers.Out;
					break;
				case "this":
					mods |= Modifiers.This;
					break;
				default:
					return mods; // Cancelling...
			}
		return mods;
	}

	public void OnReduceSemanticNode(ParseTree.Node node, string fileName = null)
	{
		if (node.numValidNodes == 0)
			return;

		var semantics = node.semantics;
		var declaration = semantics & SemanticFlags.SymbolDeclarationsMask;
		if (declaration == SemanticFlags.None)
		{
			GetNodeScope(node, fileName);
		}
		else
		{
			GetNodeDeclaration(node, fileName);
			if ((node.semantics & SemanticFlags.ScopesMask) != SemanticFlags.None)
				GetNodeScope(node, fileName);
			//var enclosingScopeNode = EnclosingSemanticNode(node, SemanticAttribute.ScopesMask);
			//if (enclosingScopeNode == null)
			//{
			//    while (node.parent != null)
			//        node = node.parent;
			//    throw new InvalidOperationException("Symbol declaration " + declaration + " outside of declaration space!\nnode: " + node);
			//}

			////Debug.Log("Found " + declaration);
			//var scope = GetNodeScope(enclosingScopeNode);
		}
	}


	public class Scanner : IScanner
	{
		public readonly string fileName;

		readonly CsGrammar grammar;
		readonly FGTextBuffer.FormatedLine[] lines;
		List<SyntaxToken> tokens;

		int currentLine = -1;
		int currentTokenIndex = -1;
	//	int nonTriviaTokenIndex = -1;

		private static SyntaxToken EOF;

		public FGGrammar.Node CurrentGrammarNode { get; set; }
		private ParseTree.Node _currentPTN;
		public ParseTree.Node CurrentParseTreeNode {
			get { return _currentPTN; }
			set {
				//if (value == null && _currentPTN != null && _currentPTN.RuleName != "compilationUnit")
				//	Debug.Log("Setting currentPTN to null!");
				_currentPTN = value;
			}
		}

		public ParseTree.Leaf ErrorToken { get; set; }
		public string ErrorMessage { get; set; }
		public FGGrammar.Node ErrorGrammarNode { get; set; }
		public ParseTree.Node ErrorParseTreeNode { get; set; }

		public bool Seeking { get; set; }

		private SyntaxToken currentTokenCache;

		//private Dictionary<Node, bool> memoizationTable = new Dictionary<Node, bool>();
		
		//public Dictionary<Node, int> numLookaheads = new Dictionary<Node, int>();
		//public Dictionary<Node, long> timeLookaheads = new Dictionary<Node, long>();

		public int CurrentLine() { return currentLine + 1; }
		public int CurrentTokenIndex() { return currentTokenIndex; }

		public Scanner(CsGrammar grammar, FGTextBuffer.FormatedLine[] formatedLines, string fileName)
		{
			this.grammar = grammar;
			this.fileName = fileName;
			lines = formatedLines;
				
			if (EOF == null)
				EOF = new SyntaxToken(SyntaxToken.Kind.EOF, string.Empty) { tokenId = grammar.tokenEOF };
		}

		public IScanner Clone()
		{
			var clone = new Scanner(grammar, lines, fileName)
			{
				tokens = tokens,
				currentLine = currentLine,
				currentTokenIndex = currentTokenIndex,
			//	nonTriviaTokenIndex = nonTriviaTokenIndex,
				currentTokenCache = currentTokenCache,
				CurrentGrammarNode = CurrentGrammarNode,
				CurrentParseTreeNode = CurrentParseTreeNode,
				ErrorToken = ErrorToken,
				ErrorMessage = ErrorMessage,
				ErrorGrammarNode = ErrorGrammarNode,
				ErrorParseTreeNode = ErrorParseTreeNode,
			};

			return clone;
		}

		public void OnReduceSemanticNode(ParseTree.Node node)
		{
			grammar.OnReduceSemanticNode(node, fileName);
		}

		public void SyntaxErrorExpected(TokenSet lookahead)
		{
			if (ErrorMessage != null)
				return;

			ErrorMessage = "Syntax error: Expected " + lookahead.ToString(grammar.GetParser);
		//	Debug.LogError(message + "\nCurrentParseTreeNode:\n" + CurrentParseTreeNode);				ErrorMessage = message;
			if (CurrentParseTreeNode != null && CurrentParseTreeNode.syntaxError == null)
			{
				CurrentParseTreeNode.syntaxError = ErrorMessage;

			//	ErrorGrammarNode = CurrentGrammarNode;
			//	ErrorParseTreeNode = CurrentParseTreeNode;
			}
		}

		public bool CollectCompletions(TokenSet tokenSet)
		{
			var result = (CurrentGrammarNode ?? CsGrammar.instance.r_compilationUnit).CollectCompletions(tokenSet, this, grammar.tokenIdentifier);
			if (CurrentGrammarNode != null && tokenSet.Matches(Instance.tokenIdentifier))
			{
				currentTokenCache = new SyntaxToken(SyntaxToken.Kind.Identifier, "special");
				currentTokenCache.tokenId = grammar.tokenIdentifier;
				Lookahead(CurrentGrammarNode, 1);
				currentTokenCache = null;
			}
			return result;
		}

		public void InsertMissingToken(string errorMessage)
		{
			//Debug.Log("Missing at line " + (currentLine + 1) + "\n" + errorMessage);

			var missingAtLine = currentLine;
			var missingAtIndex = currentTokenIndex;
			while (true)
			{
				if (--missingAtIndex < 0)
				{
					if (--missingAtLine < 0)
					{
						missingAtLine = missingAtIndex = 0;
						break;
					}
					missingAtIndex = lines[missingAtLine].tokens.Count;
					continue;
				}
				var tokenKind = lines[missingAtLine].tokens[missingAtIndex].tokenKind;
				if (tokenKind > SyntaxToken.Kind.LastWSToken)
				{
					++missingAtIndex;
					break;
				}
				else if (tokenKind == SyntaxToken.Kind.Missing)
				{
					ErrorToken = lines[missingAtLine].tokens[missingAtIndex].parent;
					return;
				}
			}

			var missingLine = lines[missingAtLine];
			//for (var i = missingAtIndex; i < missingLine.tokens.Count; ++i)
			//{
			//	var token = missingLine.tokens[i];
			//	if (token.parent != null)
			//		++token.parent.tokenIndex;
			//}

			var missingToken = new SyntaxToken(SyntaxToken.Kind.Missing, string.Empty) { style = null, formatedLine = missingLine };
			missingLine.tokens.Insert(missingAtIndex, missingToken);
			var leaf = ErrorParseTreeNode.AddToken(missingToken);
			leaf.missing = true;
			leaf.syntaxError = errorMessage;
			leaf.grammarNode = ErrorGrammarNode;
			//leaf.line = CurrentLine() - 1;

			if (ErrorToken == null)
				ErrorToken = leaf;

			if (missingAtLine == currentLine)
				++currentTokenIndex;
		}

		public void MoveToLine(int line, ParseTree parseTree)
		{
			for (var prevLine = line - 1; prevLine >= 0; --prevLine)
			{
				tokens = lines[prevLine].tokens;
				for (var i = tokens.Count - 1; i >= 0; --i)
				{
					var token = tokens[i];
					var leaf = token.parent;
					if (token.tokenKind == SyntaxToken.Kind.Missing)
					{
						if (token.parent != null && token.parent.parent != null)
							token.parent.parent.syntaxError = null;
						tokens.RemoveAt(i);
						//for (var j = i; j < tokens.Count; ++j)
						//{
						//	var t = tokens[j];
						//	if (t.parent != null)
						//		--t.parent.tokenIndex;
						//}
						continue;
					}

					if (leaf == null || leaf.grammarNode == null)
						continue;

					if (token.tokenKind < SyntaxToken.Kind.LastWSToken)
						continue;

					if (leaf.syntaxError != null)
					{
						ErrorToken = leaf;
						ErrorMessage = leaf.syntaxError;
						continue;
					}

					MoveAfterLeaf(leaf);
				//	Debug.Log("Moved after leaf " + leaf + "\nCurrent line:" + (currentLine + 1));
					return;

					//currentLine = prevLine;
					//currentTokenIndex = i;

					//nonTriviaTokenIndex = i;
					//for (var j = 0; j < i; ++j)
					//    if (tokens[j].tokenKind < SyntaxToken.Kind.LastWSToken)
					//        --nonTriviaTokenIndex;

					//if (!MoveNext())
					//    return; //throw new InvalidOperationException("Can't move beyond end of file!!!");

					//CurrentParseTreeNode = leaf.parent;
					//CurrentGrammarNode = leaf.grammarNode;
					//ErrorGrammarNode = CurrentGrammarNode = leaf.grammarNode.parent.NextAfterChild(leaf.grammarNode, this);
					//ErrorParseTreeNode = CurrentParseTreeNode;
					//return;
				}
			}

			tokens = null;
			currentLine = -1;
			currentTokenIndex = -1;
		//	nonTriviaTokenIndex = -1;

			CurrentParseTreeNode = null;
			CurrentGrammarNode = null;
			ErrorToken = null;
			ErrorMessage = null;
			ErrorParseTreeNode = null;
			ErrorGrammarNode = null;

			//numLookaheads = new Dictionary<Node, int>();
			//timeLookaheads = new Dictionary<Node, long>();
			MoveNext();

			Rule startRule = CsGrammar.Instance.r_compilationUnit;

			//if (parseTree == null)
			//{
			//	parseTree = new ParseTree();
			//	var rootId = new Id(startRule.GetNt());
			//	ids[Start.GetNt()] = startRule;
			//	rootId.SetLookahead(this);
			//	Start.parent = rootId;
			//}
			CurrentParseTreeNode = parseTree.root;// = new ParseTree.Node(rootId);
			CurrentGrammarNode = startRule;//.Parse(scanner);

			ErrorParseTreeNode = CurrentParseTreeNode;
			ErrorGrammarNode = CurrentGrammarNode;
				//}
		}

		public bool MoveAfterLeaf(ParseTree.Leaf leaf)
		{
		//	Debug.Log("Moving after leaf " + leaf);
			if (leaf == null || leaf.grammarNode == null)
				return false;

			if (leaf.syntaxError != null)
			{
				Debug.LogError("Can't move after error node! " + leaf.syntaxError);
				return false;
			}

			var parseTreeNode = leaf.parent;
			if (parseTreeNode == null)
				return false;

			//currentLine = -1;
			//currentTokenIndex = -1;
			//nonTriviaTokenIndex = -1;
			//tokens = null;

			CurrentParseTreeNode = null;
			CurrentGrammarNode = null;
			ErrorToken = null;
			ErrorMessage = null;
			ErrorParseTreeNode = null;
			ErrorGrammarNode = null;

		//	numLookaheads = new Dictionary<Node, int>();
		//	timeLookaheads = new Dictionary<Node, long>();

			tokens = lines[leaf.line].tokens;

			currentLine = leaf.line;
			currentTokenIndex = leaf.tokenIndex;

			//nonTriviaTokenIndex = leaf.tokenIndex;
			//for (var j = 0; j < leaf.tokenIndex; ++j)
			//    if (tokens[j].tokenKind < SyntaxToken.Kind.LastWSToken)
			//        --nonTriviaTokenIndex;

			//if (!MoveNext())
			//	return false;
			MoveNext();

			CurrentParseTreeNode = leaf.parent;
			ErrorMessage = null;
			Seeking = true;
			ErrorGrammarNode = CurrentGrammarNode = leaf.grammarNode.parent.NextAfterChild(leaf.grammarNode, this);
			Seeking = false;
			ErrorParseTreeNode = CurrentParseTreeNode;

		//	Debug.Log("Current = " + Current);
		//	Debug.Log("CurrentGrammarNode = " + CurrentGrammarNode);
		//	Debug.Log("CurrentParseTreeNode = " + CurrentParseTreeNode);
			return true;
		}

		private int maxScanDistance;
		public bool KeepScanning { get { return maxScanDistance > 0; } }

		public bool Lookahead(Node node, int maxDistance = int.MaxValue)
		{
			if (tokens == null && currentLine > 0)
				return false;

//			long laTime;
//			if (!timeLookaheads.TryGetValue(node, out laTime))
//				laTime = 0;
//
//			int numLAs;
//			if (!numLookaheads.TryGetValue(node, out numLAs))
//				numLAs = 0;
//			numLookaheads[node] = numLAs + 1;
//
//			var timer = new Stopwatch();
//			timer.Start();

			//bool memValue;
			//var id = node as Id;
			//if (id != null)
			//{
			//    if (memoizationTable.TryGetValue(id.peer, out memValue))
			//        return memValue;
			//}
			//else
			//{
			//    if (memoizationTable.TryGetValue(node, out memValue))
			//        return memValue;
			//}
				
			var line = currentLine;
			var index = currentTokenIndex;
		//	var realIndex = nonTriviaTokenIndex;

			var temp = maxScanDistance;
			maxScanDistance = maxDistance;
			var match = node.Scan(this);
			maxScanDistance = temp;

			for (var i = currentLine; i > line; --i)
				if (i < lines.Length)
					lines[i].laLines = Math.Max(lines[i].laLines, i - line);
			
			currentLine = line;
			currentTokenIndex = index;
		//	nonTriviaTokenIndex = realIndex;
			tokens = currentLine < lines.Length ? lines[currentLine].tokens : null;

			//if (id != null)
			//    memoizationTable[id.peer] = match;
			//else
			//    memoizationTable[node] = match;

//			timer.Stop();
//			laTime += timer.ElapsedTicks;
//			timeLookaheads[node] = laTime;

			return match;
		}

		public SyntaxToken Lookahead(int offset, bool skipWhitespace = true)
		{
			if (!skipWhitespace)
			{
				return currentTokenIndex + 1 < tokens.Count ? tokens[currentTokenIndex + 1] : EOF;
			}
				
			var t = tokens;
			var cl = currentLine;
			var cti = currentTokenIndex;

			while (offset --> 0)
			{
				if (!MoveNext())
				{
					tokens = t;
					currentLine = cl;
					currentTokenIndex = cti;
					return EOF;
				}
			}
			var token = tokens[currentTokenIndex];
				
			for (var i = currentLine; i > cl; --i)
				if (i < lines.Length)
					lines[i].laLines = Math.Max(lines[i].laLines, i - cl);

			tokens = t;
			currentLine = cl;
			currentTokenIndex = cti;
			return token;
		}

		public SyntaxToken Current
		{
			get
			{
				if (currentTokenCache != null)
					return currentTokenCache;
				return tokens != null ? tokens[currentTokenIndex] : EOF;
			}
		}

		public SyntaxToken CurrentToken()
		{
			if (currentTokenCache != null)
				return currentTokenCache;
			return tokens != null ? tokens[currentTokenIndex] : EOF;
		}

		public void Dispose()
		{
		}

		object System.Collections.IEnumerator.Current
		{
			get { return Current; }
		}

		public bool MoveNext()
		{
			if (maxScanDistance > 0)
				--maxScanDistance;

			while (MoveNextSingle())
			{
				if (tokens[currentTokenIndex].tokenId == -1)
				{
					var token = tokens[currentTokenIndex];
					switch (token.tokenKind)
					{
						case SyntaxToken.Kind.Missing:
						case SyntaxToken.Kind.Whitespace:
						case SyntaxToken.Kind.Comment:
						case SyntaxToken.Kind.EOF:
						case SyntaxToken.Kind.Preprocessor:
						case SyntaxToken.Kind.PreprocessorSymbol:
						case SyntaxToken.Kind.PreprocessorArguments:
						case SyntaxToken.Kind.PreprocessorDirectiveExpected:
						case SyntaxToken.Kind.PreprocessorCommentExpected:
						case SyntaxToken.Kind.PreprocessorUnexpectedDirective:
						case SyntaxToken.Kind.VerbatimStringLiteral:
							break;
						case SyntaxToken.Kind.Punctuator:
						case SyntaxToken.Kind.Keyword:
						case SyntaxToken.Kind.BuiltInLiteral:
							token.tokenId = grammar.TokenToId(token.text);
							break;
						case SyntaxToken.Kind.Identifier:
						case SyntaxToken.Kind.ContextualKeyword:
							token.tokenId = grammar.tokenIdentifier;
							break;
						case SyntaxToken.Kind.IntegerLiteral:
						case SyntaxToken.Kind.RealLiteral:
						case SyntaxToken.Kind.CharLiteral:
						case SyntaxToken.Kind.StringLiteral:
						case SyntaxToken.Kind.VerbatimStringBegin:
							token.tokenId = grammar.tokenLiteral;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				//	tokens[currentTokenIndex] = token;
				}

				if (tokens[currentTokenIndex].tokenKind > SyntaxToken.Kind.VerbatimStringLiteral)
				{
			//		++nonTriviaTokenIndex;
					return true;
				}
			}
			tokens = null;
			++currentLine;
			currentTokenIndex = -1;
		//	nonTriviaTokenIndex = 0;
			return false;
		}

		public bool MoveNextSingle()
		{
			//memoizationTable.Clear();

			while (tokens == null)
			{
				if (currentLine + 1 >= lines.Length)
					return false;
				currentTokenIndex = -1;
			//	nonTriviaTokenIndex = 0;
				tokens = lines[++currentLine].tokens;
			}
			while (currentTokenIndex + 1 >= tokens.Count)
			{
				if (currentLine + 1 >= lines.Length)
				{
					tokens = null;
					return false;
				}
				currentTokenIndex = -1;
			//	nonTriviaTokenIndex = 0;
				tokens = lines[++currentLine].tokens;
				while (tokens == null)
				{
					if (currentLine + 1 >= lines.Length)
						return false;
					tokens = lines[++currentLine].tokens;
				}
			}
			++currentTokenIndex;
			return true;
		}

		public void Reset()
		{
			currentLine = -1;
			currentTokenIndex = -1;
		//	nonTriviaTokenIndex = 0;
			tokens = null;
		}
	}
}

}
