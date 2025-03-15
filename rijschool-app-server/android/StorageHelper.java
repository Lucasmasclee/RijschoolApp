package com.yourcompany.plugin;

import android.content.Context;
import android.content.SharedPreferences;

public class StorageHelper {
    public static String getStoredCode(Context context) {
        SharedPreferences prefs = context.getSharedPreferences("RijschoolApp", Context.MODE_PRIVATE);
        return prefs.getString("rijschoolAppCode", "");
    }
} 