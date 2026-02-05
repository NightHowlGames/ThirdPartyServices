# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## IAP Services Module Overview

This module provides In-App Purchase (IAP) functionality for Unity mobile games, part of the Third Party Services package (`com.gdk.3rd`). It supports both Unity IAP implementation and dummy implementation for testing.

## Architecture

### Service Pattern
- **Interface**: `IIapServices` - Main service contract
- **Implementations**:
  - `UnityIapServices` - Production Unity IAP implementation
  - `DummyIapServices` - Testing/editor implementation
- **Namespace**: `ServiceImplementation.IAPServices`

### Dependency Injection Support
The module supports two DI frameworks via conditional compilation:
- **Zenject** (`GDK_ZENJECT`): Uses `IapInstaller`
- **VContainer** (`GDK_VCONTAINER`): Uses `IAPVContainer` extension methods

### Key Components

#### Core Files
- `IIapServices.cs` - Service interface defining IAP operations
- `UnityIapServices.cs` - Unity IAP Store implementation
- `DummyIapServices.cs` - Mock implementation for testing
- `IAPModel.cs` - Data model for IAP products
- `ProductData.cs` - Product information wrapper
- `IapInstaller.cs` - Zenject DI installer
- `IAPVContainer.cs` - VContainer DI extensions

#### Signals (Event System)
Located in `Signals/` directory:
- `OnStartDoingIAPSignal` - Purchase initiation
- `OnIAPPurchaseSuccessSignal` - Successful purchase
- `OnIAPPurchaseFailedSignal` - Failed purchase
- `UnityIAPOnPurchaseCompleteSignal` - Unity IAP completion
- `OnRestorePurchaseCompleteSignal` - Restore purchases completion

## Assembly Definition

**Assembly**: `3rd.IAP`
**Key Dependencies**:
- Unity IAP packages (`UnityEngine.Purchasing.*`)
- Unity Services Core
- GameFoundation utilities and signals
- DI frameworks (Zenject/VContainer)
- Security packages for receipt validation

## Usage

### Service Registration

#### VContainer
```csharp
builder.RegisterIAPService(); // Extension method from IAPVContainer
```

#### Zenject
```csharp
Container.Install<IapInstaller>();
```

### Service Initialization
```csharp
// Initialize with product catalog
Dictionary<string, IAPModel> products = LoadProducts();
iapService.InitIapServices(products, "production");
```

### Making Purchases
```csharp
iapService.BuyProductID(
    productId, 
    onComplete: (id, quantity) => { /* success */ },
    onFailed: (error) => { /* failure */ }
);
```

## Build Configuration

### Conditional Compilation Flags
- `THEONE_IAP` - Enables real IAP implementation
- `GDK_ZENJECT` - Zenject DI framework
- `GDK_VCONTAINER` - VContainer DI framework  
- `UNITY_EDITOR` - Editor mode (uses dummy)
- `CREATIVE` - Creative/test mode (uses dummy)

### Production vs Test
- **Production**: Real `UnityIapServices` when `THEONE_IAP && !UNITY_EDITOR && !CREATIVE`
- **Test/Editor**: `DummyIapServices` otherwise

## Common Operations

### Get Product Price
```csharp
string price = iapService.GetPriceById(productId, defaultPrice);
```

### Check Product Ownership
```csharp
bool owned = iapService.IsProductOwned(productId);
```

### Restore Purchases
```csharp
iapService.RestorePurchases(
    onComplete: () => { /* restored */ },
    onFail: () => { /* failed */ }
);
```

### Get Product Data
```csharp
ProductData data = iapService.GetProductData(productId);
```

## Integration Notes

1. **Initialization Required**: Must call `InitIapServices` before any purchase operations
2. **Signal Subscriptions**: Subscribe to signals for purchase flow monitoring
3. **Receipt Validation**: Unity IAP Security packages included for receipt validation
4. **Platform Support**: Configured for Google Play and iOS App Store
5. **Error Handling**: Always provide error callbacks for purchase operations

## Testing

Use `DummyIapServices` in editor/test builds:
- Simulates successful purchases
- No real transactions
- Useful for UI/flow testing

## Dependencies from Parent Package

This module is part of `com.gdk.3rd` (Third Party Services) which includes:
- Analytics services
- Ad services  
- Remote config
- Other third-party integrations

Repository: https://github.com/GameDevelopmentKit/ThirdPartyServices