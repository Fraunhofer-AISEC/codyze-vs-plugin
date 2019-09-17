using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CodyzeVSPlugin.Highlighting
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(HighlightingFormatHandler.DefaultFormat)]
    [UserVisible(true)]
    internal class HighlightWordDefaultFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordDefaultFormatDefinition()
        {
            this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkBlue;
            this.DisplayName = "Highlight Word";
            this.ZOrder = 5;
        }
    }
}
