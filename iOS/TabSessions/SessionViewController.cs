using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Diagnostics;
using System.Linq;
using MonoTouch.Twitter;
using MonoTouch.Social;
using MonoTouch.EventKit;

namespace Monospace11
{
	public class SessionViewController : WebViewControllerBase
	{
		public MonkeySpace.Core.Session DisplaySession;
		public bool IsFromFavoritesView = false;
		public SessionViewController (MonkeySpace.Core.Session session) : base()
		{
			DisplaySession = session;
		}
		public SessionViewController (MonkeySpace.Core.Session session, bool isFromFavs) : this (session)
		{
			IsFromFavoritesView = isFromFavs;
			DisplaySession = session;
		}
		public SessionViewController (string sessionCode) : base()
		{
			foreach (var s in MonkeySpace.Core.ConferenceManager.Sessions.Values.ToList () ) //AppDelegate.ConferenceData.Sessions)
			{
				if (s.Code == sessionCode)
				{	
					DisplaySession = s;
				}
			}
			
		}
		public void Update (string sessionCode) 
		{
			foreach (var s in MonkeySpace.Core.ConferenceManager.Sessions.Values.ToList () ) //AppDelegate.ConferenceData.Sessions)
			{
				if (s.Code == sessionCode)
				{
					DisplaySession = s;
				}
			}
			if (DisplaySession != null) LoadHtmlString(FormatText()); 
		}

		public void Update (MonkeySpace.Core.Session session) 
		{
			DisplaySession = session;
			LoadHtmlString(FormatText());
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			webView.ShouldStartLoad = delegate (UIWebView webViewParam, NSUrlRequest request, UIWebViewNavigationType navigationType)
			{	// Catch the link click, and process the add/remove favorites
				if (navigationType == UIWebViewNavigationType.LinkClicked)
				{
					string path = request.Url.Path.Substring(1);
					string host = request.Url.Host.ToLower ();
					if (host == "tweet.mix10.app") {
						var tweet = new TWTweetComposeViewController();
						tweet.SetInitialText ("I'm in '" + DisplaySession.Title + "' at #monkeyspace" );
						PresentModalViewController(tweet, true);
					} else if (host == "social.mix10.app") {
						var message = "I'm in '" + DisplaySession.Title + "' at #monkeyspace";
						var social = new UIActivityViewController(new NSObject[] { new NSString(message) }, 
																  new UIActivity[] { new UIActivity() });
						PresentViewController(social, true, null);
					} else if (host == "add.mix10.app") {
						AppDelegate.UserData.AddFavoriteSession(path);
						App.Current.EventStore.RequestAccess (EKEntityType.Reminder, (bool granted, NSError e) => {
							if (granted) {
								
								EKReminder reminder = EKReminder.Create (App.Current.EventStore); 
								reminder.Title = DisplaySession.Title;
								reminder.Calendar = App.Current.EventStore.DefaultCalendarForNewReminders;
								reminder.Notes = DisplaySession.Abstract;
								
								var date = new NSDateComponents();
								date.Day = DisplaySession.Start.Day;
								date.Month = DisplaySession.Start.Month;
								date.Year = DisplaySession.Start.Year;
								date.Hour = DisplaySession.Start.Hour;
								date.Minute = DisplaySession.Start.Minute;
								date.Second = DisplaySession.Start.Second;
								
								reminder.StartDateComponents = date;
								reminder.DueDateComponents = date;
								
								App.Current.EventStore.SaveReminder (reminder, true, out e);
							} else
								new UIAlertView ( "Access Denied", "User Denied Access to Calendar Data", null, "OK", null).Show ();
						});
						Update(DisplaySession);
					}
					else if (host == "remove.mix10.app")
					{	// "remove.MIX10.app"
						AppDelegate.UserData.RemoveFavoriteSession(path);
						if (IsFromFavoritesView)
						{	// once unfavorited, hide and go back to list view
							NavigationController.PopViewControllerAnimated(true);
						}
						else
						{
							Update(DisplaySession);
						}
					}
					else
					{
						NavigationController.PushViewController (new WebViewController (request), true);
						return false;
					}
				}
				return true;
			};
		}

		protected override string FormatText ()
		{
			string timeFormat = "H:mm";
			StringBuilder sb = new StringBuilder ();
			sb.Append (StyleHtmlSnippet);
			sb.Append ("<h2>" + DisplaySession.Title + "</h2>" + Environment.NewLine);

			if (AppDelegate.UserData.IsFavorite(DisplaySession.Code))
			{	// okay this is a little bit of a HACK:
				sb.Append(@"<p><nobr><a href=""http://remove.mix10.app/"+DisplaySession.Code+@"""><img src='Images/favorited.png' align='right' border='0'/></a></nobr></p>");
			}
			else {
				sb.Append(@"<p><nobr><a href=""http://add.mix10.app/"+DisplaySession.Code+@"""><img src='Images/favorite.png' align='right' border='0'/></a></nobr></p>");
			}
			sb.Append("<br/>");
			if (DisplaySession.Speakers.Count > 0)
			{
				sb.Append("<span class='sessionspeaker'>"+DisplaySession.GetSpeakerList() +"</span> "+ Environment.NewLine);
				sb.Append("<br/>");
			}

			sb.Append("<span class='sessiontime'>"
					+ DisplaySession.Start.ToString("ddd MMM dd") + " " 
					+ DisplaySession.Start.ToString(timeFormat)+" - " 
					+ DisplaySession.End.ToString(timeFormat) +"</span><br />"+ Environment.NewLine);

			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) {
				sb.Append ("<a href='http://social.mix10.app/' style='font-weight:normal'><img height=22 width=58 align='right' src='Images/Social.png'></a>");
			} else if (TWTweetComposeViewController.CanSendTweet) {
				sb.Append ("<a href='http://tweet.mix10.app/' style='font-weight:normal'><img height=22 width=58 align='right' src='Images/Tweet.png'></a>");
			}
			if (!String.IsNullOrEmpty (DisplaySession.Location))
			{
				sb.Append("<span class='sessionroom'>"+DisplaySession.LocationDisplay+"</span><br />"+ Environment.NewLine);
				sb.Append("<br />"+ Environment.NewLine);
			}
			sb.Append("<span class='body'>"+DisplaySession.Abstract+"</span>"+ Environment.NewLine);
				
			return sb.ToString();
		}
	}
}