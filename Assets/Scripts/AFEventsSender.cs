using AppsFlyerSDK;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Purchasing;

public class AFEventsSender
{
    public static void SendPurchaseEvent(Product product)
    {
        var price = product.metadata.localizedPrice;
        var currency = product.metadata.isoCurrencyCode;


        Dictionary<string, string> iapData = new Dictionary<string, string>
        {
           { AFInAppEvents.REVENUE, price.ToString(CultureInfo.InvariantCulture) },
           { AFInAppEvents.CURRENCY, currency },
           { AFInAppEvents.CONTENT_ID, product.definition.id }
        };


        AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, iapData);
        Debug.Log($"[AppsFlyer] af_purchase: {price} {currency} — Отправлено");
    }
}
