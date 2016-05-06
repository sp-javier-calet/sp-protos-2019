package es.socialpoint.sparta.purchase;

import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;
import java.util.List;

import es.socialpoint.unity.base.SPUnityActivityEventListener;
import es.socialpoint.unity.base.SPUnityActivityEventManager;
import es.socialpoint.unity.base.UnityGameObject;

import es.socialpoint.sparta.purchase.utils.IabBroadcastReceiver;
import es.socialpoint.sparta.purchase.utils.IabBroadcastReceiver.IabBroadcastListener;
import es.socialpoint.sparta.purchase.utils.IabHelper;
import es.socialpoint.sparta.purchase.utils.IabHelper.IabAsyncInProgressException;
import es.socialpoint.sparta.purchase.utils.IabResult;
import es.socialpoint.sparta.purchase.utils.Inventory;
import es.socialpoint.sparta.purchase.utils.Purchase;
import es.socialpoint.sparta.purchase.utils.SkuDetails;

public class SPPurchaseNativeServices implements IabBroadcastListener, SPUnityActivityEventListener {

    // Type values must match the possible states defined by google
    //http://developer.android.com/intl/es/google/play/billing/billing_reference.html
    static class PurchaseState {
        public static final int Purchased = 0;
        public static final int Canceled = 1;
        public static final int Refunded = 2;
    }

    // Debug tag, for logging
    private static final String TAG = "[SP-IAP]";

    //Class to send messages to Unity
    private UnityGameObject _unityMessageSender;
    // The helper object
    private IabHelper _helper;
    // Provides purchase notification while this app is running
    private IabBroadcastReceiver _broadcastReceiver;

    // Updated inventory
    private Inventory _inventory;
    // Latest requested products
    List<String> _lastRequestedProductIds;

    private boolean _highDetailedLogEnabled;
    private boolean _setupReady;

    public SPPurchaseNativeServices(String listenerObjectName)
    {
        _unityMessageSender = new UnityGameObject(listenerObjectName);
        _unityMessageSender.SendMessage("StoreDebugLog", "*** TEST Hello World");

        _lastRequestedProductIds = new ArrayList<String>();

        _setupReady = false;

        // Create the helper, passing it our context and the public key to verify signatures with
        detailedLog("Creating IAB helper.");
        _helper = new IabHelper(UnityPlayer.currentActivity);

        // Start setup. This is asynchronous and the specified listener
        // will be called once setup completes.
        detailedLog("Starting setup.");
        _helper.startSetup(new IabHelper.OnIabSetupFinishedListener()
        {
            public void onIabSetupFinished(IabResult result)
            {
                detailedLog("Setup finished.");
                // Have we been disposed of in the meantime? If so, quit.
                if (_helper == null) return;

                if (!result.isSuccess())
                {
                    // Oh noes, there was a problem.
                    String errorMessage = "Problem setting up in-app billing: " + result;
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnBillingNotSupported", errorMessage);
                    return;
                }

                _setupReady = true;
                SPUnityActivityEventManager.register(SPPurchaseNativeServices.this);

                // Important: Dynamically register for broadcast messages about updated purchases.
                // We register the receiver here instead of as a <receiver> in the Manifest
                // because we always call getPurchases() at startup, so therefore we can ignore
                // any broadcasts sent while the app isn't running.
                // Note: registering this listener in an Activity is a bad idea, but is done here
                // because this is a SAMPLE. Regardless, the receiver must be registered after
                // IabHelper is setup, but before first call to getPurchases().
                _broadcastReceiver = new IabBroadcastReceiver(SPPurchaseNativeServices.this);
                IntentFilter broadcastFilter = new IntentFilter(IabBroadcastReceiver.ACTION);
                UnityPlayer.currentActivity.registerReceiver(_broadcastReceiver, broadcastFilter);

                // IAB is fully set up.
                _unityMessageSender.SendMessage("OnBillingSupported", "");
            }
        });
    }

    public void unbind()
    {
        SPUnityActivityEventManager.unregister(SPPurchaseNativeServices.this);
    }

