using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

/// <summary>
/// An example of basic Unity IAP functionality.
/// To use with your account, configure the product ids (AddProduct)
/// and Google Play key (SetPublicKey).
/// </summary>
[AddComponentMenu("Unity IAP/Demo")]
public class IAPDemo : MonoBehaviour, IStoreListener
{
    // Unity IAP objects 
    private IStoreController m_Controller;
    private IAppleExtensions m_AppleExtensions;

    private int m_SelectedItemIndex = -1; // -1 == no product
    private bool m_PurchaseInProgress;

    private Selectable m_InteractableSelectable; // Optimization used for UI state management

    /// <summary>
    /// This will be called when Unity IAP has finished initialising.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_Controller = controller;
        m_AppleExtensions = extensions.GetExtension<IAppleExtensions> ();

		InitUI(controller.products.all);

        Debug.Log("Available items:");
        foreach (var item in controller.products.all)
        {
            if (item.availableToPurchase)
            {
                Debug.Log(string.Join(" - ",
                    new[]
                    {
                        item.metadata.localizedTitle,
                        item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode,
                        item.metadata.localizedPrice.ToString(),
                        item.metadata.localizedPriceString
                    }));
            }
        }

        // Prepare model for purchasing
        if (m_Controller.products.all.Length > 0) 
        {
            m_SelectedItemIndex = 0;
        }

        // Populate the product menu now that we have Products
        for (int t = 0; t < m_Controller.products.all.Length; t++)
        {
            var item = m_Controller.products.all[t];
            var description = string.Format("{0} - {1}", item.metadata.localizedTitle, item.metadata.localizedPriceString);

            // NOTE: my options list is created in InitUI
            GetDropdown().options[t] = new Dropdown.OptionData(description);
        }

        // Ensure I render the selected list element
        GetDropdown().RefreshShownValue();

