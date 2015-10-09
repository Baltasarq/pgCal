using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace pgcal.Gui {
	/// <summary>
	/// Date changed event arguments.
	/// </summary>
	public class DateChangedEventArgs: EventArgs {
		public DateChangedEventArgs(DateTime dt)
		{
			this.Date = dt;
		}
		
		public DateTime Date {
			get; set;
		}
	}
	
	public class CalendarView: Panel {
		public const int MaxDaysOfWeek = 7;

		public CalendarView()
			: this( SystemFonts.MessageBoxFont )
		{
		}

		public CalendarView(Font font)
		{
			this.Font = new Font( font, font.Style );
			this.Build();

			this.ImportantDaysBgColor = Color.Wheat;
			this.ImportantDaysFgColor = Color.Blue;
			this.HolidaysFgColor = Color.Red;
			this.HolidaysBgColor = Color.AntiqueWhite;
			this.DisabledBgColor = Color.Gray;
			this.disabledStyle.BackColor = this.DisabledBgColor;
			this.currentDate = DateTime.Now;
		}
		
		/// <summary>
		/// Cell position inside the calendar
		/// </summary>
		public class CellPosition {
			public CellPosition(int x = 0, int y = 0)
			{
				Row = x;
				Col = y;
			}
			
			public int Row {
				get; set;
			}
			
			public int Col {
				get; set;
			}

			public override string ToString()
			{
				return string.Format( "{0}, {1}", this.Row, this.Col );
			}
		}

		public static void BuildHeaders(DateTime date)
		{
			var headers = new string[ MaxDaysOfWeek ];
			
			for(int i = 0; i < MaxDaysOfWeek; ++i) {
				int pos = ( (int) date.DayOfWeek ) -1;
	
				if ( pos < 0 ) {
					pos = 6;
				}
				
				headers[ pos ] = date.ToString( "ddd" );
				date = date.AddDays( 1 );
			}
			
			Headers = new ReadOnlyCollection<string>( headers );
			
		}
		
		protected void UpdateDateInfo(DateTime dt)
		{
			
			var fmtDate = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			
            this.lblCurrentDate.Hide();
			this.lblCurrentDate.Text = dt.ToString( fmtDate );
            this.lblCurrentDate.Show();
		}
		
		public delegate void DateChangedHandler(object sender, DateChangedEventArgs evt);
		
		public DateChangedHandler DateChanged = null;
		public DateChangedHandler MonthChanged = null;
		
		public void DateChangedManager(object sender, DataGridViewCellEventArgs e)
		{
			try {
				int day = this.GetDayForCellPosition( e.RowIndex, e.ColumnIndex );
				this.currentDate = new DateTime( this.CurrentDate.Year, this.CurrentDate.Month, day );
				this.UpdateDateInfo( this.currentDate );
				
				if ( DateChanged != null ) {
					this.DateChanged( this, new DateChangedEventArgs( this.currentDate ) );
				}
			}
			catch(ArgumentException)
			{
				// Do nothing. The user clicked and invalid cell.
			}
			
			return;
		}
		
		public void MonthChangedManager(object sender, EventArgs args)
		{
			try {
				// Decide which button was selected and set current date
				if ( sender == this.btPreviousMonth ) {
					this.currentDate = this.CurrentDate.AddMonths( -1 );
				} else {
					this.currentDate = this.CurrentDate.AddMonths( 1 );
				}
				
				// Update calendar
				this.FillDaysGrid();
				
				if ( MonthChanged != null ) {
					this.MonthChanged( this, new DateChangedEventArgs( this.currentDate ) );
				}
				
				if ( DateChanged != null ) {
					this.DateChanged( this, new DateChangedEventArgs( this.currentDate ) );
				}
			} catch(ArgumentException)
			{
				// Do nothing. The user clicked and invalid cell.
			}
			
			return;
		}
		
		protected void SyncSelectedDay()
		{
			CellPosition pos = this.GetCellPositionForDay( this.CurrentDate.Day );
			this.grdDayGrid.ClearSelection();
			this.grdDayGrid.CurrentCell = this.grdDayGrid[ pos.Col, pos.Row ];
			
			return;
		}
		
		protected void Build()
		{
			Graphics grf = this.CreateGraphics();
			SizeF fontSize = grf.MeasureString( "W", this.Font );
			int widthNeeded = ( (int) fontSize.Width ) + 25;
            int heightNeeded = ( (int) fontSize.Width ) + 5;

			BuildHeaders( DateTime.Now );
			
			
			// Start preparing the calendar view
			this.SuspendLayout();
			this.Controls.Clear();
			this.grdDayGrid = new DataGridView();
			
			// Prepare grid
            this.grdDayGrid.Font = new Font( this.Font, this.Font.Style );
            this.grdDayGrid.Dock = DockStyle.Fill;
            this.grdDayGrid.AutoGenerateColumns = false;
            this.grdDayGrid.MultiSelect = false;
            this.grdDayGrid.ReadOnly = true;
            this.grdDayGrid.RowHeadersVisible = false;
            this.grdDayGrid.RowTemplate.Height = heightNeeded;
            this.grdDayGrid.RowTemplate.MinimumHeight = heightNeeded;
			this.grdDayGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.grdDayGrid.ColumnHeadersHeight = heightNeeded;
			this.grdDayGrid.AllowUserToAddRows = false;
			this.grdDayGrid.AllowUserToDeleteRows = false;
			this.grdDayGrid.AllowDrop = false;
			this.grdDayGrid.AllowUserToOrderColumns = false;
			this.grdDayGrid.AllowUserToResizeColumns = false;
			this.grdDayGrid.AllowUserToResizeRows = false;
			this.grdDayGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            var textCellTemplate = new DataGridViewTextBoxCell();
			textCellTemplate.Style.BackColor = Color.White;
			textCellTemplate.Style.Font = new Font( this.Font, this.Font.Style );
			this.disabledStyle = new DataGridViewCellStyle( textCellTemplate.Style );
			this.grdDayGrid.CellClick += DateChangedManager;
			
			// Prepare columns in grid
			var column0 = new DataGridViewTextBoxColumn();
            var column1 = new DataGridViewTextBoxColumn();
            var column2 = new DataGridViewTextBoxColumn();
            var column3 = new DataGridViewTextBoxColumn();
            var column4 = new DataGridViewTextBoxColumn();
            var column5 = new DataGridViewTextBoxColumn();
            var column6 = new DataGridViewTextBoxColumn();
					
			column0.CellTemplate = textCellTemplate;
            column1.CellTemplate = textCellTemplate;
            column2.CellTemplate = textCellTemplate;
            column3.CellTemplate = textCellTemplate;
            column4.CellTemplate = textCellTemplate;
            column5.CellTemplate = textCellTemplate;
            column6.CellTemplate = textCellTemplate;
			
            column0.HeaderText = Headers[ 0 ];
			column0.HeaderCell.Style.Font = this.Font;
            column0.HeaderCell.ToolTipText = Headers[ 0 ];
            column0.Width = widthNeeded;
            column0.SortMode = DataGridViewColumnSortMode.NotSortable;
            column1.HeaderText = Headers[ 1 ];
            column1.HeaderCell.ToolTipText = Headers[ 1 ];
			column1.HeaderCell.Style.Font = this.Font;
            column1.Width = widthNeeded;
            column1.SortMode = DataGridViewColumnSortMode.NotSortable;
			column2.HeaderCell.Style.Font = this.Font;
            column2.HeaderText = Headers[ 2 ];
            column2.HeaderCell.ToolTipText = Headers[ 2 ];
            column2.Width = widthNeeded;
            column2.SortMode = DataGridViewColumnSortMode.NotSortable;
            column3.HeaderText = Headers[ 3 ];
            column3.HeaderCell.ToolTipText = Headers[ 3 ];
			column3.HeaderCell.Style.Font = this.Font;
            column3.Width = widthNeeded;
            column3.SortMode = DataGridViewColumnSortMode.NotSortable;
            column4.HeaderText = Headers[ 4 ];
            column4.HeaderCell.ToolTipText = Headers[ 4 ];
			column4.HeaderCell.Style.Font = this.Font;
            column4.Width = widthNeeded;
            column4.SortMode = DataGridViewColumnSortMode.NotSortable;
            column5.HeaderText = Headers[ 5 ];
            column5.HeaderCell.ToolTipText = Headers[ 5 ];
			column5.HeaderCell.Style.Font = this.Font;
			column5.Width = widthNeeded;
            column5.SortMode = DataGridViewColumnSortMode.NotSortable;
            column6.HeaderText = Headers[ 6 ];
            column6.HeaderCell.ToolTipText = Headers[ 6 ];
			column6.HeaderCell.Style.Font = this.Font;
            column6.Width = widthNeeded;
            column6.SortMode = DataGridViewColumnSortMode.NotSortable;
			
			this.grdDayGrid.Columns.Clear();
            this.grdDayGrid.Columns.AddRange( new DataGridViewColumn[] {
                column0,
				column1,
				column2,
				column3,
				column4, 
				column5,
				column6
		    	} );
			
			// Prepare title bar over calendar (prev, date, next)
			var pnlTitle = new Panel();
			pnlTitle.SuspendLayout();

			this.lblCurrentDate = new Label();
			this.lblCurrentDate.Font = new Font( this.Font, FontStyle.Bold );
			this.lblCurrentDate.Text = this.CurrentDate.ToString( "yyyy-mm-dd" );
			this.lblCurrentDate.TextAlign = ContentAlignment.MiddleCenter;
			this.lblCurrentDate.Dock = DockStyle.Top;
			this.lblCurrentDate.Height = (int) ( this.lblCurrentDate.Height * 1.5 );
			this.btNextMonth = new Button();
			this.btNextMonth.Dock = DockStyle.Right;
			this.btPreviousMonth = new Button();
			this.btPreviousMonth.Dock = DockStyle.Left;
			this.btNextMonth.Text = ">";
			this.btPreviousMonth.Text = "<";
			this.btNextMonth.Click += MonthChangedManager;
			this.btPreviousMonth.Click += MonthChangedManager;
			pnlTitle.MinimumSize = new Size( this.grdDayGrid.Width, this.lblCurrentDate.Height +5 );
            pnlTitle.MaximumSize = new Size( this.grdDayGrid.Width + 100000, this.lblCurrentDate.Height +5 );
			
			
			// Finish
			pnlTitle.Controls.Add( this.lblCurrentDate );
			pnlTitle.Controls.Add( this.btNextMonth );
			pnlTitle.Controls.Add( this.btPreviousMonth );
			pnlTitle.Dock = DockStyle.Top;
			this.Controls.Add( this.grdDayGrid );
			this.Controls.Add( pnlTitle );
			this.Dock = DockStyle.Fill;
			pnlTitle.ResumeLayout( false );
			this.MinimumSize = new Size( this.grdDayGrid.Width, heightNeeded * 10 );
			this.ResumeLayout( false );
		}

		public DateTime GetFirstDay(DateTime t)
		{
			return new DateTime( t.Year, t.Month, 1 );
		}
		
		public DateTime GetLastDay(DateTime t)
		{
			return new DateTime( t.Year, t.Month, 1 ).AddMonths( 1 ).AddDays( -1 );
		}
		
		public DateTime LastDay {
			get { return this.GetLastDay( this.CurrentDate ); }
		}
		
		public DateTime FirstDay {
			get { return this.GetFirstDay( this.CurrentDate ); }
		}
		
		public int FirstDayShift {
			get { return ( (int) FirstDay.DayOfWeek ) - ( (int) Day.Monday ); }
		}
		
		public int FirstDayPosition {
			get {
				int toret = this.FirstDayShift -1;
				
				if ( toret < 0 ) {
					toret += MaxDaysOfWeek;
				}
				
				return toret;
			}
		}
		
		/// <summary>
		/// Fills the calendar with all days
		/// </summary>
		protected void FillDaysGrid()
		{
			var importantDays = new List<int>( this.importantDaysOfMonth );
			var columnData = new string[ MaxDaysOfWeek ];
			
			// Prepare
			int numDays = this.LastDay.Day;
			int numDay = 1;
			this.grdDayGrid.Hide();

			// Actually fill the cells with the numbers for each day
			this.grdDayGrid.Rows.Clear();
			while( numDay <= numDays ) {
				this.grdDayGrid.Rows.Add( columnData );
				int lastRow = this.grdDayGrid.Rows.Count -1;
				int firstPos = 0;
				
				if ( lastRow == 0 ) {
					firstPos = this.FirstDayPosition;
				}
				
				for(int numCell = firstPos; numCell < MaxDaysOfWeek; ++numCell) {
					var cell = this.grdDayGrid.Rows[ lastRow ].Cells[ numCell ];

					if ( numDay <= numDays ) {
						cell.Value = numDay.ToString().PadLeft( 3 );
						cell.ToolTipText = new DateTime(
							this.currentDate.Year, this.currentDate.Month, numDay
						).ToString( "d" );
						
						// Highlight, if needed
						if ( importantDays.Exists( x => x == numDay ) ) {
							this.HighlightCell( cell );
						}
						else
						if ( numCell == ( MaxDaysOfWeek - 1 )
						  || this.Holidays.Exists(
							      x => x.Equals( new DateTime( this.CurrentDate.Year, this.currentDate.Month, numDay ) ) ) )
						{
							this.MarkCellAsHoliday( cell );
						}
							                          
						
						// Pass to the next day in calendar
						++numDay;
					} else {
						cell.Style = disabledStyle;
					}
				}
			}
			
			this.UpdateDateInfo( this.CurrentDate );
			this.SyncSelectedDay();
			this.grdDayGrid.Show();
			return;
		}
		
		/// <summary>
		/// Show the calendar and its panel container.
		/// </summary>
		public void UpdateView()
		{
			this.FillDaysGrid();
		}

		/// <summary>
		/// Gets the cell position for a given day.
		/// </summary>
		/// <returns>
		/// The cell position for day, as a CellPosition object.
		/// </returns>
		/// <param name='day'>
		/// Day to get the coordinates for.
		/// </param>
		public CellPosition GetCellPositionForDay(int day)
		{
			var toret = new CellPosition();

			// Check for a valid day
			if ( day < 1
			  || day > this.LastDay.Day )
			{
				throw new ArgumentOutOfRangeException(
					"GetCellPositionForDay(), day: " + Convert.ToString( day ) );
			}
			
			// Do it -- yep, this is a sequential search
			// Tired of calculating (and always nearly working)
			for(int i = 0; i < this.grdDayGrid.Rows.Count; ++i) {
				var cells = this.grdDayGrid.Rows[ i ].Cells;

				for(int j = 0; j < cells.Count; ++j) {
					int calDay = 0;
					string value = (string) cells[ j ].Value;

					if ( value != null ) {
						if ( int.TryParse( value.Trim(), out calDay ) ) {
							if ( day == calDay ) {
								toret.Row = i;
								toret.Col = j;
								break;
							}
						}
					}
				}
			}

			return toret;
		}
		
		/// <summary>
		/// Gets the day for a given cell position.
		/// </summary>
		/// <returns>
		/// The day for cell position.
		/// </returns>
		/// <param name='pos'>
		/// Position.
		/// </param>
		public int GetDayForCellPosition(CellPosition pos)
		{
			// Check for sanity
			if ( pos.Row < 0
			  || pos.Col < 0 )
			{
				throw new ArgumentOutOfRangeException( "invalid cell: " + pos.ToString() );
			}
			
			// Do it
			string strDay = (string) this.grdDayGrid.Rows[ pos.Row ].Cells[ pos.Col ].Value;
			int toret = 1;

			if ( strDay != null ) {
				strDay = strDay.Trim();
				if (!( Int32.TryParse( strDay, out toret ) )) {
					toret = 1;
				}
			} else {
				throw new ArgumentOutOfRangeException( "invalid cell: " + pos.ToString() );
			}

			return toret;
		}
		
		/// <summary>
		/// Gets the day for cell position.
		/// </summary>
		/// <returns>
		/// The day for cell position.
		/// </returns>
		/// <param name='row'>
		/// Row.
		/// </param>
		/// <param name='col'>
		/// Col.
		/// </param>
		public int GetDayForCellPosition(int row, int col)
		{
			return GetDayForCellPosition( new CellPosition( row, col ) );
		}
		
		/// <summary>
		/// Highlight the specified row and column.
		/// </summary>
		/// <param name='row'>
		/// Row.
		/// </param>
		/// <param name='column'>
		/// Column.
		/// </param>
		protected void HighlightCell(int row, int column)
		{
			this.HighlightCell( this.grdDayGrid.Rows[ row ].Cells[ column ] );
		}
		
		protected void HighlightCell(DataGridViewCell cell)
		{
			var style = new DataGridViewCellStyle();
			
			style.Font = new Font( this.Font, FontStyle.Bold );
			cell.Style = style;
			cell.Style.BackColor = this.ImportantDaysBgColor;
			cell.Style.ForeColor = this.ImportantDaysFgColor;
		}
		
		/// <summary>
		/// Highlights the cell.
		/// </summary>
		/// <param name='pos'>
		/// Position.
		/// </param>
		protected void HighlightCell(CellPosition pos)
		{
			this.HighlightCell( pos.Row, pos.Col );
		}
		
		/// <summary>
		/// Marks the cell as holiday.
		/// </summary>
		/// <param name='pos'>
		/// The position of the cell, as a CellPosition object.
		/// </param>
		protected void MarkCellAsHoliday(CellPosition pos)
		{
			this.MarkCellAsHoliday( pos.Row, pos.Col );
		}
		
		/// <summary>
		/// Marks the cell as holiday.
		/// </summary>
		/// <param name='row'>
		/// Row.
		/// </param>
		/// <param name='column'>
		/// Column.
		/// </param>
		protected void MarkCellAsHoliday(int row, int column)
		{
			this.MarkCellAsHoliday( this.grdDayGrid.Rows[ row ].Cells[ column ] );
			
		}
		
		/// <summary>
		/// Marks the cell as holiday.
		/// </summary>
		/// <param name='cell'>
		/// The cell to mark, as an existing Cell object
		/// </param>
		protected void MarkCellAsHoliday(DataGridViewCell cell)
		{
			cell.Style = new DataGridViewCellStyle();
			cell.Style.Font = new Font( this.Font, this.Font.Style );
			cell.Style.BackColor = this.HolidaysBgColor;
			cell.Style.ForeColor = this.HolidaysFgColor;
		}
		
		/// <summary>
		/// Highlights the days in importantDates.
		/// </summary>
		public void HighlightDays()
		{
			this.HighlightDays( this.ImportantDaysOfMonth );
		}
		
		/// <summary>
		/// Highlights the days.
		/// </summary>
		/// <param name='importantDays'>
		/// Days to have highlighted.
		/// </param>
		public void HighlightDays(int[] importantDays)
		{
			if ( this.Visible ) {
				var importantDaysList = new List<int>( importantDays );
							
				// Prepare important days info
				importantDaysList.Sort();
				for(int i = 0; i < importantDaysList.Count; ++i) {
					if ( importantDaysList[ i ] < 1
					  || importantDaysList[ i ] > GetLastDay( this.CurrentDate ).Day )
					{
						importantDaysList.RemoveAt( i );
					}
				}
				
				this.importantDaysOfMonth = importantDaysList.ToArray();
				
				// Run all over list, updating each cell of the day grid
				foreach(int day in this.ImportantDaysOfMonth) {
					this.HighlightCell( GetCellPositionForDay( day ) );
				}
			}
			
			return;
		}
		
		/// <summary>
		/// Gets or sets the current date.
		/// </summary>
		/// <value>
		/// The current date.
		/// </value>
		public DateTime CurrentDate {
			get { return this.currentDate; }
			set { this.currentDate = value; this.FillDaysGrid(); }
		}
		
		/// <summary>
		/// Gets or sets the important dates.
		/// </summary>
		/// <value>
		/// The important dates.
		/// </value>
		public int[] ImportantDaysOfMonth {
			get { return this.importantDaysOfMonth; }
			set { this.importantDaysOfMonth = value; }
		}
		
		public Color ImportantDaysBgColor {
			get; set;
		}
		
		public Color ImportantDaysFgColor {
			get; set;
		}
		
		public Color HolidaysFgColor {
			get; set;
		}
		
		public Color HolidaysBgColor {
			get; set;
		}

		public Color DisabledBgColor {
			get { return this.disabledBgColor; }
			set { this.disabledBgColor = this.disabledStyle.BackColor = value; }
		}
		
		public List<DateTime> Holidays {
			get { return this.holidays; }
		}

		public new int Width {
			get { return base.Width; }
			set {
				base.Width = value;
				int width = this.ClientRectangle.Width;
				int colWidth = 0;
				int numCols = this.grdDayGrid.ColumnCount;

				// Resize
				colWidth = (int) Math.Floor( ( (double) width ) / numCols );
				foreach (DataGridViewColumn col in this.grdDayGrid.Columns) {
					col.Width = colWidth;
				}
			}
		}
		
		private Button btNextMonth;
		private Button btPreviousMonth;
		private DataGridView grdDayGrid;
		private DateTime currentDate;
		private Label lblCurrentDate;
		private int[] importantDaysOfMonth = new int[]{};
		private List<DateTime> holidays = new List<DateTime>();
		private DataGridViewCellStyle disabledStyle = null;
		private Color disabledBgColor = Color.Gray;
		public static ReadOnlyCollection<string> Headers;
	}
}