    public void enableHighDetailLogs(boolean shouldEnable)
    {
        _highDetailedLogEnabled = shouldEnable;
        if(_helper != null)
        {
            _helper.enableDebugLogging(_highDetailedLogEnabled);
        }
    }

    private boolean isHelperReady()
    {
        return _setupReady && (_helper != null);
    }


    /* Product Operations */

    public void loadProducts(final List<String> productIds)
    {
        detailedLog("Products Request Started");

        _lastRequestedProductIds = productIds;

        if(!isHelperReady())
        {
            _unityMessageSender.SendMessage("OnQueryInventoryFailed", "Setup not ready");
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable()
        {
            public void run()
            {
                try
                {
                    _helper.queryInventoryAsync(true, productIds, null, _gotInventoryListener);
                }
                catch (IabAsyncInProgressException e)
                {
                    String errorMessage = "Products Request Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnQueryInventoryFailed", errorMessage);
                }
            }
        });
    }

    public void purchaseProduct(final String productIdentifier)
    {
        detailedLog("Product Purchase Started: " + productIdentifier);

        if(!isHelperReady())
        {
            _unityMessageSender.SendMessage("OnPurchaseFailed", "Setup not ready");
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable()
        {
            public void run()
            {
                try
                {
                    int requestCode = 123;//Arbitrary
                    _helper.launchPurchaseFlow(UnityPlayer.currentActivity, productIdentifier, requestCode, _purchaseFinishedListener);
                }
                catch (IabAsyncInProgressException e)
                {
                    String errorMessage = "Product Purchase Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnPurchaseFailed", errorMessage);
                }
            }
        });
    }

    private String getProductJson(SkuDetails product)
    {
        String json = "{"
                + dictionaryKeyFormat("itemType") + dictionaryValueFormat_String(product.getItemType(), false)
                + dictionaryKeyFormat("sku") +  dictionaryValueFormat_String(product.getSku(), false)
                + dictionaryKeyFormat("type") + dictionaryValueFormat_String(product.getType(), false)
                + dictionaryKeyFormat("price") + dictionaryValueFormat_String(product.getPrice(), false)
                + dictionaryKeyFormat("title") +  dictionaryValueFormat_String(product.getTitle(), false)
                + dictionaryKeyFormat("description") +  dictionaryValueFormat_String(product.getDescription(), false)
                + dictionaryKeyFormat("currencyCode") +  dictionaryValueFormat_String(product.getPriceCurrencyCode(), false)
                + dictionaryKeyFormat("priceValue") + dictionaryValueFormat_Long(product.getPriceAmountMicros(), true)
                + "}";
        return json;
    }

    private String getProductsJson(List<SkuDetails> products)
    {
        int count = 0;
        String json = "[";
        for (SkuDetails p : products)
        {
            if(count > 0)
            {
                json += ",";
            }
            ++count;
            json += getProductJson(p);
        }
        json += "]";
        return json;
    }

    /* Transaction Operations */

    public void finishPendingTransaction(final String productIdentifier)
    {
        detailedLog("Finishing Transaction: " + productIdentifier);

        if(!isHelperReady() || _inventory == null)
        {
            _unityMessageSender.SendMessage("OnConsumePurchaseFailed", "Setup not ready");
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable()
        {
            public void run()
            {
                try
                {
                    if(_inventory.hasPurchase(productIdentifier))
                    {
                        Purchase purchase = _inventory.getPurchase(productIdentifier);
                        _helper.consumeAsync(purchase, _consumeFinishedListener);
                    }
                    else
                    {
                        _unityMessageSender.SendMessage("OnConsumePurchaseFailed", "Invalid transaction data");
                    }
                }
                catch (IabAsyncInProgressException e)
                {
                    String errorMessage = "Consume Product Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnConsumePurchaseFailed", errorMessage);
                }
            }
        });
    }

    public void forceFinishPendingTransactions()
    {
        detailedLog("Forcefull Finishing All Transactions.");

        if(!isHelperReady() || _inventory == null)
        {
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable()
        {
            public void run()
            {
                try
                {
                    List<Purchase> allTransactions = _inventory.getAllPurchases();
                    _helper.consumeAsync(allTransactions, _consumeMultiFinishedListener);
                }
                catch (IabAsyncInProgressException e)
                {
                    String errorMessage = "Consume Product Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnConsumePurchaseFailed", errorMessage);
                }
            }
        });
    }

    private void updateTransaction(Purchase purchase)
    {
        if(purchase == null)
        {
            return;
        }

        switch (purchase.getPurchaseState())
        {
            case PurchaseState.Purchased:
            {
                detailedLog("Purchase successful: " + purchase.getSku());
                _unityMessageSender.SendMessage("OnPurchaseSucceeded", getTransactionJson(purchase));
            }
                break;
            case PurchaseState.Canceled:
            {
                String message = "Purchase canceled: " + purchase.getSku();
                detailedLog(message);
                _unityMessageSender.SendMessage("OnPurchaseFailed", message);
            }
                break;
            case PurchaseState.Refunded:
            {
                String message = "Purchase refunded: " + purchase.getSku();
                detailedLog(message);
                //Implement refund logic if needed...
            }
            break;
            default:
                break;
        }
    }

    private String getTransactionJson(Purchase purchase)
    {
        String json = "{"
                + dictionaryKeyFormat("itemType") + dictionaryValueFormat_String(purchase.getItemType(), false)
                + dictionaryKeyFormat("orderId") + dictionaryValueFormat_String(purchase.getOrderId(), false)
                + dictionaryKeyFormat("packageName") + dictionaryValueFormat_String(purchase.getPackageName(), false)
                + dictionaryKeyFormat("sku") + dictionaryValueFormat_String(purchase.getSku(), false)
                + dictionaryKeyFormat("purchaseTime") +  dictionaryValueFormat_Long(purchase.getPurchaseTime(), false)
                + dictionaryKeyFormat("purchaseState") +  dictionaryValueFormat_Int(purchase.getPurchaseState(), false)
                + dictionaryKeyFormat("developerPayload") +  dictionaryValueFormat_String(purchase.getDeveloperPayload(), false)
                + dictionaryKeyFormat("token") +  dictionaryValueFormat_String(purchase.getToken(), false)
                + dictionaryKeyFormat("originalJson") +  dictionaryValueFormat_Raw(purchase.getOriginalJson(), false)
                + dictionaryKeyFormat("signature") + dictionaryValueFormat_String(purchase.getSignature(), true)
                + "}";
        return json;
    }

    private String getTransactionsJson(List<Purchase> purchases)
    {
        int count = 0;
        String json = "[";
        for (Purchase p : purchases)
        {
            if(count > 0)
            {
                json += ",";
            }
            ++count;
            json += getTransactionJson(p);
        }
        json += "]";
        return json;
    }

    /* Debug */

    void detailedLog(String message)
    {
        if(_highDetailedLogEnabled)
        {
            Log.d(TAG, message);
        }
    }

    String dictionaryKeyFormat(String key)
    {
        return "\"" + key + "\":";
    }

    String dictionaryValueFormat_String(String value, boolean isFinalValue)
    {
        return "\"" + value + "\"" + getConcatString(isFinalValue);
    }

    String dictionaryValueFormat_Raw(String value, boolean isFinalValue)
    {
        return value + getConcatString(isFinalValue);
    }

    String dictionaryValueFormat_Long(Long value, boolean isFinalValue)
    {
        return Long.toString(value) + getConcatString(isFinalValue);
    }

    String dictionaryValueFormat_Int(int value, boolean isFinalValue)
    {
        return Integer.toString(value) + getConcatString(isFinalValue);
    }

    String getConcatString(boolean isFinalValue)
    {
        String concatString = "";
        if(!isFinalValue)
        {
            concatString += ",";
        }
        return concatString;
    }

    /* Listeners */

    @Override
    public void receivedBroadcast()
    {
        // Received a broadcast notification that the inventory of items has changed
        detailedLog("Received broadcast notification. Querying inventory.");
        loadProducts(_lastRequestedProductIds);
    }

    @Override
    public void handleActivityResult(int requestCode, int resultCode, Intent data)
    {
        if(isHelperReady())
        {
           _helper.handleActivityResult(requestCode, resultCode, data);
        }
    }

    // Listener that's called when we finish querying the items and subscriptions we own
    IabHelper.QueryInventoryFinishedListener _gotInventoryListener = new IabHelper.QueryInventoryFinishedListener()
    {
        public void onQueryInventoryFinished(IabResult result, Inventory inventory)
        {
            detailedLog("Query inventory finished.");
            // Have we been disposed of in the meantime? If so, quit.
            if (_helper == null) return;

            // Is it a failure?
            if (result.isFailure() || inventory == null)
            {
                detailedLog("Failed to query inventory: " + result);
                _unityMessageSender.SendMessage("OnQueryInventoryFailed", result.toString());
                return;
            }

            detailedLog("Query inventory was successful.");
            _inventory = inventory;

            // Loaded products
            List<SkuDetails> skus = inventory.getAllSkuDetails();
            _unityMessageSender.SendMessage("OnQueryInventorySucceeded", getProductsJson(skus));

            //Pending purchases
            List<Purchase> purchases = inventory.getAllPurchases();
            for (Purchase p : purchases)
            {
                updateTransaction(p);
            }
        }
    };

    // Callback for when a purchase is finished
    IabHelper.OnIabPurchaseFinishedListener _purchaseFinishedListener = new IabHelper.OnIabPurchaseFinishedListener()
    {
        public void onIabPurchaseFinished(IabResult result, Purchase purchase)
        {
            detailedLog("Purchase finished: " + result + ", purchase: " + purchase);
            // if we were disposed of in the meantime, quit.
            if (_helper == null) return;

            if (result.isFailure() || purchase == null)
            {
                detailedLog("Purchase failed: " + result);
                switch (result.getResponse())
                {
                    case IabHelper.IABHELPER_USER_CANCELLED:
                    {
                        _unityMessageSender.SendMessage("OnPurchaseCancelled", result.toString());
                    }
                        break;
                    default:
                    {
                        _unityMessageSender.SendMessage("OnPurchaseFailed", result.toString());
                    }
                        break;
                }
                return;
            }

            // Update local inventory to keep track of completed purchases to consume
            if(purchase.getPurchaseState() == PurchaseState.Purchased
                && _inventory != null && !_inventory.hasPurchase(purchase.getSku()))
            {
                _inventory.addPurchase(purchase);
            }

            updateTransaction(purchase);
        }
    };

    // Called when consumption is complete
    IabHelper.OnConsumeFinishedListener _consumeFinishedListener = new IabHelper.OnConsumeFinishedListener()
    {
        public void onConsumeFinished(Purchase purchase, IabResult result)
        {
            detailedLog("Consumption finished. Purchase: " + purchase + ", result: " + result);
            // if we were disposed of in the meantime, quit.
            if (_helper == null) return;

            if (result.isFailure() || purchase == null)
            {
                detailedLog("Consume failed: " + result);
                _unityMessageSender.SendMessage("OnConsumePurchaseFailed", result.toString());
                return;
            }

            _inventory.erasePurchase(purchase.getSku());
            detailedLog("Consume successful: " + purchase.getSku());
            _unityMessageSender.SendMessage("OnConsumePurchaseSucceeded", getTransactionJson(purchase));
        }
    };

    // Callback that notifies when a multi-item consumption operation finishes.
    IabHelper.OnConsumeMultiFinishedListener _consumeMultiFinishedListener = new IabHelper.OnConsumeMultiFinishedListener()
    {
        public void onConsumeMultiFinished(List<Purchase> purchases, List<IabResult> results)
        {
            detailedLog("Multi Consumption Finished.");
            // if we were disposed of in the meantime, quit.
            if (_helper == null) return;

            int totalPurchases = purchases.size();
            int totalResults = results.size();
            if(totalPurchases != totalResults) return;

            for(int i = 0; i < totalPurchases; ++i)
            {
                _consumeFinishedListener.onConsumeFinished(purchases.get(i), results.get(i));
            }
        }
    };
}
