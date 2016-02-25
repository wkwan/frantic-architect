using UnityEngine;
using UnityEngine.Purchasing;

public class Store : MonoBehaviour, IStoreListener {
	
	IStoreController storeController;
	IExtensionProvider storeExtensions;
	
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
	
	public void OnInitialized(IStoreController controller, IExtensionProvider extensions) 
	{
		storeController = controller;
		storeExtensions = extensions;
		Debug.Log("store initialize success");
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
	
	public void InitiateNoAdsPurchase()
	{
		storeController.InitiatePurchase(Game.NO_ADS_ID);
	}
	
	public void RestoreNoAdsPurchaseIOS()
	{
		#if UNITY_IOS
		if (storeExtensions != null) 
		{
			var apple = storeExtensions.GetExtension<IAppleExtensions>();
			apple.RestoreTransactions((result) =>
			{
				if (result) {
					//purchase restored
					//TODO: show dialog box
				}
			});
		}
		#endif
	}
}