
PLEASE READ THIS CAREFULLY FOR ISSUES RELATED TO ANDROID MANIFEST DETAILS.


//For Video Rendering - Hardware acceleration should be on.
1. Make sure android:hardwareAccelerated="true" is in your ROOT manifest application settings manifest file. This is to allow video rendering.
		ex:   <application android:theme="@android:style/Theme.NoTitleBar" android:icon="@drawable/app_icon" android:label="@string/app_name" android:debuggable="true" android:hardwareAccelerated="true">


//For receiving touches in Webview, ForwardNativeEventsToDalvik should be true in your ROOT manifest file.
2. "unityplayer.ForwardNativeEventsToDalvik" value should be true. This is to pass touch events to other views [Webviews touch is dependent on this flag] 
		ex: <meta-data android:name="unityplayer.ForwardNativeEventsToDalvik" android:value="true" />


//Please refer AndroidManifest_root_activity_settings_sample.txt for sample reference related to flags required in root manifest.
