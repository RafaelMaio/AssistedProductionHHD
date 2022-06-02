# AssistedProductionHHD

AR mobile application for supporting human operators in industrial picking tasks.

### Versions:

- Unity 2020.3.30f1

- ARFoundation 4.2.3

### Use the application:

1. Verify if the smartphone supports ARCore (https://developers.google.com/ar/devices).
2. Transfer the "*assisted_production.apk*" APK to the smartphone.
3. Install the application through the APK.

### Make changes to the application:

1. Open the project folder in a compatible Unity version.
2. Make the desired changes.
3. Go to *File -> Build Settings -> Player Settings... -> Publish Settings*.
4. At the *Project Keystore* select the *user.keystore* keystore in the *Path* field.
5. Insert the password *123456*in the *Password* field.
6. At the *Project Key* select the "*bolsa*" alias in the *Alias* field.
7. Insert the password *123456* in the *Password* field.
8. Build and Run to generate the new APK.
