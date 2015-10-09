using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

using Google.GData.Extensions;
using Google.GData.Calendar;

namespace pgcal.Core
{
	public class GCalFramework {
		public const string GCalURI = "https://www.google.com/calendar/feeds/default/private/full";
		public const string EtqGoogleGmailAddress = "gmail.com";
		public const string EtqNotAvailable = "n/a";
		public const string EtqHourMark = "h";
		
		public GCalFramework(string usr, string psw)
		{
			this.OnTransaction = false;
			this.SetUsrInfo( usr, psw );
			this.currentDate = DateTime.Now.AddYears( -50 );
		}

		/// <summary>
		/// Sets the usr info, preparing strings. Usr is trunk if @- added.
		/// </summary>
		/// <param name='login'>
		/// The user login.
		/// </param>
		/// <param name='password'>
		/// The password.
		/// </param>
		protected void SetUsrInfo(String login, String password)
		{
			// Prepare parameters
			if ( login == null ) {
				login = string.Empty;
			}

			if ( password == null ) {
				login = string.Empty;
			}

			// Prepare psw
			this.psw = password.Trim();

			// Prepare usr
			login = login.Trim();
			if ( login.Length == 0 ) {
				throw new FormatException( "User field cannot be empty" );
			}
			
			int posAtSign = login.IndexOf( '@' );
			if ( posAtSign > -1
			  && login.EndsWith( EtqGoogleGmailAddress ) )
			{
				login = login.Substring( 0, posAtSign );
			}

			this.usr = login;
		}
		
