using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

using Google.GData.Extensions;
using Google.GData.Calendar;

using pgcal.Core;

namespace pgcal.Gui {
	public class MainWindow: Form {
		public const string EtqWidth  = "width";
		public const string EtqHeight = "height";
		public const string EtqUsr    = "usr";
		public const string EtqClosingMinimizes = "closing_minimizes";
		public const string EtqLocale = "locale";
		public const string EtqShowMainMenu = "show_mainmenu";
		public const string CfgFileName = ".pgcal.cfg";

		public MainWindow()
		{
			this.OnTransaction = false;
			this.ReadConfiguration();
			this.Build();
		}

		/// <summary>
		/// This is executed after the constructor,
		/// and when the window is already created.
		/// </summary>
		protected override void OnLoad(EventArgs e)
        {
            this.SetStatus();
            this.SetConnectMode();
        }

		private void BuildIcons()
		{
            this.appIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.calendarIcon.png" )
			);

			this.backIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.backIcon.png" )
			);

            this.editIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.editIcon.png" )
			);

            this.saveIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.saveIcon.png" )
			);

            this.deleteIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.deleteIcon.png" )
			);

			this.exportIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.exportIcon.png" )
			);

			this.infoIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.infoIcon.png" )
			);

			this.refreshIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.refreshIcon.png" )
			);

			this.settingsIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.settingsIcon.png" )
			);

			this.webIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.webIcon.png" )
			);

			this.userIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.userIcon.png" )
			);

			this.shutdownIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.shutdownIcon.png" )
			);

			this.windowIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.windowIcon.png" )
			);

			this.nextIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.nextIcon.png" )
			);

			this.previousIconBmp = new Bitmap(
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream( "pgcal.Res.previousIcon.png" )
			);

			return;
		}

		private void BuildNotificationIcon()
		{
			this.trayIcon = new NotifyIcon( new System.ComponentModel.Container() );
            this.trayIcon.Icon = Icon.FromHandle( appIconBmp.GetHicon() );
            this.trayIcon.Text = AppInfo.Name + ' ' + AppInfo.Version;
            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += this.OnTrayIconClicked;

			// Create context menu
			this.trayIcon.ContextMenuStrip = new ContextMenuStrip();

            this.opCntxtShowOrHide = new ToolStripMenuItem();
			this.opCntxtExit = new ToolStripMenuItem();
            this.opCntxtShowOrHide.Text = StringsL18n.Get( StringsL18n.StringId.CtHide );
            this.opCntxtShowOrHide.Click += this.OnTrayIconClicked;
			this.opCntxtShowOrHide.Image = this.windowIconBmp;
			this.opCntxtExit.Text = StringsL18n.Get( StringsL18n.StringId.CtQuit );
			this.opCntxtExit.Click += this.OnQuit;
			this.opCntxtExit.Image = this.shutdownIconBmp;

			this.trayIcon.ContextMenuStrip.Items.AddRange(
				new ToolStripItem[] { this.opCntxtShowOrHide, this.opCntxtExit }
			);

			return;
		}

		private void BuildCalendar()
		{
			this.calCalendar = new CalendarView( new Font( SystemFonts.MessageBoxFont.FontFamily, 14 ) );
			this.calCalendar.HolidaysBgColor = Color.White;
			this.calCalendar.Dock = DockStyle.Bottom;
			this.calCalendar.TabIndex = 2;
			this.calCalendar.DateChanged += OnDateChanged;
		}

		private void BuildEventsListTable()
		{
			this.grdEventsList = new DataGridView();
			this.grdEventsList.AllowUserToResizeRows = false;
			this.grdEventsList.RowHeadersVisible = false;
			this.grdEventsList.AutoGenerateColumns = false;
			this.grdEventsList.MultiSelect = false;

			var textCellTemplate = new DataGridViewTextBoxCell();
			var imageSaveTemplate = new DataGridViewImageCell();
			var imageDeleteTemplate = new DataGridViewImageCell();
			textCellTemplate.Style.BackColor = Color.Wheat;
			imageSaveTemplate.Value = this.saveIconBmp;
			imageDeleteTemplate.Value = this.deleteIconBmp;

			var column0 = new DataGridViewTextBoxColumn();
			var column1 = new DataGridViewTextBoxColumn();
			var column2 = new DataGridViewTextBoxColumn();
			var column3 = new DataGridViewTextBoxColumn();
			var column4 = new DataGridViewTextBoxColumn();
			var column5 = new DataGridViewImageColumn();
			var column6 = new DataGridViewImageColumn();

			column0.CellTemplate = textCellTemplate;
			column1.CellTemplate = textCellTemplate;
			column2.CellTemplate = textCellTemplate;
			column3.CellTemplate = textCellTemplate;
			column4.CellTemplate = textCellTemplate;
			column5.CellTemplate = imageSaveTemplate;
			column6.CellTemplate = imageDeleteTemplate;

			column0.HeaderText = "Date";
			column0.Width = 75;
			column0.SortMode = DataGridViewColumnSortMode.NotSortable;
			column1.HeaderText = "Time";
			column1.Width = 60;
			column1.SortMode = DataGridViewColumnSortMode.NotSortable;
			column2.HeaderText = "Event";
			column2.Width = 225;
			column2.SortMode = DataGridViewColumnSortMode.NotSortable;
			column3.HeaderText = "End";
			column3.Width = 60;
			column3.SortMode = DataGridViewColumnSortMode.NotSortable;
			column4.HeaderText = "Place";
			column4.Width = 100;
			column4.SortMode = DataGridViewColumnSortMode.NotSortable;
			column5.HeaderText = "";
			column6.HeaderText = "";
			column5.Width = 20;
			column5.SortMode = DataGridViewColumnSortMode.NotSortable;
			column6.Width = 20;
			column6.SortMode = DataGridViewColumnSortMode.NotSortable;
			column5.ReadOnly = true;
			column6.ReadOnly = true;

			this.grdEventsList.Columns.AddRange( new DataGridViewColumn[] {
				column0,
				column1,
				column2,
				column3,
				column4,
				column5,
				column6
			} );

			this.grdEventsList.CellContentClick += this.OnCellClicked;
			this.grdEventsList.Dock = DockStyle.Fill;
			this.grdEventsList.TabIndex = 3;
			this.grdEventsList.UserAddedRow += this.OnRowAdded;
			this.grdEventsList.AllowUserToOrderColumns = false;
			this.pnlInfo = new Panel();
			this.pnlInfo.SuspendLayout();
			this.pnlInfo.Dock = DockStyle.Fill;
			this.pnlEventsContainer = new Panel();
			this.pnlEventsContainer.Dock = DockStyle.Fill;
			this.pnlEventsContainer.Controls.Add( this.grdEventsList );
			this.pnlEventsContainer.Controls.Add( this.calCalendar );
			this.pnlInfo.Controls.Add( this.pnlEventsContainer );
		}

		private void BuildMenu()
		{
			this.opRefresh = new ToolStripMenuItem( "&Refresh" );
			this.opRefresh.Click += ( obj, args ) => this.UpdateCurrentEventEntry(); 
			this.opRefresh.ShortcutKeys = Keys.F5;
			this.opRefresh.Image = this.refreshIconBmp;

			this.opNextMonth = new ToolStripMenuItem( "&Next month" );
			this.opNextMonth.Click += this.OnNextMonth;
			this.opNextMonth.ShortcutKeys = Keys.Alt | Keys.Right;
			this.opNextMonth.Image = this.nextIconBmp;

			this.opPreviousMonth = new ToolStripMenuItem( "&Previous month" );
			this.opPreviousMonth.Click += this.OnPreviousMonth;
			this.opPreviousMonth.ShortcutKeys = Keys.Alt | Keys.Left;
			this.opPreviousMonth.Image = this.previousIconBmp;

			this.opExportEvent = new ToolStripMenuItem( "&Export event..." );
			this.opExportEvent.Click += this.OnExportEvent;
			this.opExportEvent.ShortcutKeys = Keys.F2;
			this.opExportEvent.Image = this.exportIconBmp;

			this.opSettings = new ToolStripMenuItem( "&Settings" );
			this.opSettings.Click += (obj, args ) => this.ShowSettings();
			this.opSettings.Image = this.settingsIconBmp; 

			this.opChangeUsr = new ToolStripMenuItem( "&Change user" );
			this.opChangeUsr.Click += this.OnChangeUsr;
			this.opChangeUsr.Image = this.userIconBmp;

			this.opQuit = new ToolStripMenuItem( "&Quit" );
			this.opQuit.ShortcutKeys = Keys.Control | Keys.Q;
			this.opQuit.Click += this.OnQuit;
			this.opQuit.Image = this.shutdownIconBmp;

			this.opAbout = new ToolStripMenuItem( "&About" );
			this.opAbout.Click += this.OnAbout;
			this.opAbout.Image = this.infoIconBmp;
			var opWeb = new ToolStripMenuItem( "&Web" );
			opWeb.Click += (sender, e) => this.OnShowWeb();
			opWeb.Image = this.webIconBmp;

			this.mFile = new ToolStripMenuItem( "&File" );
			this.mHelp = new ToolStripMenuItem( "&Help" );
			this.mTools = new ToolStripMenuItem( "&Tools" );

			this.mFile.DropDownItems.AddRange( new ToolStripItem[] {
				this.opChangeUsr, this.opExportEvent, this.opQuit
			});

			this.mHelp.DropDownItems.AddRange( new ToolStripItem[]{
				opWeb, this.opAbout
			});

			this.mTools.DropDownItems.AddRange( new ToolStripItem[]{
				this.opRefresh, this.opNextMonth,
				this.opPreviousMonth, this.opSettings
			});

			this.mMain = new MenuStrip();
			this.mMain.Items.AddRange( new ToolStripItem[] {
				this.mFile, this.mTools, this.mHelp }
			);

			return;
		}

		private void BuildStatusBar()
		{
			this.sbStatus = new StatusStrip();
			this.lblStatus = new ToolStripStatusLabel();
			this.sbStatus.Items.Add( this.lblStatus );
			this.sbStatus.Dock = DockStyle.Bottom;
		}

		private void BuildLoginPanel()
		{
			this.pnlLogin = new Panel();
            this.pnlLogin.SuspendLayout();
            this.pnlLogin.Dock = DockStyle.Bottom;
            this.pnlLogin.BackColor = Color.LightYellow;
            this.pnlInfo.Controls.Add( this.pnlLogin );

            this.edtPsw = new TextBox();
            this.edtPsw.Dock = DockStyle.Right;
            this.edtPsw.TabIndex = 1;
            this.edtPsw.PasswordChar = '*';
            this.edtUsr = new TextBox();
            this.edtUsr.Dock = DockStyle.Right;
            this.edtUsr.TabIndex = 0;
            this.edtUsr.KeyDown += this.OnEdtUsrInfoKeyDown;
            this.edtPsw.KeyDown += this.OnEdtUsrInfoKeyDown;

            var pnlUsr = new Panel();
            pnlUsr.SuspendLayout();
            pnlUsr.Dock = DockStyle.Left;
            var pnlPsw = new Panel();
            pnlPsw.SuspendLayout();
            pnlPsw.Dock = DockStyle.Right;

            this.lblUsr = new Label();
            this.lblUsr.Text = "User";
			this.lblUsr.TabIndex = 2;
            this.lblUsr.Dock = DockStyle.Left;
            this.lblPsw = new Label();
            this.lblPsw.Text = "Password";
			this.lblPsw.TabIndex = 3;
            this.lblPsw.Dock = DockStyle.Left;

            pnlUsr.Controls.Add( this.lblUsr );
            pnlUsr.Controls.Add( edtUsr );

            pnlPsw.Controls.Add( this.lblPsw );
            pnlPsw.Controls.Add( edtPsw );

            this.pnlLogin.Controls.Add( pnlPsw );
            this.pnlLogin.Controls.Add( pnlUsr );
            this.pnlLogin.ClientSize = new Size( this.pnlLogin.Width, this.edtUsr.Height );

			pnlUsr.ResumeLayout( false );
            pnlPsw.ResumeLayout( false );
		}

		void BuildAboutPanel(int charSize)
		{
			this.pnlAbout = new Panel();
			this.pnlAbout.SuspendLayout();
			this.pnlAbout.Dock = DockStyle.Bottom;
			this.pnlAbout.BackColor = Color.LightYellow;

			var lblAbout = new Label();
			lblAbout.Text = AppInfo.Name + " v" + AppInfo.Version + ", " + AppInfo.Author;
			lblAbout.Dock = DockStyle.Left;
			lblAbout.TextAlign = ContentAlignment.MiddleCenter;
			lblAbout.AutoSize = true;

			var font = new Font( lblAbout.Font, FontStyle.Bold );
			font = new Font( font.FontFamily, 14 );
			lblAbout.Font = font;

			var btCloseAboutPanel = new Button();
			btCloseAboutPanel.Text = "X";
			btCloseAboutPanel.Font = new Font( btCloseAboutPanel.Font, FontStyle.Bold );
			btCloseAboutPanel.Dock = DockStyle.Right;
			btCloseAboutPanel.Width = charSize * 5;
			btCloseAboutPanel.FlatStyle = FlatStyle.Flat;
			btCloseAboutPanel.FlatAppearance.BorderSize = 0;
			btCloseAboutPanel.Click += (obj, args) => this.pnlAbout.Hide();

			this.pnlAbout.Controls.Add( lblAbout );
			this.pnlAbout.Controls.Add( btCloseAboutPanel );
			this.pnlAbout.Hide();
			this.pnlAbout.MinimumSize = new Size( this.Width, lblAbout.Height + 5 );
			this.pnlAbout.MaximumSize = new Size( Int32.MaxValue, lblAbout.Height + 5 );
			this.pnlAbout.ResumeLayout( false );
			this.pnlInfo.Controls.Add( this.pnlAbout );
		}

		private void BuildSettingsPanel()
		{
			this.pnlSettings = new TableLayoutPanel();
			this.pnlSettings.BackColor = Color.White;
			this.pnlSettings.Dock = DockStyle.Top;
			this.pnlSettings.ColumnCount = 1;
			this.pnlSettings.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
			this.pnlSettings.SuspendLayout();

			// Button
			var btClose = new Button();
			btClose.BackColor = Color.White;
			btClose.Image = this.backIconBmp;
			btClose.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			btClose.Font = new Font( btClose.Font, FontStyle.Bold );
			btClose.FlatStyle = FlatStyle.Flat;
			btClose.FlatAppearance.BorderSize = 0;
			btClose.Click += (sender, e) => this.ChangeSettings();
			this.pnlSettings.Controls.Add( btClose );

			var pnlChks = new TableLayoutPanel();
			pnlChks.Dock = DockStyle.Top;
			pnlChks.SuspendLayout();
			this.pnlSettings.Controls.Add( pnlChks );

			// Checkbox about minimizing to notify icon
			this.chkMinimizeToNotifyIcon = new CheckBox();
			this.chkMinimizeToNotifyIcon.Dock = DockStyle.Top;
			this.chkMinimizeToNotifyIcon.Text = StringsL18n.Get( StringsL18n.StringId.CkClose );
			this.chkMinimizeToNotifyIcon.Checked = this.IsMinimizedToNotifyIcon;
			pnlChks.Controls.Add( this.chkMinimizeToNotifyIcon );

			// Checkbox about showing the menu
			this.chkShowMainMenu = new CheckBox();
			this.chkShowMainMenu.Dock = DockStyle.Top;
			this.chkShowMainMenu.Text = StringsL18n.Get( StringsL18n.StringId.CkShowMenu );
			this.chkShowMainMenu.Checked = this.IsMainMenuShown;
			pnlChks.Controls.Add( this.chkShowMainMenu );

			// Locale
			var pnlLocales = new Panel();
			pnlLocales.Dock = DockStyle.Top;
			this.lblLocales = new Label();
			this.lblLocales.Text = StringsL18n.Get( StringsL18n.StringId.LblLanguage );;
			this.lblLocales.Dock = DockStyle.Left;

			this.cbLocales = new ComboBox();
			this.cbLocales.Dock = DockStyle.Fill;
			this.cbLocales.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cbLocales.Text = Locale.CurrentLocale.ToString();

			CultureInfo[] locales = CultureInfo.GetCultures( CultureTypes.SpecificCultures );
			Array.Sort( locales,
				((CultureInfo x, CultureInfo y) => x.ToString().CompareTo( y.ToString() ) )
			);

			this.cbLocales.Items.Add( "<local>" );
			foreach(CultureInfo locale in locales ) {
				this.cbLocales.Items.Add( locale.NativeName + ": " + locale.ToString() );
			}

			pnlLocales.Controls.Add( this.cbLocales );
			pnlLocales.Controls.Add( this.lblLocales );
			this.pnlSettings.Controls.Add( pnlLocales );

			// Finishing
			pnlChks.ResumeLayout( false );
			this.pnlSettings.ResumeLayout( false );
			this.pnlSettings.Hide();

			// Sizes
			this.pnlSettings.MinimumSize = new Size(
				this.ClientSize.Width,
				SystemInformation.VirtualScreen.Height
			);
			return;
		}

		private void ShowSettings()
		{
			// Prepare configuration
			this.cbLocales.Text = Locale.CurrentLocaleToDescription();
			this.chkMinimizeToNotifyIcon.Checked = this.IsMinimizedToNotifyIcon;
			this.chkShowMainMenu.Checked = this.IsMainMenuShown;

			// UI
			this.SaveCurrentStatus();
			this.SetStatus( opSettings.Text );
			this.tbBar.Hide();
			this.mMain.Hide();
			this.pnlInfo.Hide();
			this.pnlSettings.Show();
			this.chkShowMainMenu.Focus();
		}

		private void ChangeSettings()
		{
			string locale = string.Empty;

			if ( this.cbLocales.SelectedItem != null ) {
				locale = this.cbLocales.Text;
			}

			// Apply configuration
			Locale.SetLocaleFromDescription( locale );
			this.ChangeUILanguage( Locale.CurrentLocale );
			this.IsMinimizedToNotifyIcon = this.chkMinimizeToNotifyIcon.Checked;
			this.MainMenuStrip.Visible = this.IsMainMenuShown = this.chkShowMainMenu.Checked;

			// UI
			this.RestoreStatus();
			this.tbBar.Show();
			this.mMain.Visible = this.IsMainMenuShown;
			this.pnlInfo.Show();
			this.pnlSettings.Hide();
			this.UpdateCurrentEventEntry();
		}

		private void BuildToolbar()
		{
			this.tbBar = new ToolBar();

			// Create image list
			var imgList = new ImageList();
			imgList.ImageSize = new Size( 24, 24 );
			imgList.Images.AddRange( new Image[]{
				this.exportIconBmp, this.infoIconBmp,
				this.refreshIconBmp, this.webIconBmp,
				this.settingsIconBmp, this.userIconBmp,
				this.shutdownIconBmp
			});

			// Buttons
			this.tbbExport = new ToolBarButton();
			this.tbbExport.ImageIndex = 0;
			this.tbbRefresh = new ToolBarButton();
			this.tbbRefresh.ImageIndex = 2;
			this.tbbSettings = new ToolBarButton();
			this.tbbSettings.ImageIndex = 4;
			this.tbbWeb = new ToolBarButton();
			this.tbbWeb.ImageIndex = 3;
			this.tbbInfo = new ToolBarButton();
			this.tbbInfo.ImageIndex = 1;
			this.tbbUser = new ToolBarButton();
			this.tbbUser.ImageIndex = 5;
			this.tbbShutdown = new ToolBarButton();
			this.tbbShutdown.ImageIndex = 6;

			// Triggers
			this.tbBar.ButtonClick += (object sender, ToolBarButtonClickEventArgs e)
				=> this.OnToolbarButton( e.Button );

			// Polishing
			this.tbBar.ShowToolTips = true;
			this.tbBar.ImageList = imgList;
			this.tbBar.Dock = DockStyle.Top;
			this.tbBar.BorderStyle = BorderStyle.None;
			this.tbBar.Appearance = ToolBarAppearance.Flat;
			this.tbBar.Buttons.AddRange( new ToolBarButton[] {
				this.tbbUser, this.tbbExport, this.tbbRefresh,
				this.tbbSettings, this.tbbWeb, this.tbbInfo,
				this.tbbShutdown
			});
		}

		private void Build()
		{
			// Start
			this.Hide();
			this.SuspendLayout();

			// Sizes
			Graphics grf = this.CreateGraphics();
			SizeF fontSize = grf.MeasureString( "W", this.Font );
			int charSize = (int) fontSize.Width + 5;

			// Build it!
			this.BuildIcons();
			this.BuildToolbar();
			this.BuildNotificationIcon();   
			this.BuildCalendar();
            this.BuildEventsListTable();
            this.BuildMenu();            
			this.BuildStatusBar();
			this.BuildLoginPanel();
			this.BuildAboutPanel( charSize );
			this.BuildSettingsPanel();

            // Add all to the UI
			this.MainMenuStrip = this.mMain;
			this.Controls.Add( this.pnlInfo );
            this.Controls.Add( this.sbStatus );
			this.Controls.Add( this.pnlSettings );
			this.Controls.Add( this.tbBar );
			this.Controls.Add( this.mMain );

            // Polish UI
            this.Text = AppInfo.Name;
            this.Icon = Icon.FromHandle( appIconBmp.GetHicon() );
            this.Cursor = Cursors.Arrow;
            this.MinimumSize = new Size( 620, 460 );
            this.FormClosing += this.OnFakeClose;
			this.Resize += (obj, args) => this.OnResizeWindow();
			this.TopMost = true;
			
			// Prepare timer for alarms
            this.timAlarm = new System.Windows.Forms.Timer();
			this.timAlarm.Tick += this.OnAlarmCheck;
			this.timAlarm.Interval = 300000;         // 5 sec
			this.timAlarm.Start();

			// Prepare timer for auto refreshing
			this.timRefresh = new System.Windows.Forms.Timer();
			this.timRefresh.Tick += ( o, e ) => { if ( !this.Visible ) { this.UpdateCurrentEventEntry(); } };
			this.timRefresh.Interval = 108000000;    // 30 min
			this.timRefresh.Start();

			// Apply configuration
			this.Width = this.CfgWidth;
			this.Height = this.CfgHeight;
			this.edtUsr.Text = this.CfgUsr;
			this.ChangeUILanguage( Locale.CurrentLocale );
			this.MainMenuStrip.Visible = this.IsMainMenuShown;

            // End
			this.pnlEventsContainer.ResumeLayout( false );
            this.pnlLogin.ResumeLayout( false );
			this.pnlInfo.ResumeLayout( false );
			this.ResumeLayout( true );

			// Show form
			this.StartPosition = FormStartPosition.CenterScreen;
			this.CenterToScreen();
			this.Show();
			this.OnResizeWindow();
			this.Activated += (sender, e) => this.UpdateCurrentEventEntry();
            return;			
        }
		
		/// <summary>
		/// Refresheses the ui for the new items
		/// (if needed, not mandatory)
		/// </summary>
		/// <param name='time'>
		/// The events that should be reflected in the ui
		/// </param>
		protected void Update(DateTime time)
		{
			this.Update( time, false );
		}
		
		/// <summary>
		/// Refresheses the ui for the new items
		/// </summary>
		/// <param name='time'>
		/// The events that should be reflected in the ui
		/// </param>
		/// <param name='mandatory'>
		/// Indidcates whether the update should be done mandatorily
		/// </param>
		protected void Update(DateTime time, bool mandatory)
        {
			Exception exc = null;
			
            if ( this.GCalFrm != null )
			{
                if ( !this.OnTransaction
				  && ( this.GCalFrm.NeedsUpdateFor( time )
					|| mandatory ) )
				{
                    // Prepare
                    this.DeactivateGui();
                    this.OnTransaction = true;
					this.SetStatus( StringsL18n.Get( StringsL18n.StringId.StRetrieve )
					               		+ this.GCalFrm.Usr );
					
                    // Retrieve event data
                    Thread dataThread = new Thread( delegate() {
						try {
							this.GCalFrm.RefreshFeed( time, mandatory );
						} catch(Exception e)
						{
							exc = e;
						}
                    }
                    );
                    dataThread.Start();
					this.WaitForThread( dataThread, ref exc );
					
					// Compile all dates
                    this.MarkDaysInCalendar();
					
                    // Populate the gridview table with the events retrieved
                    this.UpdateTableEventsList();
					
                    // Prepare the calendar GUI
					this.calCalendar.CurrentDate = time;
                    this.OnTransaction = false;
                    this.ActivateGui();
                    this.SetStatus();
                }
				
                // Mark the event
                bool dateMatched;
                this.SelectEventEntryByDate(
					this.GCalFrm.LocateNextEventInCurrentEvents( time, out dateMatched ),
					dateMatched,
					time
				);
            }
			
            return;
        }

		/// <summary>
		/// Waits for a thread to end.
		/// </summary>
		/// <param name='thread'>
		/// The thread to wait for.
		/// </param> 
		protected void WaitForThread(Thread thread, ref Exception exc)
		{
			this.Enabled = false;
			
			while ( !thread.Join( 1 ) ) {
                Application.DoEvents();
                Thread.Sleep( 5 );
            }
						
			this.Enabled = true;
			
			if ( exc != null ) {
				throw exc;
			}
		}

		void SelectEventEntryRow(int pos)
		{
			try {
				this.grdEventsList.ClearSelection();

				if ( pos >= 0
				    && this.grdEventsList.Rows.Count > pos )
				{			
					this.grdEventsList.CurrentCell = this.grdEventsList.Rows[ pos ].Cells[ 0 ];
					this.grdEventsList.Rows[ pos ].Selected = true;
				}
			} catch(Exception)
			{
			}

			return;
		}
		
		/// <summary>
		/// Selects the event entry given by row number pos.
		/// </summary>
		/// <param name='pos'>
		/// The row number that identifies the event entry
		/// </param>
		protected void SelectEventEntryByDate(int pos)
		{
			this.SelectEventEntryByDate( pos, true, this.calCalendar.CurrentDate );
		}
		
		/// <summary>
		/// Selects the event entry given by row number pos.
		/// If the match parameter is false, then it fills that date in the grid.
		/// </summary>
		/// <param name='pos'>
		/// The row number that identifies the event entry
		/// </param>
		/// <param name='match'>
		/// Fills that date in the grid if true, it does nothing else otherwise
		/// </param>
		/// <param name='time'>
		/// The time to get the info to fill in
		/// </param>
		protected void SelectEventEntryByDate(int pos, bool match, DateTime time)
        {
			if ( this.GCalFrm != null ){
	            SelectEventEntryRow( pos );
				
	            if ( !match ) {
	                // Remove spurious rows
	                var rowIndex = this.GCalFrm.CurrentEvents.Length;
	                for (; rowIndex < ( this.grdEventsList.Rows.Count -1 ); ++rowIndex ) {
	                    if ( this.IsEmptyRow( this.grdEventsList.Rows[ rowIndex ] ) ) {
	                        this.grdEventsList.Rows.RemoveAt( rowIndex );
	                    }
	                }
					
	                // Add row with the date
	                var lastRow = this.grdEventsList.Rows.Count - 1;
                    var fmtDate = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

	                this.grdEventsList.Rows.Add();
                    var row = this.grdEventsList.Rows[ lastRow ];
                    row.Cells[ 0 ].Value = time.ToString( fmtDate );
                    this.UpdateRowIcons( lastRow );
                    this.UpdateRowIcons( lastRow +1 );
	            }
			}
			
            return;
        }

        protected void UpdateRowIcons(int ndx)
        {
            var saveColumnIndex = this.grdEventsList.Columns.Count -2;
            var deleteColumnIndex = this.grdEventsList.Columns.Count -1;
            var row = this.grdEventsList.Rows[ ndx ];

            row.Cells[ saveColumnIndex ].Value = saveIconBmp;
            row.Cells[ deleteColumnIndex ].Value = deleteIconBmp;
        }

        /// <summary>
        /// Determines whether a given row of the DataGridView is empty.
        /// </summary>
        /// <returns>
        /// <c>true</c> if row is an empty row; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='row'>
        /// The row to be checked.
        /// </param>
        protected bool IsEmptyRow(DataGridViewRow row)
        {
            int i = 1;
            int MaxDataRows = row.Cells.Count - 2;

            for (; i < MaxDataRows; ++i) {
                var data = (string) row.Cells[ i ].Value;

                if ( data != null
                  && data.Trim().Length > 0 )
                {
                    break;
                }
            }

            return ( i >= MaxDataRows );
        }
		
		/// <summary>
		/// Marks the days in calendar,
		/// honoring the current list of events in the google calendar framework.
		/// </summary>
		protected void MarkDaysInCalendar()
		{
			var dates = new List<int>();
			
			// Mark days
			this.SetStatus( StringsL18n.Get( StringsL18n.StringId.StMarking ) );
			
			foreach(EventEntry entry in this.GCalFrm.CurrentEvents)
            {
                if ( entry.Times.Count > 0 ) {
                    foreach(When w in entry.Times) {
                        dates.Add( w.StartTime.Day ); 
                    }
                }
            }
			
			this.calCalendar.ImportantDaysOfMonth = dates.ToArray();
			this.SetStatus();
			return;
		}
		
		/// <summary>
		/// Populates the table dedicated to the event entries,
		/// honoring the current list of events in the google calendar framework.
		/// </summary>
		protected void UpdateTableEventsList()
		{
			this.SetStatus( StringsL18n.Get( StringsL18n.StringId.StListing ) );
			
			this.grdEventsList.Rows.Clear();
            foreach(var entry in this.GCalFrm.CurrentEvents) 
            {
				AddTableEventsListRowWithEvent( entry );
            }
			
			this.SetStatus();
			return;
		}

		/// <summary>
		/// Adds a new row to the table events list with the given event.
		/// </summary>
		/// <param name='entry'>
		/// The event to add to that row
		/// </param>
		protected void AddTableEventsListRowWithEvent(EventEntry entry)
		{
			var columnData = new List<object>();
			var today = DateTime.Now;
					
			// Add the row with the event's data
			columnData.AddRange( CnvtEntryToRow( entry ) );
			columnData.Add( editIconBmp );
			columnData.Add( deleteIconBmp );
			
            int rowIndex = this.grdEventsList.Rows.Add( columnData.ToArray() );

			// Mark as obsolete, if needed
			if ( entry.Times.Count > 0
			  && entry.Times[ 0 ].EndTime < today )
			{
				var row = this.grdEventsList.Rows[ rowIndex ];
				var style = new DataGridViewCellStyle( row.DefaultCellStyle );
				var font = new Font( SystemFonts.DefaultFont, FontStyle.Strikeout );
				style.Font = font;
				row.DefaultCellStyle = style;
			}

			return;
		}
		
		/// <summary>
		/// Converts the entry to a list row
		/// </summary>
		/// <param name='entry'>
		/// The entry to have its information shown.
		/// </param>
		protected static string[] CnvtEntryToRow(EventEntry entry)
		{
			var toret = new List<string>();
					
			// Add date and time
			if ( entry.Times.Count > 0 ) {
				toret.Add( entry.Times[ 0 ].StartTime.ToShortDateString() );
				toret.Add( entry.Times[ 0 ].StartTime.TimeOfDay.ToString() );
            } else {
				toret.Add( GCalFramework.EtqNotAvailable );
				toret.Add( GCalFramework.EtqNotAvailable );
			}
			
			// Title
			toret.Add( entry.Title.Text );

			// Add end
			if ( entry.Times.Count > 0 ) {
				toret.Add( entry.Times[ 0 ].EndTime.TimeOfDay.ToString() );
			} else { 
				toret.Add( GCalFramework.EtqNotAvailable );
			}
			
			// Add place
			if ( entry.Locations.Count > 0 ) {
				Where place = entry.Locations[ 0 ];
				if ( place != null ) {
					toret.Add( place.ValueString );
				}
			} else {
				toret.Add( GCalFramework.EtqNotAvailable );
			}
			
			return toret.ToArray();
		}
		
		/// <summary>
		/// Sets the contents of an entry from a row of the table.
		/// </summary>
		/// <param name='entry'>
		/// The EventEntry to modify
		/// </param>
		/// <param name='row'>
		/// The row number in which the data sits.
		/// </param>
		protected void CnvtRowToEntry(EventEntry entry, int row)
		{
			DateTime start;
			DateTime end;
			const int DateStartColumnIndex = 0;
			const int TimeStartColumnIndex = 1;
			const int TitleColumnIndex = 2;
			const int TimeEndColumnIndex = 3;
			const int PlaceColumnIndex = 4;
			string timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
			string dateStart = "";
			string timeStart = "";
			string timeEnd = "";
			
			// Prepare time information
			entry.Times.Clear();
			entry.Locations.Clear();
			
			try {
				dateStart = ( (string) this.grdEventsList.Rows[ row ].Cells[ DateStartColumnIndex ].Value ).Trim();
			} catch(Exception)
			{
				throw new FormatException( StringsL18n.Get( StringsL18n.StringId.ErStartDateMissing ) );
			}
			
			try {
				timeStart = ( (string) this.grdEventsList.Rows[ row ].Cells[ TimeStartColumnIndex ].Value ).Trim();
			} catch(Exception)
			{
				throw new FormatException( StringsL18n.Get( StringsL18n.StringId.ErStartDateMissing ) );
			}
			
			try {
				timeEnd = ( (string) this.grdEventsList.Rows[ row ].Cells[ TimeEndColumnIndex ].Value ).Trim();
			} catch(Exception)
			{
				timeEnd = timeStart;
			}
			
			// Prepare times
			timeEnd = timeEnd.Trim();
			timeStart = timeStart.Trim();
			
			if ( timeStart.ToLower().EndsWith( GCalFramework.EtqHourMark ) ) {
				timeStart = timeStart.Substring( 0, timeStart.Length -1 ) + timeSeparator + "00";
			}
			
			if ( timeEnd.ToLower().EndsWith( GCalFramework.EtqHourMark ) ) {
				timeEnd = timeEnd.Substring( 0, timeEnd.Length -1 ) + timeSeparator + "00";
			}
			
			// Set start/end date/time
			timeEnd = dateStart + ' ' + timeEnd;
			dateStart = dateStart + ' ' + timeStart;
			
			try {
				start = DateTime.Parse( dateStart, CultureInfo.CurrentCulture.DateTimeFormat  );
			} catch(FormatException exc) {
				throw new FormatException( StringsL18n.Get( StringsL18n.StringId.ErParsingStartTime )
					+ ": " + exc.Message );
			}
				
			try {
				end = DateTime.Parse( timeEnd, CultureInfo.CurrentCulture.DateTimeFormat  );
			} catch(FormatException)
			{
				// Adapt the end time by one hour from the start
				end = start.AddHours( 1 );
			}
			
			// Set times
			if ( start == end ) {
				end = start.AddHours( 1 );
			}
			
			var eventTime = new When( start, end );
			entry.Times.Add( eventTime );
			
			// Set title
			try {
				entry.Title.Text = ( (string) this.grdEventsList.Rows[ row ].Cells[ TitleColumnIndex ].Value ).Trim();
			} catch(Exception)
			{
				entry.Title.Text = GCalFramework.EtqNotAvailable;
			}
			
			if ( entry.Title.Text.Trim().Length == 0 ) {
				entry.Title.Text = GCalFramework.EtqNotAvailable;
			}
			
			// Set place
			string strPlace = null;
			try {
				strPlace = ( (string) this.grdEventsList.Rows[ row ].Cells[ PlaceColumnIndex ].Value ).Trim();
			} catch(Exception)
			{
				strPlace = GCalFramework.EtqNotAvailable;
			}
			
			if ( strPlace.Trim().Length == 0 ) {
				entry.Title.Text = GCalFramework.EtqNotAvailable;
			}
			
			var eventLocation = new Where();
			eventLocation.ValueString = strPlace;
			entry.Locations.Add( eventLocation );
			
			// Set alarm
			Reminder reminder = new Reminder();
			reminder.Minutes = 30;
			reminder.Method = Reminder.ReminderMethod.all;
			entry.Reminder = reminder;
			
			return;
		}
		
		/// <summary>
		/// Deactivates the GUI. Uses EnableGui.
		/// </summary>
		protected void DeactivateGui()
		{
			this.grdEventsList.Rows.Clear();
			this.EnableGui( false );
		}
		
		/// <summary>
		/// Activates the GUI. Uses EnableGui.
		/// </summary>
		protected void ActivateGui()
		{
			this.EnableGui( true );
		}
		
		/// <summary>
		/// Enables or disables the GUI.
		/// </summary>
		/// <param name='active'>
		/// Indicates whether the Gui should be enabled or disabled
		/// </param>
		protected void EnableGui(bool active)
		{
			// Components
			this.calCalendar.Enabled = active;
			this.grdEventsList.Enabled = active;

			// Menu options
			this.opRefresh.Enabled = active;
			this.opPreviousMonth.Enabled = active;
			this.opNextMonth.Enabled = active;
			this.opExportEvent.Enabled = active;

			// Toolbar buttons
			this.tbbExport.Enabled = active;
			this.tbbRefresh.Enabled = active;
		}
		
		/// <summary>
		/// Prepares the UI for connection to google calendar
		/// </summary>
		protected void SetConnectMode()
        {
            // Data
            this.gcalFrm = null;
			
            // Gui
            this.DeactivateGui();
			this.pnlEventsContainer.Hide();
            this.SetStatus( StringsL18n.Get( StringsL18n.StringId.StLogin ) );
			
            this.pnlLogin.Show();
            this.edtUsr.Focus();
            this.ActiveControl = this.edtUsr;
        }
		
		/// <summary>
		/// Prepares the UI to be the client of google calendar
		/// </summary>
		protected void SetCalendarClientMode()
		{
			this.pnlEventsContainer.Show();
			this.pnlLogin.Hide();
			this.ActivateGui();
		}
		
		
		/// <summary>
		/// Tries to connect to google calendar, through a new framework.
		/// </summary>
		protected void Connect()
		{
			try {
				this.SetStatus( StringsL18n.Get( StringsL18n.StringId.StCreatingFramework ) );
				this.gcalFrm = new GCalFramework( this.edtUsr.Text, this.edtPsw.Text );
				this.OnTransaction = false;
				this.SetCalendarClientMode();
				this.UpdateCurrentEventEntry();
				
				this.edtUsr.Text = this.gcalFrm.Usr;
				this.edtPsw.Text = this.gcalFrm.Psw;
				this.SetStatus();
			}
			catch(Exception exc) {
				MessageBox.Show(
					this,
					string.Format( "{0}:\n{1}", 
						StringsL18n.Get( StringsL18n.StringId.ErConnection ),
						exc.Message ),
					AppInfo.Name,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
				
				this.SetConnectMode();
			}
		}
		
		/// <summary>
		/// Inserts a new event, which its data is already in the gridListEvents.
		/// </summary>
		/// <param name='row'>
		/// The number of the row in which the new event sits.
		/// </param>
		protected void InsertNewEvent(int row)
		{
			Exception exc = null;
			EventEntry entry = null;
			string statusMsg = "Inserting new event...";
			
			// Update all newly created events -- remember the last row is not a new event
			for(int i = this.GCalFrm.CurrentEvents.Length; i < ( this.grdEventsList.Rows.Count -1 ); ++i)
			{
				try {
				    entry = new EventEntry();
					
					// Notify user
					statusMsg = string.Format( "{0} #{1}",
						StringsL18n.Get( StringsL18n.StringId.StInsertingEvent ),
						row + 1 );
					this.CnvtRowToEntry( entry, i );
					this.SetStatus( statusMsg + ": " +  entry.Title.Text );
					
					// Do it -- insert
					var insertThread = new Thread( delegate() {
						this.GCalFrm.InsertEvent( entry );
					} );
					insertThread.Start();
					this.WaitForThread( insertThread, ref exc );
				} catch(Exception e)
				{
					MessageBox.Show( statusMsg + '\n' + e.Message, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
			}
			
			// End
			if ( entry != null
			  && entry.Times.Count > 0 )
			{
				this.Update( entry.Times[ 0 ].StartTime, true );
			}
			
			this.SetStatus();
			return;
		}
		
		/// <summary>
		/// Updates an existing event.
		/// </summary>
		/// <param name='row'>
		/// The index of the row of the event to update.
		/// </param>
		protected void ModifyEvent(int row)
		{
			Exception exc = null;
			EventEntry entry = null;
			
			try {
				// Notify
				entry = this.GCalFrm.CurrentEvents[ row ];
				this.SetStatus( string.Format( "{0} #{1}: {2}",
					StringsL18n.Get( StringsL18n.StringId.StModifyingEvent ),
					row + 1,
					entry.Title.Text )
				);
						
				// Modify event
				this.CnvtRowToEntry( entry, row );
				
				var modifyThread = new Thread( delegate() {
					this.GCalFrm.ModifyEvent( row, entry );
				} );
				modifyThread.Start();
				this.WaitForThread( modifyThread, ref exc );
			} catch(Exception)
			{
				string statusMsg = StringsL18n.Get( StringsL18n.StringId.StModifyingEvent );
				
				if ( entry  != null ) {
					statusMsg = string.Format( "{0}: '{1}'",
						StringsL18n.Get( StringsL18n.StringId.ErUnableToUpdateEvent ),
						entry.Title
					);
				}
				
				MessageBox.Show(
					statusMsg
					+ exc.Message,
					AppInfo.Name,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
			}
			finally {
				this.SetStatus();
			}
			
		}
		
		/// <summary>
		/// Deletes the event in position row
		/// </summary>
		/// <param name='row'>
		/// The number of the event in the table
		/// (same position in the table of the framework)
		/// </param>
		protected void DeleteEvent(int row)
		{
			Exception exc = null;
			EventEntry entry = null;
			
			try {
				// Notify user
				entry = this.GCalFrm.CurrentEvents[ row ];
				this.SetStatus( string.Format( "{0} #{1}: {2}",
					StringsL18n.Get( StringsL18n.StringId.StDeletingEvent ),
					row + 1,
					entry.Title.Text )
				);
				
				// Do it -- delete
				var deleteThread = new Thread( delegate() {
					this.GCalFrm.DeleteEvent( row );
				} );
				deleteThread.Start();
				this.WaitForThread( deleteThread, ref exc );
				
				// Notify user -- event deleted
				this.grdEventsList.Rows.RemoveAt( row );
				this.MarkDaysInCalendar();
			} catch(Exception e)
			{
				MessageBox.Show(
					string.Format( "{0}: '{1}': {2}",
						StringsL18n.Get( StringsL18n.StringId.ErUnableToDeleteEvent ),
						entry.Title,
						e.Message ),
					AppInfo.Name,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
			}
			finally {
				this.SetStatus();
			}
			
			return;
		}

		/// <summary>
		/// Checks all alarms
		/// </summary>
		protected void OnAlarmCheck(object sender, EventArgs evt)
		{
			if ( this.GCalFrm != null
			  && this.GCalFrm.CurrentEvents != null )
			{
				// For each event
				for(int i = 0; i < this.GCalFrm.CurrentEvents.Length; ++i) {
					var entry = this.GCalFrm.CurrentEvents[ i ];
					
					// Chk it has start/end times
					if ( entry.Times.Count > 0 ) {
						// and it happens today
						DateTime eventTime = entry.Times[ 0 ].StartTime;
						
						if ( eventTime.Date == DateTime.Now.Date
						  && eventTime > DateTime.Now )
						{
							// Assess difference
							if ( ( eventTime - DateTime.Now ).TotalMinutes <= 10 )	{
								this.SelectEventEntryByDate( i );
								this.trayIcon.ShowBalloonTip(
									30000,
									StringsL18n.Get( StringsL18n.StringId.CtAlarm ),
									entry.Title.Text,
									ToolTipIcon.Info
								);
							}
						}
					}
				}
			}
			
			return;
		}

		/// <summary>
		/// Saves the current status.
		/// </summary>
		protected void SaveCurrentStatus()
		{
			this.statusBeforeSettings = this.lblStatus.Text;
		}

		/// <summary>
		/// Restores the status.
		/// </summary>
		protected void RestoreStatus()
		{
			this.SetStatus( this.statusBeforeSettings );
		}

        /// <summary>
        /// Sets the visible status to the default "Ready"
        /// </summary>
        protected void SetStatus()
        {
            this.SetStatus( StringsL18n.Get( StringsL18n.StringId.StReady ) );
        }

        /// <summary>
        /// Sets the visible status to the message passed.
        /// </summary>
        /// <param name='msg'>
        /// The new message to set as status.
        /// </param>
        protected void SetStatus(string msg)
        {
			this.lblStatus.Text = msg;
			this.sbStatus.Refresh();
            Application.DoEvents();
        }

        /// <summary>
        /// Processes the date being changed on the calendar
        /// </summary>
		protected void OnDateChanged(object sender, DateChangedEventArgs evt)
		{
			// Prepare the ui to reflect the new data
			this.Update( evt.Date );
		}
		
		/// <summary>
		/// Processes the event of the user wanting to change user calendar
		/// </summary>
		protected void OnChangeUsr(object sender, EventArgs evt)
		{
			this.SetConnectMode();
		}
		
		/// <summary>
		/// Triggered by the menu option "export event"
		/// </summary>
		protected void OnExportEvent(object sender, EventArgs e)
		{
			EventEntry[] evts = gcalFrm.CurrentEvents;
			
			if ( evts.Length > 0 ) {
				int selectedRow = this.grdEventsList.CurrentCell.RowIndex;

				if ( selectedRow < 0
				  || selectedRow >= this.GCalFrm.CurrentEvents.Length )
				{
					selectedRow = 0;
				}
				
				var dlgSave = new SaveFileDialog();
				dlgSave.OverwritePrompt = true;
				dlgSave.ValidateNames = true;
				dlgSave.AddExtension = true;
				dlgSave.Filter = "html files (*.html)|*.html|vevent files (*.evt)|*.evt";
				
				this.Hide();
				if ( dlgSave.ShowDialog() == DialogResult.OK ) {
					string fileName = dlgSave.FileName;
					string tempFile = Path.GetTempFileName();
					var cnvt = new EventConverter( this.GCalFrm, evts[ selectedRow ] );
					string contents = "";

					using ( var tw = new StreamWriter( tempFile ) ) {
						// Check we can write the destination file
						if ( !File.Exists( fileName ) ) {
							File.Create( fileName );
							File.Delete( fileName );
						}

						if ( dlgSave.FilterIndex == 1 ) {
							contents = cnvt.ConvertToHtml();
						} else {
							contents = cnvt.ConvertToVEvent();
						}
		
			            // write the contents of the file
			            tw.WriteLine( contents );
			
			            // close the stream
			            tw.Close();
					}

					File.Delete( fileName );
					File.Move( tempFile, fileName );
				}
				
				this.Show();
			}
			
			return;
		}

        /// <summary>
        /// Triggered when the user presses a key in the user or password fields.
        /// </summary>
        protected void OnEdtUsrInfoKeyDown(object sender, KeyEventArgs evt)
        {
			if ( evt.KeyCode == Keys.Enter ) {
				evt.Handled = true;
				evt.SuppressKeyPress = true;

				if ( sender == this.edtUsr ) {
					this.edtPsw.Focus();
				}
				else
				if ( sender == this.edtPsw ) {
					this.Connect();
				}
			}

			return;
        }
		
		/// <summary>
		/// Triggered when the user adds a new row to the data grid.
		/// Adds the "save" meta cell on the last column
		/// </summary>
		protected void OnRowAdded(object sender, DataGridViewRowEventArgs evt)
		{
			var saveColumnIndex = this.grdEventsList.Columns.Count -2;
			var deleteColumnIndex = this.grdEventsList.Columns.Count -1;
			
			evt.Row.Cells[ saveColumnIndex ].Value = saveIconBmp;
			evt.Row.Cells[ deleteColumnIndex ].Value = deleteIconBmp;
		}
		
		/// <summary>
		/// Triggered when the user clicks a cell in the data grid
		/// Saves or deletes the new/modified event.
		/// The last row in the grid is special; don't mess with it.
		/// </summary>
		protected void OnCellClicked(object sender, DataGridViewCellEventArgs evt)
		{			
			if ( evt.ColumnIndex == ( this.grdEventsList.Columns.Count -2 ) ) {
				if ( evt.RowIndex >= this.GCalFrm.CurrentEvents.Length )
				{
					if ( evt.RowIndex < ( this.grdEventsList.Rows.Count -1 ) ) {
						this.InsertNewEvent( evt.RowIndex );
					}
				} else {
					this.ModifyEvent( evt.RowIndex );
				}
			}
			else
			if ( evt.ColumnIndex == ( this.grdEventsList.Columns.Count -1 ) ) {
				if ( evt.RowIndex >= this.GCalFrm.CurrentEvents.Length ) {
					// This row does not have a related event yet, just remove it
					if ( evt.RowIndex < ( this.grdEventsList.Rows.Count -1 ) ) {
						this.grdEventsList.Rows.RemoveAt( evt.RowIndex );
					}
				} else {
					// Remove the row and the event
					this.DeleteEvent( evt.RowIndex );
				}
			}

            return;
		}
		
		protected void OnNextMonth(object sender, EventArgs evt)
		{
			this.Update( this.calCalendar.CurrentDate.AddMonths( 1 ) );
		}
		
		protected void OnPreviousMonth(object sender, EventArgs evt)
		{
			this.Update( this.calCalendar.CurrentDate.AddMonths( -1 ) );
		}

		protected void OnToolbarButton(ToolBarButton bt)
		{
			switch ( this.tbBar.Buttons.IndexOf( bt ) ) {
			case 0:
				this.SetConnectMode();
				break;
			case 1:
				this.OnExportEvent( null, null );
				break;
			case 2:
				this.UpdateCurrentEventEntry();
				break;
			case 3:
				this.ShowSettings();
				break;
			case 4:
				this.OnShowWeb();
				break;
			case 5:
				this.OnAbout( null, null );
				break;
			case 6:
				this.OnQuit( null, null );
				break;
			default:
				throw new ArgumentException( "toolbar button not found" );
			}

			return;
		}

		/// <summary>
		/// Triggers the web site
		/// </summary>
		protected void OnShowWeb()
		{
			System.Diagnostics.Process.Start( AppInfo.Web );
		}
		
		/// <summary>
		/// Show info about the application
		/// </summary>
		protected void OnAbout(object sender, EventArgs evt)
		{
			this.pnlAbout.Show();
		}

		/// <summary>
		/// Raises the fake close event.
		/// It will actually minimize the application
		/// </summary>
		protected void OnFakeClose(object sender, System.ComponentModel.CancelEventArgs evt)
		{
			evt.Cancel = true;
			this.HideWindow();
		}
		
		/// <summary>
		/// Quit the application
		/// </summary>
		protected void OnQuit(object sender, EventArgs evt)
		{
			this.timAlarm.Stop();
			this.FormClosing -= this.OnFakeClose;

			this.trayIcon.Visible = false;
			this.trayIcon.Dispose();

			this.WriteConfiguration();
			Application.Exit();
		}
		
		/// <summary>
		/// Raises the tray icon show event.
		/// </summary>
		protected void OnTrayIconClicked(object sender, EventArgs evt)
        {
			// Determine the state of the window, then hide or show it.
            if ( this.windowHidden ) {
                this.ShowWindow();
            } else {
                this.HideWindow();
			}

			return;
		}

        /// <summary>
        /// Hides the window.
        /// </summary>
        protected void HideWindow()
		{
			this.opCntxtShowOrHide.Text = StringsL18n.Get( StringsL18n.StringId.CtShow );
			this.WindowState = FormWindowState.Minimized;
			this.windowHidden = true;

			if ( this.IsMinimizedToNotifyIcon ) {
				if ( this.ShowInTaskbar ) {
					this.ShowInTaskbar = false;
				}

				this.Hide();
			} else {
				this.ShowInTaskbar |= !this.ShowInTaskbar;
			}

			return;
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        protected void ShowWindow()
		{
			this.opCntxtShowOrHide.Text = StringsL18n.Get( StringsL18n.StringId.CtHide );
			this.Show();
			this.WindowState = FormWindowState.Normal;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.windowHidden = false;
			Application.DoEvents();

			// this.UpdateCurrentEventEntry(); // Not needed, since the Activated event updates
        }

		private void UpdateCurrentEventEntry()
		{
			if ( this.GCalFrm != null ) {
				this.Update( DateTime.Now, true );
			}

			return;
		}

		/// <summary>
		/// Resizes the window.
		/// 7 columns:			560 -40 (fixed columns)		
		/// Date				 75	15%
		/// Time				 60	12%
		/// Event				225	38%
		/// End					 60	12%
		/// Place				100	15%
		/// Create/Modify		 20	FIXED	4%
		/// Delete				 20	FIXED	4%
		/// </summary>
		protected void OnResizeWindow()
		{
			// Get the new measures
			int width = this.pnlEventsContainer.ClientRectangle.Width;

			// Resize the calendar
			this.calCalendar.Width = width;

			// Resize the table of events
			this.grdEventsList.Width = width;

			int grdWidth = width - 40;								// 40 (fixed cols + margin needed)
			this.grdEventsList.Columns[ 0 ].Width = (int) Math.Floor( grdWidth *.16 );		// Date
			this.grdEventsList.Columns[ 1 ].Width = (int) Math.Floor( grdWidth *.12 );		// Time
			this.grdEventsList.Columns[ 2 ].Width = (int) Math.Floor( grdWidth *.40 );		// Event
			this.grdEventsList.Columns[ 3 ].Width = (int) Math.Floor( grdWidth *.12 );		// End
			this.grdEventsList.Columns[ 4 ].Width = (int) Math.Floor( grdWidth *.20 );		// Place
		}

		public string CfgFile
		{
			get {
				if ( this.cfgFile.Trim().Length == 0 ) {
					this.cfgFile = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
							? Environment.GetEnvironmentVariable( "HOME" )
							: Environment.ExpandEnvironmentVariables( "%HOMEDRIVE%%HOMEPATH%" );
					this.cfgFile = System.IO.Path.Combine( cfgFile, CfgFileName );
				}

				return cfgFile;
			}
		}

		protected void ReadConfiguration()
		{
			string line;
			StreamReader file = null;
			Size currentSize = this.Size;

			this.CfgWidth = currentSize.Width;
			this.CfgHeight = currentSize.Height;
			this.IsMainMenuShown = false;

			try {
				// Set current values for height and width, in case they are not set.
				try {
					file = new StreamReader( CfgFile );
				} catch(Exception) {
					var fileCreate = new StreamWriter( CfgFile );
					fileCreate.Close();
					file = new StreamReader( CfgFile );
				}

				line = file.ReadLine();
				while( !file.EndOfStream ) {
					int pos = line.IndexOf( '=' );
					string arg = line.Substring( pos + 1 ).Trim();

					if ( line.ToLower().StartsWith( EtqWidth ) ) {
						if ( pos > 0 ) {
							this.CfgWidth = Convert.ToInt32( arg );
						}
					}
					else
					if ( line.ToLower().StartsWith( EtqHeight ) ) {
						if ( pos > 0 ) {
							this.CfgHeight = Convert.ToInt32( arg );
						}
					}
					else
					if ( line.ToLower().StartsWith( EtqClosingMinimizes ) ) {
						if ( pos > 0 ) {
							this.IsMinimizedToNotifyIcon = Convert.ToBoolean( arg );
						}
					}
					else
					if ( line.ToLower().StartsWith( EtqLocale ) ) {
						if ( pos > 0 ) {
							Locale.SetLocale( arg );
						}
					}
					else
					if ( line.ToLower().StartsWith( EtqUsr ) ) {
						if ( pos > 0 ) {
							this.CfgUsr = arg;
						}
					}
					else
					if ( line.ToLower().StartsWith( EtqShowMainMenu ) ) {
						if ( pos > 0 ) {
							this.IsMainMenuShown = Convert.ToBoolean( arg );
						}
					}

					line = file.ReadLine();
				}

				file.Close();
			} catch(Exception exc)
			{
				MessageBox.Show(
					this,
					string.Format( "{0}:\n{1}",
						StringsL18n.Get( StringsL18n.StringId.StReadingConfig ),
						exc.Message ),
					AppInfo.Name,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
			}
		}

		protected void WriteConfiguration()
		{
			Size currentSize = this.Size;
			int width = currentSize.Width;
			int height = currentSize.Height;
			string usr = this.CfgUsr;

			if ( this.edtUsr != null ) {
				usr = this.edtUsr.Text.Trim();
			}

			// Write configuration
			try {
				var file = new StreamWriter( this.CfgFile );
				file.WriteLine( "{0}={1}", EtqClosingMinimizes, this.IsMinimizedToNotifyIcon );
				file.WriteLine( "{0}={1}", EtqShowMainMenu, this.IsMainMenuShown );
				file.WriteLine( "{0}={1}", EtqWidth, width );
				file.WriteLine( "{0}={1}", EtqHeight, height );
				file.WriteLine( "{0}={1}", EtqUsr, usr );
				file.WriteLine( "{0}={1}", EtqLocale, Locale.GetCurrentLocaleCode() );
				file.WriteLine();
				file.Close();
			} catch(Exception exc)
			{
				MessageBox.Show(
					this,
					string.Format( "{0}:\n{1}",
						StringsL18n.Get( StringsL18n.StringId.StWritingConfig ),
						exc.Message ),
					AppInfo.Name,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);
			}
		}

		private void ChangeUILanguage(CultureInfo locale)
		{
			StringsL18n.SetLanguage( locale );

			// Menus
			this.mFile.Text = StringsL18n.Get( StringsL18n.StringId.MnFile );
			this.mTools.Text = StringsL18n.Get( StringsL18n.StringId.MnTools );
			this.mHelp.Text = StringsL18n.Get( StringsL18n.StringId.MnHelp );
			this.mHelp.Text = StringsL18n.Get( StringsL18n.StringId.MnHelp );
			this.opRefresh.Text = StringsL18n.Get( StringsL18n.StringId.OpRefresh );
			this.opNextMonth.Text = StringsL18n.Get( StringsL18n.StringId.OpNextMonth );
			this.opPreviousMonth.Text = StringsL18n.Get( StringsL18n.StringId.OpPreviousMonth );
			this.opExportEvent.Text = StringsL18n.Get( StringsL18n.StringId.OpExportEvent );
			this.opSettings.Text = StringsL18n.Get( StringsL18n.StringId.OpSettings );
			this.opChangeUsr.Text = StringsL18n.Get( StringsL18n.StringId.OpChangeUser );
			this.opQuit.Text = StringsL18n.Get( StringsL18n.StringId.OpQuit );
			this.opAbout.Text = StringsL18n.Get( StringsL18n.StringId.OpAbout );

			// Toolbar
			this.tbbUser.ToolTipText = this.opChangeUsr.Text;
			this.tbbExport.ToolTipText = this.opExportEvent.Text;
			this.tbbRefresh.ToolTipText = this.opRefresh.Text;
			this.tbbSettings.ToolTipText = this.opSettings.Text;
			this.tbbWeb.ToolTipText = "Web";
			this.tbbInfo.ToolTipText = this.opAbout.Text;
			this.tbbShutdown.ToolTipText = this.opQuit.Text;

			// Login
			this.lblUsr.Text = StringsL18n.Get( StringsL18n.StringId.LblUsr );
			this.lblPsw.Text = StringsL18n.Get( StringsL18n.StringId.LblPsw );

			// Table headers
			this.grdEventsList.Columns[ 0 ].HeaderText =
								StringsL18n.Get( StringsL18n.StringId.HdDate );
			this.grdEventsList.Columns[ 1 ].HeaderText =
								StringsL18n.Get( StringsL18n.StringId.HdTime );
			this.grdEventsList.Columns[ 2 ].HeaderText =
								StringsL18n.Get( StringsL18n.StringId.HdEvent );
			this.grdEventsList.Columns[ 3 ].HeaderText =
								StringsL18n.Get( StringsL18n.StringId.HdEnd );
			this.grdEventsList.Columns[ 4 ].HeaderText =
								StringsL18n.Get( StringsL18n.StringId.HdPlace );

			// Settings
			this.chkShowMainMenu.Text = StringsL18n.Get( StringsL18n.StringId.CkShowMenu );
			this.chkMinimizeToNotifyIcon.Text = StringsL18n.Get( StringsL18n.StringId.CkClose );
			this.lblLocales.Text = StringsL18n.Get( StringsL18n.StringId.LblLanguage );

			// Context menu
			this.opCntxtExit.Text = StringsL18n.Get( StringsL18n.StringId.CtQuit );
			this.opCntxtShowOrHide.Text = StringsL18n.Get( StringsL18n.StringId.CtHide );
		}

		/// <summary>
		/// Gets the current google calendar framework
		/// </summary>
		/// <value>
		/// The current calendar, as a GCalFramework
		/// </value>
		public GCalFramework GCalFrm {
			get { return this.gcalFrm; }
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

		public bool IsOnConnectMode {
			get {
				return ( this.pnlLogin.Visible );
			}
		}

		public bool IsMinimizedToNotifyIcon {
			get; set;
		}

		public string CfgUsr {
			get; set;
		}

		public int CfgHeight {
			get; set;
		}

		public int CfgWidth {
			get; set;
		}

		public bool IsMainMenuShown {
			get; set;
		}
		
		private bool windowHidden = false;
		private string cfgFile = "";
		private string statusBeforeSettings;

		private CalendarView calCalendar;
		private DataGridView grdEventsList;
		private Panel pnlEventsContainer;
		private StatusStrip sbStatus;
		private ToolStripStatusLabel lblStatus;
		private Panel pnlLogin;
		private Panel pnlAbout;
		private TableLayoutPanel pnlSettings;
		private CheckBox chkMinimizeToNotifyIcon;
		private CheckBox chkShowMainMenu;
		private ComboBox cbLocales;
        private TextBox edtUsr;
        private TextBox edtPsw;
		private Panel pnlInfo;
		private NotifyIcon trayIcon;
		private GCalFramework gcalFrm;
		private System.Windows.Forms.Timer timAlarm;
		private System.Windows.Forms.Timer timRefresh;
		private Label lblUsr;
		private Label lblPsw;
		private Label lblLocales;
		private ToolStripMenuItem opCntxtShowOrHide;
		private ToolStripMenuItem opCntxtExit;
		private ToolBar tbBar;

		private ToolBarButton tbbUser;
		private ToolBarButton tbbRefresh;
		private ToolBarButton tbbSettings;
		private ToolBarButton tbbInfo;
		private ToolBarButton tbbWeb;
		private ToolBarButton tbbExport;
		private ToolBarButton tbbShutdown;

		private MenuStrip mMain;
		private ToolStripMenuItem mFile;
		private ToolStripMenuItem mTools;
		private ToolStripMenuItem mHelp;
		private ToolStripMenuItem opRefresh;
		private ToolStripMenuItem opNextMonth;
		private ToolStripMenuItem opPreviousMonth;
		private ToolStripMenuItem opExportEvent;
		private ToolStripMenuItem opSettings;
		private ToolStripMenuItem opChangeUsr;
		private ToolStripMenuItem opQuit;
		private ToolStripMenuItem opAbout;


		private Bitmap appIconBmp;
		private Bitmap backIconBmp;
		private Bitmap saveIconBmp;
		private Bitmap deleteIconBmp;
		private Bitmap editIconBmp;
		private Bitmap exportIconBmp;
		private Bitmap infoIconBmp;
		private Bitmap refreshIconBmp;
		private Bitmap webIconBmp;
		private Bitmap settingsIconBmp;
		private Bitmap userIconBmp;
		private Bitmap shutdownIconBmp;
		private Bitmap windowIconBmp;
		private Bitmap nextIconBmp;
		private Bitmap previousIconBmp;
	}
}

