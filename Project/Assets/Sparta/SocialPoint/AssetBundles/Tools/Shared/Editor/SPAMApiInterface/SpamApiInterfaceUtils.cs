using UnityEngine;
using SocialPoint.Tool.Shared;
using System.IO;
using System.Net;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip;

namespace SocialPoint.Tool.Shared
{
	public partial class SPAMApiInterface
	{
		public const string HD_CONTENT_DISPOSITION = "Content-Disposition";
		public const string HD_LOCATION = "Location";
		
		bool RequiresDownload(WebResponse response)
		{
			return response.ContentType == SPAMAuthenticator.OCTETSTREAM || response.ContentType == SPAMAuthenticator.ZIP;
		}
		
		string GetAttachmentFileName(WebResponse response)
		{
			string url = response.ResponseUri.OriginalString;

			if (response.Headers [HD_CONTENT_DISPOSITION] != null)
				return response.Headers [HD_CONTENT_DISPOSITION].Replace ("attachment; filename=", "").Replace ("\"", "");
			else if (response.Headers [HD_LOCATION] != null)
				return Path.GetFileName (response.Headers [HD_LOCATION]);
			else if (Path.GetFileName (url).Contains ("?") || Path.GetFileName (url).Contains ("="))
				return Path.GetFileName (response.ResponseUri.ToString ());
			else
				return Path.GetRandomFileName();
		}

		void DownloadAttachment(WebResponse response, string toPath, RequestProgressParams rpParams = null)
		{

			using (Stream attachment = response.GetResponseStream ()) {

				using( FileStream f = File.Open(toPath, FileMode.Create, FileAccess.Write, FileShare.None)) {

					byte[] buffer = new byte[4096];
					int bytesRead = 0;
					long accumBytes = 0;
					long totalBytes = response.ContentLength > -1 ? response.ContentLength : 0;
					float progressCount = 0;
					bool bUsingProgressCallback = rpParams != null && response.ContentLength > -1;

					while ((bytesRead = attachment.Read(buffer, 0, buffer.Length)) > 0) {
						accumBytes += bytesRead;
						f.Write(buffer, 0, bytesRead);
						if (bUsingProgressCallback) {
							float newProgress = ((float)accumBytes / (float)totalBytes) * 100f;
							if (newProgress != progressCount) {
								progressCount = newProgress;
								rpParams.progress(progressCount);
							}
						}
					}
				}
			}
		}

		public static void UnpackZipFile( string zipfile, string deploypath, bool clearcontents ) 
		{
			// Clear previous contents in the asset directory
			if( clearcontents )
			{
				DirectoryInfo deploypathinfo = new DirectoryInfo( deploypath );
				foreach( FileInfo file in deploypathinfo.GetFiles() )
					if( file.Name != Path.GetFileName( zipfile ) )
						file.Delete();

				foreach( DirectoryInfo dir in deploypathinfo.GetDirectories() )
					dir.Delete(true); 
			}
			
			// Unzip file contents
			using( ZipFile zf = new ZipFile( zipfile ))
			{
				IEnumerator entryenum = zf.GetEnumerator();
				while( entryenum.MoveNext() )
				{
					ZipEntry entry = (ZipEntry)entryenum.Current;
					if( entry.IsDirectory )
					{
						Directory.CreateDirectory( ZipEntry.CleanName( entry.ToString() ) );
					}
					else
					{
						string entryfile = Path.Combine( deploypath, ZipEntry.CleanName( entry.ToString() ) );
						Directory.CreateDirectory( Path.GetDirectoryName( entryfile ) );
						
						using( Stream entrystream = zf.GetInputStream( entry ) )
						{
							using( FileStream f = File.Open(entryfile, FileMode.Create, FileAccess.Write, FileShare.None) )
							{
								byte[] buff = ReadFully( entrystream );
								f.Write(buff, 0, buff.Length);
							}
						}
					}
				}
				
			}
		}

		public static void ExtractMemeberFromZip( string zipFile, string entryName, string deployPath )
		{
			using (ZipFile zf = new ZipFile( zipFile )) {
				ZipEntry entry = zf.GetEntry(entryName);

				if( entry.IsDirectory ) {
					Directory.CreateDirectory( ZipEntry.CleanName( entry.ToString() ) );

					IEnumerator entryEnum = zf.GetEnumerator();
					while( entryEnum.MoveNext() ){
						entry = (ZipEntry)entryEnum.Current;
						string entryDir = ZipEntry.CleanName(entry.ToString());
						do {
							entryDir = Path.GetDirectoryName(entryDir);
							if( entryDir!=null && entryDir == entryName) {

								if(entry.IsDirectory){
									Directory.CreateDirectory( ZipEntry.CleanName( entry.ToString() ) );
								} else {
									string nEntryFile = Path.Combine( deployPath, ZipEntry.CleanName( entry.ToString() ) );

									using( Stream entryStream = zf.GetInputStream( entry ) ) {
										using( FileStream f = File.Open(nEntryFile, FileMode.Create, FileAccess.Write, FileShare.None) ) {
											byte[] buff = ReadFully( entryStream );
											f.Write(buff, 0, buff.Length);
										}
									}
								}

							}
						} while( entryDir!=null || entryDir!=string.Empty );
					}
				} else {
					string nEntryFile = Path.Combine( deployPath, entryName );
					Directory.CreateDirectory( Path.GetDirectoryName( nEntryFile ) );

					using( Stream entryStream = zf.GetInputStream( entry ) ) {
						using( FileStream f = File.Open(nEntryFile, FileMode.Create, FileAccess.Write, FileShare.None) ) {
							byte[] buff = ReadFully( entryStream );
							f.Write(buff, 0, buff.Length);
						}
					}
				}
			}
		}

		public static byte[] ReadFully(Stream input)
		{
			byte[] buffer = new byte[16*1024];
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
					ms.Write(buffer, 0, read);

				return ms.ToArray();
			}
		}
	}
}



