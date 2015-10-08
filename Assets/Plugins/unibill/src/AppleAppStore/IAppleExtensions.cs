using System;

namespace Unibill
{
    /// <summary>
    /// Access iOS specific functionality.
    /// </summary>
    public interface IAppleExtensions {

        /// <summary>
        /// A request to refreshAppReceipt completed successfully.
        /// </summary>
        event Action<string> onAppReceiptRefreshed;

        /// <summary>
        /// A request to refreshReceipt failed.
        /// 
        /// This may occur when a user enters invalid credentials,
        /// cancels the modal dialog or if there is no Internet connection.
        /// </summary>
        event Action onAppReceiptRefreshFailed;

        /// <summary>
        /// Fetch the latest App Receipt from Apple.
        /// 
        /// This requires an Internet connection and will prompt the user for their credentials.
        /// </summary>
        void refreshAppReceipt();
    }
}
