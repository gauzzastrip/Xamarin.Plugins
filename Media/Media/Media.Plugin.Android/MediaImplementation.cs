//
//  Copyright 2011-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Android.Graphics;
using Android.Media;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using Android.Runtime;
using Android.Database;
using Java.Util.Regex;
using Java.Lang;



namespace Plugin.Media
{
	/// <summary>
	/// Implementation for Feature
	/// </summary>
	[Android.Runtime.Preserve (AllMembers = true)]
	public class MediaImplementation : IMedia
	{
		/// <summary>
		/// Implementation
		/// </summary>
		public MediaImplementation ()
		{

			this.context = Android.App.Application.Context;
			IsCameraAvailable = context.PackageManager.HasSystemFeature (PackageManager.FeatureCamera);

			if (Build.VERSION.SdkInt >= BuildVersionCodes.Gingerbread)
				IsCameraAvailable |= context.PackageManager.HasSystemFeature (PackageManager.FeatureCameraFront);
		}

		///<inheritdoc/>
		public Task<bool> Initialize ()
		{
			return Task.FromResult (true);
		}

		/// <inheritdoc/>
		public bool IsCameraAvailable {
			get;
			private set;
		}

		/// <inheritdoc/>
		public bool IsTakePhotoSupported {
			get { return true; }
		}

		/// <inheritdoc/>
		public bool IsPickPhotoSupported {
			get { return true; }
		}

		/// <inheritdoc/>
		public bool IsTakeVideoSupported {
			get { return true; }
		}

		/// <inheritdoc/>
		public bool IsPickVideoSupported {
			get { return true; }
		}

		public int Orientation {
			get ;
			set ;
		}

