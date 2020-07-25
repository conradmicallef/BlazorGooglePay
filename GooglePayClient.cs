using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrowserInterop.Extensions;
using Microsoft.JSInterop;

namespace BlazorGooglePay
{
    public class GooglePayClient : IAsyncDisposable
    {
        private IJSRuntime _jsRuntime;
        private JsRuntimeObjectRef _jsObjectRef;
        
        public event Func<object, GooglePayButton, bool>? ButtonClicked;
        
        internal GooglePayClient(IJSRuntime jsRuntime, JsRuntimeObjectRef jsObjectRef)
        {
            _jsRuntime = jsRuntime;
            _jsObjectRef = jsObjectRef;
        }

        public async ValueTask<GooglePayButton> CreateButtonAsync(GoogleButtonType type)
        {
            var button = new GooglePayButton(_jsRuntime);
            var callback = CallBackInteropWrapper.Create(async () =>
            {
                var isHandled = await OnButtonClicked(button);
                if (!isHandled)
                {
                    await button.OnClicked();
                }
            },
            serializationSpec: false);
            button.JsObjectRef = await _jsRuntime.InvokeAsync<JsRuntimeObjectRef>(
                "blazorGooglePay.createButton",
                _jsObjectRef,
                callback,
                type);
            return button;
        }
        
        public ValueTask<GooglePayIsReadyToPayResponse> IsReadyToPayAsync()
        {
            return _jsRuntime.InvokeAsync<GooglePayIsReadyToPayResponse>(
                "blazorGooglePay.isReadyToPay",
                _jsObjectRef);
        }
        
        public ValueTask PrefetchGooglePaymentDataAsync(string currencyCode)
        {
            return _jsRuntime.InvokeVoidAsync(
                "blazorGooglePay.prefetchGooglePaymentData",
                _jsObjectRef,
                currencyCode);
        }

        public ValueTask SetAllowedAuthMethodsAsync(params string[] authMethods)
        {
            return _jsRuntime.InvokeVoidAsync(
                "blazorGooglePay.setAllowedAuthMethods",
                _jsObjectRef,
                authMethods);
        }
        
        public ValueTask SetAllowedCardNetworksAsync(params string[] cardNetworks)
        {
            return _jsRuntime.InvokeVoidAsync(
                "blazorGooglePay.setAllowedCardNetworks",
                _jsObjectRef,
                cardNetworks);
        }
        
        public ValueTask ProcessPayment(GooglePayTransactionInfo tranInfo)
        {
            return _jsRuntime.InvokeVoidAsync(
                "blazorGooglePay.processPayment",
                _jsObjectRef,
                tranInfo);
        }
        
        protected virtual async ValueTask<bool> OnButtonClicked(GooglePayButton button)
        {
            if (ButtonClicked?.GetInvocationList() != null)
            {
                foreach (var handler in ButtonClicked?.GetInvocationList()!)
                {
                    var isHandled = (bool) handler.DynamicInvoke(this, button);
                    if (isHandled)
                    {
                        return isHandled;
                    }
                }
            }
            return false;
        }
        
        public async ValueTask DisposeAsync()
        {
            await _jsObjectRef.DisposeAsync();
            ButtonClicked = null;
        }
    }
}