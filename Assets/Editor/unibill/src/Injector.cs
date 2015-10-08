using System;
using Uniject;
using Uniject.Editor;
using Uniject.Impl;
using Unibill;
using Unibill.Impl;


public class UnibillInjector {

	public static IResourceLoader GetResourceLoader() {
		return new UnityResourceLoader ();
	}

	public static UnibillConfiguration GetConfig() {
        return new UnibillConfiguration(getXml(), UnityEngine.Application.platform, GetLogger());
	}

	private static string getXml() {
		return new UnityResourceLoader ().openTextFile ("unibillInventory.json").ReadToEnd();
	}

	public static ProductIdRemapper GetRemapper() {
		return new ProductIdRemapper (GetConfig ());
	}

	public static ILogger GetLogger() {
		return new UnityLogger ();
	}

	public static IEditorUtil GetEditorUtil() {
		return new UnityEditorUtil ();
	}

	public static AmazonJSONGenerator GetAmazonGenerator() {
        return new AmazonJSONGenerator (GetConfig());
	}

	public static GooglePlayCSVGenerator GetGooglePlayCSVGenerator() {
		return new GooglePlayCSVGenerator (GetEditorUtil (), GetConfig ());
	}

	public static StorekitMassImportTemplateGenerator GetStorekitGenerator() {
		return new StorekitMassImportTemplateGenerator (GetConfig (), GetEditorUtil ());
	}
}

