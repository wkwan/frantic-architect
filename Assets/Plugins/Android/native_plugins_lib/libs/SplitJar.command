

cd pwd
echo pwd
echo “splitting jar files…”

jar -xf androidnativeplugin.jar

echo “extracted files…”

rm -rf androidnativeplugin.jar

jar -cf billing_interface.jar com/android/vending
rm -rf com/android/vending
jar -cf androidnativeplugin.jar com/voxelbusters
rm -rf com
rm -rf META-INF