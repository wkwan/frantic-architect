using UnityEngine;
using UnityEngine.Purchasing;

public class Store : MonoBehaviour, IStoreListener {
	public void Awake()
	{
		DontDestroyOnLoad(gameObject);
		//  Initialize Unity IAP here.
		Debug.Log("~~~~~init iap");
		var module = StandardPurchasingModule.Instance();
		ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
		builder.AddProduct(Game.NO_ADS_ID, ProductType.NonConsumable);
		UnityPurchasing.Initialize(this, builder);
	}
	// Implementation of IStoreListener...

	public void InitiateNoAdsPurchase()
	{
	}

	public void RestoreNoAdsPurchaseIOS()
	{
	}

	static IStoreController storeController;
	static IExtensionProvider storeExtensions;

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions) 
	{
		storeController = controller;
		storeExtensions = extensions;
		//Debug.Log("store initialize success");
	}
	public void OnInitializeFailed(InitializationFailureReason error) 
	{
		Debug.Log("store initialize fail " + error.ToString());

	}
	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) 
	{ 
		Debug.Log("store purchase complete");
		PlayerPrefs.SetInt(Game.NO_ADS_ID, 1);
		return PurchaseProcessingResult.Complete; 
	}
	public void OnPurchaseFailed(Product item, PurchaseFailureReason r) 
	{
		Debug.Log("store purchase fail");
	}
}