        // Now that I have real products, begin showing product purchase history
        UpdateHistoryUI();
    }

    /// <summary>
    /// This will be called when a purchase completes.
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
		Debug.Log("Purchase OK: " + e.purchasedProduct.definition.id);
		Debug.Log("Receipt: " + e.purchasedProduct.receipt);

        m_PurchaseInProgress = false;

        // Now that my purchase history has changed, update its UI
        UpdateHistoryUI();

        // Indicate we have handled this purchase, we will not be informed of it again.x
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// This will be called is an attempted purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
        Debug.Log("Purchase failed: " + item.definition.id);
        Debug.Log(r);

        m_PurchaseInProgress = false;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("Billing failed to initialize!");
        switch (error)
        {
            case InitializationFailureReason.AppNotKnown:
                Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                // Ask the user if billing is disabled in device settings.
                Debug.Log("Billing disabled!");
                break;
            case InitializationFailureReason.NoProductsAvailable:
                // Developer configuration error; check product metadata.
                Debug.Log("No products available for purchase!");
                break;
        }
    }

    public void Awake()
    {
        var module = StandardPurchasingModule.Instance();
        module.useMockBillingSystem = true; // Microsoft
        // The FakeStore supports: no-ui (always succeeding), basic ui (purchase pass/fail), and 
        // developer ui (initialization, purchase, failure code setting). These correspond to 
        // the FakeStoreUIMode Enum values passed into StandardPurchasingModule.useFakeStoreUIMode.
        module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

        var builder = ConfigurationBuilder.Instance(module);

        builder.Configure<IGooglePlayConfiguration>().SetPublicKey("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA2O/9/H7jYjOsLFT/uSy3ZEk5KaNg1xx60RN7yWJaoQZ7qMeLy4hsVB3IpgMXgiYFiKELkBaUEkObiPDlCxcHnWVlhnzJBvTfeCPrYNVOOSJFZrXdotp5L0iS2NVHjnllM+HA1M0W2eSNjdYzdLmZl1bxTpXa4th+dVli9lZu7B7C2ly79i/hGTmvaClzPBNyX+Rtj7Bmo336zh2lYbRdpD5glozUq+10u91PMDPH+jqhx10eyZpiapr8dFqXl5diMiobknw9CgcjxqMTVBQHK6hS0qYKPmUDONquJn280fBs1PTeA6NMG03gb9FLESKFclcuEZtvM8ZwMMRxSLA9GwIDAQAB");
        builder.AddProduct("coins", ProductType.Consumable, new IDs
        {
            {"com.unity3d.unityiap.unityiapdemo.100goldcoins.v2.c", GooglePlay.Name},
            {"com.unity3d.unityiap.unityiapdemo.100goldcoins.6", AppleAppStore.Name},
            {"com.unity3d.unityiap.unityiapdemo.100goldcoins.mac", MacAppStore.Name},
            {"com.unity3d.unityiap.unityiapdemo.100goldcoins.win8", WinRT.Name}
        });

        builder.AddProduct("sword", ProductType.NonConsumable, new IDs
        {
            {"com.unity3d.unityiap.unityiapdemo.sword.c", GooglePlay.Name},
            {"com.unity3d.unityiap.unityiapdemo.sword.6", AppleAppStore.Name},
            {"com.unity3d.unityiap.unityiapdemo.sword.mac", MacAppStore.Name},
            {"com.unity3d.unityiap.unityiapdemo.sword", WindowsPhone8.Name}
        });
        builder.AddProduct("subscription", ProductType.Subscription, new IDs
        {
            {"com.unity3d.unityiap.unityiapdemo.subscription", GooglePlay.Name, AppleAppStore.Name}
        });

        // Now we're ready to initialize Unity IAP.
        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// This will be called after a call to IAppleExtensions.RestoreTransactions().
    /// </summary>
    private void OnTransactionsRestored(bool success)
    {
        Debug.Log("Transactions restored.");
    }

    /// <summary>
    /// iOS Specific.
    /// This is called as part of Apple's 'Ask to buy' functionality,
    /// when a purchase is requested by a minor and referred to a parent
    /// for approval.
    /// 
    /// When the purchase is approved or rejected, the normal purchase events
    /// will fire.
    /// </summary>
    /// <param name="item">Item.</param>
    private void OnDeferred(Product item)
    {
        Debug.Log("Purchase deferred: " + item.definition.id);
    }

	private void InitUI(IEnumerable<Product> items)
    {
        // Disable the UI while IAP is initializing
        // See also UpdateInteractable()
        m_InteractableSelectable = GetDropdown(); // References any one of the disabled components

        // Show Restore button on Apple platforms
        if (Application.platform != RuntimePlatform.IPhonePlayer && 
            Application.platform != RuntimePlatform.OSXPlayer)
        {
            GetRestoreButton().gameObject.SetActive(false);
        }

        foreach (var item in items)
        {
            // Add initial pre-IAP-initialization content. Update later in OnInitialized.
			var description = string.Format("{0} - {1}", item.definition.id, item.definition.type);

            GetDropdown().options.Add(new Dropdown.OptionData(description));
        }

        // Ensure I render the selected list element
        GetDropdown().RefreshShownValue();

        GetDropdown().onValueChanged.AddListener((int selectedItem) => {
            Debug.Log("OnClickDropdown item " + selectedItem);
            m_SelectedItemIndex = selectedItem;
        });

        // Initialize my button event handling
        GetBuyButton().onClick.AddListener(() => { 
            if (m_PurchaseInProgress == true) {
                return;
            }

            m_Controller.InitiatePurchase(m_Controller.products.all[m_SelectedItemIndex]); 

            // Don't need to draw our UI whilst a purchase is in progress.
            // This is not a requirement for IAP Applications but makes the demo
            // scene tidier whilst the fake purchase dialog is showing.
            m_PurchaseInProgress = true;
        });

        if (GetRestoreButton() != null)
        {
            GetRestoreButton().onClick.AddListener(() =>
            { 
                m_AppleExtensions.RestoreTransactions(OnTransactionsRestored);
            });
        }
    }

    public void UpdateHistoryUI()
    {
        if (m_Controller == null)
        {
            return;
        }

        var itemText = "Item\n\n";
        var countText = "Purchased\n\n";

        for (int t = 0; t < m_Controller.products.all.Length; t++)
        {
            var item = m_Controller.products.all [t];

            // Collect history status report

            itemText += "\n\n" + item.definition.id;
            countText += "\n\n" + item.hasReceipt.ToString();
        }

        // Show history
        GetText(false).text = itemText;
        GetText(true).text = countText;
    }

    protected void UpdateInteractable()
    {
        if (m_InteractableSelectable == null)
        {
            return;
        }

        bool interactable = m_Controller != null;
        if (interactable != m_InteractableSelectable.interactable)
        {
            if (GetRestoreButton() != null)
            {
                GetRestoreButton().interactable = interactable;
            }
            GetBuyButton().interactable = interactable;
            GetDropdown().interactable = interactable;
        }
    }

    public void Update()
    {
        UpdateInteractable();
    }

    private Dropdown GetDropdown()
    {
        return GameObject.Find("Dropdown").GetComponent<Dropdown>();
    }

    private Button GetBuyButton()
    {
        return GameObject.Find("Buy").GetComponent<Button>();
    }

    /// <summary>
    /// Gets the restore button when available
    /// </summary>
    /// <returns><c>null</c> or the restore button.</returns>
    private Button GetRestoreButton()
    {
        GameObject restoreButtonGameObject = GameObject.Find("Restore");
        if (restoreButtonGameObject != null)
        {
            return restoreButtonGameObject.GetComponent<Button>();
        }
        else
        {
            return null;
        }
    }

    private Text GetText(bool right)
    {
        var which = right ? "TextR" : "TextL";
        return GameObject.Find(which).GetComponent<Text>();
    }
}
