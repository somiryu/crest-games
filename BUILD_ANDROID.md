# Cómo generar los APK de MiniSEL (Minijuegos e Historias)

El certificado (keystore) con el que HiHat firmó los APK de 2024 se perdió y no se puede recuperar. Desde ahora usamos un certificado nuevo, propio. Esto tiene una consecuencia: **Android no deja instalar la versión nueva encima de la vieja**, porque las firmas son distintas. La primera vez hay que desinstalar e instalar (ver "Primera instalación en las tablets").

## 1. Requisitos (una sola vez por computador)

1. **Unity Hub** con **Unity 2022.3.13f1** (la versión exacta del proyecto, ver `Proj_Andes/ProjectSettings/ProjectVersion.txt`).
2. En Unity Hub → Installs → 2022.3.13f1 → Add modules: **Android Build Support**, con sus submódulos **OpenJDK** y **Android SDK & NDK Tools**. No hace falta instalar Android Studio.
3. Clonar `crest-games` y abrir la carpeta `Proj_Andes` desde Unity Hub.
4. **Poner Android como plataforma activa**: File → Build Settings → Android → Switch Platform, y esperar a que termine de recompilar. Si se construye desde otra plataforma, Burst falla con "Burst requires the Android NDK to be correctly installed" aunque el NDK esté instalado. El menú de build lo revisa y ofrece cambiarla.
5. **Librerías Android de Firebase**: la carpeta `Proj_Andes/Assets/Plugins` no está en git. En un clon nuevo, antes del primer build: Assets → External Dependency Manager → Android Resolver → Force Resolve. Si durante un build pregunta "Enable Android Auto-resolution?", responder Disable (las librerías ya están resueltas).
6. El archivo de Firebase `Proj_Andes/Assets/google-services.json` ya está en el repo y registra los dos apps (`com.HiHatGames.CREST` y `com.HiHatGames.MiniSelNarratives`). Cambiar de certificado no afecta a Firebase: el app usa autenticación anónima y Firestore, que no dependen de la firma.

## 2. Crear el certificado nuevo (una sola vez, y guardarlo bien)

1. En Unity: Edit → Project Settings → Player → pestaña Android → **Publishing Settings** → **Keystore Manager…** → Keystore… → **Create New** → Anywhere.
2. Guardarlo **fuera del repositorio**, por ejemplo `~/MiniSEL-firma/minisel.keystore`. El repo ignora `*.keystore`; nunca debe subirse.
3. Crear una clave (alias), por ejemplo `minisel`, con validez de 50 años o más. Llenar nombre y organización.
4. **Guardar en un gestor de contraseñas**: el archivo `.keystore`, su contraseña, el alias y la contraseña del alias. Hacer al menos una copia de respaldo fuera del computador (por ejemplo, en el Drive del equipo, en una carpeta restringida). Si se pierde otra vez, cada actualización obligará de nuevo a desinstalar en todas las tablets y se perderán los datos sin sincronizar.
5. El mismo certificado sirve para los dos apps.

## 3. Generar un build

1. **Subir la versión** en `Proj_Andes/Assets/Proj_Andes/Scripts/AppVersion.cs` (`Number`), una vez por cada versión publicada. El menú de build la copia a la versión de Android. Los APK de 2024 son la versión 1; la primera versión nueva es la 2.
2. En Project Settings → Player → Android → Publishing Settings: activar **Custom Keystore**, elegir el `.keystore` y escribir las contraseñas. Unity no guarda las contraseñas en el proyecto: hay que escribirlas cada vez que se abre Unity.
3. **Quitar el paquete de Unity MCP antes de un build de release** (Window → Package Manager → `com.ivanmurzak.unity.mcp` → Remove), o el APK llevaría sus librerías. El menú de build avisa si sigue instalado. Volver a instalarlo después si se usa para desarrollo.
4. Menú **Hi Hat Games → Build →**
   - **Minijuegos - Android (release)**, o
   - **Historias - Android (release)**.

   El menú aplica nombre, identificador y versión de cada app. Los builds "(desarrollo)" muestran el panel de depuración (saltar escena); no deben llegar a las tablets.
5. El APK queda en `Proj_Andes/Builds/<App>/<nombre>_<versión>.apk` y se abre la carpeta.

## 4. Primera instalación en las tablets (cambio de certificado)

Por cada tablet, en este orden:

1. Abrir el app viejo **con internet** y entrar hasta la pantalla de selección de jugador, para que sincronice lo pendiente. Esperar el mensaje de datos subidos.
2. Desinstalar el app viejo. Esto borra lo que no se haya sincronizado.
3. Instalar el APK nuevo (habilitar "instalar apps de origen desconocido" si lo pide).

Las versiones siguientes ya se instalan encima, sin desinstalar, mientras se use el mismo certificado.

## 5. Cortar una versión vieja

Cuando una versión nueva deba reemplazar a la anterior:

1. En Firestore, `config/app`: subir `min_version_minijuegos` (o `min_version_narrativas`) al número de la versión nueva. Las tablets con la versión vieja muestran "borra esta versión y descarga la última".
2. Cuando las tablets ya sincronizaron, subir `MIN_UPLOAD_VERSION` en las reglas de Firestore (`crest-cms/firestore.rules`, se publican desde la consola de Firebase) para que Firebase rechace datos de la versión vieja.
