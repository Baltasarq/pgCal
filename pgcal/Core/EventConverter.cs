// EventConverter.cs

using System;
using System.Text;

using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Calendar;

namespace pgcal.Core {
	/// <summary>
	/// Converts events to other formats.
	/// </summary>
	public class EventConverter
	{
		/// <summary>
		/// Template for HTML events.
		/// </summary>
		public const string HtmlDocument =
			"<html><head>"
				+ "<meta charset=\"UTF-8\">\n"
				+ "<title>{0}</title></head>"
				+ "\n<body><h1>&bull; {0}</h1>"
				+ "\n<p><h2>&gt; {1} - {2}</h2></p>"
				+ "\n<p><h3>&Oslash; {3}</h3></p>"
				+ "\n<p><h4>@ {4}</h4></p>"
				+ "\n<p><h4>&sect;</h4></p>"
			+ "\n</body></html>";

		/// <summary>
		/// Initializes a new instance of the <see cref="pgcal.Core.EventConverter"/> class.
		/// </summary>
		/// <param name="gcal">the google calendar user framework,
		/// as an GCalFramework object</param>
		/// <param name="evt">the event, as an EventEntry object</param>
		public EventConverter(GCalFramework gcal, EventEntry evt)
		{
			this.place = "";
			this.gcal = gcal;
			this.evt = evt;
		}

		/// <summary>
		/// Converts to VEvent.
		/// </summary>
		/// <returns>The to VEvent contents, as  a string.</returns>
		public string ConvertToVEvent()
		{
			string id = evt.EventId + "@" + "pgcal.baltasarq.info";
			var toret = new StringBuilder();
			string dateFmt = "{0:yyyyMMddTHHmmsszz}\n";
			
			// Format it
			toret.AppendLine( "BEGIN:VCALENDAR" );
			toret.AppendLine( "VERSION:2.0" );
			toret.AppendLine( "PRODID:-//dev.baltasarq.info//NONSGML pgcal//EN" );
			toret.AppendLine( "BEGIN:VEVENT" );
			toret.AppendLine( "UID:" + id );
			toret.Append( "DTSTAMP:" );
			toret.AppendFormat( dateFmt, this.Start );
			toret.AppendFormat( "ORGANIZER;CN={0}:MAILTO:{0}@gmail.com\n", this.GCal.Usr );
			toret.Append( "DTSART:" );
			toret.AppendFormat( dateFmt, this.Start );
			toret.Append( "DTEND:" );
			toret.AppendFormat( dateFmt, this.End );
			toret.AppendLine( "SUMMARY:" + evt.Title.Text );
			toret.AppendLine( "END:VEVENT" );
			toret.AppendLine( "END:VCALENDAR" );

			return toret.ToString();
		}

		/// <summary>
		/// Converts to html.
		/// </summary>
		/// <returns>The html page, as a string.</returns>
		public string ConvertToHtml()
		{
			string toret = string.Format( HtmlDocument,
			                              this.Event.Title.Text,
			                              this.Start,
			                              this.End,
			                              this.Place,
			                              this.GCal.Usr );

			return toret;
		}

		/// <summary>
		/// Returns the event to be converted
		/// </summary>
		/// <value>The event.</value>
		public EventEntry Event {
			get { return this.evt; }
		}

		/// <summary>
		/// Returns the google framework
		/// </summary>
		/// <value>The G cal.</value>
		public GCalFramework GCal {
			get { return this.gcal; }
		}

		/// <summary>
		/// Gets the start time for the event.
		/// </summary>
		/// <value>The start, as a DateTime object.</value>
		public DateTime Start {
			get {
				DateTime toret = DateTime.Now;

				if ( this.Event.Times.Count > 0 ) {
					toret = evt.Times[ 0 ].StartTime;
				}

				return toret;
			}
		}

		/// <summary>
		/// Gets the end time for the event
		/// </summary>
		/// <value>The end, as a DateTime object.</value>
		public DateTime End {
			get {
				DateTime toret = DateTime.Now;

				if ( this.Event.Times.Count > 0 ) {
					return evt.Times[ 0 ].EndTime;
				}

				return toret;
			}
		}

		/// <summary>
		/// Gets the place the event is going to happen in.
		/// </summary>
		/// <value>The place, as string</value>
		public string Place {
			get {
				if ( this.place.Length == 0 ) {
					if ( this.Event.Locations.Count > 0 ) {
						Where place = this.Event.Locations[ 0 ];
						if ( place != null ) {
							this.place = place.ValueString;
						}
					} else {
						this.place = GCalFramework.EtqNotAvailable;
					}
				}

				return this.place;
			}
		}
		      
		private GCalFramework gcal;
		private EventEntry evt;
		private string place;
	}
}

