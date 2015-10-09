using System;
using System.Globalization;
using System.Collections.ObjectModel;

namespace pgcal.Core {
	public static class StringsL18n {

		public enum StringId {
			MnFile,
			MnTools,
			MnHelp,
			OpRefresh,
			OpNextMonth,
			OpPreviousMonth,
			OpExportEvent,
			OpSettings,
			OpChangeUser,
			OpQuit,
			OpAbout,
			LblUsr,
			LblPsw,
			StReady,
			StLogin,
			StRetrieve,
			StMarking,
			StListing,
			StCreatingFramework,
			StInsertingEvent,
			StModifyingEvent,
			StUpdatingEvents,
			StDeletingEvent,
			StReadingConfig,
			StWritingConfig,
			HdDate,
			HdTime,
			HdEvent,
			HdEnd,
			HdPlace,
			CtShow,
			CtHide,
			CtQuit,
			CtAlarm,
			CkClose,
			CkShowMenu,
			LblLanguage,
			ErStartDateMissing,
			ErStartTimeMissing,
			ErParsingStartTime,
			ErConnection,
			ErUnableToUpdateEvent,
			ErUnableToDeleteEvent,
		};

		public static readonly ReadOnlyCollection<string> StringsEN =
				new ReadOnlyCollection<string>( new string[] {
				"&File",
				"&Tools",
				"&Help",
				"&Refresh",
				"&Next month",
				"&Previous month",
				"&Export event",
				"&Settings",
				"&Change user",
				"&Quit",
				"&About...",
				"User",
				"Password",
				"Ready",
				"Please, enter user and password for Google Calendar",
				"Retrieving calendar info for: ",
				"Marking days with events...",
				"Creating list...",
				"Creating framework...",
				"Inserting event",
				"Modifying event",
				"Updating events...",
				"Deleting event",
				"Reading settings",
				"Writing settings",
				"Date",
				"Time",
				"Event",
				"End",
				"Place",
				"&Show",
				"&Hide",
				"&Quit",
				"Alarm",
				"Closing &minimizes to notify icon",
				"Show m&ain menu",
				"Language",
				"Start date missing",
				"Start time missing",
				"Error while parsing start time",
				"Connection error",
				"Unable to update event",
				"Unable to delete event",
			}
		);

		public static readonly ReadOnlyCollection<string> StringsES =
			new ReadOnlyCollection<string>( new string[] {
				"&Archivo",
				"&Herramientas",
				"&Ayuda",
				"&Refrescar",
				"&Siguiente mes",
				"&Anterior mes",
				"&Exportar evento",
				"&Preferencias",
				"&Cambiar de usuario",
				"&Terminar",
				"&Acerca de...",
				"Usuario",
				"Contraseña",
				"Preparado",
				"Por favor, introduzca usuario y contraseña de Google Calendar",
				"Descargando calendario para: ",
				"Marcando eventos...",
				"Creando lista...",
				"Creando infraestructura...",
				"Insertando evento",
				"Modificando evento",
				"Refrescando eventos...",
				"Eliminando evento",
				"Leyendo preferencias",
				"Escribiendo preferencias",
				"Fecha",
				"Hora",
				"Evento",
				"Fin",
				"Lugar",
				"&Restaurar",
				"&Ocultar",
				"&Terminar",
				"Alarma",
				"&Cerrar minimiza al icono de notificaciones",
				"Mostrar menu princip&al",
				"Lengua",
				"Falta fecha de comienzo",
				"Falta hora de comienzo",
				"Error interpretando hora de inicio",
				"Error conectando",
				"Imposible refrescar evento",
				"Imposible eliminar evento",
			}
		);

		private static ReadOnlyCollection<string> strings = StringsEN;

		public static void SetLanguage(CultureInfo locale)
		{

			if ( locale.TwoLetterISOLanguageName.ToUpper() == "ES" ) {
				strings = StringsES;
			}
			else
			if ( locale.TwoLetterISOLanguageName.ToUpper() == "EN" ) {
				strings = StringsEN;
			}

			return;
		}

		public static string Get(StringId id)
		{
			string toret = null;
			int numId = (int) id;

			if ( numId < strings.Count ) {
				toret = strings[ numId ];
			}

			return toret;
		}
	}
}
