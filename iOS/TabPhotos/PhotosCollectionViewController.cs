using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Threading;
using MonoTouch.Dialog.Utilities;

namespace Monospace11
{
	public class PhotosCollectionViewController : UICollectionViewController
	{
		static NSString cellId = new NSString ("ImageCell");
		List<string> imageUrls;
		List<string> largeImageUrls;
		Bing bing;
		static NSString headerId = new NSString ("Header");
		
		public PhotosCollectionViewController (UICollectionViewLayout layout) : base (layout)
		{
			imageUrls = new List<string> ();
			
			bing = new Bing ((results) => {
				InvokeOnMainThread (delegate {   
					imageUrls = results[0];
					largeImageUrls = results[1];
					CollectionView.ReloadData ();
				});
			});
			
			bing.ImageSearch ();
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			CollectionView.RegisterClassForCell (typeof(ImageCell), cellId);
			CollectionView.RegisterClassForSupplementaryView (typeof(Header), UICollectionElementKindSection.Header, headerId);
		}
		
		
		public override int GetItemsCount (UICollectionView collectionView, int section)
		{
			return imageUrls.Count;
		}
		
		public override UICollectionViewCell GetCell (UICollectionView collectionView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var imageCell = (ImageCell)collectionView.DequeueReusableCell (cellId, indexPath);
			
			string imageUrl = imageUrls [indexPath.Row].Replace ("\"", "");
			
			imageCell.UpdateCell (new Uri (imageUrl));
			
			return imageCell;
		}
		
		public override void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath)
		{
			//base.ItemSelected (collectionView, indexPath);
			
			var viewController = new UIViewController();
			var imageView = new MyImageView(viewController.View.Bounds);
			string imageUrl = largeImageUrls [indexPath.Row].Replace ("\"", "");
			imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			imageView.BackgroundColor = UIColor.Black;
			imageView.UpdatedImage(new Uri (imageUrl));
			viewController.View.AddSubview(imageView);
			NavigationController.PushViewController (viewController, true);
		}
		
		public override UICollectionReusableView GetViewForSupplementaryElement (UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
		{
			var headerView = (Header) collectionView.DequeueReusableSupplementaryView (elementKind, headerId, indexPath);
			headerView.Center = collectionView.Center;
			return headerView;
		}
		
	}
	
	public class Header : UICollectionReusableView
	{
		UIImageView imageView;
		
		[Export ("initWithFrame:")]
		public Header (System.Drawing.RectangleF frame) : base (frame)
		{
			imageView = new UIImageView (new RectangleF(0f, 0f, 300f, 70f));
			imageView.Image = UIImage.FromFile("logo.png");
			AddSubview (imageView);
		}
	}
	
	public class MyImageView : UIImageView, IImageUpdated
	{
		public MyImageView (RectangleF frame) : base (frame)
		{
		}
		
		public void UpdateCell (Uri uri)
		{
			Console.WriteLine ("Update Cell");
			Image = ImageLoader.DefaultRequestImage (uri, this);
		}
		
		public void UpdatedImage (Uri uri)
		{
			Console.WriteLine ("Updated Image");
			
			Image = ImageLoader.DefaultRequestImage (uri, this);
		}
	}
	
	public class ImageCell : UICollectionViewCell, IImageUpdated
	{
		UIImageView imageView;
		
		[Export ("initWithFrame:")]
		public ImageCell (System.Drawing.RectangleF frame) : base (frame)
		{
			imageView = new UIImageView (new RectangleF (0, 0, 50, 50)); 
			imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			ContentView.AddSubview (imageView);
		}
		
		public void UpdateCell (Uri uri)
		{
			imageView.Image = ImageLoader.DefaultRequestImage (uri, this);
		}
		
		public void UpdatedImage (Uri uri)
		{
			imageView.Image = ImageLoader.DefaultRequestImage (uri, this);
		}
	}
	
}