		public Task CrossPermission { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Intent GetPickPhotoUI ()
		{
			int id = GetRequestId ();
			return CreateMediaIntent (id, "image/*", Intent.ActionPick, null, tasked: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public Intent GetTakePhotoUI (StoreCameraMediaOptions options)
		{
			if (!IsCameraAvailable)
				throw new NotSupportedException ();
			
			VerifyOptions (options);

			int id = GetRequestId ();
			return CreateMediaIntent (id, "image/*", MediaStore.ActionImageCapture, options, tasked: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Intent GetPickVideoUI ()
		{
			int id = GetRequestId ();
			return CreateMediaIntent (id, "video/*", Intent.ActionPick, null, tasked: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public Intent GetTakeVideoUI (StoreVideoOptions options)
		{
			if (!IsCameraAvailable)
				throw new NotSupportedException ();

			VerifyOptions (options);

			return CreateMediaIntent (GetRequestId (), "video/*", MediaStore.ActionVideoCapture, options, tasked: false);
		}

		/// <summary>
		/// Picks a photo from the default gallery
		/// </summary>
		/// <returns>Media file or null if canceled</returns>
		public async Task<MediaFile> PickPhotoAsync ()
		{
			if (!(await RequestStoragePermission ().ConfigureAwait (false))) {
				return null;
			}
			return await TakeMediaAsync ("image/*", Intent.ActionPick, null);
		}


		async Task<bool> RequestCameraPermission ()
		{
			var status1 = await CrossPermissions.Current.CheckPermissionStatusAsync (Permissions.Abstractions.Permission.Storage);
			var status2 = await CrossPermissions.Current.CheckPermissionStatusAsync (Permissions.Abstractions.Permission.Camera);
			if (status1 != Permissions.Abstractions.PermissionStatus.Granted ||
			    status2 != Permissions.Abstractions.PermissionStatus.Granted) {
				Console.WriteLine ("Does not have storage or camera permissions granted, requesting.");
				var results = await CrossPermissions.Current.RequestPermissionsAsync (Permissions.Abstractions.Permission.Camera, Permissions.Abstractions.Permission.Storage);
				if (results [Permissions.Abstractions.Permission.Storage] != Permissions.Abstractions.PermissionStatus.Granted ||
				    results [Permissions.Abstractions.Permission.Camera] != Permissions.Abstractions.PermissionStatus.Granted) {
					Console.WriteLine ("Storage or Camera permission Denied.");
					return false;
				}
			}

			return true;
		}

		async Task<bool> RequestStoragePermission ()
		{
			var status = await CrossPermissions.Current.CheckPermissionStatusAsync (Permissions.Abstractions.Permission.Storage);
			if (status != Permissions.Abstractions.PermissionStatus.Granted) {
				Console.WriteLine ("Does not have storage permissions granted, requesting.");
				var results = await CrossPermissions.Current.RequestPermissionsAsync (Permissions.Abstractions.Permission.Storage);
				if (results [Permissions.Abstractions.Permission.Storage] != Permissions.Abstractions.PermissionStatus.Granted) {
					Console.WriteLine ("Storage permission Denied.");
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Take a photo async with specified options
		/// </summary>
		/// <param name="options">Camera Media Options</param>
		/// <returns>Media file of photo or null if canceled</returns>
		public async Task<MediaFile> TakePhotoAsync (StoreCameraMediaOptions options)
		{
			if (!IsCameraAvailable)
				throw new NotSupportedException ();

			if (!(await RequestCameraPermission ().ConfigureAwait (false))) {
				return null;
			}


			VerifyOptions (options);

			return await TakeMediaAsync ("image/*", MediaStore.ActionImageCapture, options);
		}

		/// <summary>
		/// Picks a video from the default gallery
		/// </summary>
		/// <returns>Media file of video or null if canceled</returns>
		public async Task<MediaFile> PickVideoAsync ()
		{

			if (!(await RequestStoragePermission ().ConfigureAwait (false))) {
				return null;
			}

			return await TakeMediaAsync ("video/*", Intent.ActionPick, null);
		}

		/// <summary>
		/// Take a video with specified options
		/// </summary>
		/// <param name="options">Video Media Options</param>
		/// <returns>Media file of new video or null if canceled</returns>
		public async Task<MediaFile> TakeVideoAsync (StoreVideoOptions options)
		{
			if (!IsCameraAvailable)
				throw new NotSupportedException ();

			if (!(await RequestCameraPermission ().ConfigureAwait (false))) {
				return null;
			}

			VerifyOptions (options);

			return await TakeMediaAsync ("video/*", MediaStore.ActionVideoCapture, options);
		}

		private readonly Context context;
		private int requestId;
		private TaskCompletionSource<Plugin.Media.Abstractions.MediaFile> completionSource;

		private void VerifyOptions (StoreMediaOptions options)
		{
			if (options == null)
				throw new ArgumentNullException ("options");
			if (System.IO.Path.IsPathRooted (options.Directory))
				throw new ArgumentException ("options.Directory must be a relative path", "options");
		}

		private Intent CreateMediaIntent (int id, string type, string action, StoreMediaOptions options, bool tasked = true)
		{
			Intent pickerIntent = new Intent (this.context, typeof(MediaPickerActivity));
			pickerIntent.PutExtra (MediaPickerActivity.ExtraId, id);
			pickerIntent.PutExtra (MediaPickerActivity.ExtraType, type);
			pickerIntent.PutExtra (MediaPickerActivity.ExtraAction, action);
			pickerIntent.PutExtra (MediaPickerActivity.ExtraTasked, tasked);

			if (options != null) {
				pickerIntent.PutExtra (MediaPickerActivity.ExtraPath, options.Directory);
				pickerIntent.PutExtra (MediaStore.Images.ImageColumns.Title, options.Name);
		
				var cameraOptions = (options as StoreCameraMediaOptions);

				if (cameraOptions != null)
					pickerIntent.PutExtra (MediaPickerActivity.ExtraSaveToAlbum, cameraOptions.SaveToAlbum);

				var vidOptions = (options as StoreVideoOptions);
				if (vidOptions != null) {
					pickerIntent.PutExtra (MediaStore.ExtraDurationLimit, (int)vidOptions.DesiredLength.TotalSeconds);
					pickerIntent.PutExtra (MediaStore.ExtraVideoQuality, (int)vidOptions.Quality);
				}
			}
			//pickerIntent.SetFlags(ActivityFlags.ClearTop);
			pickerIntent.SetFlags (ActivityFlags.NewTask);
		
			return pickerIntent;
		}

		private int GetRequestId ()
		{
			int id = this.requestId;
			if (this.requestId == Int32.MaxValue)
				this.requestId = 0;
			else
				this.requestId++;

			return id;
		}

		private Task<Plugin.Media.Abstractions.MediaFile> TakeMediaAsync (string type, string action, StoreMediaOptions options)
		{
			int id = GetRequestId ();

			var ntcs = new TaskCompletionSource<Plugin.Media.Abstractions.MediaFile> (id);
			if (Interlocked.CompareExchange (ref this.completionSource, ntcs, null) != null)
				throw new InvalidOperationException ("Only one operation can be active at a time");



			this.context.StartActivity (CreateMediaIntent (id, type, action, options));

			EventHandler<MediaPickedEventArgs> handler = null;
			handler = async (s, e) => {
				var tcs = Interlocked.Exchange (ref this.completionSource, null);

				MediaPickerActivity.MediaPicked -= handler;

				if (e.RequestId != id)
					return;

				if (e.Error != null)
					tcs.SetResult (null);
				else if (e.IsCanceled)
					tcs.SetResult (null);
				else {
//					var wm = context. (Android.Content.Context.WindowService)
//						.JavaCast<IWindowManager> ();
//					var d = wm.DefaultDisplay;
//					Point size = new Point ();
//					d.GetSize (size);
//					int height = size.Y;
//					int width = size.X;

					await LoadAndResizeBitmap (e.Media.Path, 4096, 4096);

					tcs.SetResult (e.Media);
				}
			};

			MediaPickerActivity.MediaPicked += handler;

			return ntcs.Task;
		}

	
	
		private async Task<bool> LoadAndResizeBitmap (string fileName, int maxWidth, int maxHeight)
		{

			// First we get the the dimensions of the file on disk
			BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
			await BitmapFactory.DecodeFileAsync (fileName, options);

			// Next we calculate the ratio that we need to resize the image by
			// in order to fit the requested dimensions.
				
			int inSampleSize = CalculateInSampleSize (options, maxWidth, maxHeight);

			options.InSampleSize = inSampleSize;
			options.InJustDecodeBounds = false;
			//options.OutWidth = width;
			//options.OutHeight = height;
		
			Bitmap resizedBitmap = await BitmapFactory.DecodeFileAsync (fileName, options);
			int orientation = -1;
	
			ExifInterface exif = new ExifInterface (fileName);
			orientation = exif.GetAttributeInt (ExifInterface.TagOrientation, -1);
			//if it's -1 try to query the context for the orientation
			if (orientation == -1) {
				orientation = getOrientationFromMediaStore (Android.App.Application.Context, fileName);
			}

			var mediaOrientation = (Android.Media.Orientation)orientation;

			Matrix mtx = new Matrix ();
		 
			float scale = System.Math.Min (((float)maxHeight / options.OutWidth), ((float)maxWidth / options.OutHeight));
			mtx.PostScale (scale, scale);
	
			try {
				switch (mediaOrientation) {
				case Android.Media.Orientation.Undefined: // Nexus 7 landscape...
//					mtx.PreRotate (CrossMedia.Current.Orientation);
//					resizedBitmap = Bitmap.CreateBitmap (resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					break;
				case Android.Media.Orientation.Normal: // landscape
					break;
				case Android.Media.Orientation.FlipHorizontal:
					break;
				case Android.Media.Orientation.Rotate180:
					mtx.PreRotate (180);
					resizedBitmap = Bitmap.CreateBitmap (resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					break;
				case Android.Media.Orientation.FlipVertical:
					break;
				case Android.Media.Orientation.Transpose:
					break;
				case Android.Media.Orientation.Rotate90: // portrait
					mtx.PreRotate (90);
					resizedBitmap = Bitmap.CreateBitmap (resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					break;
				case Android.Media.Orientation.Transverse:
					break;
				case Android.Media.Orientation.Rotate270: // might need to flip horizontally too...
					mtx.PreRotate (270);
					resizedBitmap = Bitmap.CreateBitmap (resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					break;
				default:
//					mtx.PreRotate (CrossMedia.Current.Orientation);
//					resizedBitmap = Bitmap.CreateBitmap (resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					break;
				}
				mtx.Dispose ();
				mtx = null;
				using (FileStream f = new FileStream (fileName, FileMode.OpenOrCreate)) {
					resizedBitmap.Compress (Bitmap.CompressFormat.Jpeg, 75, f);
					f.Flush ();
				}
			} catch (System.Exception ex) {
				Log.Error ("resizing image", ex.Message);
				return false;
			} finally {
				resizedBitmap.Recycle ();
				resizedBitmap.Dispose ();
			}
			return true;
		
			//return resizedBitmap;
		}

	
		public int CalculateInSampleSize (BitmapFactory.Options options, int reqWidth, int reqHeight)
		{
			// Raw height and width of image
			float height = options.OutHeight;
			float width = options.OutWidth;
			double inSampleSize = 1D;

			if (height > reqHeight || width > reqWidth) {
				int halfHeight = (int)(height / 2);
				int halfWidth = (int)(width / 2);

				// Calculate a inSampleSize that is a power of 2 - the decoder will use a value that is a power of two anyway.
				while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth) {
					inSampleSize *= 2;
				}
			}

			return (int)inSampleSize;
		}

		private static int getOrientationFromMediaStore (Context context, System.String imagePath)
		{
			Android.Net.Uri imageUri = getImageContentUri (context, imagePath);
			if (imageUri == null) {
				return -1;
			}

			System.String[] projection = { MediaStore.Images.ImageColumns.Orientation };
			ICursor cursor = context.ContentResolver.Query (imageUri, projection, null, null, null);

			int orientation = -1;
			if (cursor != null && cursor.MoveToFirst ()) {
				orientation = cursor.GetInt (0);
				cursor.Close ();
			}

			return orientation;
		}

		private static Android.Net.Uri getImageContentUri (Context context, System.String imagePath)
		{
			System.String[] projection = new System.String[] { MediaStore.Images.Media.InterfaceConsts.Id };
			var selection = MediaStore.Images.Media.InterfaceConsts.Data + "=? ";
			System.String[] selectionArgs = new System.String[]{ imagePath };
			ICursor cursor = context.ContentResolver.Query (Android.Provider.MediaStore.Images.Media.ExternalContentUri, projection, 
				                 selection, selectionArgs, null);

			if (cursor != null && cursor.MoveToFirst ()) {
				int imageId = cursor.GetInt (0);
				cursor.Close ();

				return Android.Net.Uri.WithAppendedPath (Android.Provider.MediaStore.Images.Media.ExternalContentUri, Integer.ToString (imageId));
			} 

			if (new Java.IO.File (imagePath).Exists ()) {
				ContentValues values = new ContentValues ();
				values.Put (MediaStore.Images.Media.InterfaceConsts.Data, imagePath);

				return  context.ContentResolver.Insert (
					MediaStore.Images.Media.ExternalContentUri, values);
			} 

			return null;
		}
	}




}