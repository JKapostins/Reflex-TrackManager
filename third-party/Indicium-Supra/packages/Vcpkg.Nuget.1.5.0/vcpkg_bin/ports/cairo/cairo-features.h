#ifndef CAIRO_FEATURES_H
#define CAIRO_FEATURES_H
/* Chosen from the various possible defines in "../build/Makefile.win32.features.h""
   guided by "../build/Makefile.win32.features". Modify at your own risk.
*/

/* Always for Win32 */
#define CAIRO_HAS_WIN32_SURFACE 1
#define CAIRO_HAS_WIN32_FONT 1

/* Require libpng */
#define CAIRO_HAS_PNG_FUNCTIONS 1
#define CAIRO_HAS_PS_SURFACE 1
#define CAIRO_HAS_PDF_SURFACE 1

// Likely available
#define CAIRO_HAS_SCRIPT_SURFACE 1
#define CAIRO_HAS_SVG_SURFACE 1

/* Always available */
#define CAIRO_HAS_IMAGE_SURFACE 1
#define CAIRO_HAS_MIME_SURFACE 1
#define CAIRO_HAS_RECORDING_SURFACE 1
#define CAIRO_HAS_OBSERVER_SURFACE 1
#define CAIRO_HAS_USER_FONT 1

/* Require GObject */
#define CAIRO_HAS_GOBJECT_FUNCTIONS 1

/* Require FreeType */
#define CAIRO_HAS_FT_FONT 1

/* Require FontConfig */
#define CAIRO_HAS_FC_FONT 1

#endif
