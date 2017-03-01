package es.socialpoint.unity.base;

import java.io.Closeable;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.security.Key;
import java.util.Properties;

import javax.crypto.Cipher;
import javax.crypto.CipherInputStream;
import javax.crypto.CipherOutputStream;
import javax.crypto.spec.SecretKeySpec;

import android.content.Context;
import android.os.Environment;
import android.util.Log;

/**
 * Persistent Attr Storage Allows to store key-value pairs with serialized Attrs
 * in the device's sd card, keeping the storage after app uninstall.
 * 
 * @author Manuel J. Alvarez de Toledo Vergara
 * @date September, 2014
 */
public class PersistentAttrStorage {

	private static final String TAG = "PersistentAttrStorage";

	// Storage file path
	private static final String storageFileName = ".spstorage";
	private static final String ALGORITHM = "AES";
	private static final String DEFAULT_CHIPHER_KEY = "ea37jm4nl2l0at15";
	private String storageFilePath;
	private String entryPrefix = "";
	private String cipherKey;

	public PersistentAttrStorage(Context context, String deviceKey){
		init(context,deviceKey);
	}

	public void init(Context context, String deviceKey) {
		storageFilePath = Environment.getExternalStorageDirectory() + "/" + storageFileName;
		entryPrefix = context.getPackageName() + ".";
		
		// Create the cipher 16 bytes key using the device serial
		if(deviceKey == null || deviceKey.isEmpty()) {
			cipherKey = DEFAULT_CHIPHER_KEY;
		} else if(deviceKey.length() < 16) {
			cipherKey = deviceKey + DEFAULT_CHIPHER_KEY.substring(deviceKey.length());
		} else {
			cipherKey = deviceKey.substring(0, 16);
		}
	}
	
	public Properties getContent() {
		return loadPersistentStorage();
	}

	private void closeQuietly(Closeable stream) {
		try {
			if(stream != null) {
				stream.close();
			}
		} catch(IOException e) {
			Log.w(TAG, "Error closing stream");
		}
	}
	
	private Cipher getCipher(int mode) throws CipherException {
		Key secretKey = new SecretKeySpec(cipherKey.getBytes(), ALGORITHM);
		Cipher cipher = null;
		try {
			cipher = Cipher.getInstance(ALGORITHM);
			cipher.init(mode, secretKey);
		} catch (Exception e) {
			Log.e(TAG, "Error reading persistent store data. Causes: " + e.toString());
			throw new CipherException();
		}
		
		return cipher;
	}
	
	public boolean removeStorageFile() {
		File f = new File(storageFilePath);
		return f.delete();
	}
	
	public String getStorageFile() {
		Properties properties = loadPersistentStorage();
		return properties.toString();
	}

	private Properties loadPersistentStorage() {
		Properties storage = new Properties();
		File f = new File(storageFilePath);
		FileInputStream fileStream = null;
		CipherInputStream input = null;
		
		if (f.exists()) {
			try {
				fileStream = new FileInputStream(f);
				input = new CipherInputStream(fileStream, getCipher(Cipher.DECRYPT_MODE));
				storage.load(input);
			} catch (CipherException e) {
				// Remove file if there is an cipher error
				Log.e(TAG, "Error during cipher initialization. Causes:" + e.getMessage());
				removeStorageFile();
			} catch (IOException e) {
				// Remove file if there is an cipher error
				Log.e(TAG, "Error loading Persistent storage file. Causes:" + e.getMessage());
				removeStorageFile();
			} finally {
				closeQuietly(input);
				closeQuietly(fileStream);
			}
		}

		return storage;
	}

	private boolean savePersistentStorage(Properties storage) {
		boolean success = false;
		FileOutputStream fileStream = null;
		CipherOutputStream out = null;

		try {
			fileStream = new FileOutputStream(storageFilePath);
			out = new CipherOutputStream(fileStream, getCipher(Cipher.ENCRYPT_MODE));
			storage.store(out, "");
			out.flush();
			success = true;
		} catch (Exception e) {
			Log.e(TAG, "Error writing Persistent storage file.");
		} finally {	
			closeQuietly(out);
			closeQuietly(fileStream);
		}

		return success;
	}

	private synchronized String getAttr(String key, String defaultValue) {
		String attr = loadPersistentStorage().getProperty(key);
		return (attr != null)? attr : defaultValue;
	}

	private synchronized boolean setAttr(String key, String attr) {
		boolean success = false;
		Properties storage = loadPersistentStorage();
		storage.setProperty(key, attr);
		success = savePersistentStorage(storage);

		return success;
	}

	private synchronized boolean removeAttr(String key) {
		Properties storage = loadPersistentStorage();
		storage.remove(key);
		savePersistentStorage(storage);

		return savePersistentStorage(storage);
	}
	
	private String createKey(String groupPrefix, String customPrefix, String key) {
		String finalKey;
		if(groupPrefix != null && !groupPrefix.isEmpty())
		{
			finalKey = groupPrefix;
		}
		else
		{
			finalKey = entryPrefix;
		}

		if(customPrefix != null && !customPrefix.isEmpty())
		{
			finalKey += customPrefix;
		}

		finalKey += key;

		return finalKey;
	}

	/** JNI Interface **/

	public String getAttrForKey(String groupPrefix, String customPrefix, String key, String defaultValue) {
		String finalKey = createKey(groupPrefix, customPrefix, key);
		return getAttr(finalKey, defaultValue);
	}

	public boolean setAttrForKey(String groupPrefix, String customPrefix, String key, String attr) {
		String finalKey = createKey(groupPrefix, customPrefix, key);
		return setAttr(finalKey, attr);
	}

	public boolean removeAttrForKey(String groupPrefix, String customPrefix, String key) {
		String finalKey = createKey(groupPrefix, customPrefix, key);
		return removeAttr(finalKey);
	}

	public boolean contains(String groupPrefix, String customPrefix, String key) {
		String finalKey = createKey(groupPrefix, customPrefix, key);
		return getAttr(finalKey, null) != null;
	}
	
	/* Internal exceptions */
	private class CipherException extends Exception {
		private static final long serialVersionUID = 1L;
	}
}
