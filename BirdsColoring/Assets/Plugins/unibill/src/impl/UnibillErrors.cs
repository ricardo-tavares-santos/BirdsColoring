//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Reflection;

public enum UnibillError {
    BILLER_NOT_READY,

    STOREKIT_BILLING_UNAVAILABLE,
    STOREKIT_RETURNED_NO_PRODUCTS,
    STOREKIT_REQUESTPRODUCTS_MISSING_PRODUCT,
    STOREKIT_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_STOREKIT,
    STOREKIT_FAILED_TO_RETRIEVE_PRODUCT_DATA,
    STOREKIT_UNKNOWN_PRODUCT_ID,

    GOOGLEPLAY_BILLING_UNAVAILABLE,
    GOOGLEPLAY_PUBLICKEY_NOTCONFIGURED,
    GOOGLEPLAY_PUBLICKEY_INVALID,
    GOOGLEPLAY_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_GOOGLEPLAY,
    GOOGLEPLAY_NO_PRODUCTS_RETURNED,
	GOOGLEPLAY_MISSING_PRODUCT,

    AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED,
    AMAZONAPPSTORE_GETITEMDATAREQUEST_MISSING_PRODUCT,
    AMAZONAPPSTORE_GETITEMDATAREQUEST_NO_PRODUCTS_RETURNED,
    AMAZONAPPSTORE_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_AMAZON,

	SAMSUNG_APPS_MISSING_PRODUCT,
	SAMSUNG_APPS_NO_PRODUCTS_RETURNED,
	SAMSUNG_APPS_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_SAMSUNG,

	WP8_MISSING_PRODUCT,
	WP8_NO_PRODUCTS_RETURNED,
	WP8_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_MICROSOFT,
    WP8_APP_ID_NOT_KNOWN,

    WIN_8_1_MISSING_PRODUCT,
    WIN_8_1_NO_PRODUCTS_RETURNED,
    WIN_8_1_APP_NOT_KNOWN,
    WIN_8_1_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_MICROSOFT,

    UNIBILL_UNKNOWN_PRODUCTID,
    UNIBILL_INITIALISE_FAILED_WITH_CRITICAL_ERROR,
    UNIBILL_NO_PRODUCTS_DEFINED,
    UNIBILL_ATTEMPTING_TO_PURCHASE_ALREADY_OWNED_NON_CONSUMABLE,
    UNIBILL_MISSING_PRODUCT
}