		protected static bool Validator(object sender, X509Certificate certificate, X509Chain chain, 
                                      SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
		
		/// <summary>
		/// Inserts the event in the calendar.
		/// </summary>
		public void InsertEvent(EventEntry entry)
		{
			var service = new CalendarService( AppInfo.Name );

            if ( this.IsUsrSet ) {
                service.setUserCredentials( this.Usr, this.Psw );
				service.Insert( new Uri( GCalURI ), entry );
			}
			
			return;
		}
		
		/// <summary>
		/// Inserts a new event
		/// </summary>
		/// <param name='start'>
		/// The start, date and time, of the event, as a DateTime object.
		/// </param>
		/// <param name='title'>
		/// The title of the event.
		/// </param>
		/// <param name='end'>
		/// The end, date and time, of the event, as a DateTime object.
		/// </param>
		/// <param name='place'>
		/// The place in which the event will happen.
		/// </param>
		public void InsertEvent(DateTime start, string title, DateTime end, string place)
		{
			// Wait to have a slot
			while( this.OnTransaction ) {}
			
			// Prepare the entry
			this.OnTransaction = true;
			var entry = new EventEntry();

			// Set the title and content of the entry.
			entry.Title.Text = title;
			
			// Set a location for the event.
			Where eventLocation = new Where();
			eventLocation.ValueString = place;
			entry.Locations.Add( eventLocation );
			
			// Set when it happens
			When eventTime = new When( start, end );
			entry.Times.Add( eventTime );
			
			// Really insert
			this.InsertEvent( entry );
			this.OnTransaction = false;
			return;
		}
		
		/// <summary>
		/// Deletes the event.
		/// </summary>
		/// <param name='index'>
		/// The index of the event.
		/// </param>
		public void DeleteEvent(int index)
		{
			// Wait to have a slot
			while( this.OnTransaction ) {}
			
			// Prepare the entry
			this.OnTransaction = true;
			var entry = this.CurrentEvents[ index ];
			
			// Remove from the list of events
			EventEntry[] newListOfEntries = new EventEntry[ this.CurrentEvents.Length -1 ];
			int j = 0;
			for(int i = 0; i < newListOfEntries.Length; ++i) {
				if ( j != index ) {
					newListOfEntries[ i ] = this.CurrentEvents[ j ];
				} else {
					// Pass the element to delete
					--i;
				}
				
				++j;
			}
			
			// Delete it
			this.currentEvents = newListOfEntries;
			entry.Delete();
			
			// End
			this.OnTransaction = false;
			return;
		}
		
		/// <summary>
		/// Updates a given event.
		/// </summary>
		/// <param name='index'>
		/// The number of the entry in the list of entries.
		/// </param>
		/// <param name='entry'>
		/// The entry itself, with the modifications already done.
		/// </param>
		public void ModifyEvent(int index, EventEntry entry)
		{
			entry.Update();
		}
		
		
		/// <summary>
		/// Mandatorily refreshes the feed of events.
		/// Uses currentmonth, so it should be always called after a normal
		/// operation of RefreshFeed, or it will retrieve very oldfashioned events
		/// </summary>
		/// <returns>
		/// A primitive vector of EventEntry object, which is also the new
		/// feed of entries of the object.
		/// </returns>
		public EventEntry[] RefreshFeed() 
        {
			return this.RefreshFeed( this.CurrentDate, true );
		}

		/// <summary>
		/// Refreshes the current entries for this month.
		/// Does only really retrieve if it is not in this month.
		/// </summary>
		/// <returns>
		/// A primitive vector of EventEntry object, which is also the new
		/// feed of entries of the object.
		/// </returns>
		/// <param name='time'>
		/// The time the events should be retrieved for.
		/// </param>
		public EventEntry[] RefreshFeed(DateTime time, bool forced)
        {
            if ( this.NeedsUpdateFor( time )
			  || forced )
			{
                ServicePointManager.ServerCertificateValidationCallback = Validator;
                EventQuery query = new EventQuery();
                var service = new CalendarService( AppInfo.Name );
                var toret = new List<EventEntry>();
				
                while ( this.OnTransaction ) {
                }
				
                this.OnTransaction = true;
				
                if ( this.IsUsrSet ) {
                    service.setUserCredentials( this.Usr, this.Psw );
	
                    // only get event's for this month
                    this.currentDate = new DateTime( time.Year, time.Month, 1 );
                    query.Uri = new Uri( GCalURI );
                    query.StartTime = this.CurrentDate;
                    query.EndTime = new DateTime(
						time.Year, time.Month, DateTime.DaysInMonth( time.Year, time.Month )
					);
                    query.EndTime = query.EndTime.AddDays( 1 );
		
                    // create the list of events
                    var calFeed = ( service.Query( query ) as EventFeed );
                    while ( calFeed != null
						 && calFeed.Entries.Count > 0 ) {
                        foreach (EventEntry entry in calFeed.Entries) {
                            InsertOrdered( toret, entry );
                        }
						
                        // just query the same query again.
                        if ( calFeed.NextChunk != null ) {
                            query.Uri = new Uri( calFeed.NextChunk ); 
                            calFeed = service.Query( query ) as EventFeed;
                        } else {
                            calFeed = null;
                        }
                    }
					
                    // Update current entries record
                    this.currentEvents = toret.ToArray();
                } else {
                    throw new Exception( "User field cannot be empty" );
                }
				
                this.OnTransaction = false;
            }
		
            return this.CurrentEvents;
        }
		
		/// <summary>
		/// Locates the event for the given time in current events.
		/// </summary>
		/// <returns>
		/// The event index in current events.
		/// </returns>
		/// <param name='time'>
		/// The time the first event should present.
		/// </param>
		public int LocateNextEventInCurrentEvents(DateTime time)
		{
			bool exactMatch;
			return this.LocateNextEventInCurrentEvents( time, out exactMatch );
		}
		
		/// <summary>
		/// Locates the event for the given time in current events.
		/// It informs whether the time found matches in date.
		/// </summary>
		/// <returns>
		/// The event index in current events.
		/// </returns>
		/// <param name='time'>
		/// The time the first event should present.
		/// </param>
		/// <param name='exactDateMatch'>
		/// Returns true if the date of the time it was looked matches the date of the event found.
		/// </param>
		public int LocateNextEventInCurrentEvents(DateTime time, out bool exactDateMatch)
		{
			exactDateMatch = false;
			int toret = -1;
			
			if ( this.CurrentEvents != null ) {
				for(int i = 0; i < this.CurrentEvents.Length; ++i) {
					EventEntry entry = this.CurrentEvents[ i ];
					
					if ( entry.Times.Count > 0 ) {
						if ( entry.Times[ 0 ].StartTime >= time )
						{
							toret = i;
							exactDateMatch = ( entry.Times[ 0 ].StartTime.Date == time.Date );
							break;
						}
					}
				}
			}
			
			return toret;
		}
		
		/// <summary>
		/// Inserts an event, ordered in the list.
		/// </summary>
		/// <param name='l'>
		/// The list of events.
		/// </param>
		/// <param name='entry'>
		/// The event to insert in the list.
		/// </param>
		protected static void InsertOrdered(List<EventEntry> l, EventEntry entry)
		{
			int pos = 0;
			
			if ( entry.Times.Count > 0 ) {
				// Look for the correct place
				for(; pos < l.Count; ++pos) {
					var lstEvt = l[ pos ];
					
					if ( lstEvt.Times.Count > 0 ) {
						if ( lstEvt.Times[ 0 ].StartTime > entry.Times[ 0 ].StartTime ) {
							break;
						}
					}
				}
			}
					
			// Now insert it
			l.Insert( pos, entry );
			return;
		}
		
		/// <summary>
		/// Determines whether the framework needs updating
		/// </summary>
		/// <returns>
		/// true when there is need to communicate with google for that time, false otherwise
		/// </returns>
		/// <param name='time'>
		/// The date/time to consider.
		/// </param>
		public bool NeedsUpdateFor(DateTime time)
		{
			return ( this.CurrentDate != new DateTime( time.Year, time.Month, 1 ) );
		}

		/// <summary>
		/// Gets or sets the user.
		/// </summary>
		/// <value>
		/// The user, as a string.
		/// </value>
		public string Usr {
			get { return usr; }
		}
		
		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>
		/// The psw, as a string.
		/// </value>
		public string Psw {
			get { return psw; }
		}
		
		/// <summary>
		/// Gets or sets the current month.
		/// When setting, it automatically calls to RefreshFeed
		/// </summary>
		/// <value>
		/// The current month, as a DateTime object.
		/// </value>
		public DateTime CurrentDate {
            get {
                return this.currentDate;
            }
            set {
                this.currentDate = value;
                this.RefreshFeed( this.currentDate, false );
            }
        }
		
		/// <summary>
		/// Gets all current entries.
		/// </summary>
		/// <value>
		/// The current entries, as a vector of EventEntry objects.
		/// </value>
		public EventEntry[] CurrentEvents {
			get {
				return this.currentEvents;
			}
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether there is a transaction going on.
		/// </summary>
		/// <value>
		/// <c>true</c> if on transaction; otherwise, <c>false</c>.
		/// </value>
		public bool OnTransaction {
			get; set;
		}

		/// <summary>
		/// Gets a value indicating whether the usr is set.
		/// </summary>
		/// <value>
		/// <c>true</c> if the usr login is iset; otherwise, <c>false</c>.
		/// </value>
		public bool IsUsrSet {
			get {
				return ( this.Usr != null && this.Usr.Length > 0 );
			}
		}
		
		private string usr;
		private string psw;
		private DateTime currentDate;
		private EventEntry[] currentEvents = null;
	}
}
